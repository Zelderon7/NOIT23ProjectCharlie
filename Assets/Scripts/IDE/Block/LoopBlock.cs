using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class LoopBlock : Block
{
    [SerializeField]
    int IterationsCount;
    int iteration = 0;
    public override void RunBlock(InputConnector connectorInUse = null)
    {
        if (connectorInUse == null)
            throw new ArgumentNullException(nameof(connectorInUse));

        if(connectorInUse == inputConnectorsScripts[0])
        {
            iteration = 0;
        }

        if(iteration < IterationsCount)
        {
            iteration++;
            if (!outConnectorsScripts[0].GoNext())
            {
                iteration = IterationsCount;
            }
        }
        else
        {
            if (outConnectorsScripts[1].Connected == null)
                GameManager.Instance.GameOver();
            else
                outConnectorsScripts[1].GoNext();
        }
        
    }
}
