using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CommunicationManager : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern void SendData(string data);
}
