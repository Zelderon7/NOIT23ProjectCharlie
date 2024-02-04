using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour, IInteractableGridObject
{
    public void Interact(Action callback)
    {
        StartCoroutine(PickMeUp(callback));
    }

    IEnumerator PickMeUp(Action callback)
    {
        GameManager.Instance.Victory();
        callback?.Invoke();
        GetComponent<AudioSource>().Play(); 
        yield return new WaitForSeconds(GetComponent<AudioSource>().clip.length);
        Destroy(gameObject);
    }
}
