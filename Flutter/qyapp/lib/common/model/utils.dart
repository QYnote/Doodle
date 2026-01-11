extension ListExtension<T> on List<T>{
  int find(List<T> target, {int startIndex = 0}){
    if(target.isEmpty || startIndex >= length) return -1;
    if(length < target.length) return -1;

    int index = -1;

    for(int i = startIndex; i <= length - target.length; i++){
      bool isMatch = true;

      for(int j = 0; j < target.length; j++){
        if(this[i + j] != target[j]){
          isMatch = false;
          break;
        }
      }

      if(isMatch) return i;
    }

    return index;
  }
}

