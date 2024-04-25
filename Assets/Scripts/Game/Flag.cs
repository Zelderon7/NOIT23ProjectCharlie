using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour, IInteractableGridObject
{
    static int FlagsCount = 0;
    static HashSet<int> Finishers;

    [SerializeField]
    Color collectedColor;

    private void Awake()
    {
        Finishers = new HashSet<int>();
        FlagsCount++;
    }

    private void OnDestroy()
    {
        FlagsCount--;
    }

    public void Interact(Action callback, int callerId)
    {
        StartCoroutine(PickMeUp(callback, callerId));
    }

    IEnumerator PickMeUp(Action callback, int callerId)
    {
        callback?.Invoke();
        yield return new WaitForSeconds(.6f);
        GetComponent<SpriteRenderer>().color = collectedColor;
        GetComponent<AudioSource>().Play();
        Finishers.Add(callerId);
        if (Finishers.Count >= FlagsCount)
            GameManager.Instance.Victory();
    }
}
