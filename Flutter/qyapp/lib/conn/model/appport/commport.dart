import 'dart:async';
import 'dart:collection';
import 'dart:isolate';
import 'dart:typed_data';
import 'package:flutter/foundation.dart';
import 'package:qyapp/conn/model/appport/appport.dart';
import 'package:qyapp/conn/model/osport/osport.dart';
import 'package:qyapp/conn/model/protocol/modbus.dart';
import 'package:qyapp/conn/model/protocol/protocolbase.dart';

class CommPort extends AppPortBase{
  //0. Event
  final _onLog = StreamController<Map>.broadcast();

  Stream<Map> get onLog => _onLog.stream;

  //1. Fields - 속성
  bool _isAppOpen = false;
  int _protocolType = ProtocolType.modbus;
  Protocolbase? _protocol;

  //1. Fields - 통신
  Uint8List? _buffer;
  Uint8List? _sendingItem;
  int _lastBufferLength = 0;
  DateTime _sendingTime = DateTime.now();
  DateTime _lastReceiveTime = DateTime.now();

  //2. Property - 상태값
  @override
  bool get isAppOpen => _isAppOpen;

  //2. Property - 통신처리기 옵션
  int timeoutNoneReceive = 3000;
  int timeoutTooLong = 10000;
  int timeoutStopReceive = 5000;

  int get protocolType => _protocolType;
      set protocolType(int value){
        switch(value){
          case ProtocolType.none:
            _protocolType = value;
            _protocol = null;
          case ProtocolType.modbus:
            _protocolType = value;
            _protocol = Modbus();
        }
      }
  Protocolbase? get protocol => _protocol;

  //2. Property - protocol 옵션
  bool errorCodeAdding = false;

  //3. 생성자
  ///통신테스터기 포트
  CommPort(){
    _init();
  }

  //4. Method - override
  @override
  void initialize() {
    _isAppOpen = false;
    super.osPort.close();

    _buffer = null;
    _lastBufferLength = 0;
    _sendingTime = DateTime.now();
    _lastReceiveTime = DateTime.now();
  }
  
  @override
  Future<bool> connect() async {
    if(isAppOpen) return false;
    //OS Port 열기
    if(await super.osPort.open() == false) return false;
    
    _isAppOpen = true;

    return true;
  }

  @override
  Future<bool> disconnect() async {
    _isAppOpen = false;
    super.osPort.close();

    return true;
  }

  @override
  Uint8List? read(){
    Uint8List? bytes = super.osPort.read();
    if(bytes == null) return null;

    if(_buffer == null){
      _buffer = bytes;
    }
    else{
      _buffer!.addAll(bytes);
    }

    _lastReceiveTime = DateTime.now();

    //Event 수신 알림
    if(_onLog.hasListener){
      _onLog.add({
        'type': 'READ',
        'data': _buffer!
      });
    }

    return _buffer;
  }

  @override
  void write(Uint8List bytes){
    if(bytes.isEmpty) return;

    if(protocolType != ProtocolType.none && errorCodeAdding){
      Uint8List? errcode = _protocol!.errcode.createErrorCode(bytes);

      if(errcode != null){
        bytes.addAll(errcode);
      }
    }

    _buffer = null;
    _lastBufferLength = 0;
    _lastReceiveTime = DateTime.now();
    _sendingTime = DateTime.now();

    super.osPort.write(bytes);

    //전송 알림
    if(_onLog.hasListener){
      _onLog.add({
        'type': 'WRITE',
        'data': bytes
      });
    }
  }

  //4. Method - private
  void _init(){
    super.type = PortType.serial;
  }

  Future<bool> protocolResult() async{
    if(_buffer == null) 
    {
      Uint8List? frame = _protocol!.response.extractFrame(_buffer!);

      if(frame != null)
      {
        //Response Frame 추출됨
        bool isErrCodeError = _protocol!.errcode.checkErrorCode(frame) == false;

        //ErrorCode 검사
        if(isErrCodeError){
          //미일치
          if(_onLog.hasListener){
            _onLog.add({
              'type': 'PROTOCOL_ERRORCODE_DISMATCH',
              'data': frame,
            });
          }
        }
        else{
          //Protocol 결과 Item 추출
          List<Object>? items = _protocol!.response.extractItem(frame, subdata: [_sendingItem!]);

          if(_onLog.hasListener){
            _onLog.add({
              'type': 'PROTOCOL_FINISH',
              'data': frame,
              'items': items
            });
          }
        }
      }
    }

    return false;
  }

  ///Timeout
  ///* **returns - true**: Timeout 발생
  ///* **returns - false**: Timeout 미발생
  bool isTimeout(){
    if(_sendingItem == null) return true;

    if(_lastBufferLength == 0){
      //수신 데이터 없음
      if(DateTime.now().difference(_sendingTime).inMilliseconds > timeoutNoneReceive){
        if(_onLog.hasListener){
          _onLog.add({
            'type': 'TIMEOUT_NONE',
            'data': _buffer!,
          });
        }
        return true;
      }
    }
    else{
      if(_buffer != null){
        if((_buffer?.length ?? 0) > _lastBufferLength
          && DateTime.now().difference(_sendingTime).inMilliseconds > timeoutTooLong){
            if(_onLog.hasListener){
              _onLog.add({
                'type': 'TIMEOUT_LONG',
                'data': _buffer!,
              });
            }
            //수신데이터 너무 김
            return true;
        }

        if(DateTime.now().difference(_lastReceiveTime).inMilliseconds > timeoutStopReceive){
          //Receive 중단됨
          if(_onLog.hasListener){
            _onLog.add({
              'type': 'TIMEOUT_STOP',
              'data': _buffer!,
            });
          }
          return true;
        }
      }
    }

    _lastBufferLength = _buffer?.length ?? 0;

    return false;
  }

