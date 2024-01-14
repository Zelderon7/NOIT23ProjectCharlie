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
            if (_instance != null)
                return _instance;
            else
            {
                GameObject obj = new GameObject("IDEManager");
                obj.AddComponent<IDEManager>();
                _instance = obj.GetComponent<IDEManager>();
                DontDestroyOnLoad(obj);
                return _instance;
            }
        }
    }

    #endregion

    [SerializeField] float scrollSpeed = 20f;
    [SerializeField] GameObject IDEBackground;
    public ICodeable CurrentlyProgramed = null;
    public bool IsActive { get { return GameManager.Instance.CurrentMenu == GameManager.Menus.IDE; } }

    public Action OnCodeStart = () => { };

    public void RunCode()
    {
        OnCodeStart?.Invoke();
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
