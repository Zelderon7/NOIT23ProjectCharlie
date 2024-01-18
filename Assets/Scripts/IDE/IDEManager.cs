using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] float scrollSpeed = 20f;
    [SerializeField] GameObject IDEBackground;
    public ICodeable CurrentlyProgramed { get => currentlyProgramed;
        set
        {
            SaveProgram(currentlyProgramed);
            currentlyProgramed = value;
            LoadProgram(currentlyProgramed);
        }
    }

    Dictionary<ICodeable, List<GameObject>> SavedPrograms = new Dictionary<ICodeable, List<GameObject>>();
    public bool IsActive { get { return GameManager.Instance.CurrentMenu == GameManager.Menus.IDE; } }

    public Action OnCodeStart = () => { };
    private ICodeable currentlyProgramed = null;

    public void OnBlockCreation(GameObject a)
    {
        if(!SavedPrograms.ContainsKey(currentlyProgramed))
            SavedPrograms.Add(currentlyProgramed, new List<GameObject>());

        SavedPrograms[currentlyProgramed].Add(a);
    }

    public void StartCode()
    {
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

            DontDestroyOnLoad(gameObject);
        }

        #endregion

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

    public void CodeDrawer()
    {
        
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
