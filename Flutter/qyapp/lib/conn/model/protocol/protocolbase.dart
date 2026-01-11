import 'dart:typed_data';

class ProtocolType{
  static const int none = 0;
  static const int modbus = 1;
}

abstract class Protocolbase {
  //1. Fields
  late Response _response;
  late Request _request;
  late ErrorCode _errcode;
  final int errorCodeLength;

  //2. Property
  Response get response => _response;
  Request get request => _request;
  ErrorCode get errcode => _errcode;
  
  //3. 생성자
  Protocolbase(this.errorCodeLength){
    _init();
  }

  //4. Method
  void _init(){
    _response = createResponse(this);
    _request = createRequest(this);
    _errcode = createErrorCode(this);
  }

  Response createResponse(Protocolbase manager);
  Request createRequest(Protocolbase manager);
  ErrorCode createErrorCode(Protocolbase manager);
}

abstract class Response{
  //1. Fields
  //2. Property
  final Protocolbase manager;
  //3. 생성자
  Response(this.manager);
  //4. Method
  ///수신된 Buffer에서 Response(응답) Frame 추출
  ///* [buffer]: 수신된 Buffer
  ///* [subdata]: 추출에 사용되는 서브 Data
  Uint8List? extractFrame(Uint8List buffer, {List<Object?>? subdata});
  ///Response(응답)에따른 수신정보 생성
  ///* [resFrame]: Response(응답) Data
  ///* [subdata]: 생성에 사용되는 서브 Data
  List<Object>? extractItem(Uint8List resFrame, {List<Object?>? subdata});
}

abstract class Request{
  //1. Fields
  //2. Property
  final Protocolbase manager;
  //3. 생성자
  Request(this.manager);
  //4. Method
  ///수신된 Buffer에서 Request(요청) Frame 추출
  ///* [buffer]: 수신된 Buffer
  ///* [subdata]: 추출에 사용되는 서브 Data
  Uint8List? extractFrame(Uint8List buffer, {List<Object?>? subdata});
  ///Request(요청)에따른 Response(응답) 생성
  ///* [reqFrame]: Request(요청) Data
  ///* [subdata]: 생성에 사용되는 서브 Data
  Uint8List? createResponse(Uint8List reqFrame, {List<Object?>? subdata});
}

abstract class ErrorCode{
  //1. Fields
  //2. Property
  final Protocolbase manager;
  //3. 생성자
  ErrorCode(this.manager);
  //4. Method
  ///Frame에 따른 ErrorCode 검사
  ///* [frame]: 검사 할 Data Frame
  ///* **Returns**: true: 정상 / false: Fail
  bool checkErrorCode(Uint8List? frame);
  ///Frame에 따른 ErrorCode 생성
  ///* [frame]: 생성 Data Frame
  Uint8List? createErrorCode(Uint8List? frame);
}