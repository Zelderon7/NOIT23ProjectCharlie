using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBlock : Block
{
    private void Start()
    {
        IDEManager.Instance.OnCodeStart += RunBlock;
    }

    public override void RunBlock()
    {
        if (outConnectorsScripts?[0] != null)
            outConnectorsScripts[0].GoNext();
    }
}