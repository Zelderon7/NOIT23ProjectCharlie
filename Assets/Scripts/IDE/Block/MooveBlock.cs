using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MooveBlock : Block
{
    internal override async void RunBlock()
    {
        base.RunBlock();

        Debug.Log("I move");

        if(outConnectorsScripts?[0] != null)
            outConnectorsScripts[0].GoNext();
    }
}
