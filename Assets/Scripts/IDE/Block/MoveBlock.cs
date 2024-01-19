using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MoveBlock : Block
{
    public override void RunBlock()
    {
        if (!(Owner is IWalkable))
            throw new System.Exception("Owner is not Walkable");

        if (!GameManager.Instance.IsCellWalkable((int)Owner.GridPos.x, (int)Owner.GridPos.y))
            throw new System.Exception("TODO: Replace this Exception");

        (Owner as IWalkable).MoveMe();

        if(outConnectorsScripts?[0] != null)
            outConnectorsScripts[0].GoNext();
    }
}
