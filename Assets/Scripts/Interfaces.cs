using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ICodeable
{
    int Id { get; set; }
    int GridRotation
    {
        get; set;
    }

    Vector2 GridPosition { get; set; }
    BlockTypes[] BlockTypes { get; set; }
}
public interface IInteractableGridObject
{
    void Interact(Action callback);
}

public interface IWalkable : ICodeable
{
    Vector2 FacingDirection { get; }
    /// <summary>
    /// Moves the object by 1 cell in the specified direction if possible; otherwise, does nothing.
    /// </summary>
    /// <param name="direction">The direction of the movement (x + y = 1 && (x == 0 || y == 0))</param>
    void MoveMeTo(Vector2 direction, Action callback);

    /// <summary>
    /// Moves the object by 1 cell in the direction it faces if possible; otherwise, does nothing.
    /// </summary>
    void MoveMe(Action callback);

    /// <summary>
    /// Makes sure that the object is facing the given direction.
    /// </summary>
    /// <param name="RightOrLeft">The direction of the turn: true => clockwise(right)</param>
    void Turn(bool RightOrLeft, Action callback);
}
public interface IBlockDataGetter<T>
{
    public T Value { get; }
}

