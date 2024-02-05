using System.Linq;
using TMPro;
using UnityEngine;

public class DrawerBlock : MonoBehaviour
{
    public TextMeshPro Text;
    public GameObject Prefab;
    Block _currentBlock;

    int _count;

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            RefreshText();
        }
    }

    void OnMouseDown()
    {
        if (_count != 0)
        {
            _count--;
            GameObject temp = Instantiate(Prefab, parent: IDEManager.Instance.gameObject.transform);
            temp.transform.localScale = Vector3.one * IDEManager.Instance.BlockSize;
            temp.GetComponentInChildren<Block>().Parent = this;
            SpriteRenderer[] _spRs = temp.GetComponentsInChildren<SpriteRenderer>();
            _spRs.ToList().ForEach(x => x.sortingLayerName = "IDEScreen");
            _spRs.ToList().ForEach(x => x.sortingOrder += 100);
            var _tempTextRef = temp.GetComponentInChildren<TextMeshPro>();
            if( _tempTextRef != null )
            {
                _tempTextRef.sortingOrder += 100;
            }
            temp.transform.position = new Vector3(GetMousePos().x, GetMousePos().y, 0);
            _currentBlock = temp.GetComponentInChildren<Block>();
            _currentBlock?.PrepDragAlong();
        }
        RefreshText();
    }

    private void OnMouseDrag()
    {
        _currentBlock?.MoveWithMouse();
    }

    private void OnMouseUp()
    {
        if (_currentBlock == null)
            return;

        _currentBlock.PutDownBlock();
        _currentBlock.transform.parent.localPosition -= Vector3.forward;
        SpriteRenderer[] _spRs = _currentBlock.transform.parent.GetComponentsInChildren<SpriteRenderer>();
        _spRs.ToList().ForEach(x => x.sortingOrder -= 100);
        var _tempTextRef = _currentBlock.transform.parent.GetComponentInChildren<TextMeshPro>();
        if (_tempTextRef != null)
        {
            _tempTextRef.sortingOrder -= 100;
        }
        _currentBlock = null;
    }

    public void RefreshText()
    {
        if (_count < 0)
        {
            Text.text = "";
        }
        else
        {
            Text.text = "x" + _count;
        }
    }
    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
