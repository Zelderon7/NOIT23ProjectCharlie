using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWalkable : ICodeable
{
    /// <summary>
    /// Moves the object by 1 cell in the specified direction if possible; otherwise, does nothing.
    /// </summary>
    /// <param name="direction">The direction of the movement (x + y = 1 && (x == 0 || y == 0))</param>
    void MoveMeTo(Vector2 direction);

    /// <summary>
    /// Moves the object by 1 cell in the direction it faces if possible; otherwise, does nothing.
    /// </summary>
    void MoveMe();

    /// <summary>
    /// Makes sure that the object is facing the given direction.
    /// </summary>
    /// <param name="direction">The direction to face (x + y = 1 && (x == 0 || y == 0))</param>
    void Face(Vector2 direction);
}
