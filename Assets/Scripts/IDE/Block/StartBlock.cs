public class StartBlock : Block
{
    internal override void Awake()
    {
        base.Awake();
        if(IDEManager.Instance != null)
            IDEManager.Instance.OnCodeStart += RunBlock;
    }

    public override void RunBlock()
    {
        base.RunBlock();
        if (_outConnectorsScripts?[0] != null)
            _outConnectorsScripts[0].GoNext();
    }

    internal override void OnDestroy()
    {
        if(IDEManager.Instance != null)
            IDEManager.Instance.OnCodeStart -= RunBlock;
    }
}
