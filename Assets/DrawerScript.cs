using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerScript : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnMenusClose[GameManager.Menus.IDEDrawer] += OnClose;
        GameManager.Instance.OnMenusOpen[GameManager.Menus.IDEDrawer] += OnOpen;
    }

    private void OnOpen()
    {
        StopCoroutine(GameManager.Instance.MoveTransform(transform.parent, new Vector3(-7, 0, 0), 1f));
    }

    private void OnClose()
    {
        StopCoroutine(GameManager.Instance.MoveTransform(transform.parent, new Vector3(-11, 0, 0), 1f));
    }

    private void OnMouseDown()
    {
        Debug.Log("Drawer MouseDown");

        if(GameManager.Instance.CurrentMenu == GameManager.Menus.IDEDrawer)
        {
            GameManager.Instance.CurrentMenu = GameManager.Menus.IDE;
        }
        else if(GameManager.Instance.CurrentMenu == GameManager.Menus.IDE)
        {
            Debug.Log("Moving to Drawer");
            GameManager.Instance.CurrentMenu = GameManager.Menus.IDEDrawer;
        }
    }
}
