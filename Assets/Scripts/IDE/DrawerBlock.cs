using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DrawerBlock : MonoBehaviour
{
    public TextMeshPro text;
    public int count;
    public GameObject MePrefab;
    private Block curBlock;

    private void OnMouseDown()
    {
        if (count != 0)
        {
            count--;
            GameObject temp = Instantiate(MePrefab, parent: IDEManager.Instance.gameObject.transform);
            temp.transform.position = new Vector3(GetMousePos().x, GetMousePos().y, 0);
            curBlock = temp.GetComponentInChildren<Block>();
            curBlock?.PrepDragAlong();
        }
        RefreshText();
    }

    private void OnMouseDrag()
    {
        curBlock?.MoveWithMouse();
    }

    private void OnMouseUp()
    {
        curBlock?.PutDownBlock();
    }

    public void RefreshText()
    {
        if (count < 0)
        {
            text.text = "x99";
        }
        else
        {
            text.text = "x" + count;
        }
    }
    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
