using System;
using UnityEngine;

public class IntInputDataBlock : MonoBehaviour, IBlockDataGetter<int>
{
    int IBlockDataGetter<int>.Value => throw new NotImplementedException();
}
