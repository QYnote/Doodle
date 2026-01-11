import 'package:flutter/foundation.dart';
import 'package:qyapp/common/model/utils.dart';
import 'package:qyapp/conn/model/protocol/protocolbase.dart';

class Modbus extends Protocolbase{
  Modbus() : super(2);
  
  @override
  Request createRequest(Protocolbase manager) => ModbusRequest(manager);

  @override
  Response createResponse(Protocolbase manager) => ModbusResponse(manager);

  @override
  ErrorCode createErrorCode(Protocolbase manager) => ModbusErrorCode(manager);
}

class ModbusResponse extends Response{
  //1. Fields
  //2. Property
  //3. 생성자
  ModbusResponse(super.manager);
  //4. Method - override
  @override
  Uint8List? extractFrame(Uint8List buffer, {List<Object?>? subdata}) {
    if(subdata == null) return null;
    if(subdata.length != 1) return null;
    if(subdata[0] is Uint8List == false) return null;
    Uint8List reqFrame = subdata[0] as Uint8List;

    //Header: Addr[1] + Command[1]
    if(buffer.length < 2) return null;

    int idxHandle = 0,
        startIndex = -1,
        frameLength = -1,
        cmd;
    
    //Frame 검색
    while(idxHandle < buffer.length - 1){
      //Header 들어왔는지 검사
      startIndex = buffer.find([reqFrame[0], reqFrame[1]], startIndex: idxHandle);
      if(startIndex < 0){
        //ErrorCode가 날라온건지 검사
        startIndex = buffer.find([reqFrame[0], reqFrame[1] + 0x80], startIndex: idxHandle);
      }
      idxHandle++;
      if(startIndex < 0) continue;

      cmd = buffer[startIndex + 1];

      frameLength = -1;
      if(cmd == 0x01 || cmd == 0x02 || cmd == 0x03 || cmd == 0x04 || cmd == 0x17){
        //Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]
        if(buffer.length < startIndex + 3) continue; //ByteCount Receive 검사
        int byteCount = buffer[startIndex + 2];

        frameLength = 1 + 1 + 1 + byteCount + super.manager.errorCodeLength;
      }
      else if (cmd == 0x05 || cmd == 0x06 || cmd == 0x0F || cmd == 0x10)
      {
        //0x05: Addr[1] + Cmd[1] + StartAddr[2] + Value[2]
        //0x06: Addr[1] + Cmd[1] + StartAddr[2] + RegValue[2]
        //0x0F: Addr[1] + Cmd[1] + StartAddr[2] + WriteRegCount[2]
        //0x10: Addr[1] + Cmd[1] + StartAddr[2] + WriteRegCount[2]
        frameLength = 1 + 1 + 2 + 2 + super.manager.errorCodeLength;
      }
      else if (cmd == 0x07 || cmd >= 0x80)
      {
          //0x07: Addr[1] + Cmd[1] + Status[1] - Device 상태 Bit 호출 시 사용
          //Error: Addr[1] + Cmd[1] + ErrCode[1]
          frameLength = 1 + 1 + 1 + super.manager.errorCodeLength;
      }
      if(frameLength < 0) continue; //Command가 설정되지 않음

      //Buffer가 다 들어오지 않음
      if(buffer.length < startIndex + frameLength) continue;

      Uint8List frame = Uint8List(frameLength);
      frame.setRange(0, frameLength, buffer, startIndex);

      return frame;
    }

    return null;
  }

  @override
  List<Object>? extractItem(Uint8List resFrame, {List<Object?>? subdata}) {
    if(subdata == null || subdata.isEmpty) return null;
    if(subdata[0] is Uint8List == false) return null;

    Uint8List reqFrame = subdata[0] as Uint8List;
    int cmd = resFrame[1];

    switch(cmd){
      case 0x01:
      case 0x02: return _readCoils(reqFrame, resFrame);
      case 0x03:
      case 0x04: return _readHoldingRegister(reqFrame, resFrame);
      case 0x05: return _writeSingleCoil(resFrame);
      case 0x06: return _writeSingleRegister(resFrame);
      case 0x0F: return _writeMultipleCoils(resFrame);
      case 0x10: return _writeMultipleRegisters(resFrame);
    }

    return null;
  }

  @protected
  ///Read Coils Frame 읽기<br/>
  ///01(0x01), 02(0x02)
  ///* [req]: Request Frame
  ///* [res]: Response Frame
  List<Object>? _readCoils(Uint8List req, Uint8List res){
    //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
    //Res : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]
    List<Object> list = [];
    int startAddr = (req[2] << 8) + req[3],
        readCount = (req[4] << 8) + req[5];
    
    for(int i = 0; i< readCount; i++){
      list.add(DataFrameModbus(
        res[0], //Device Address
        res[1], //FuncCode
        startAddr + i,  //Data Address
        ((res[(3 + (i / 8)).toInt()] >> (i % 8)) & 1) == 1  //Value
        ));
    }

    if(list.isEmpty) return null;
    
    return list;
  }
  
