using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerScript : MonoBehaviour
{
    bool Opened = false;
    private void Start()
    {
        
    }

    private void OnOpen()
    {
        StartCoroutine(GameManager.Instance.MoveTransform(transform.parent, new Vector3(-7, 0, 0), 1f));
        Opened = true;
    }

    private void OnClose()
    {
        StartCoroutine(GameManager.Instance.MoveTransform(transform.parent, new Vector3(-11, 0, 0), 1f));
        Opened = false;
    }

    private void OnMouseDown()
    {
        //Debug.Log("Drawer MouseDown");
        if(Opened)
            OnClose();
        else
            OnOpen();
    }
}
