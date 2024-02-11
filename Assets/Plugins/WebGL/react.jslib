mergeInto(LibraryManager.library, {
  SendData: function(data){
    window.dispatchReactUnityEvent("SendData", Pointer_stringify(data));
  },
});