  void dispose(){
    _onLog.close();
  }
}

class CommTester{
  //1. Fields
  bool _isAppOpen = false;
  ReceivePort? _bgEvent;
  SendPort? _isolateSender;
  Isolate? _bgWorker;

  //1. Fields - 통신
  int _protocol = ProtocolType.modbus;

  //2. Property - 상태값
  bool get isAppOpen => _isAppOpen;
  int get protocol => _protocol;
  set protocol(int value){
    switch(value){
      case ProtocolType.none:
      case ProtocolType.modbus:
        _protocol = value;

        if(_isolateSender != null){
          _isolateSender!.send({
            'type': 'PROTOCOL',
            'data': _protocol
          });
        }
    }
  }

  //3.생성자
  CommTester(){
    _init();
  }

  //4. Method
  void _init() async {
    _bgEvent = ReceivePort();
    _bgWorker = await Isolate.spawn(_testerWorker, _bgEvent!.sendPort);

    _bgEvent!.listen((data){
      //TODO: Thread → UIThread 처리
      if(data is SendPort){
        //Isolate롤 전송할 Sender 등록
        _isolateSender = data;
      }
      //2. OSPort 상태
      //3. Port 이벤트 결과
      //4. 전송 상태값
    });
  }

  void stop(){
    if(_isolateSender != null){
      _isolateSender!.send({
        'type': 'STOP',
        'data': true,
      });
    }
  }

  void send(Uint8List bytes){
    if(_isolateSender != null){
      _isolateSender!.send({
        'type': 'SEND',
        'data': bytes
      });
    }
  }

  void dispose(){
    _bgWorker?.kill();
    _bgEvent?.close();
  }

  Future<bool> connect() async{
    if(_isolateSender != null){
      _isolateSender!.send({
        'type': 'CONNECTION',
        'data': true
      });
    }
    _isAppOpen = true;
    return true;
  }
  
  Future<bool> disconnect() async {
    if(_isolateSender != null){
      _isolateSender!.send({
        'type': 'CONNECTION',
        'data': false
      });
    }
    _isAppOpen = false;

    return true;
  }



  void _testerWorker(SendPort uiPort) async{
    ReceivePort receivePort = ReceivePort();
    uiPort.send(receivePort.sendPort);

    CommPort port = CommPort();
    //1. Fields - 속성
    bool doStop = false;
    int maxTryCount = 3;
    bool errorCodeAdding = false;

    //1. Fields - 통신
    Uint8List? sendingItem; 
    final Queue<Uint8List> sendingQueue = Queue<Uint8List>();
    int curTryCount = 1;

    //TODO: UIThread → Thread 처리
    receivePort.listen((data){
      //UIThread → Thread 처리
      if(data is Map){
        //1. 전송중단
        if(data['type']== 'STOP'){
          doStop = true;
        }
        //2. port 연결 / 연결 해제
        if(data['type'] == 'CONNECTION'){
          if(data['data']){
            port.connect();
          }
          else{
            port.disconnect();
            port.dispose();
          }
        }
        //3. Port Property 수정
        if(data['type'] == 'PROTOCOL'){
          port.protocolType = data['data'];
        }
        //5. tester Property 수정
        maxTryCount = 3;
        //6. 전송Queue 등록
        if(data['type'] == 'SEND'){
          if(data['data'] != null && data['data'] is Uint8List){
            Uint8List bytes = data['data'];
            if(errorCodeAdding && port.protocolType != ProtocolType.none){
              Uint8List? errcode = port.protocol!.errcode.createErrorCode(bytes);

              if(errcode != null){
                bytes.addAll(bytes);
              }
            }

            if(bytes.isNotEmpty){
              sendingQueue.add(bytes);
            }
          }
        }
      }
    });

    while(true){
      try{
        //통신 연결해제 감지
        if(port.isAppOpen == false) continue;

        if(port.osPort.isOpen == false){
          port.osPort.open();

          await Future.delayed(Duration(seconds: 3));
          continue;
        }

        if(doStop){
          port.initialize();
          curTryCount = 1;
          sendingItem  = null;
          sendingQueue.clear();

          doStop = false;
        }

        //데이터 전송
        if(sendingItem == null){
          if(sendingQueue.isNotEmpty){
            sendingItem = sendingQueue.removeFirst();

            port.write(sendingItem);
            curTryCount = 1;
          }
        }
        //데이터 수신
        else{
          //1. Timeout 검사
          if(port.isTimeout()){
            if(curTryCount >= maxTryCount){
              //반복 시도횟수 초과
              sendingItem  = null;
            }
            else{
              //재시도
              curTryCount++;
              port.write(sendingItem);
            }

            continue;
          }

          //2. 데이터 수신
          port.read();

          //3. Protocol 처리
          if(port.protocolType != ProtocolType.none){
            if(await port.protocolResult()){
              //Frame 종료처리
              if(curTryCount >= maxTryCount){
                //반복시도회수 종
                sendingItem = null;
              }
              else{
                curTryCount++;
                port.write(sendingItem!);
              }
            }
          }
        }
      }
      catch(e){
        port.initialize();
        sendingItem  = null;
        curTryCount = 1;
        sendingQueue.clear();
      }
      finally{
        //과부화 방지
        await Future.delayed(Duration(milliseconds: 20));
      }
    }
  }
}