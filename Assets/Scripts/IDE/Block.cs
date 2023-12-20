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
    OutputConnectionScript[] outConnectorsScripts = null;

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
        if (caller != connectedIn)
            return;
        RunBlock();
    }

    internal virtual void RunBlock()
    {

    }

    private void OnMouseDown()
    {
        Debug.Log("MD");
        foreach(var t in outConnectorsScripts)
        {
            t.Disconnect();
        }

        InputConnector inputConnector = inpConnector?.GetComponent<InputConnector>();
        if (inputConnector != null)
        {
            OutputConnectionScript outputConnection = inputConnector.Connected?.GetComponent<OutputConnectionScript>();
            if (outputConnection != null)
            {
                outputConnection.Disconnect();
            }
        }

        dragOffset = GetMousePos() - (Vector2)transform.parent.transform.position;
    }

    private void OnMouseUp()
    {
        foreach (var item in outConnectorsScripts)
        {
            item.Connect();
        }
    }

    private void OnMouseDrag()
    {
        transform.parent.transform.position = GetMousePos() - dragOffset;
    }

    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint( Input.mousePosition );
    }
}
