using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CodeablePort : MonoBehaviour
{
    public ICodeable MyParent;
    public static Action OnGameStart = () => { };
    public void InvokeOnGameStart() => OnGameStart.Invoke();

    private void Awake()
    {
        OnGameStart += MyOnGameStart;
        MyParent = transform.parent.GetComponent<ICodeable>();
        if (MyParent == null)
            throw new System.Exception("No ICodeable found in parent");
    }

    void MyOnGameStart()
    {
        this.gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        //Debug.Log("Hello, I've been clicked");
        IDEManager.Instance.CurrentlyProgramed = MyParent;
        GameManager.Instance.CurrentMenu = GameManager.Menus.IDE;
    }

    void OnDestroy()
    {
        OnGameStart -= MyOnGameStart;
    }
}
