using Unity.VisualScripting;

public class StartBlock : Block
{
    private void Awake()
    {
        base.Awake();
        if (IDEManager.Instance != null)
            IDEManager.Instance.OnCodeStart += RunBlock;
    }

    public void RunBlock() => RunBlock(null);

    public override void RunBlock(InputConnector connectorInUse = null)
    {
        if (outConnectorsScripts[0] != null)
            outConnectorsScripts[0].GoNext();
        else
            (IDEManager.Instance.GetICodeableById(Owner) as ICodeable).OnCodeEnd();
    }

    internal override void OnDestroy()
    {
        if(IDEManager.Instance != null)
            IDEManager.Instance.OnCodeStart -= RunBlock;
    }
}
