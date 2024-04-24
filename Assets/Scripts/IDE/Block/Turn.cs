using System.Linq;
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
            if (outConnectorsScripts[0].Connected != null)
            {
                outConnectorsScripts[0].GoNext();
            }
            else
                (IDEManager.Instance.GetICodeableById(Owner) as ICodeable).OnCodeEnd();
        });
    }
}
