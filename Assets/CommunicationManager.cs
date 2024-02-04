using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CommunicationManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SendData(string data);

    public static void SendDataMethod(string data)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false
    SendData (data);
#endif
    }
}
