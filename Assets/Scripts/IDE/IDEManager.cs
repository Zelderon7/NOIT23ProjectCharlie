using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public struct BlockTypes
{
    public int Id;
    public int Count;

    public BlockTypes(int id, int count)
    {
        this.Id = id;
        this.Count = count;
    }
}

[Serializable]
public struct CodeBlocksPrefabs
{
    public int Id;
    public string Name;
    public GameObject Prefab;

    public CodeBlocksPrefabs(int id, string name, GameObject prefab)
    {
        this.Id = id;
        Name = name;
        Prefab = prefab;
    }
}

public class IDEManager : MonoBehaviour
{
    private readonly float _maxDistanceToBlock = 5.3f;
    private readonly float _step = 1f;
    private bool _canScroll = true;

    [SerializeField]
    SliderBall _sizeSlider;
    float minBlockSize = .3f;

    const float BLOCKSIZESTEP = .1f;

    private Dictionary<int, float> _blockSizes = new Dictionary<int, float>();
    private Dictionary<int, List<GameObject>> _savedPrograms = new Dictionary<int, List<GameObject>>();
    private Dictionary<int, ICodeable> CodeableObjectsDictionary = new Dictionary<int, ICodeable>();

    [SerializeField] private List<CodeBlocksPrefabs> blockTypesPrefs = new List<CodeBlocksPrefabs>();
    [SerializeField] private Button StartButton;
    [SerializeField] private GameObject IDEBackground;
    [SerializeField] private GameObject Drawer;

    public ICodeable GetICodeableById(int id) => CodeableObjectsDictionary[id];

    public int CurrentlyProgramedId
    {
        get; private set;
    }

    public bool IsActive { get; private set; } = false;

    public Action OnCodeStart = () => { };

    public float BlockSize
    {
        get => _blockSizes[CurrentlyProgramedId]; private set => _blockSizes[CurrentlyProgramedId] = value;
    }

    public List<CodeBlocksPrefabs> BlockTypesPrefs
    {
        get => blockTypesPrefs; private set => blockTypesPrefs = value;
    }

    #region Singleton pattern

    private static IDEManager _instance;

    public static IDEManager Instance
    {
        get
        {
            return _instance;
        }
    }

    #endregion Singleton pattern

    public ICodeable CurrentlyProgramed
    {
        get
        {
            if (CodeableObjectsDictionary.ContainsKey(CurrentlyProgramedId))
                return CodeableObjectsDictionary[CurrentlyProgramedId];
            Debug.LogWarning($"CodeableObject[{CurrentlyProgramedId}] does not exists");
            return null;
        }
        set
        {
            if (value == null)
                return;
            if (CurrentlyProgramed != null)
                SaveProgram(CurrentlyProgramed.Id);
            if (!CodeableObjectsDictionary.ContainsKey(value.Id))
                CodeableObjectsDictionary.Add(value.Id, value);
            if (CodeableObjectsDictionary[value.Id] != value)
                CodeableObjectsDictionary[value.Id] = value;

            if (!_blockSizes.ContainsKey(value.Id))
                _blockSizes.Add(value.Id, 1f);

            CurrentlyProgramedId = value.Id;
            if (!_savedPrograms.ContainsKey(CurrentlyProgramed.Id))
                _savedPrograms.Add(CurrentlyProgramed.Id, new List<GameObject>());
            LoadProgram(CurrentlyProgramed.Id);
        }
    }

    [SerializeField]
    SliderBall _transparencySlider;

    private void Awake()
    {
        #region Singleton pattern

        //Changes from Cursor
        if (_instance == null)
            _instance = this;

        #endregion Singleton pattern

        _sizeSlider.OnChange += OnZoomInOut;
        _transparencySlider.OnChange += TransparencyOnChange;
        OnZoomInOut(5);
    }

    private void TransparencyOnChange(int value)
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, (float)value / 100);
    }

    private void Start()
    {
        GameManager.Instance.OnMenusOpen[GameManager.Menus.IDE] += OnOpen;
    }

    void OnZoomInOut(int x)
    {
        BlockSize = minBlockSize + x * BLOCKSIZESTEP;
        Block.OnResize(BlockSize);
    }

    public void OnBlockRepositioning(Block block)
    {
        
    }

    public void OnBlockDestruction(Block block)
    {
        _savedPrograms[CurrentlyProgramedId].Remove(block.transform.parent.gameObject);
    }

    public void OnBlockCreation(GameObject block)
    {
        _savedPrograms[CurrentlyProgramedId].Add(block);
    }

    public void StartCode()
    {
        if (GameManager.Instance.CurrentMenu != GameManager.Menus.Game)
        {
            GameManager.Instance.CurrentMenu = GameManager.Menus.Game;
            return;
        }

        if (!_savedPrograms.ContainsKey(CurrentlyProgramedId))//Possible problems
            return;

        if (OnCodeStart.GetInvocationList().Length <= 1)
            return;

        StartButton.enabled = false;
        OnCodeStart.Invoke();
        CodeablePort.OnGameStart?.Invoke();
    }

    private IEnumerator WaitForNextScroll()
    {
        yield return new WaitForSeconds(.02f);
        _canScroll = true;
    }

    public void OnScroll(InputAction.CallbackContext ctx)
    {
        if (!IsActive || !ctx.performed || ctx.ReadValue<Vector2>().y == 0 || !_canScroll || _savedPrograms[CurrentlyProgramedId].Count == 0)
            return;

        _canScroll = false;
        StartCoroutine(WaitForNextScroll());

        float scrollDelta = ctx.ReadValue<Vector2>().y > 0 ? -1 : 1; // Inverse the direction
        Vector3 delta = Vector3.up * scrollDelta * _step; // Use _step to control the scroll amount

        // Check if at least one of the blocks stays within _maxDistanceToBlock away on the Y axis from 0
        if (!_savedPrograms[CurrentlyProgramedId].Any(x => Math.Abs(x.transform.position.y + delta.y) < _maxDistanceToBlock))
        {
            // If moving would violate the bounds, do not proceed with the move
            return;
        }

        // Move each block in the currently programmed set
        foreach (var programElement in _savedPrograms[CurrentlyProgramedId].Distinct())
        {
            programElement.transform.position += delta;
        }
    }

    public void OnClose()
    {
        IsActive = false;
        if (IDEBackground != null)
            IDEBackground.transform.position = Vector3.zero;
        else
            Debug.LogError("IDEBackground is not assigned");

        Camera.main.transform.position = Vector3.forward * -10;
    }

    private void SaveProgram(int programId)
    {
        foreach (var item in _savedPrograms[programId])
        {
            item.SetActive(false);
        }
    }

    private void LoadProgram(int programId)
    {
        OnZoomInOut(_sizeSlider.CurrentStep);
        foreach (var item in _savedPrograms[programId])
        {
            item.SetActive(true);
        }
    }

    private void OnOpen()
    {
        StartCoroutine(WaitToOpen());
        Drawer.GetComponentInChildren<DrawerScript>().RefreshDrawer(CurrentlyProgramedId);
    }

    IEnumerator WaitToOpen()
    {
        yield return new WaitForSeconds(1.5f);
        IsActive = true;
    }
}