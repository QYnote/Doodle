import 'dart:isolate';

abstract class ThreadHandler{
  
  //0. Event
  void Function(dynamic)? dataToThread;

  //1. Fields
  final ReceivePort _threadPort = ReceivePort();

  //2. Property
  Isolate? threadHandler;
  SendPort? threadSender;
  
  //3. 생성자
  ThreadHandler(){
    _init();
  }

  //4. Method
  void _init() async {
    threadHandler = await Isolate.spawn(threadDoWork, _threadPort.sendPort);

    _threadPort.listen(dataToThread);
  }

  @override
  void dispose(){
    threadHandler?.kill();
    _threadPort.close();
  }

  void threadDoWork(SendPort appPort);
}