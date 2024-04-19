using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableArgument : MonoBehaviour
{
    public ArgHolder Holder;
    [SerializeField]
    public VariableTypes Type;
    public string Value;

    Vector2 _offset;

    private void OnMouseDown()
    {
        _offset = GetMousePos() - (Vector2)transform.position;
    }

    private void OnMouseDrag()
    {
        transform.position = GetMousePos() + _offset;
    }

    private void OnMouseUp()
    {
        if(Holder != null)
        {
            transform.localPosition = Vector3.zero;
        }
    }

    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
