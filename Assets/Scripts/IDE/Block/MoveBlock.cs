using UnityEngine;

public class MoveBlock : Block
{
    public override void RunBlock()
    {
        if (!(IDEManager.Instance.GetICodeableById(Owner) is IWalkable))
            throw new System.Exception("Owner is not Walkable");

        Vector2 targetLocation = new Vector2((int)(IDEManager.Instance.GetICodeableById(Owner)).GridPosition.x + ((IDEManager.Instance.GetICodeableById(Owner)) as IWalkable).FacingDirection.x, (int)(IDEManager.Instance.GetICodeableById(Owner)).GridPosition.y - ((IDEManager.Instance.GetICodeableById(Owner)) as IWalkable).FacingDirection.y);

        if (!GameManager.Instance.IsCellWalkable((int)targetLocation.x, (int)targetLocation.y))
            GameManager.Instance.GameOver();

        ((IDEManager.Instance.GetICodeableById(Owner)) as IWalkable).MoveMe(() =>
        {
            base.RunBlock();
            if (_outConnectorsScripts?[0] != null)
                _outConnectorsScripts[0].GoNext();
        });

        
    }
}
