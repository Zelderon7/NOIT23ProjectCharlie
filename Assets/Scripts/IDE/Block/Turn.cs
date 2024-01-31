using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : Block
{
    [SerializeField]
    bool RightOrLeft = true;
    public override void RunBlock()
    {
        if (!(Owner is IWalkable))
            throw new System.Exception("Owner is not Walkable");

        (Owner as IWalkable).Turn(RightOrLeft, () =>
        {
            if (outConnectorsScripts?[0] != null)
                outConnectorsScripts[0].GoNext();
            else
                GameManager.Instance.GameOver();
        });


    }
}
