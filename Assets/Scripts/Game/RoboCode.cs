// Ignore Spelling: Robo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class RoboCode : MonoBehaviour, ICodeable, IWalkable
{
    float _moveSpeed = 1.5f;
    int _currentDirectionIndex = 1;
    int _id;

    Animator _animator;
    Vector2 _gridPos = Vector2.zero;
    Block _starterBlock;

    [SerializeField] Transform Arrow;
    [SerializeField] AudioClip RotateClip;

    BlockTypes[] _blocks;
    AudioSource _audioSource;

    readonly Vector2[] _directions =
        {
            Vector2.up,
            Vector2.right,
            Vector2.down,
            Vector2.left
        };
    
    public Vector2 FacingDirection { get => _directions[_currentDirectionIndex]; 
        private set
        {
            int temp = _directions.ToList().IndexOf(value);
            if (temp == -1)
                throw new ArgumentException($"Invalid direction: {value}");
            _currentDirectionIndex = temp;
            UpdateArrow();
        } }


    Block ICodeable.StarterBlock { get => _starterBlock; set => _starterBlock = value; }
    Vector2 ICodeable.GridPosition
    {
        get => _gridPos;
        set
        {
            _gridPos = value;
            transform.position = GameManager.Instance.GetCellPos((int)value.x, (int)value.y);
        }
    }
    int ICodeable.GridRotation
    {
        get => _currentDirectionIndex;
        set => FacingDirection = _directions[value];
    }
    BlockTypes[] ICodeable.BlockTypes { get => _blocks; set => _blocks = value; }
    public int Id
    {
        get => _id;
        set
        {
            _id = value;
        }
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        if (IDEManager.Instance != null)
        {
            IDEManager.Instance.CurrentlyProgramed = this;
            transform.position = GameManager.Instance.GetCellPos((int)_gridPos.x, (int)_gridPos.y);
            if (FacingDirection.x != 0)
                transform.localScale = new Vector2(GameManager.Instance.CellSize * FacingDirection.x * -1, GameManager.Instance.CellSize);
        }
    }

    public void Start()
    {
        IDEManager.Instance.CurrentlyProgramed = this;
        transform.position = GameManager.Instance.GetCellPos((int)_gridPos.x, (int)_gridPos.y);
        if (FacingDirection.x != 0)
            transform.localScale = new Vector2(GameManager.Instance.CellSize * FacingDirection.x * -1, GameManager.Instance.CellSize);
    }

    void UpdateArrow()
    {
        Arrow.transform.rotation = Quaternion.identity;

        switch (_currentDirectionIndex)
        {
            case 0: Arrow.transform.Rotate(0f, 0f, 180f);
                break;

            case 1:
                Arrow.transform.Rotate(0f, 0f, -90f);
                break;

            case 2:
                Arrow.transform.Rotate(0f, 0f, 0f);
                break;

            case 3:
                Arrow.transform.Rotate(0f, 0f, -90f);
                break;
        }

        if (transform.localScale.x > 0)
            Arrow.transform.Rotate(0, 180, 0);
    }

    public void Turn(bool RightOrLeft, Action callback = null)
    {
        int prevDir = _currentDirectionIndex;
        _currentDirectionIndex += RightOrLeft ? 1 : -1;
        if (_currentDirectionIndex < 0)
            _currentDirectionIndex = 3;
        _currentDirectionIndex %= 4;
            
        if(FacingDirection.x != 0)
            transform.localScale *= new Vector2(transform.localScale.x < 0? FacingDirection.x : FacingDirection.x * -1, 1);

        UpdateArrow();

        if(RotateClip != null)
        {
            _audioSource.clip = RotateClip;
            _audioSource.Play();
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

    public void MoveMeTo(Vector2 direction, Action callback = null)
    {
        GameManager.Instance.Interact((int)(_gridPos.x + direction.x), (int)(_gridPos.y - direction.y), () => {

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
                    _animator.SetBool("IsMoving", false);
                };

                if (_invList != null)
                    _invList.ToList().ForEach(x => callback += (System.Action)x) ;
            }
            _animator.SetBool("IsMoving", true);
            StartCoroutine(MoveCoroutine(direction, GameManager.Instance.CellSize + GameManager.Instance.gridSpacing, callback));
            _gridPos = new Vector2(_gridPos.x + direction.x, _gridPos.y - direction.y);
        });

    }

    public void MoveMe(Action callback)
    {
        MoveMeTo(FacingDirection, callback);
    }

    private IEnumerator MoveCoroutine(Vector3 direction, float distance, Action callback)
    {
        float elapsedTime = 0f;
        float totalTime = distance / _moveSpeed;

        Vector3 startPosition = transform.position;


        while (elapsedTime < totalTime)
        {
            transform.Translate(direction * _moveSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = startPosition + direction * distance;
        callback?.Invoke();
    }
}
