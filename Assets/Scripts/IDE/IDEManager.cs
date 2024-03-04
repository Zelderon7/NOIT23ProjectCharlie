using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public struct BlockTypes {
    public int Id;
    public int Count;

    public BlockTypes(int id, int count)
    {
        this.Id = id;
        this.Count = count;
    }
}

[Serializable]
public struct CodeBlocksPrefabs {
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

public class IDEManager : MonoBehaviour {
    private readonly float _maxDistanceToBlock = 8f;
    private readonly float _step = 1f;
    private bool _canScroll = true;

    [SerializeField]
    SliderBall _sizeSlider;
    int curZoomLevel = 5;
    float minBlockSize = .3f;

    const float BLOCKSIZESTEP = .1f;

    private Dictionary<int, float> _blockSizes = new Dictionary<int, float>();
    private Dictionary<int, List<GameObject>> _savedPrograms = new Dictionary<int, List<GameObject>>();
    private Dictionary<int, ICodeable> CodeableObjectsDictionary = new Dictionary<int, ICodeable>();

    [SerializeField] private List<CodeBlocksPrefabs> blockTypesPrefs = new List<CodeBlocksPrefabs>();
    [SerializeField] private Button StartButton;
    [SerializeField] private GameObject IDEBackground;
    [SerializeField] private GameObject Drawer;

    public Dictionary<int, Block> HighestBlock { get; private set; } = new Dictionary<int, Block>();
    public Dictionary<int, Block> LowestBlock { get; private set; } = new Dictionary<int, Block>();

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

            if (!LowestBlock.ContainsKey(value.Id))
                LowestBlock.Add(value.Id, null);
            if (!HighestBlock.ContainsKey(value.Id))
                HighestBlock.Add(value.Id, null);

            if (!_blockSizes.ContainsKey(value.Id))
                _blockSizes.Add(value.Id, 1f);

