using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntInputDataBlock : MonoBehaviour, IBlockDataGetter<int>
{
    int IBlockDataGetter<int>.Value => throw new NotImplementedException();
}
