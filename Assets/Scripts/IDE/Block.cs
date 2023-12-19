using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private GameObject inpConnector = null;
    Block connectedIn = null;

    [SerializeField]
    private GameObject outConnectorsHolder = null;
    private GameObject[] outConnectors = null;
    Block[] connectedOut = null;

    [SerializeField]
    private GameObject argsholder = null;
    private GameObject[] args = null;//GameObject => DataBlock

    private void Awake()
    {
        outConnectors = outConnectorsHolder.GetComponentsInChildren<GameObject>();
        args = argsholder.GetComponentsInChildren<GameObject>();//GameObject => DataBlock
    }

    public void StartBlock(GameObject caller)
    {
        if (caller != connectedIn)
            return;
        RunBlock();
    }

    internal virtual void RunBlock()
    {

    }
}
