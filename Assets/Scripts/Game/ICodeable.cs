using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICodeable
{
    int Id { get; set; }
    Block StarterBlock { get; set; }
    Vector2 GridPosition { get; set; }
    int GridRotation{ get; set; }
    BlockTypes[] MyBlockTypes { get; set;  }
    void OnRestart();
}
