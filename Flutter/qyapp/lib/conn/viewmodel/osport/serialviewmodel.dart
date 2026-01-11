import 'package:flutter/foundation.dart';
import 'package:flutter_libserialport/flutter_libserialport.dart';
import 'package:qyapp/common/viewmodel/utils.dart';
import 'package:qyapp/conn/model/osport/osport.dart';
import 'package:qyapp/conn/model/osport/serialport/android_serialport.dart';
import 'package:qyapp/conn/model/osport/serialport/desktop_serialport.dart';
import 'package:usb_serial/usb_serial.dart';

class SerialViewModel extends ChangeNotifier{
  //0. Event
  //1. Fields
  final OSPortBase _port;
  //2. Property
  //2. Property - [Desktop 전용]
  List<ComboItem> desktopConnectionList = [];
  String get selectedPort{
    if(_port is DesktopSerialPort){
      final DesktopSerialPort port = _port;
      return port.portName;
    }

    return '';
  }
  set selectedPort(String portName){
    if(_port is DesktopSerialPort){
      final DesktopSerialPort port = _port;
      if(selectedPort != portName){
        port.portName = portName;

        super.notifyListeners();
      }
    }
  }
  
  //2. Property - [Android 전용]
  List<ComboItem> androidConnectionList = [];
  UsbDevice? get selectedDevice{
    if(_port is AndroidSerialport){
      final AndroidSerialport port = _port;
      return port.selectedDevice;
    }

    return null;
  }
  set selectedDevice(UsbDevice? device){
    if(_port is AndroidSerialport){
      final AndroidSerialport port = _port;
      if(selectedDevice != device){
        port.selectedDevice = device;

        super.notifyListeners();
      }
    }
  }
  
  //2. Property - [공통]
  List<ComboItem> baudRateList = [];
  List<ComboItem> databitsList = [];
  List<ComboItem> stopbitsList = [];
  List<ComboItem> parityList = [];
  ///BaudRate
  int get baudRate {
    if(_port is DesktopSerialPort){
      final DesktopSerialPort port = _port;
      return port.baudRate == -1 ? baudRateList[0].value : port.baudRate;
    }
    else if(_port is AndroidSerialport){
      final AndroidSerialport port = _port;
      return port.baudRate;
    }

    return 0;
  }
  set baudRate(int value){
    if(baudRate != value){
      if(_port is DesktopSerialPort){
        final DesktopSerialPort port = _port;

        port.baudRate = value;

        super.notifyListeners();
      }
      else if(_port is AndroidSerialport){
        final AndroidSerialport port = _port;

        port.baudRate = value;
        super.notifyListeners();
      }
    }
  }
  ///DataBits
  int get dataBits {
    if(_port is DesktopSerialPort){
      final DesktopSerialPort port = _port;
      return port.dataBits == -1 ? databitsList[0].value : port.dataBits;
    }
    else if(_port is AndroidSerialport){
      final AndroidSerialport port = _port;
      return port.dataBits;
    }

    return 0;
  }
  set dataBits(int value){
    if(dataBits != value){
      if(_port is DesktopSerialPort){
        final DesktopSerialPort port = _port;
        if(7 <= value && value <= 8){
          port.dataBits = value;

          super.notifyListeners();
        }
      }
      else if(_port is AndroidSerialport){
        final AndroidSerialport port = _port;

        switch(value){
          case 5: port.dataBits = UsbPort.DATABITS_5;
          case 6: port.dataBits = UsbPort.DATABITS_6;
          case 7: port.dataBits = UsbPort.DATABITS_7;
          case 8: port.dataBits = UsbPort.DATABITS_8;
        }
        super.notifyListeners();
      }
    }
  }
  ///StopBits
  int get stopBits {
    if(_port is DesktopSerialPort){
      final DesktopSerialPort port = _port;
      return port.stopBits == -1 ? stopbitsList[0].value : port.stopBits;
    }
    else if(_port is AndroidSerialport){
      final AndroidSerialport port = _port;
      return port.stopBits;
    }

    return 0;
  }
  set stopBits(int value){
    if(stopBits != value){
      if(_port is DesktopSerialPort){
        final DesktopSerialPort port = _port;
        if(1 <= value && value <= 2){
          port.stopBits = value;

          super.notifyListeners();
        }
      }
      else if(_port is AndroidSerialport){
        final AndroidSerialport port = _port;

        switch(value){
          case 1: port.stopBits = UsbPort.STOPBITS_1;
          case 2: port.stopBits = UsbPort.STOPBITS_2;
          case 3: port.stopBits = UsbPort.STOPBITS_1_5;
        }
        super.notifyListeners();
      }
    }
  }
  ///Parity
  int get parity {
    if(_port is DesktopSerialPort){
      final DesktopSerialPort port = _port;
      return port.parity == -1 ? parityList[0].value : port.parity;
    }
    else if(_port is AndroidSerialport){
      final AndroidSerialport port = _port;
      return port.parity;
    }

    return 0;
  }
  set parity(int value){
    if(parity != value){
      if(_port is DesktopSerialPort){
        final DesktopSerialPort port = _port;
        switch(value){
          case 0: port.parity = SerialPortParity.none;
          case 1: port.parity = SerialPortParity.odd;
          case 2: port.parity = SerialPortParity.even;
          case 3: port.parity = SerialPortParity.mark;
          case 4: port.parity = SerialPortParity.space;
        }

        super.notifyListeners();
      }
      else if(_port is AndroidSerialport){
        final AndroidSerialport port = _port;
        switch(value){
          case 0: port.parity = UsbPort.PARITY_NONE;
          case 1: port.parity = UsbPort.PARITY_ODD;
          case 2: port.parity = UsbPort.PARITY_EVEN;
          case 3: port.parity = UsbPort.PARITY_MARK;
          case 4: port.parity = UsbPort.PARITY_SPACE;
        }
        super.notifyListeners();
      }
    }
  }
  
