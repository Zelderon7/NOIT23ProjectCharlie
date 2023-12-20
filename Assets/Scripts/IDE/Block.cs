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
    private GameObject outConnectorsHolder = null;
    private GameObject[] outConnectors = null;
    public bool ConnectibleHere = false;
    OutputConnectionScript[] outConnectorsScripts = null;

    [SerializeField]
    private GameObject argsholder = null;
    private GameObject[] args = null;//GameObject => DataBlock

    

    private void Awake()
    {
        outConnectors = outConnectorsHolder.GetComponentsInChildren<GameObject>();
        outConnectorsScripts = new OutputConnectionScript[outConnectors.Length];
        for (int i = 0; i < outConnectors.Length; i++)
            outConnectorsScripts[i] = outConnectors[i].GetComponent<OutputConnectionScript>();

        args = argsholder.GetComponentsInChildren<GameObject>();//GameObject => DataBlock
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
}
