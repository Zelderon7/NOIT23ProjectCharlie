using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Menus
    {
        Game,
        IDE,
        Pause,
        Settings,
    }
    
    
    [SerializeField] private GameObject IDEScreen;
    [SerializeField] private GameObject GameScreen;
    [SerializeField] AnimationCurve EaseInOutQuad = new AnimationCurve();
    
    
    public Menus CurrentMenu 
    { 
        get { return _currentMenu; }
        set
        {
            if (!_curMenuChangable)
                return;
            if(_currentMenu != value)
            {
                OnMenusClose[_currentMenu]?.Invoke();
                _currentMenu = value;
                OnMenusOpen[_currentMenu]?.Invoke();
            }
            
        }
    }

    public Dictionary<Menus, Action> OnMenusOpen { get; private set; } = new Dictionary<Menus, Action>();
    public Dictionary<Menus, Action> OnMenusClose { get; private set; } = new Dictionary<Menus, Action>();
    private bool _curMenuChangable = true;
    private Menus _currentMenu = Menus.Game;
    
    private void Awake()
    {
        OnMenusClose.Clear();
        OnMenusOpen.Clear();
        foreach(var item in Enum.GetValues(typeof(Menus)))
        {
            OnMenusClose.Add((Menus)item, new Action(() => { }));
            OnMenusOpen.Add((Menus)item, new Action(() => { }));
        }

        OnMenusOpen[Menus.IDE] += OnIDEOpen;
        OnMenusClose[Menus.IDE] += OnIDEClose;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentMenu = Menus.IDE;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnIDEOpen()
    {
        StartCoroutine(MoveTransform(IDEScreen.transform, Vector3.zero, 1.5f));        
        StartCoroutine(MoveTransform(GameScreen.transform, new Vector3(18, 0, 0), 1.5f));
    }

    private void OnIDEClose()
    {
        StartCoroutine(MoveTransform(IDEScreen.transform, new Vector3(-18f, 0, 0), 1.5f));
        StartCoroutine(MoveTransform(GameScreen.transform, Vector3.zero, 1.5f));
    }

    public void SwitchGameIDE()
    {
        CurrentMenu = CurrentMenu == Menus.Game ? Menus.IDE : Menus.Game;
    }

    IEnumerator MoveTransform(Transform targetTransform, Vector3 targetPosition, float duration)
    {
        _curMenuChangable = false;
        // Store the initial position of the transform
        Vector3 startPosition = targetTransform.position;

        // Variable to track elapsed time
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the interpolation factor between 0 and 1 based on elapsed time and duration
            float t = elapsedTime / duration;

            float easedT = EaseInOutQuad.Evaluate(t); // Use the easing function

            // Use Vector3.Lerp to interpolate between the start and target positions
            targetTransform.position = Vector3.Lerp(startPosition, targetPosition, easedT);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure that the transform reaches the exact target position
        targetTransform.position = targetPosition;
        _curMenuChangable = true;
    }
    

}
