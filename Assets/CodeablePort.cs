using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeablePort : MonoBehaviour
{
    public ICodeable MyParent;

    private void Awake()
    {
        MyParent = transform.parent.GetComponent<ICodeable>();
        if (MyParent == null)
            throw new System.Exception("No ICodeable found in parent");
    }

    private void OnMouseDown()
    {
        IDEManager.Instance.CurrentlyProgramed = MyParent;
        GameManager.Instance.CurrentMenu = GameManager.Menus.IDE;
    }
}
