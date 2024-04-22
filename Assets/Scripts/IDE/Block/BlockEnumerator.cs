using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEnumerator : IEnumerator<Block>
{
    Block _start;
    Block _cur;
    public BlockEnumerator(Block Start)
    {
        _start = Start;
        _cur = null;
    }
    public object Current => _cur;

    Block IEnumerator<Block>.Current => _cur;

    public bool MoveNext()
    {
        if (_cur == null) 
        { 
            _cur = _start;
            return true;
        }

        if(_cur.outConnectorsScripts[^1]?.Connected != null && _cur.outConnectorsScripts[^1].Connected.IsPrimary)
        {
            _cur = _cur.outConnectorsScripts[^1]?.Connected.Block;
            return true;
        }
        else
        {
            _cur = null;
            return false;
        }
    }

    public void Reset()
    {
        _cur = null;
    }

    void IDisposable.Dispose()
    {
        
    }
}
