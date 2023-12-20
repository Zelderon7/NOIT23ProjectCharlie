using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputConnector : MonoBehaviour
{
    [SerializeField] Block myBlock;
    public Block Connected = null;
    public void CallMe()
    {
        myBlock.StartBlock(Connected);
    }
}
