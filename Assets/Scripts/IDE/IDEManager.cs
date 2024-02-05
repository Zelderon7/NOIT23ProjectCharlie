using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public struct BlockTypes
{
    public int id;
    public int count;

    public BlockTypes(int id, int count)
    {
        this.id = id;
        this.count = count;
    }
}

[Serializable]
public struct CodeBlocksPrefabs
{
    public int id;
    public string Name;
    public GameObject Prefab;

    public CodeBlocksPrefabs(int id, string name, GameObject prefab)
    {
        this.id = id;
        Name = name;
        Prefab = prefab;
    }
}

public class IDEManager : MonoBehaviour
{

    #region Singleton pattern

    private static IDEManager _instance;
    public static IDEManager Instance
    {
        get
        {
            return _instance;
        }
    }

    #endregion


    public float BlockSize { get => blockSizes[CurrentlyProgramedId]; private set => blockSizes[CurrentlyProgramedId] = value; }

    [SerializeField]
    Button StartButton;
    public List<CodeBlocksPrefabs> BlockTypesPrefs { get => blockTypesPrefs; private set => blockTypesPrefs = value; }
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] GameObject IDEBackground;

    [SerializeField]
    GameObject Drawer;
    public ICodeable CurrentlyProgramed
    {
        get 
        {
            if(CodeableObjectsDictionary.ContainsKey(CurrentlyProgramedId))
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
            if(!CodeableObjectsDictionary.ContainsKey(value.Id))
                CodeableObjectsDictionary.Add(value.Id, value);
            if (CodeableObjectsDictionary[value.Id] != value)
                CodeableObjectsDictionary[value.Id] = value;

            if (!LowestBlock.ContainsKey(value.Id))
                LowestBlock.Add(value.Id, null);
            if (!HighestBlock.ContainsKey(value.Id))
                HighestBlock.Add(value.Id, null);

            if (!blockSizes.ContainsKey(value.Id))
                blockSizes.Add(value.Id, 1f);

            CurrentlyProgramedId = value.Id;
            if (!SavedPrograms.ContainsKey(CurrentlyProgramed.Id))
                SavedPrograms.Add(CurrentlyProgramed.Id, new List<GameObject>());
            LoadProgram(CurrentlyProgramed.Id);
        }
    }

    Dictionary<int, float> blockSizes = new Dictionary<int, float>();
    Dictionary<int, List<GameObject>> SavedPrograms = new Dictionary<int, List<GameObject>>();
    Dictionary<int, ICodeable> CodeableObjectsDictionary = new Dictionary<int, ICodeable>();
    public bool IsActive { get { return GameManager.Instance.CurrentMenu == GameManager.Menus.IDE; } }

    public Action OnCodeStart = () => { };

    [SerializeField]
    private List<CodeBlocksPrefabs> blockTypesPrefs = new List<CodeBlocksPrefabs>();

    public int CurrentlyProgramedId { get; private set; }

    public ICodeable GetICodeableById(int id) => CodeableObjectsDictionary[id];

    #region ScrollingAndNavigationVariables

    float maxDistanceToBlock = 8f;
    [SerializeField]
    float step = 1f;

    public Dictionary<int, Block> HighestBlock { get; private set; } = new Dictionary<int, Block>();
    public Dictionary<int, Block> LowestBlock { get; private set; } = new Dictionary<int, Block>();

    #endregion

    #region ScrollingAndNavigation

    public bool CheckPlacingPositionY(Block block)
    {
        if (LowestBlock[CurrentlyProgramedId] != null)
        {
            if (block.transform.parent.position.y < LowestBlock[CurrentlyProgramedId].transform.parent.position.y - maxDistanceToBlock)
                return false;
        }
        else if(block.transform.parent.position.y < -maxDistanceToBlock)
            return false;

        if (HighestBlock[CurrentlyProgramedId] != null)
        {
            if (block.transform.parent.position.y > HighestBlock[CurrentlyProgramedId].transform.parent.position.y + maxDistanceToBlock)
                return false;
        }
        else if (block.transform.parent.position.y > maxDistanceToBlock)
            return false;

        return true;
    }

    void OnLowestpickup()
    {
        LowestBlock[CurrentlyProgramedId].OnPickup -= OnLowestpickup;
        GameObject _temp = SavedPrograms[CurrentlyProgramedId]
            .OrderBy(x => x.transform.position.y)
            .FirstOrDefault(x => x != LowestBlock[CurrentlyProgramedId].transform.parent.gameObject);
        if (_temp != null)
        {
            LowestBlock[CurrentlyProgramedId] = _temp.GetComponentInChildren<Block>();
            LowestBlock[CurrentlyProgramedId].OnPickup += OnLowestpickup;
        }
        else
            LowestBlock[CurrentlyProgramedId] = null;
        //Debug.Log($"(LP)Highest: {HighestBlock[CurrentlyProgramedId]}\nLowest: {LowestBlock[CurrentlyProgramedId]}");
    }

    void OnHighestpickup()
    {
        HighestBlock[CurrentlyProgramedId].OnPickup -= OnHighestpickup;
        GameObject _temp = SavedPrograms[CurrentlyProgramedId]
            .OrderByDescending(x => x.transform.position.y)
            .FirstOrDefault(x => x != HighestBlock[CurrentlyProgramedId].transform.parent.gameObject);
        if (_temp != null)
        {
            HighestBlock[CurrentlyProgramedId] = _temp.GetComponentInChildren<Block>();
            HighestBlock[CurrentlyProgramedId].OnPickup += OnHighestpickup;
        }
        else
            HighestBlock[CurrentlyProgramedId] = null;

        //Debug.Log($"(HP)Highest: {HighestBlock[CurrentlyProgramedId]}\nLowest: {LowestBlock[CurrentlyProgramedId]}");
    }

    public void OnBlockRepositioning(Block block)
    {
        if (HighestBlock[CurrentlyProgramedId] == null)
        {
            HighestBlock[CurrentlyProgramedId] = block;
            HighestBlock[CurrentlyProgramedId].OnPickup += OnHighestpickup;
        }
        else if (block.transform.parent.position.y > HighestBlock[CurrentlyProgramedId].transform.parent.position.y)
        {
            HighestBlock[CurrentlyProgramedId].OnPickup -= OnHighestpickup;
            HighestBlock[CurrentlyProgramedId] = block;
            HighestBlock[CurrentlyProgramedId].OnPickup += OnHighestpickup;
        }

        if (LowestBlock[CurrentlyProgramedId] == null)
        {
            LowestBlock[CurrentlyProgramedId] = block;
            LowestBlock[CurrentlyProgramedId].OnPickup += OnLowestpickup;
        }
        else if (block.transform.parent.position.y < LowestBlock[CurrentlyProgramedId].transform.parent.position.y)
        {
            LowestBlock[CurrentlyProgramedId].OnPickup -= OnLowestpickup;
            LowestBlock[CurrentlyProgramedId] = block;
            LowestBlock[CurrentlyProgramedId].OnPickup += OnLowestpickup;
        }

        //Debug.Log($"(Repos)Highest: {HighestBlock[CurrentlyProgramedId]}\nLowest: {LowestBlock[CurrentlyProgramedId]}");
    }

    bool CanScroll = true;

    #endregion

    public void OnBlockDestruction(Block block)
    {
        SavedPrograms[CurrentlyProgramedId].Remove(block.transform.parent.gameObject);
        if (LowestBlock[CurrentlyProgramedId] == block)
            OnLowestpickup();
        if (HighestBlock[CurrentlyProgramedId] == block)
            OnHighestpickup();
    }

    public void OnBlockCreation(GameObject block)
    {
        SavedPrograms[CurrentlyProgramedId].Add(block);
        OnBlockRepositioning(block.GetComponentInChildren<Block>());
    }

    public void StartCode()
    {
        if(GameManager.Instance.CurrentMenu != GameManager.Menus.Game)
            GameManager.Instance.CurrentMenu = GameManager.Menus.Game;
        else
        {
            StartButton.enabled = false;
            OnCodeStart?.Invoke();
            CodeablePort.OnGameStart?.Invoke();
        }
            
    }

    void SaveProgram(int programId)
    {
        foreach (var item in SavedPrograms[programId])
        {
            //transform.position -= Vector3.right * 50;
            item.SetActive(false);
        }
    }

    void LoadProgram(int programId)
    {
        foreach (var item in SavedPrograms[programId])
        {
            //transform.position += Vector3.right * 50;
            item.SetActive(true);
        }
    }

    private void Awake()
    {

        #region Singleton pattern
        if (_instance != null && _instance != this)
            Destroy(gameObject);
        else
        {
            if(_instance == null)
                _instance = this;

            //DontDestroyOnLoad(gameObject);
        }

        #endregion

    }

    private void Start()
    {
        GameManager.Instance.OnMenusOpen[GameManager.Menus.IDE] += OnOpen;
    }

    public void OnQ(InputAction.CallbackContext ctx)
    {
        if (!IsActive)
            return;

        if (ctx.performed)
        {
            float newSize = BlockSize / 1.2f;
            if (newSize < .2f)
                newSize = .2f;
            Block.OnResize(newSize);
            BlockSize = newSize;
        }
    }

    public void OnE(InputAction.CallbackContext ctx)
    {
        if (!IsActive)
            return;

        if (ctx.performed)
        {
            float newSize = BlockSize * 1.2f;
            if (newSize > 1.5f)
                newSize = 1.5f;
            Block.OnResize(newSize);
            BlockSize = newSize;
        }
    }

    IEnumerator WaitForNextScroll() { yield return new WaitForSeconds(.05f); CanScroll = true; }

    public void OnScroll(InputAction.CallbackContext ctx)
    {
        if (!IsActive)
            return;

        if (!ctx.performed)
            return;

        if (ctx.ReadValue<Vector2>().y == 0)
            return;

        if (!CanScroll)
            return;

        CanScroll = false;
        StartCoroutine(WaitForNextScroll());

        float scrollDelta = ctx.ReadValue<Vector2>().y < 0? -1 : 1;

        if (IDEBackground == null)
        {
            IDEBackground = GameManager.Instance.IDEScreen.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.gameObject.name == "Background")?.gameObject;
            if (IDEBackground == null)
                throw new System.Exception("Couldn't find the Background of IDEScreen");
        }

        // Define boundaries
        float minY, maxY;
        if (LowestBlock[CurrentlyProgramedId] != null)
            minY = LowestBlock[CurrentlyProgramedId].transform.parent.position.y - maxDistanceToBlock / 2; // Minimum Y position
        else
            minY = -maxDistanceToBlock / 2;

        if (HighestBlock[CurrentlyProgramedId] != null)
            maxY = HighestBlock[CurrentlyProgramedId].transform.parent.position.y + maxDistanceToBlock / 2; // Maximum Y position (adjust as needed)
        else
            maxY = maxDistanceToBlock / 2;

        // Calculate the delta based on the scroll input
        float delta = scrollDelta;

        // Move the IDEBackground and the main camera
        IDEBackground.transform.Translate(Vector3.up * delta, Space.World);
        Camera.main.transform.Translate(Vector3.up * delta, Space.World);

        // Clamp the positions within the defined boundaries
        Vector3 clampedBackgroundPosition = new Vector3(IDEBackground.transform.position.x, Mathf.Clamp(IDEBackground.transform.position.y, minY, maxY), IDEBackground.transform.position.z);
        Vector3 clampedCameraPosition = new Vector3(Camera.main.transform.position.x, Mathf.Clamp(Camera.main.transform.position.y, minY, maxY), Camera.main.transform.position.z);

        // Set the clamped positions
        IDEBackground.transform.position = clampedBackgroundPosition;
        Camera.main.transform.position = clampedCameraPosition;

        // Calculate the step based on the desired interval
        float newBackgroundY = Mathf.Round(IDEBackground.transform.position.y / step) * step;
        float newCameraY = Mathf.Round(Camera.main.transform.position.y / step) * step;

        // Set the positions to the nearest step
        IDEBackground.transform.position = new Vector3(IDEBackground.transform.position.x, newBackgroundY, IDEBackground.transform.position.z);
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, newCameraY, Camera.main.transform.position.z);
    }

    public void PrepToClose()
    {
        if (IDEBackground != null)
            IDEBackground.transform.position = Vector3.zero;
        else
            Debug.LogError("IDEBackground is not assigned in IDEManager");
            
        Camera.main.transform.position = Vector3.forward * -10;
    }

    private void OnOpen()
    {
        Drawer.GetComponentInChildren<DrawerScript>().RefreshDrawer(CurrentlyProgramedId);
    }

}


