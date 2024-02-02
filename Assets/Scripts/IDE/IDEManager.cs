using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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

    
    public List<CodeBlocksPrefabs> BlockTypesPrefs { get => blockTypesPrefs; private set => blockTypesPrefs = value; }
    [SerializeField] float scrollSpeed = 20f;
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
            CurrentlyProgramedId = value.Id;
            if (!SavedPrograms.ContainsKey(CurrentlyProgramed))
                SavedPrograms.Add(CurrentlyProgramed, new List<GameObject>());
            LoadProgram(CurrentlyProgramed.Id);
        }
    }

    Dictionary<ICodeable, List<GameObject>> SavedPrograms = new Dictionary<ICodeable, List<GameObject>>();
    Dictionary<int, ICodeable> CodeableObjectsDictionary = new Dictionary<int, ICodeable>();
    public bool IsActive { get { return GameManager.Instance.CurrentMenu == GameManager.Menus.IDE; } }

    public Action OnCodeStart = () => { };

    [SerializeField]
    private List<CodeBlocksPrefabs> blockTypesPrefs = new List<CodeBlocksPrefabs>();

    public int CurrentlyProgramedId { get; private set; }

    public ICodeable GetICodeableById(int id) => CodeableObjectsDictionary[id];

    public void OnBlockCreation(GameObject a)
    {
        SavedPrograms[CurrentlyProgramed].Add(a);
    }

    public void StartCode()
    {
        if(GameManager.Instance.CurrentMenu != GameManager.Menus.Game)
            GameManager.Instance.CurrentMenu = GameManager.Menus.Game;
        else
        {
            OnCodeStart?.Invoke();
            CodeablePort.OnGameStart?.Invoke();
        }
            
    }

    void SaveProgram(int programId)
    {
        foreach (var item in SavedPrograms[GetICodeableById(programId)])
        {
            transform.position -= Vector3.right * 50;
        }
    }

    void LoadProgram(int programId)
    {
        foreach (var item in SavedPrograms[GetICodeableById(programId)])
        {
            transform.position += Vector3.right * 50;
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

    private void Update()
    {
        if (IsActive)
        {
            // Get the scroll wheel delta
            float scrollDelta = Input.mouseScrollDelta.y;

            if (scrollDelta != 0)
                ScrollUpAndDown(scrollDelta);
        }
        
    }

    /// <summary>
    /// Moves the IDEBackground and The main camera up or down based on the <see cref="scrollSpeed"/>
    /// </summary>
    /// <param name="scrollDelta">The current scroll direction</param>
    /// <exception cref="System.Exception"></exception>
    void ScrollUpAndDown(float scrollDelta)
    {
        if (IDEBackground == null)
           IDEBackground = GameManager.Instance.IDEScreen.GetComponentsInChildren<Transform>().First(x => (x.gameObject.name == "Background")).gameObject;

        if (IDEBackground == null)
            throw new System.Exception("Couldn't find the Background of IDEScreen");

        IDEBackground.transform.Translate(Vector3.up * scrollDelta * scrollSpeed * Time.deltaTime);
        Camera.main.transform.Translate(Vector3.up * scrollDelta * scrollSpeed * Time.deltaTime);
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