  //3. 생성자
  SerialViewModel(this._port){
    _init();
  }

  //4. Method
  void _init(){
    refreshList();

    //BaudRate
    baudRateList.addAll([
        ComboItem(value: 9600, displayText: '9,600'),
        ComboItem(value: 19200, displayText: '19,200'),
        ComboItem(value: 38400, displayText: '38,400'),
        ComboItem(value: 57600, displayText: '57,600'),
        ComboItem(value: 115200, displayText: '115,200'),
        ]);

    if(_port is DesktopSerialPort){
      databitsList.addAll([
        ComboItem(value: 5, displayText: '5'),
        ComboItem(value: 6, displayText: '6'),
        ComboItem(value: 7, displayText: '7'),
        ComboItem(value: 8, displayText: '8'),
      ]);
      stopbitsList.addAll([
        ComboItem(value: 1, displayText: 'One'),
        ComboItem(value: 2, displayText: 'Two'),
      ]);
      parityList.addAll([
        ComboItem(value: SerialPortParity.none, displayText: 'None'),
        ComboItem(value: SerialPortParity.odd, displayText: 'Odd'),
        ComboItem(value: SerialPortParity.even, displayText: 'Even'),
        ComboItem(value: SerialPortParity.mark, displayText: 'Mark'),
        ComboItem(value: SerialPortParity.space, displayText: 'Space'),
      ]);
    }
    else if(_port is AndroidSerialport){
      databitsList.addAll([
        ComboItem(value: UsbPort.DATABITS_5, displayText: '5'),
        ComboItem(value: UsbPort.DATABITS_6, displayText: '6'),
        ComboItem(value: UsbPort.DATABITS_7, displayText: '7'),
        ComboItem(value: UsbPort.DATABITS_8, displayText: '8'),
      ]);
      stopbitsList.addAll([
        ComboItem(value: UsbPort.STOPBITS_1, displayText: 'One'),
        ComboItem(value: UsbPort.STOPBITS_2, displayText: 'Two'),
        ComboItem(value: UsbPort.STOPBITS_1_5, displayText: '1 to 5'),
      ]);
      parityList.addAll([
        ComboItem(value: UsbPort.PARITY_NONE, displayText: 'None'),
        ComboItem(value: UsbPort.PARITY_ODD, displayText: 'Odd'),
        ComboItem(value: UsbPort.PARITY_EVEN, displayText: 'Even'),
        ComboItem(value: UsbPort.PARITY_MARK, displayText: 'Mark'),
        ComboItem(value: UsbPort.PARITY_SPACE, displayText: 'Space'),
      ]);
    }
  }
  void refreshList(){
    if(_port is DesktopSerialPort){
      final DesktopSerialPort port = _port;
      desktopConnectionList.clear();
      
      for (var port in port.portList) {
        desktopConnectionList.add(ComboItem(value: port, displayText: port));
      }
    }
    else if(_port is AndroidSerialport){
      final AndroidSerialport port = _port;
      androidConnectionList.clear();

      for (var device in port.deviceList) {
        androidConnectionList.add(ComboItem(value: device, displayText: device.deviceName));
      }
    }
  }
}