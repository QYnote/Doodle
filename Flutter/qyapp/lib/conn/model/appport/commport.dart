import 'dart:async';
import 'dart:collection';
import 'dart:isolate';
import 'dart:typed_data';
import 'package:qyapp/conn/model/appport/appport.dart';
import 'package:qyapp/conn/model/osport/osport.dart';
import 'package:qyapp/conn/model/protocol/modbus.dart';
import 'package:qyapp/conn/model/protocol/protocolbase.dart';

class EventType{
  static const int write = -3;
  static const int read = -2;
  static const int finish = -1;
  static const int noneReceive = 0;
  static const int tooLong = 1;
  static const int stopReceive = 2;
  static const int errorcodeMatch = 3;
}

class CommPort extends AppPortBase{
  //0. Event
  final _onReadBuffer = StreamController<Uint8List>.broadcast();
  final _onWriteBuffer = StreamController<Uint8List>.broadcast();
  final _onResponseError = StreamController<(int, Uint8List)>.broadcast();
  final _onResponseFinish = StreamController<(int, Uint8List, List<Object>?)>.broadcast();

  Stream<Uint8List> get onRead => _onReadBuffer.stream;
  Stream<Uint8List> get onWrite => _onWriteBuffer.stream;
  Stream<(int, Uint8List)> get onResponseError => _onResponseError.stream;
  Stream<(int, Uint8List, List<Object>?)> get onResponseFinish => _onResponseFinish.stream;

  //1. Fields
  ///Background Thread 실행기
  Isolate? _bgWokrer;
  ///Background Data Thread간 송,수신기
  ReceivePort? _bgEvent;
  bool _isAppOpen = false;
  Uint8List? _buffer;
  Uint8List? _sendingItem;
  int _lastBufferLength = 0;
  DateTime _sendingTime = DateTime.now();
  DateTime _lastReceiveTime = DateTime.now();
  final Queue<Uint8List> _sendingQueue = Queue<Uint8List>();
  int _protocolType = ProtocolType.modbus;
  Protocolbase? _protocol;

  int _curTryCount = 1;
  bool _doStop = false;

  //2. Property - 통신처리기 옵션
  @override
  bool get isAppOpen => _isAppOpen;
  int maxTryCount = 3;
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
  
  //2. Property - CommTester기 옵션
  bool regOptionErrorCodeAdding = false;

  //3. 생성자
  ///통신테스터기 포트
  CommPort(){
    _init();
  }

  //4. Method - override
  @override
  void initialize() {
    _isAppOpen = false;
    _bgWokrer?.kill();
    _bgEvent?.close();
    super.osPort.close();

    _buffer = null;
    _curTryCount = 1;
    _lastBufferLength = 0;
    _sendingTime = DateTime.now();
    _lastReceiveTime = DateTime.now();
    _sendingQueue.clear();
  }
  
  @override
  Future<bool> connect() async {
    if(isAppOpen) return false;
    //OS Port 열기
    if(await super.osPort.open()) return false;
    
    _isAppOpen = true;
    _runBgWorker();

    return true;
  }

  @override
  Future<bool> disconnect() async {
    _isAppOpen = false;
    _bgWokrer?.kill();
    _bgEvent?.close();
    super.osPort.close();

    return true;
  }

  @override
  Uint8List? read() => super.osPort.read();

  Uint8List? readBytes(SendPort bgPort){
    Uint8List? bytes = read();
    if(bytes == null) return null;

    if(_buffer == null){
      _buffer = bytes;
    }
    else{
      _buffer!.addAll(bytes);
    }

    _lastReceiveTime = DateTime.now();
    bgPort.send((EventType.read, _buffer!));

    return _buffer;
  }

  @override
  void write(Uint8List bytes) => super.osPort.write(bytes);

  void writeBytes(SendPort bgPort, Uint8List bytes){
    if(bytes.isEmpty) return;

    _buffer = null;
    _lastBufferLength = 0;
    _lastReceiveTime = DateTime.now();
    _sendingTime = DateTime.now();

    write(bytes);

    bgPort.send((EventType.write, bytes));
  }

  //4. Method - private
  void _init(){
    super.type = PortType.serial;
  }

