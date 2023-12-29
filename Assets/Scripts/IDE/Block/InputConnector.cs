using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputConnector : MonoBehaviour
{
    public Block myBlock { get; private set; }
    public OutputConnectionScript Connected = null;
    public OutputConnectionScript forConnection = null;

    private void Awake()
    {
        myBlock = transform.parent.gameObject.GetComponentInChildren<Block>();
        if (myBlock == null)
            Debug.LogError("BlockNotFound");
    }
    public void CallMe()
    {
        Block temp = Connected.transform.parent.parent.GetComponentInChildren<Block>();
        if (temp == null)
            throw new System.Exception(Connected != null? "problem" : "Not Connected");
        myBlock.StartBlock(temp);
    }
}
