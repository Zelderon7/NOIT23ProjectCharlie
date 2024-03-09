using Unity.VisualScripting;

public class StartBlock : Block
{
    internal void OnEnable()
    {
        base.Awake();
        if (IDEManager.Instance != null)
            IDEManager.Instance.OnCodeStart += RunBlock;
    }

    public void RunBlock() => RunBlock(null);

    public override void RunBlock(InputConnector connectorInUse = null)
    {
        base.RunBlock(connectorInUse);
        if (outConnectorsScripts?[0] != null)
            outConnectorsScripts[0].GoNext();
    }

    internal override void OnDestroy()
    {
        if(IDEManager.Instance != null)
            IDEManager.Instance.OnCodeStart -= RunBlock;
    }
}