  ///Port 실제동삭 실행
  void _runBgWorker() async{
    _bgEvent = ReceivePort();
    _bgWokrer = await Isolate.spawn(_portWorker, _bgEvent!.sendPort);

    _bgEvent!.listen((message) {
      //Event
      final (int type, dynamic item) = message as (int, Object?);

      switch(type){
        case EventType.read:
          _onReadBuffer.add(item as Uint8List);
        case EventType.write:
          _onWriteBuffer.add(item as Uint8List);
        case EventType.noneReceive:
        case EventType.tooLong:
        case EventType.stopReceive:
        case EventType.errorcodeMatch:
          final Uint8List bytes = item;
          _onResponseError.add((type, bytes));
        case EventType.finish:
          final (Uint8List bytes, List<Object>? items) = item;
          _onResponseFinish.add((type, bytes, items));
      }
    });
  }

  ///Port 실제 동작
  void _portWorker(SendPort bgPort) async {
    while(true){
      try{
        if(isAppOpen == false) break;

        //통신 연결해제 감지
        if(super.osPort.isOpen == false){
          super.osPort.open();

          await Future.delayed(Duration(seconds: 3));
          continue;
        }

        if(_sendingItem == null){
          //Write 처리
          if(_doStop == false || _sendingQueue.isNotEmpty){
            _sendingItem = _sendingQueue.removeFirst();
            writeBytes(bgPort, _sendingItem!);

            _curTryCount = 1;
          }
        }
        else{
          //Read 처리
          //1. Timeout 검사
          if(_isTimeout(bgPort)){
            if(_doStop || _curTryCount >= maxTryCount){
              //반복 시도횟수 초과
              _sendingItem  = null;
              _doStop = false;
              
              super.osPort.initialize();
            }
            else{
              //재시도
              _curTryCount++;
              writeBytes(bgPort, _sendingItem!);
            }

            continue;
          }

          readBytes(bgPort);

          if(_protocol != null && _buffer != null){
            Uint8List? frame = _protocol!.response.extractFrame(_buffer!);

            if(frame != null){
              //Response Frame 추출됨
              bool isErrCodeError = _protocol!.errcode.checkErrorCode(frame) == false;

              //ErrorCode 검사
              if(isErrCodeError){
                //미일치
                bgPort.send((EventType.errorcodeMatch, frame));
              }
              else{
                //Protocol 결과 Item 추출
                List<Object>? items = _protocol!.response.extractItem(frame, subdata: [_sendingItem!]);
                bgPort.send((EventType.finish, (frame, items ?? List.empty())));
              }

              //Frame 종료처리
              if(_doStop || _curTryCount >= maxTryCount){
                //반복시도회수 종료
                _sendingItem = null;
                _doStop = false;
              }
              else{
                _curTryCount++;
                writeBytes(bgPort, _sendingItem!);
              }
            }
          }
        }
      }
      catch(e)
      {
        _buffer = null;
        _sendingItem  = null;
        _curTryCount = 1;
        _sendingQueue.clear();

        await Future.delayed(Duration(seconds: 2));
      }
      finally{
        //과부화 방지
        await Future.delayed(Duration(milliseconds: 20));
      }
    }
  }

  ///Timeout
  ///* **returns - true**: Timeout 발생
  ///* **returns - false**: Timeout 미발생
  bool _isTimeout(SendPort bgPort){
    if(_sendingItem == null) return true;

    if(_lastBufferLength == 0){
      //수신 데이터 없음
      if(DateTime.now().difference(_sendingTime).inMilliseconds > timeoutNoneReceive){
        bgPort.send((EventType.noneReceive, _buffer!));
        return true;
      }
    }
    else{
      if(_buffer != null){
        if((_buffer?.length ?? 0) > _lastBufferLength
          && DateTime.now().difference(_sendingTime).inMilliseconds > timeoutTooLong){
            bgPort.send((EventType.tooLong, _buffer!));
            //수신데이터 너무 김
            return true;
        }

        if(DateTime.now().difference(_lastReceiveTime).inMilliseconds > timeoutStopReceive){
          //Receive 중단됨
          bgPort.send((EventType.stopReceive, _buffer!));
          return true;
        }
      }
    }

    _lastBufferLength = _buffer?.length ?? 0;

    return false;
  }
  
  ///전송 대기열 등록
  ///* [bytes]: 전송할 Bytes Array
  void regist(Uint8List bytes){
    if(regOptionErrorCodeAdding && protocolType != ProtocolType.none){
      Uint8List? errcode = _protocol?.errcode.createErrorCode(bytes);

      if(errcode != null)
      {
        bytes.addAll(errcode);
      }
    }

    _sendingQueue.add(bytes);
  }

  ///AppPort 자체는 중단시키지 않고 전송 Request 중단
  void doStop(){
    _doStop = true;
    _sendingQueue.clear();
  }

  void dispose(){
    _onReadBuffer.close();
    _onWriteBuffer.close();
    _onResponseFinish.close();
  }
}