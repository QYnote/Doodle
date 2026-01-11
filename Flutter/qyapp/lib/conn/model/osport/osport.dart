import 'dart:typed_data';

abstract class OSPortBase{
  //0. Event
  void Function(String)? getLog;

  //1.Fields
  int _type;

  //2. Property - Runtime Readonly
  ///Port 종류
  int get type => _type;
    set type(int value){
      switch(value){
        case 0: _type = PortType.serial;
        case 1: _type = PortType.ethernet;
      }
    }
  //2. Property - 일반
  ///Port 이름
  abstract String portName;
  ///Port 연결 상태
  bool get isOpen;

  //3. 생성자
  OSPortBase(this._type);

  //4. Method - public
  ///Log Event 실행
  void onLog(String msg){
    if(getLog != null)
    {
      getLog!(msg);
    }
  }

  //4. Method - Abstract
  ///Port 초기화
  void initialize();
  ///Port 연결
  ///```dart
  ///--override--
  ///Future<bool> open() asynce { Active }
  ///--using return--
  ///await osport.open();
  ///--not using return--
  ///osport.open();
  ///```
  Future<bool> open();
  ///Port 연결해제
  ///```dart
  ///--override--
  ///Future<bool> close() asynce { Active }
  ///--using return--
  ///await osport.close();
  ///--not using return--
  ///osport.close();
  ///```
  Future<bool> close();
  ///Port 읽기
  Uint8List? read();
  ///Port 쓰기
  ///* [bytes]: 전송할 Byte Array
  void write(Uint8List bytes);
}

//C언어 호환용 Enum → int 호환용 class 전환
///Port 종류
abstract class PortType{
  static const int serial = 0;
  static const int ethernet = 1;
}