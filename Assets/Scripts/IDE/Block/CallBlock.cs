using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CallBlock : Block
{
    public override void RunBlock(InputConnector connectorInUse = null)
    {
        if(WaitBlock.waiters.Count > 0)
            WaitBlock.waiters[0].Call();

        if (!outConnectorsScripts[0].GoNext())
            (IDEManager.Instance.GetICodeableById(Owner) as ICodeable).OnCodeEnd();
    }
}
