using System;
using System.Collections;
using UnityEngine;

public class Flag : MonoBehaviour, IInteractableGridObject
{
    public void Interact(Action callback)
    {
        StartCoroutine(PickMeUp(callback));
    }

    IEnumerator PickMeUp(Action callback)
    {
        callback?.Invoke();
        yield return new WaitForSeconds(GameManager.Instance.CellSize / 1.5f);
        GetComponent<AudioSource>().Play(); 
        //yield return new WaitForSeconds(GetComponent<AudioSource>().clip.length);
        GameManager.Instance.Victory();
    }
}
