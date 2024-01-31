// Ignore Spelling: Robo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class RoboCode : MonoBehaviour, ICodeable, IWalkable
{
    Animator animator;
    private Vector2 gridPos = Vector2.zero;
    private Block starterBlock;
    private float moveSpeed = 1f;

    Vector2[] _directions =
        {
            Vector2.up,
            Vector2.right,
            Vector2.down,
            Vector2.left
        };
    
    public Vector2 FacingDirection { get => _directions[curDirI]; private set
        {
            int temp = _directions.ToList().IndexOf(value);
            if (temp == -1)
                throw new ArgumentException($"Invalid direction: {value}");
            curDirI = temp;
        } }

    private int curDirI = 1;

    Block ICodeable.StarterBlock { get => starterBlock; set => starterBlock = value; }
    Vector2 ICodeable.GridPos { get => gridPos; set => MoveMeTo(value, null); }//TODO: Change the set to move method

    #region Audio

    AudioSource myAudioSource;

    [SerializeField]
    AudioClip RotateClip;

    #endregion

    public void Turn(bool RightOrLeft, Action callback = null)
    {
        Debug.Log($"Turning {(RightOrLeft? "Right" : "Left")}, curDir = {FacingDirection}");
        int prevDir = curDirI;
        curDirI += RightOrLeft ? 1 : -1;
        if (curDirI < 0)
            curDirI = 3;
        curDirI %= 4;
            
        if(FacingDirection.x != 0)
            transform.localScale *= new Vector2(FacingDirection.x, 1);

        Debug.Log($"Turned to {FacingDirection}");

        if(RotateClip != null)
        {
            myAudioSource.clip = RotateClip;
            myAudioSource.Play();
            StartCoroutine(WaitSeconds(RotateClip.length, callback));
        }
        else
            callback?.Invoke();
    }

    IEnumerator WaitSeconds(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        myAudioSource = GetComponent<AudioSource>();
    }

    public void Start()
    {
        IDEManager.Instance.CurrentlyProgramed = this;
        transform.position = GameManager.Instance.GetCellPos((int)gridPos.x, (int)gridPos.y);
        if(FacingDirection.x != 0)
            transform.localScale = new Vector2(GameManager.Instance.cellSize * FacingDirection.x * -1, GameManager.Instance.cellSize);
    }

    public void MoveMeTo(Vector2 direction, Action callback = null)
    {
        GameManager.Instance.Interact((int)(gridPos.x + direction.x), (int)(gridPos.y - direction.y), () => {

            if (callback != null)
            {
                Delegate[] _invList = null;
                if (callback.GetInvocationList().Length > 0)
                {
                    _invList = callback.GetInvocationList();
                    callback = null;
                }
                    
                callback += () =>
                {
                    animator.SetBool("IsMoving", false);
                };

                if (_invList != null)
                    _invList.ToList().ForEach(x => callback += (System.Action)x) ;
            }
            animator.SetBool("IsMoving", true);
            StartCoroutine(MoveCoroutine(direction, GameManager.Instance.cellSize + GameManager.Instance.gridSpacing, callback));
            gridPos = new Vector2(gridPos.x + direction.x, gridPos.y - direction.y);
        });

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

    void ICodeable.OnRestart()
    {
        
    }
}
