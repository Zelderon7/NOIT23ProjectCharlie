using UnityEngine;

public class Turn : Block
{
    [SerializeField] bool RightOrLeft = true;
    public override void RunBlock()
    {
        if (IDEManager.Instance.GetICodeableById(Owner) is not IWalkable)
            throw new System.Exception("Owner is not Walkable");

        (IDEManager.Instance.GetICodeableById(Owner) as IWalkable).Turn(RightOrLeft, () =>
        {
            base.RunBlock();
            if (_outConnectorsScripts?[0] != null)
                _outConnectorsScripts[0].GoNext();
        });
    }
}
