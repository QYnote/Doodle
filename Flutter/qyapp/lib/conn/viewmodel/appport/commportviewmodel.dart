import 'package:flutter/foundation.dart';
import 'package:qyapp/common/viewmodel/utils.dart';
import 'package:qyapp/conn/model/appport/commport.dart';
import 'package:qyapp/conn/model/osport/osport.dart';
import 'package:qyapp/conn/model/protocol/protocolbase.dart';
import 'package:qyapp/conn/viewmodel/osport/serialviewmodel.dart';

class CommPortViewModel extends ChangeNotifier{
  //1. Fields
  final _port = CommPort();
  
  bool _isRepeatEnable = true;
  bool _isRepeatIninity = false;
  int _lastRepeatCount = 3;
  String _text = "";

  //2.Property - Port Option
  int get portType => _port.type;
    set portType(int value){
      if(portType != value){
        switch(value){
          case PortType.serial:
          case PortType.ethernet:
            _port.type = value;

            super.notifyListeners();
        }
      }
    }
  late SerialViewModel serialViewModel;

  //2. Property - Protocol Option
  final List<ComboItem> protocolTypeList = [
    ComboItem(value: ProtocolType.none, displayText: "None"),
    ComboItem(value: ProtocolType.modbus, displayText: "Modbus"),
  ];
  int get protocolType => _port.protocolType;
    set protocolType(int value){
      switch(value){
          case ProtocolType.none:
          case ProtocolType.modbus:
            _port.protocolType = value;

            super.notifyListeners();
        }
    }
  bool get addingErrorCode => _port.regOptionErrorCodeAdding;
    set addingErrorCode(bool value){
      if(addingErrorCode != value){
        _port.regOptionErrorCodeAdding = value;

        super.notifyListeners();
      }
    }
  
  //2. Property - Send Option
  bool get repeatEnable => _isRepeatEnable;
      set repeatEnable(bool value){
        if(repeatEnable != value){
          _isRepeatEnable = value;

          if(value){
            //최근 지정반복 수 저장
            repeatCount = _lastRepeatCount;
          }
          else{
            _lastRepeatCount = repeatCount;
            
            _port.maxTryCount = 1;
          }

          super.notifyListeners();
        }
      }
  int get repeatCount => _port.maxTryCount;
    set repeatCount(int value){
      if(repeatEnable && _isRepeatIninity == false
        && repeatCount != value){
        _port.maxTryCount = value;

        _lastRepeatCount = repeatCount;

        super.notifyListeners();
      }
    }
  bool get repeatInfinity => _isRepeatIninity;
    set repeatInfinity(bool value){
      if(repeatEnable && repeatInfinity != value){
        _isRepeatIninity = value;
        
        if(value){
          //최근 지정반복 수 저장
          _lastRepeatCount = repeatCount;

          //Web을 포함한 int 최대 수
          _port.maxTryCount = (1 << 52) - 1 + (1 << 52);
        }
        else{
          //지정반복 수 복구
          repeatCount = _lastRepeatCount;
        }

        super.notifyListeners();
      }
    }
  String get text => _text;
    set text(String value){
      if(text != value){
        _text = value;

        super.notifyListeners();
      }
    }

  //3. 생성자
  CommPortViewModel(){
    _port.onRead.listen((bytes){
      
    });
    _port.onWrite.listen((bytes){

    });
    _port.onResponseError.listen((events){
      final (type, bytes) = events;
    });
    _port.onResponseFinish.listen((events){
      final (type, frame, items) = events;
    });

    _init();
  }

  void send(){
    Uint8List? bytes = _convertTextToByte(text);
    if(bytes == null) return;

    _port.regist(bytes);
  }

  void stop(){
    _port.doStop();
  }

  Uint8List? _convertTextToByte(String text)
  {
    int handle = 0;
    List<int> bytes = List.empty();

    while (handle < text.length){
        String c = text[handle];
        int len;

        //범위 지정
        if (c == '@') {
          len = 3;
        } else if (c == '#'){
          len = 2;
        }
        else
        {
            if (++handle > text.length){
              break;
            } 
            else{
              continue;
            }
        }

        if (++handle + len > text.length) break;

        //변환 시도
        String byteStr = text.substring(handle, len);
        int? b;
        if (c == '@'){
            b = int.tryParse(byteStr);
        }
        else if (c == '#'){
          b = int.tryParse(byteStr, radix: 16);
        }

        if (b != null)
        {
            bytes.add(b);
            handle += len;
        }
    }

    if (bytes.isEmpty){
        return null;
    }
    else{
        return Uint8List.fromList(bytes);
    }
  }

  void _init(){
    serialViewModel = SerialViewModel(_port.osPort);
    serialViewModel.addListener(() {
      super.notifyListeners();
    });
  }

  @override
  void dispose() {
    _port.dispose();
    super.dispose();
  }
}