import 'package:desktop_multi_window/desktop_multi_window.dart';
import 'package:flutter/material.dart';

class SubTestApp extends StatelessWidget {
  final WindowController windowController;
  final String arguments;

  const SubTestApp({
    super.key,
    required this.windowController,
    required this.arguments,
  });

  @override
  Widget build(BuildContext context) {
    return const Placeholder();
  }
}