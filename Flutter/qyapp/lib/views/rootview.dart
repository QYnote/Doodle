import 'package:flutter/material.dart';
import 'package:qyapp/conn/view/commtesterview.dart';

class Rootview extends StatefulWidget {
  const Rootview({super.key});

  @override
  State<StatefulWidget> createState() => _RootViewState();
}

class _RootViewState extends State<Rootview>{
  //1. Fields
  final List<Widget> _pages = [
    const CommTesterView(),
  ];

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        backgroundColor: theme.colorScheme.primary,
        foregroundColor: theme.colorScheme.onPrimary,
        title: Text("QYApp - 메인"),
      ),
      body: 
        Column(
          children: [
            _menuButtn(0, "통신 테스터기", Icons.ad_units),
          ],
        )
      ,
    );
  }

  Widget _menuButtn(int idx, String lbl, IconData? icon){
    return ListTile(
      leading: Icon(icon),
      title: Text(lbl),
      onTap: () => _onMenuClick(idx, lbl),
    );
  }

  void _onMenuClick(int idx, String title){
    Navigator.push(context,
      MaterialPageRoute(
        builder: (context) => _pages[idx]
      )
    );
  }
}