using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitBlock : Block
{
    public static List<WaitBlock> waiters = new List<WaitBlock>();
    public void Call()
    {
        if(outConnectorsScripts[0].GoNext())
        {
            waiters.Remove(this);
        }
        else
        {
            (IDEManager.Instance.GetICodeableById(Owner) as ICodeable).OnCodeEnd();
        }
    }
    public override void RunBlock(InputConnector connectorInUse = null)
    {
        if (waiters.Count > 0)
            (IDEManager.Instance.GetICodeableById(Owner) as ICodeable).OnCodeEnd();
        waiters.Add(this);
    }
}
