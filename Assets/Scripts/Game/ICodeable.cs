using UnityEngine;

public interface ICodeable
{
    int Id { get; set; }
    int GridRotation
    {
        get; set;
    }

    Block StarterBlock { get; set; }
    Vector2 GridPosition { get; set; }
    BlockTypes[] BlockTypes { get; set;  }
}
