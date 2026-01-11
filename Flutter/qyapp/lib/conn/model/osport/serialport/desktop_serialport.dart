import 'dart:typed_data';
import 'package:qyapp/conn/model/osport/osport.dart';
import 'package:flutter_libserialport/flutter_libserialport.dart';

class DesktopSerialPort extends OSPortBase{
  //1. Fields
  SerialPort? _port;
  final SerialPortConfig _config = SerialPortConfig();

  //2. Property - override
  @override
  bool get isOpen =>  _port?.isOpen ?? false;
  @override
  String portName = '';
  //2. Property - 일반
  ///연결 Port 목록
  List<String> portList = SerialPort.availablePorts;
  ///BaudRate
  int get baudRate => _config.baudRate;
    set baudRate (int value) => _config.baudRate = value;
  ///DataBits
  int get dataBits => _config.bits;
    set dataBits(int value)
    {
      if(5 <= value && value <= 8){
        _config.bits = value;
      }
    }
  ///StopBits
  int get stopBits => _config.stopBits;
    set stopBits(int value)
    {
      if(1 <= value && value <= 2){
        _config.stopBits = value;
      }
    }
  ///Parity
  int get parity => _config.parity;
    set parity(int value)
    {
      switch(value){
        case 0: _config.parity = SerialPortParity.none;
        case 1: _config.parity = SerialPortParity.odd;
        case 2: _config.parity = SerialPortParity.even;
        case 3: _config.parity = SerialPortParity.mark;
        case 4: _config.parity = SerialPortParity.space;
      }
    }

  //3. 생성자
  DesktopSerialPort() : super(PortType.serial);

  //4. Method - override
  @override
  void initialize() {
    if(portName == '')
    {
      _port = null;
      return;
    }
    _port = SerialPort(portName);
    _port?.config = _config;
  }

  @override
  Future<bool> open() async {
    initialize();

    return _port?.openReadWrite() ?? false;
  }

  @override
  Future<bool> close() async => _port?.close() ?? false;

  @override
  Uint8List? read() => _port?.read(_port?.bytesAvailable ?? 0);

  @override
  void write(Uint8List bytes) => _port?.write(bytes);

  //4. Method - 고유 Method
  ///Port목록 새로고침
  void refreshPortList(){
    portList = SerialPort.availablePorts;
  }
}