            CurrentlyProgramedId = value.Id;
            if (!_savedPrograms.ContainsKey(CurrentlyProgramed.Id))
                _savedPrograms.Add(CurrentlyProgramed.Id, new List<GameObject>());
            LoadProgram(CurrentlyProgramed.Id);
        }
    }

    private void Awake()
    {
        #region Singleton pattern

        if (_instance != null && _instance != this)
            Destroy(gameObject);
        else
        {
            if (_instance == null)
                _instance = this;
        }

        #endregion Singleton pattern

        _sizeSlider.OnChange += OnZoomInOut;
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

    public bool CheckPlacingPositionY(Block block)
    {
        if (LowestBlock[CurrentlyProgramedId] != null)
        {
            if (block.transform.parent.position.y < LowestBlock[CurrentlyProgramedId].transform.parent.position.y - _maxDistanceToBlock)
                return false;
        } else if (block.transform.parent.position.y < -_maxDistanceToBlock)
            return false;

        if (HighestBlock[CurrentlyProgramedId] != null)
        {
            if (block.transform.parent.position.y > HighestBlock[CurrentlyProgramedId].transform.parent.position.y + _maxDistanceToBlock)
                return false;
        } else if (block.transform.parent.position.y > _maxDistanceToBlock)
            return false;

        return true;
    }

    public void OnBlockRepositioning(Block block)
    {
        if (HighestBlock[CurrentlyProgramedId] == null)
        {
            HighestBlock[CurrentlyProgramedId] = block;
            HighestBlock[CurrentlyProgramedId].OnPickup += OnHighestpickup;
        } else if (block.transform.parent.position.y > HighestBlock[CurrentlyProgramedId].transform.parent.position.y)
        {
            HighestBlock[CurrentlyProgramedId].OnPickup -= OnHighestpickup;
            HighestBlock[CurrentlyProgramedId] = block;
            HighestBlock[CurrentlyProgramedId].OnPickup += OnHighestpickup;
        }

        if (LowestBlock[CurrentlyProgramedId] == null)
        {
            LowestBlock[CurrentlyProgramedId] = block;
            LowestBlock[CurrentlyProgramedId].OnPickup += OnLowestpickup;
        } else if (block.transform.parent.position.y < LowestBlock[CurrentlyProgramedId].transform.parent.position.y)
        {
            LowestBlock[CurrentlyProgramedId].OnPickup -= OnLowestpickup;
            LowestBlock[CurrentlyProgramedId] = block;
            LowestBlock[CurrentlyProgramedId].OnPickup += OnLowestpickup;
        }
    }

    public void OnBlockDestruction(Block block)
    {
        _savedPrograms[CurrentlyProgramedId].Remove(block.transform.parent.gameObject);

        if (LowestBlock[CurrentlyProgramedId] == block)
            OnLowestpickup();
        if (HighestBlock[CurrentlyProgramedId] == block)
            OnHighestpickup();
    }

    public void OnBlockCreation(GameObject block)
    {
        _savedPrograms[CurrentlyProgramedId].Add(block);
        OnBlockRepositioning(block.GetComponentInChildren<Block>());
    }

    public void StartCode()
    {
        if (GameManager.Instance.CurrentMenu != GameManager.Menus.Game)
        {
            GameManager.Instance.CurrentMenu = GameManager.Menus.Game;
            return;
        }

        if (!_savedPrograms.ContainsKey(CurrentlyProgramedId))
            return;

        if (OnCodeStart.GetInvocationList().Length <= 1)
            return;

        StartButton.enabled = false;
        OnCodeStart.Invoke();
        CodeablePort.OnGameStart?.Invoke();
    }

    private IEnumerator WaitForNextScroll()
    {
        yield return new WaitForSeconds(.05f);
        _canScroll = true;
    }

    public void OnScroll(InputAction.CallbackContext ctx)
    {
        float minY, maxY;

        if (!IsActive)
            return;

        if (!ctx.performed)
            return;

        if (ctx.ReadValue<Vector2>().y == 0)
            return;

        if (!_canScroll)
            return;

        _canScroll = false;
        StartCoroutine(WaitForNextScroll());

        float scrollDelta = ctx.ReadValue<Vector2>().y < 0 ? -1 : 1;
        float delta = scrollDelta;

        if (IDEBackground == null)
        {
            IDEBackground = GameManager.Instance.IDEScreen.GetComponentsInChildren<Transform>()
                .FirstOrDefault(x => x.gameObject.name == "Background")?.gameObject;
            if (IDEBackground == null)
                throw new Exception("No IDE Screen background found");
        }

        if (LowestBlock[CurrentlyProgramedId] != null)
            minY = LowestBlock[CurrentlyProgramedId].transform.parent.position.y - _maxDistanceToBlock / 2;
        else
            minY = -_maxDistanceToBlock / 2;

        if (HighestBlock[CurrentlyProgramedId] != null)
            maxY = HighestBlock[CurrentlyProgramedId].transform.parent.position.y + _maxDistanceToBlock / 2;
        else
            maxY = _maxDistanceToBlock / 2;

        IDEBackground.transform.Translate(Vector3.up * delta, Space.World);
        Camera.main.transform.Translate(Vector3.up * delta, Space.World);

        Vector3 clampedBackgroundPosition = new Vector3(IDEBackground.transform.position.x, Mathf.Clamp(IDEBackground.transform.position.y, minY, maxY), IDEBackground.transform.position.z);
        Vector3 clampedCameraPosition = new Vector3(Camera.main.transform.position.x, Mathf.Clamp(Camera.main.transform.position.y, minY, maxY), Camera.main.transform.position.z);

        IDEBackground.transform.position = clampedBackgroundPosition;
        Camera.main.transform.position = clampedCameraPosition;

        float newBackgroundY = Mathf.Round(IDEBackground.transform.position.y / _step) * _step;
        float newCameraY = Mathf.Round(Camera.main.transform.position.y / _step) * _step;

        IDEBackground.transform.position = new Vector3(IDEBackground.transform.position.x, newBackgroundY, IDEBackground.transform.position.z);
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, newCameraY, Camera.main.transform.position.z);
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

    private void OnLowestpickup()
    {
        LowestBlock[CurrentlyProgramedId].OnPickup -= OnLowestpickup;
        GameObject _temp = _savedPrograms[CurrentlyProgramedId]
            .OrderBy(x => x.transform.position.y)
            .FirstOrDefault(x => x != LowestBlock[CurrentlyProgramedId].transform.parent.gameObject);
        if (_temp != null)
        {
            LowestBlock[CurrentlyProgramedId] = _temp.GetComponentInChildren<Block>();
            LowestBlock[CurrentlyProgramedId].OnPickup += OnLowestpickup;
        } else
            LowestBlock[CurrentlyProgramedId] = null;
    }

    private void OnHighestpickup()
    {
        HighestBlock[CurrentlyProgramedId].OnPickup -= OnHighestpickup;
        GameObject _temp = _savedPrograms[CurrentlyProgramedId]
            .OrderByDescending(x => x.transform.position.y)
            .FirstOrDefault(x => x != HighestBlock[CurrentlyProgramedId].transform.parent.gameObject);
        if (_temp != null)
        {
            HighestBlock[CurrentlyProgramedId] = _temp.GetComponentInChildren<Block>();
            HighestBlock[CurrentlyProgramedId].OnPickup += OnHighestpickup;
        } else
            HighestBlock[CurrentlyProgramedId] = null;
    }
}