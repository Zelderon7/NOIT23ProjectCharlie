using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DrawerBlock : MonoBehaviour
{
    public TextMeshPro text;
    public int Count
    {
        get => count;
        set
        {
            count = value;
            RefreshText();
        }
    }
    private int count;
    public GameObject MePrefab;
    private Block curBlock;

    private void OnMouseDown()
    {
        if (count != 0)
        {
            count--;
            GameObject temp = Instantiate(MePrefab, parent: IDEManager.Instance.gameObject.transform);
            temp.transform.localScale = Vector3.one * IDEManager.Instance.BlockSize;
            temp.GetComponentInChildren<Block>().Papa = this;
            SpriteRenderer[] _spRs = temp.GetComponentsInChildren<SpriteRenderer>();
            _spRs.ToList().ForEach(x => x.sortingLayerName = "IDEScreen");
            _spRs.ToList().ForEach(x => x.sortingOrder += 100);
            var _tempTextRef = temp.GetComponentInChildren<TextMeshPro>();
            if( _tempTextRef != null )
            {
                _tempTextRef.sortingOrder += 100;
            }
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
        if (curBlock == null)
            return;

        curBlock.PutDownBlock();
        curBlock.transform.parent.localPosition -= Vector3.forward;
        SpriteRenderer[] _spRs = curBlock.transform.parent.GetComponentsInChildren<SpriteRenderer>();
        _spRs.ToList().ForEach(x => x.sortingOrder -= 100);
        var _tempTextRef = curBlock.transform.parent.GetComponentInChildren<TextMeshPro>();
        if (_tempTextRef != null)
        {
            _tempTextRef.sortingOrder -= 100;
        }
        curBlock = null;
    }

    public void RefreshText()
    {
        if (count < 0)
        {
            text.text = "";
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
