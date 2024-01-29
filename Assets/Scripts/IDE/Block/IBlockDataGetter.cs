using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBlockDataGetter<T>
{
    public T Value { get; }
}
