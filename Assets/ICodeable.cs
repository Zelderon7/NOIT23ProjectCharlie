using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICodeable
{
    //TODO: Add variables for this object and blocks it can be programed with...
    Block StarterBlock { get; set; }
    Vector2 GridPos { get; set; }
    void RunMyCode()
    {
        StarterBlock.RunBlock();
    }
}
