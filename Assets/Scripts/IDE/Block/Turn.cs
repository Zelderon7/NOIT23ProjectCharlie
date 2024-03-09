using UnityEngine;

public class Turn : Block
{
    [SerializeField] bool RightOrLeft = true;
    public override void RunBlock(InputConnector connectorInUse)
    {
        if (IDEManager.Instance.GetICodeableById(Owner) is not IWalkable)
            throw new System.Exception("Owner is not Walkable");

        (IDEManager.Instance.GetICodeableById(Owner) as IWalkable).Turn(RightOrLeft, () =>
        {
            base.RunBlock(connectorInUse);
            if (outConnectorsScripts?[0] != null)
                outConnectorsScripts[0].GoNext();
        });
    }
}
