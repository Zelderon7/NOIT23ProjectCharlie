using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RoboCode : MonoBehaviour, ICodeable, IWalkable
{
    private AutoResetEvent isMovementCompleated = new AutoResetEvent(false);
    Animator animator;
    private Vector2 gridPos;
    private Block starterBlock;
    private float moveSpeed = 5f;
    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    Block ICodeable.StarterBlock { get => starterBlock; set => starterBlock = value; }
    Vector2 ICodeable.GridPos { get => gridPos; set => MoveMeTo(value); }//TODO: Change the set to move method

    public void Face(Vector2 direction)
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        IDEManager.Instance.CurrentlyProgramed = this;
    }

    public void MoveMeTo(Vector2 direction)
    {
        animator.SetBool("IsMoving", true);
        isMovementCompleated.Reset();
        StartCoroutine(MoveCoroutine(direction, GameManager.Instance.cellSize + GameManager.Instance.gridSpacing));
        isMovementCompleated.WaitOne();
        animator.SetBool("IsMoving", false);
    }

    public void MoveMe()
    {
        MoveMeTo(FacingDirection);
    }

    private IEnumerator MoveCoroutine(Vector3 direction, float distance)
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
        isMovementCompleated.Set();
    }


}
