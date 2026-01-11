import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:qyapp/common/viewmodel/utils.dart';
import 'package:qyapp/conn/model/osport/osport.dart';
import 'package:qyapp/conn/model/protocol/protocolbase.dart';
import 'package:qyapp/conn/viewmodel/appport/commportviewmodel.dart';
import 'package:qyapp/conn/viewmodel/osport/serialviewmodel.dart';

class CommTesterView extends StatefulWidget{
  const CommTesterView({super.key});

  @override
  State<StatefulWidget> createState() => _CommTetserViewState();
}

class _CommTetserViewState extends State<CommTesterView>{
  late CommPortViewModel _port;
  late ThemeData _theme;
  final String appTitle = 'QYApp - 통신테스터기';

  @override
  void initState() {
    _port = CommPortViewModel();
    _port.addListener((){
      if(mounted) setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _port.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    double screenWidth = MediaQuery.of(context).size.width;
    _theme = Theme.of(context);

    if(screenWidth > 600){
      return _desktopLayout();
    }
    else{
      return _mobileLayout();
    }
  }//End Build

  Widget _desktopLayout(){
    return Scaffold(
      appBar: AppBar(
        backgroundColor: _theme.colorScheme.primary,
        foregroundColor: _theme.colorScheme.onPrimary,
        title: Text(appTitle),
      ),
      body: Row(
        children: [
          _settingConfig(),
          Expanded(child: _testerMain())
        ],
      )
    );
  }

  Widget _mobileLayout(){
    return Scaffold(
      appBar: AppBar(
        backgroundColor: _theme.colorScheme.primary,
        foregroundColor: _theme.colorScheme.onPrimary,
        title: Text(appTitle),
        actions: [
          IconButton(
            icon: Icon(Icons.settings),
            onPressed: () => {
              showModalBottomSheet(
                context: context,
                isScrollControlled: true,
                builder: (context){
                  return StatefulBuilder(
                    builder: (BuildContext context, StateSetter setSheetState){
                      return Container(
                        height: MediaQuery.of(context).size.height * 0.8,
                        decoration: BoxDecoration(
                          borderRadius: BorderRadius.vertical(top: Radius.circular(15))
                        ),
                        clipBehavior: Clip.antiAlias,
                        child: _settingConfig(
                          customWidth: MediaQuery.of(context).size.width * 0.8,
                          onNotify: (){
                            setSheetState(() { });
                            setState(() { });
                          },
                        ),
                      );
                    },
                    
                  );
                }
              )
            },
          ),
        ],
      ),
      backgroundColor: _theme.colorScheme.surfaceContainer,
      body: _testerMain(),
    );
  }

  Widget _settingConfig({double? customWidth, VoidCallback? onNotify}){
    return Container(
      width: customWidth ?? 250,
      color: _theme.colorScheme.surfaceContainer,
      padding: EdgeInsets.all(10),
      child: DefaultTabController(
        length: 2,
        child: Column(
          children: [
            const TabBar(
              tabs: [
                Tab(text: 'Port 설정',),
                Tab(text: '통신 설정',),
              ]
            ),
            Expanded(
              child: TabBarView(
                children: [
                  _settingPortConfig(onNotify: onNotify),
                  _settingCommConfig(onNotify: onNotify),
                ]
              )
            )
          ],
        )
      )
    );
  }

  Widget _settingPortConfig({VoidCallback? onNotify}){
    const double margin = 5;

    return SingleChildScrollView(
      child: Column(
        children: [
          //Port 종류
          Container(
            decoration: BoxDecoration(
              color: _theme.colorScheme.surface,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(color: _theme.colorScheme.outline)
            ),
            padding: EdgeInsets.all(7),
            child: 
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Port 설정',
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 16
                    ),
                  ),
                  const Divider(),
                  _createRadioItem('Serial', PortType.serial, _port.portType, (value) { _port.portType = value; if(onNotify != null) onNotify(); }),
                  _createRadioItem('Ethernet', PortType.ethernet, _port.portType, (value) { _port.portType = value; if(onNotify != null) onNotify(); }),
                ],
              )
          ),
          const SizedBox(height: margin),
          //종류별 설정
          _portTypePage(_port.portType, onNotify: onNotify),
        ],
      ),
    );
  }

  Widget _portTypePage(int portType, {VoidCallback? onNotify}){
    const double margin = 5;

    if(portType == PortType.serial){
      SerialViewModel serial = _port.serialViewModel;
      const double comboCaptionWidth = 80;
      
      List<RadioListTile> portList =_refreshSerialPortList(serial);
      BoxDecoration boxDecoration = BoxDecoration(
          color: _theme.colorScheme.surface,
          borderRadius: BorderRadius.circular(10),
          border: Border.all(color: _theme.colorScheme.outline)
      );

      return Column(
        children: [
          Container(
            decoration: boxDecoration,
            padding: EdgeInsets.all(7),
            child: Column(
              children: [
                Row(
                  children: [
                    const Text(
                      '연결 Port 선택',
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: 16
                      ),
                    ),
                    const Spacer(),
                    IconButton(
                      onPressed: () { 
                        serial.refreshList();
                        if(onNotify != null) onNotify();
                      },
                      icon: Icon(Icons.refresh_rounded)
                    )
                  ],
                ),
                const Divider(),
                ...portList,
              ],
            ),
          ),
          const SizedBox(height: margin),
          Container(
            decoration: boxDecoration,
            padding: EdgeInsets.all(7),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                //BaudRate
                Row(
                  children: [
                    SizedBox(
                      width: comboCaptionWidth,
                      child: Text(
                        'BaudRate',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 16
                        ),
                      ),
                    ),
                    Expanded(
                      child: DropdownButton<int>(
                        isExpanded: true,
                        value: serial.baudRate,
                        items: serial.baudRateList.map((item){
                          return DropdownMenuItem(
                            value: item.value as int,
                            child: Text(item.displayText),
                          );
                        }).toList(),
                        onChanged: (value) { 
                          serial.baudRate = value ?? serial.baudRateList[0].value;
                          if(onNotify != null) onNotify();
                        }
                      ),
                    ),
                  ],
                ),
                //Parity
                Row(
                  children: [
                    SizedBox(
                      width: comboCaptionWidth,
                      child: Text(
                        'Parity',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 16
                        ),
                      ),
                    ),
                    Expanded(
                      child: DropdownButton<int>(
                        isExpanded: true,
                        value: serial.parity,
                        items: serial.parityList.map((item){
                          return DropdownMenuItem(
                            value: item.value as int,
                            child: Text(item.displayText),
                          );
                        }).toList(),
                        onChanged: (value) { 
                          serial.parity = value ?? serial.parityList[0].value;
                          if(onNotify != null) onNotify();
                        }
                      ),
                    ),
                  ],
                ),
                //DataBits
                Row(
                  children: [
                    SizedBox(
                      width: comboCaptionWidth,
                      child: Text(
                        'DataBis',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 16
                        ),
                      ),
                    ),
                    Expanded(
                      child: DropdownButton<int>(
                        isExpanded: true,
                        value: serial.dataBits,
                        items: serial.databitsList.map((item){
                          return DropdownMenuItem(
                            value: item.value as int,
                            child: Text(item.displayText),
                          );
                        }).toList(),
                        onChanged: (value) { 
                          serial.dataBits = value ?? serial.databitsList[0].value;
                          if(onNotify != null) onNotify();
                        }
                      ),
                    ),
                  ],
                ),
                //StopBits
                Row(
                  children: [
                    SizedBox(
                      width: comboCaptionWidth,
                      child: Text(
                        'StopBits',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 16
                        ),
                      ),
                    ),
                    Expanded(
                      child: DropdownButton<int>(
                        isExpanded: true,
                        value: serial.stopBits,
                        items: serial.stopbitsList.map((item){
                          return DropdownMenuItem(
                            value: item.value as int,
                            child: Text(item.displayText),
                          );
                        }).toList(),
                        onChanged: (value) { 
                          serial.stopBits = value ?? serial.stopbitsList[0].value;
                          if(onNotify != null) onNotify();
                        }
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      );
    }
    else if(portType == PortType.ethernet){
      return Column(
        children: [
          Text('IP'),
          Text('PortNo'),
        ],
      );
    }
    else{
      return Text('미개발 Port 종류');
    }
  }

  List<RadioListTile> _refreshSerialPortList(SerialViewModel serial){
    //Port 목록 새로고침
    List<RadioListTile> list = [];

    if(Platform.isWindows || Platform.isIOS || Platform.isLinux){
      for (int i=0; i< serial.desktopConnectionList.length; i++){
        ComboItem item = serial.desktopConnectionList[i];
        list.add(_createRadioItem(
          item.displayText,
          item.value,
          serial.selectedPort,
          (value) { setState(() { serial.selectedPort = value; }); })
        );
      }
    }
    else if(Platform.isAndroid){
      for (int i=0; i< serial.androidConnectionList.length; i++){
        ComboItem item = serial.androidConnectionList[i];
        list.add(_createRadioItem(
          item.displayText,
          item.value,
          serial.selectedDevice,
          (value) { setState(() { serial.selectedDevice = value; }); })
        );
      }
    }

    return list;
  }

  Widget _settingCommConfig({VoidCallback? onNotify}){
    const double margin = 5;
    BoxDecoration boxDecoration = BoxDecoration(
          color: _theme.colorScheme.surface,
          borderRadius: BorderRadius.circular(10),
          border: Border.all(color: _theme.colorScheme.outline)
      );
    TextEditingController repeatCount = TextEditingController(
      text: _port.repeatCount.toString()
    );

    return SingleChildScrollView(child: Column(
      children: [
        //Protocol
        Container(
          decoration: boxDecoration,
          padding: EdgeInsets.all(7),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Protocol',
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 16
                ),
              ),
              const Divider(),
              //종류
              Row(
                children: [
                  SizedBox(
                    width: 50,
                    child: Text('종류'),
                  ),
                  Expanded(
                    child: DropdownButton<int>(
                      isExpanded: true,
                      value: _port.protocolType,
                      items: _port.protocolTypeList.map((item){
                        return DropdownMenuItem(
                          value: item.value as int,
                          child: Text(item.displayText),
                        );
                      }).toList(),
                      onChanged: (value) { 
                        _port.protocolType = value ?? 0;
                        if(onNotify != null) onNotify();
                      }
                    ),
                  ),
                ],
              ),
              //Error Code 생성
              CheckboxListTile(
                title: Text('ErrorCode 생성'),
                contentPadding: EdgeInsets.zero,
                value: _port.addingErrorCode,
                onChanged: (_port.protocolType != ProtocolType.none)?(value){ 
                  _port.addingErrorCode = value ?? false;
                  if(onNotify != null) onNotify();
                } : null
              ),
            ]
          )
        ),
        const SizedBox(height: margin),
        //반복전송
        Container(
          decoration: boxDecoration,
          padding: EdgeInsets.all(7),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                '반복전송',
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 16
                ),
              ),
              const Divider(),
              //활성화
              CheckboxListTile(
                title: Text('활성화'),
                contentPadding: EdgeInsets.zero,
                value: _port.repeatEnable,
                onChanged: (value){ 
                  _port.repeatEnable = value ?? false;
                  if(onNotify != null) onNotify();
                }
              ),
              //횟수
              TextField(
                decoration: const InputDecoration(
                  labelText: "전송 횟수",
                  border: OutlineInputBorder(),
                  hintText: "숫자만 입력",
                ),
                keyboardType: TextInputType.number,
                inputFormatters: [
                  FilteringTextInputFormatter.digitsOnly//숫자만 허용
                ],
                controller: repeatCount,
                onChanged: (_port.repeatEnable && _port.repeatInfinity == false)?(value){ 
                  int? count = int.tryParse(value);
                  if(count != null){
                    _port.repeatCount = count;
                    if(onNotify != null) onNotify();
                  }
                } : null,
                enabled: (_port.repeatEnable && _port.repeatInfinity == false),
              ),
              //무한 전송
              CheckboxListTile(
                title: Text('무한전송'),
                contentPadding: EdgeInsets.zero,
                value: _port.repeatInfinity,
                onChanged: (_port.repeatEnable)?(value){ 
                  _port.repeatInfinity = value ?? false;
                  if(onNotify != null) onNotify();
                } : null
              ),
            ]
          )
        ),
        const SizedBox(height: margin),
      ],
    ),);
  }

  RadioListTile _createRadioItem(String text, dynamic value, dynamic target, Function(dynamic) onChanged ){
    return RadioListTile(
      title: Text(text),
      visualDensity: VisualDensity(horizontal: 0, vertical: -4),
      value: value,
      groupValue: target,
      onChanged: (value) { 
        onChanged(value);
      },
    );
  }

  Widget _testerMain(){
    return Container(
      color: _theme.colorScheme.surfaceContainer,
      padding: EdgeInsets.all(10),
      child: Text('메인화면'),
    );
  }
}