import 'dart:collection';
import 'dart:async';
import 'dart:typed_data';

import 'package:qyapp/conn/model/osport/osport.dart';
import 'package:usb_serial/usb_serial.dart';

class AndroidSerialport extends OSPortBase {
  //1. Fields
  UsbPort? _port;
  UsbDevice? _selectedDevice;
  ///연결 감지장치
  StreamSubscription<UsbEvent>? _detector;
  bool _isOpen = false;
  ///데이터 수신장치
  StreamSubscription<Uint8List>? _listener;
  final Queue<int> _rxQueue = Queue<int>();
  int get _bytesToRead => _rxQueue.length;

  int _dataBits = UsbPort.DATABITS_8;
  int _stopBits = UsbPort.STOPBITS_1;
  int _parity = UsbPort.PARITY_NONE;

  //2. Property - override
  @override
  String portName = '';
  @override
  bool get isOpen => _isOpen;
  //2. Property - 일반
  ///연결 가능한 Device 목록
  List<UsbDevice> deviceList = [];
  ///사용자가 선택한 Device
  UsbDevice? get selectedDevice => _selectedDevice;
    set selectedDevice(UsbDevice? device){
      _selectedDevice = device;
      portName = device?.deviceName ?? '';
    }
  ///BaudRate
  int baudRate = 9600;
  ///DataBits
  int get dataBits => _dataBits;
    set dataBits(int value){
      switch(value){
        case 5: _dataBits = UsbPort.DATABITS_5;
        case 6: _dataBits = UsbPort.DATABITS_6;
        case 7: _dataBits = UsbPort.DATABITS_7;
        case 8: _dataBits = UsbPort.DATABITS_8;
      }
    }
  ///StopBits
  int get stopBits => _stopBits;
    set stopBits(int value){
      switch(value){
        case 1: _stopBits = UsbPort.STOPBITS_1;
        case 2: _stopBits = UsbPort.STOPBITS_2;
        case 3: _stopBits = UsbPort.STOPBITS_1_5;
      }
    }
  ///Parity
  int get parity => _parity;
    set parity(int value){
      switch(value){
        case 0: _parity = UsbPort.PARITY_NONE;
        case 1: _parity = UsbPort.PARITY_ODD;
        case 2: _parity = UsbPort.PARITY_EVEN;
        case 3: _parity = UsbPort.PARITY_MARK;
        case 4: _parity = UsbPort.PARITY_SPACE;
      }
    }

  //3. 생성자
  AndroidSerialport() : super(PortType.serial){
    _init();
  }

  //4. Method - override
  @override
  void initialize() async {
    //Deivce 목록 검사
    if((deviceList.isEmpty) || selectedDevice == null)
    {
      return;
    }

    //권한 검사
    if(await UsbSerial.createFromDeviceId(selectedDevice?.deviceId) == null){
      return;
    }

    _port = await selectedDevice?.create();
    _port?.setPortParameters(baudRate, dataBits, stopBits, parity);
  }

  @override
  Future<bool> open() async {
    initialize();
    if(_port == null) return false;

    //통신 열기
    if(await _port!.open() == false){
      return false;
    }

    _listener = _port!.inputStream!.listen((Uint8List data){
      _rxQueue.addAll(data);
    });

    _isOpen = true;

    return true;
  }

  @override
  Future<bool> close() async {
    await _listener?.cancel();
    await _port?.close();

    _isOpen = false;

    return true;
  }

  @override
  Uint8List? read()
  {
    if(_bytesToRead > 0){
      Uint8List readBytes = Uint8List.fromList(_rxQueue.toList());
      _rxQueue.clear();

      return readBytes;
    }

    return null;
  }

  @override
  void write(Uint8List bytes) {
    if(isOpen){
      _port?.write(bytes);
    }
  }

  //4. Method - 고유 Mehtod
  void _init(){
    refreshDeviceList();
    if((deviceList.isEmpty) == false){
      selectedDevice = deviceList.first;

      //연결 상태 감지기
      _detector = UsbSerial.usbEventStream!.listen((UsbEvent event){
        if(event.event == UsbEvent.ACTION_USB_DETACHED){
          //연결 분리됨
          close();

          _isOpen = false;
        }
      });
    }
  }
  
  ///연결목록 새로고침
  void refreshDeviceList() async {
    deviceList = await UsbSerial.listDevices();
  }

  void dispose(){
    close();
    _detector?.cancel();
  }
}