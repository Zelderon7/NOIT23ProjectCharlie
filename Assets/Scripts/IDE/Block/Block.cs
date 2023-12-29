using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private GameObject inpConnector = null;
    Block connectedIn = null;

    [SerializeField]
    private GameObject outConnectorsHolder;
    public bool ConnectableHere = false;
    internal OutputConnectionScript[] outConnectorsScripts = null;

    [SerializeField]
    private GameObject argsholder = null;
    private GameObject[] args = null;//GameObject => DataBlock
    private Vector2 dragOffset = Vector2.zero;
    

    internal virtual void Awake()
    {
        outConnectorsScripts = outConnectorsHolder?.GetComponentsInChildren<OutputConnectionScript>();

        //args = argsholder.GetComponentsInChildren<GameObject>();//GameObject => DataBlock
    }

    public void StartBlock(Block caller)
    {
        //if (caller != connectedIn)
            //throw new System.Exception("Block called from not connected block");
        RunBlock();
    }

    internal virtual void RunBlock()
    {

    }

    private void OnMouseDown()
    {
        InputConnector inputConnector = inpConnector?.GetComponent<InputConnector>();
        if (inputConnector != null)
        {
            OutputConnectionScript outputConnection = inputConnector.Connected;
            if (outputConnection != null)
            {
                outputConnection.Disconnect();
                //Debug.Log("Disconnected sucsesfully");
            }
        }

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.PrepDragAlong();
        }

        dragOffset = GetMousePos() - (Vector2)transform.parent.transform.position;
    }

    public void PrepDragAlong()
    {
        dragOffset = GetMousePos() - (Vector2)transform.parent.transform.position;

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.PrepDragAlong();
        }
    }

    /// <summary>
    /// Should only be called from the MouseUp() or on the MouseUp() on connected which is dragging the current
    /// Checks for possible connection and connects to it.
    /// </summary>
    public void ConnectIfPossible()
    {
        foreach (var item in outConnectorsScripts)
        {
            if (item.Connect())
                item.FixPositionInStack();
        }

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.ConnectIfPossible();
        }

        if(inpConnector?.GetComponent<InputConnector>().forConnection != null)
        {
            if (inpConnector.GetComponent<InputConnector>().forConnection.Connect())
            {
                inpConnector.GetComponent<InputConnector>().Connected.FixPositionInStack();
            }
        }
    }

    private void OnMouseUp()
    {
        ConnectIfPossible();

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.ConnectIfPossible();
        }
    }

    private void OnMouseDrag()
    {
        MoveWithMouse();

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.MoveWithMouse();
        }
    }

    /// <summary>
    /// Should only be called from the MouseDrag() or on the MouseDrag() on connected which is dragging the current
    /// Moves the block based of the cursor's position and the <see cref="dragOffset"/>
    /// </summary>
    public void MoveWithMouse()
    {
        transform.parent.transform.position = GetMousePos() - dragOffset;

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.MoveWithMouse();
        }
    }

    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint( Input.mousePosition );
    }
}
