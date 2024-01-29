using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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

    [SerializeField]
    GameObject[] BlockTypesPrefs;

    [SerializeField] float scrollSpeed = 20f;
    [SerializeField] GameObject IDEBackground;

    [SerializeField]
    GameObject Drawer;
    public ICodeable CurrentlyProgramed { get => currentlyProgramed;
        set
        {
            if (value == null)
                return;
            if (currentlyProgramed != null)
                SaveProgram(currentlyProgramed);
            currentlyProgramed = value;
            if (!SavedPrograms.ContainsKey(currentlyProgramed))
                SavedPrograms.Add(currentlyProgramed, new List<GameObject>());
            LoadProgram(currentlyProgramed);
        }
    }

    Dictionary<ICodeable, List<GameObject>> SavedPrograms = new Dictionary<ICodeable, List<GameObject>>();
    public bool IsActive { get { return GameManager.Instance.CurrentMenu == GameManager.Menus.IDE; } }

    public Action OnCodeStart = () => { };

    private ICodeable currentlyProgramed = null;

    public void OnBlockCreation(GameObject a)
    {
        SavedPrograms[currentlyProgramed].Add(a);
    }

    public void StartCode()
    {
        if(GameManager.Instance.CurrentMenu != GameManager.Menus.Game)
            GameManager.Instance.CurrentMenu = GameManager.Menus.Game;
        else
            OnCodeStart?.Invoke();
    }

    void SaveProgram(ICodeable program)
    {
        foreach (var item in SavedPrograms[program])
        {
            transform.position -= Vector3.right * 50;
        }
    }

    void LoadProgram(ICodeable program)
    {
        foreach (var item in SavedPrograms[program])
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
        RefreshCodeDrawer(new BlockTypes[] { new BlockTypes(0, 1), new BlockTypes(1, -1), new BlockTypes(2, -1), new BlockTypes(3, -1) });
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

    public void RefreshCodeDrawer(BlockTypes[] blocks)
    {

        for (int i = 0; i < blocks.Length; i++)
        {
            GameObject temp = Instantiate(BlockTypesPrefs[blocks[i].id], parent: Drawer.transform);
            temp.GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(x => { x.sortingLayerName = "IDEScreen"; x.sortingOrder += 11; });
            var _tempTextRef = temp.GetComponentInChildren<TextMeshPro>();
            if( _tempTextRef != null)
            {
                _tempTextRef.sortingOrder += 11;
            }
                

            temp.transform.localScale = new Vector3(.5f, .5f, 1);
            temp.transform.localPosition = new Vector3(-.4f, 5 - i*(temp.transform.localScale.y*1.8f) - temp.transform.localScale.y, 2);
            
            GameObject targetParent = temp.GetComponentInChildren<Block>().gameObject;
            Destroy(targetParent.GetComponent<Block>());
            DrawerBlock scriptRef = targetParent.AddComponent<DrawerBlock>();
            GameObject TextObject = Instantiate(new GameObject("TextHolder"),parent: temp.transform);
            scriptRef.text = TextObject.AddComponent<TextMeshPro>();
            scriptRef.text.rectTransform.localPosition = Vector3.right * 3.8f;
            scriptRef.text.rectTransform.sizeDelta = Vector2.one;
            scriptRef.text.enableAutoSizing = true;
            scriptRef.text.fontSizeMin = 0;
            scriptRef.text.color = Color.black;
            scriptRef.text.fontStyle = FontStyles.Bold;
            scriptRef.Count = blocks[i].count;
            scriptRef.RefreshText();
            scriptRef.MePrefab = BlockTypesPrefs[blocks[i].id];
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

}

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
