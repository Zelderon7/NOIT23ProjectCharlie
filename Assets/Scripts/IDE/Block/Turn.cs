using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : Block
{
    [SerializeField]
    bool RightOrLeft = true;
    public override void RunBlock()
    {
        if (!(IDEManager.Instance.GetICodeableById(Owner) is IWalkable))
            throw new System.Exception("Owner is not Walkable");

        (IDEManager.Instance.GetICodeableById(Owner) as IWalkable).Turn(RightOrLeft, () =>
        {
            base.RunBlock();
            if (outConnectorsScripts?[0] != null)
                outConnectorsScripts[0].GoNext();
        });


    }
}
