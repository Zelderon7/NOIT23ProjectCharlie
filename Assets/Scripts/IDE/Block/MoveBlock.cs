using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class MoveBlock : Block
{
    public override void RunBlock()
    {
        if (!(Owner is IWalkable))
            throw new System.Exception("Owner is not Walkable");

        Vector2 targetLocation = new Vector2((int)Owner.GridPos.x + (Owner as IWalkable).FacingDirection.x, (int)Owner.GridPos.y - (Owner as IWalkable).FacingDirection.y);

        if (!GameManager.Instance.IsCellWalkable((int)targetLocation.x, (int)targetLocation.y))
            throw new System.Exception($"Can't go to {new Vector2((int)targetLocation.x, (int)targetLocation.y)}");

        (Owner as IWalkable).MoveMe(() =>
        {
            if (outConnectorsScripts?[0] != null)
                outConnectorsScripts[0].GoNext();
        });

        
    }
}
