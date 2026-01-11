import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:qyapp/conn/model/osport/osport.dart';
import 'package:qyapp/conn/model/osport/serialport/android_serialport.dart';
import 'package:qyapp/conn/model/osport/serialport/desktop_serialport.dart';

///Application Port 기본틀
abstract class AppPortBase {
  //1. Fields
  int _osType = PortType.serial;
  OSPortBase? _osPort;

  //2. Property
  ///OS Port 종류<br/>
  ///기본값 Serial
  int get type => _osType;
  set type(int value){
    if(_osType != value){

      //OS Port 생성
      if(_osType == PortType.serial){
        if(Platform.isWindows || Platform.isIOS || Platform.isLinux){
          _osPort = DesktopSerialPort();
        }
        else if(Platform.isAndroid){
          _osPort = AndroidSerialport();
        }
      }

      _osType = value;
    }
  }
  ///OS Port
  OSPortBase get osPort => _osPort!;
  ///Application Port 열림 상태
  bool get isAppOpen;

  //3. 생성자
  ///Application Port 기본틀
  AppPortBase(){
    _init();
  }

  //4. Method - private
  ///초기 상태값 지정
  void _init(){
    if(type == PortType.serial){
      type = PortType.ethernet;
    }
    type = PortType.serial;
  }

  //4. Method - public
  ///Application 연결
  Future<bool> connect();
  ///Application 연결해제
  Future<bool> disconnect();
  ///Port Data 읽기
  Uint8List? read();
  ///Port Data 쓰기
  void write(Uint8List bytes);
  ///Port 초기화
  void initialize();
}