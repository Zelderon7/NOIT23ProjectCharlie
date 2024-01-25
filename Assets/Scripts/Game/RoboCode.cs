// Ignore Spelling: Robo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class RoboCode : MonoBehaviour, ICodeable, IWalkable
{
    bool isMovementCompleated = false;
    Animator animator;
    private Vector2 gridPos = Vector2.zero;
    private Block starterBlock;
    private float moveSpeed = 1f;

    Vector2[] _directions =
        {
            Vector2.right,
            Vector2.down,
            Vector2.left,
            Vector2.up
        };
    
    public Vector2 FacingDirection { get => _directions[curDirI]; private set
        {
            int temp = _directions.ToList().IndexOf(value);
            if (temp == -1)
                throw new ArgumentException($"Invalid direction: {value}");
            curDirI = temp;
        } }

    private int curDirI = 0;

    Block ICodeable.StarterBlock { get => starterBlock; set => starterBlock = value; }
    Vector2 ICodeable.GridPos { get => gridPos; set => MoveMeTo(value, null); }//TODO: Change the set to move method

    public void Turn(bool RightOrLeft, Action callback = null)
    {
        Debug.Log($"Turning {(RightOrLeft? "Right" : "Left")}, curDir = {FacingDirection}");
        int prevDir = curDirI;
        curDirI += RightOrLeft ? 1 : -1;
        if (curDirI < 0)
            curDirI = 3;
        curDirI %= 4;
        if (_directions[prevDir].x != FacingDirection.x && _directions[prevDir].x * FacingDirection.x != 0)
            transform.localScale *= new Vector2(-1, 0);

        Debug.Log($"Turned to {FacingDirection}");

        callback?.Invoke();
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        IDEManager.Instance.CurrentlyProgramed = this;
        transform.position = GameManager.Instance.GetCellPos((int)gridPos.x, (int)gridPos.y);
        transform.localScale = new Vector2(GameManager.Instance.cellSize * FacingDirection.x, GameManager.Instance.cellSize);
    }

    public void MoveMeTo(Vector2 direction, Action callback = null)
    {
        if(callback != null)
        {
            callback += () =>
            {
                animator.SetBool("IsMoving", false);
            };
        }
        animator.SetBool("IsMoving", true);
        StartCoroutine(MoveCoroutine(direction, GameManager.Instance.cellSize + GameManager.Instance.gridSpacing, callback));
        gridPos = new Vector2 (gridPos.x + direction.x, gridPos.y - direction.y);
    }

    public void MoveMe(Action callback)
    {
        MoveMeTo(FacingDirection, callback);
    }

    private IEnumerator MoveCoroutine(Vector3 direction, float distance, Action callback)
    {
        float elapsedTime = 0f;
        
        Vector3 startPosition = transform.position;

        // Calculate the time required to cover the specified distance with the given speed
        float totalTime = distance / moveSpeed;

        while (elapsedTime < totalTime)
        {
            // Move the object in the specified direction
            transform.Translate(direction * moveSpeed * Time.deltaTime);

            // Update the elapsed time
            elapsedTime += Time.deltaTime;

            // Yield until the next frame
            yield return null;
        }

        // Ensure the object reaches the exact target position
        transform.position = startPosition + direction * distance;
        Debug.Log("Movement Over" + distance);
        callback?.Invoke();
    }


}