import 'dart:isolate';

abstract class AppLogic{
  //0. Event
  void Function(dynamic)? onReceive;

  //1. Fields

  //2. Property
  ReceivePort receiveport = ReceivePort();
  SendPort sendPort;

  //3. 생성자
  AppLogic(this.sendPort){
    sendPort.send(receiveport.sendPort);
    
    receiveport.listen(onReceive);
  }

  //4. Method
}