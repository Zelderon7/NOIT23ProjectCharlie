public class StartBlock : Block
{
    private void Start()
    {
        IDEManager.Instance.OnCodeStart += RunBlock;
    }

    public override void RunBlock()
    {
        base.RunBlock();
        if (_outConnectorsScripts?[0] != null)
            _outConnectorsScripts[0].GoNext();
    }
}