  @protected
  ///Read Holding Register Frame 읽기<br/>
  ///03(0x03), 04(0x04)
  ///* [req]: Request Frame
  ///* [res]: Response Frame
  List<Object>? _readHoldingRegister(Uint8List req, Uint8List res){
    //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
    //Res : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount] Hi/Lo
    List<Object> list = [];
    int startAddr = (req[2] << 8) + req[3],
        byteCount = res[2];
    
    for(int i = 0; i< byteCount; i+=2){
      list.add(DataFrameModbus(
        res[0], //Device Address
        res[1], //FuncCode
        startAddr + (i / 2).toInt(),  //Data Address
        (res[3 + i] << 8) + res[3 + i + 1]  //Value
        ));
    }

    if(list.isEmpty) return null;
    
    return list;
  }
  
  @protected
  ///Write Single Coil Frame 읽기<br/>
  ///05(0x05)
  ///* [res]: Response Frame
  List<Object>? _writeSingleCoil(Uint8List res){
    //Res : Addr[1] + Cmd[1] + Addr[2] + WriteData[2]
    List<Object> list = [];
    
    if(res[5] == 0x00
      && (res[4] == 0x00 || res[4] == 0xFF)){
      list.add(DataFrameModbus(
        res[0], //Device Address
        res[1], //FuncCode
        (res[2] << 8) + res[3],  //Data Address
        res[4] == 0xFF  //Value
        ));
    }

    if(list.isEmpty) return null;
    
    return list;
  }
  
  @protected
  ///Write Single Register Frame 읽기<br/>
  ///06(0x06)
  ///* [res]: Response Frame
  List<Object>? _writeSingleRegister(Uint8List res){
    //Res : Addr[1] + Cmd[1] + StartAddr[2] + Data[2]
    List<Object> list = [];
    
    if(res[5] == 0x00
      && (res[4] == 0x00 || res[4] == 0xFF)){
      list.add(DataFrameModbus(
        res[0], //Device Address
        res[1], //FuncCode
        (res[2] << 8) + res[3],  //Data Address
        (res[4] << 8) + res[5]  //Value
        ));
    }

    if(list.isEmpty) return null;
    
    return list;
  }
  
  @protected
  ///Write Single Coil Frame 읽기<br/>
  ///15(0x0F)
  ///* [res]: Response Frame
  List<Object>? _writeMultipleCoils(Uint8List res){
    //Res : Addr[1] + Cmd[1] + StartAddr[2] + WriteCount[2]
    List<Object> list = [];
    int startAddr = (res[2] << 8) + res[3],
        readCount = (res[4] << 8) + res[5];
    
    for(int i = 0; i< readCount; i++){
      list.add(DataFrameModbus(
        res[0], //Device Address
        res[1], //FuncCode
        startAddr + i,  //Data Address
        true  //Value
        ));
    }

    if(list.isEmpty) return null;
    
    return list;
  }
  
  @protected
  ///Write Single Coil Frame 읽기<br/>
  ///15(0x0F)
  ///* [res]: Response Frame
  List<Object>? _writeMultipleRegisters(Uint8List res){
    //Res : Addr[1] + Cmd[1] + StartAddr[2] + WriteCount[2]
    List<Object> list = [];
    int startAddr = (res[2] << 8) + res[3],
        readCount = (res[4] << 8) + res[5];
    
    for(int i = 0; i< readCount; i++){
      list.add(DataFrameModbus(
        res[0], //Device Address
        res[1], //FuncCode
        startAddr + i,  //Data Address
        true  //Value
        ));
    }

    if(list.isEmpty) return null;
    
    return list;
  }
  
}

class ModbusRequest extends Request{
  //1. Fields
  //2. Property
  //3. 생성자
  ModbusRequest(super.manager);
  //4. Method - override

  @override
  Uint8List? extractFrame(Uint8List buffer, {List<Object?>? subdata}) {
    int frameLength = -1;
    if(buffer.length < 2) return null;
    int cmd = buffer[1];

    if(cmd == 0x01 || cmd == 0x02 || cmd == 0x03 || 
      cmd == 0x04 || cmd == 0x05 || cmd == 0x06){
        //0x01, 2, 3, 4: Address[1] + Command[1] + DataAddress[2] + ReadCount[2]
        //0x05, 6: Address[1] + Command[1] + DataAddress[2] + Value[2]
        frameLength = 6;
    }
    else{
      //Illegal Function(미사용 Function)
      //Address + (Command + 0x80) + 0x01
      Uint8List frame = Uint8List.fromList([buffer[0], cmd | 0x80, 0x01]);
      
      return frame;
    }

    if(frameLength < 0 || buffer.length < frameLength) return null;
    Uint8List frame = Uint8List(frameLength);
    frame.setRange(0, frameLength, buffer, 0);

    return frame;
  }
  
  @override
  Uint8List? createResponse(Uint8List reqFrame, {List<Object?>? subdata}) {
    // FIXME: Function Code가 다른데 동일 Address번호 접근 어떻게 할 지 고민 필요
    throw UnimplementedError();
  }
}

class ModbusErrorCode extends ErrorCode{
  //1. Fields
  //2. Property
  //3. 생성자
  ModbusErrorCode(super.manager);
  //4. Method - override
  @override
  bool checkErrorCode(Uint8List? frame) {
    //Original Modbus는 ErrorCode가 없음
    return true;
  }

  @override
  Uint8List? createErrorCode(Uint8List? frame) {
    //Original Modbus는 ErrorCode가 없음
    return null;
  }
}

class DataFrameModbus{
  int deviceAddress;
  int functionCode;
  int dataAddress;
  Object value;

  DataFrameModbus(this.deviceAddress, this.functionCode, this.dataAddress, this.value);
}