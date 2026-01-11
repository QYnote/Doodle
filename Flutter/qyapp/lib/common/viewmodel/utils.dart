class ComboItem{
  dynamic value;
  String displayText = '';

  ComboItem({required this.value, required this.displayText});

  @override
  String toString() => displayText;
}