mergeInto(LibraryManager.library, {
  SendData: function(data){
    window.dispatchReactUnityEvent("FetchData", UTF8ToString(data));
  },
});