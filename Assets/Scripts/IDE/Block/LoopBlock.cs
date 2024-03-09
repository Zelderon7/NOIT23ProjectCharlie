using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class LoopBlock : Block
{
    public override void RunBlock(InputConnector connectorInUse = null)
    {
        if (connectorInUse == null)
            throw new ArgumentNullException(nameof(connectorInUse));

        if (!outConnectorsScripts[0].GoNext())
        {
            GameManager.Instance.GameOver();
        }
    }
}
