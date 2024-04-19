using System;
using UnityEngine;

public class CodeablePort : MonoBehaviour
{
    public ICodeable Parent;
    public static Action OnGameStart = () => { };

    public void InvokeOnGameStart() => OnGameStart.Invoke();

    private void Awake()
    {
        OnGameStart += MyOnGameStart;
        if (!transform.parent.TryGetComponent(out Parent))
            throw new Exception("No ICodeable found in parent");

        GameManager.OnGameRestart += () => { gameObject.SetActive(true); };
    }

    void MyOnGameStart()
    {
        gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        IDEManager.Instance.CurrentlyProgramed = Parent;
        GameManager.Instance.CurrentMenu = GameManager.Menus.IDE;
    }

    void OnDestroy()
    {
        OnGameStart -= MyOnGameStart;
    }
}
