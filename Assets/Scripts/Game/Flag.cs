using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour, IInteractableGridObject
{
    public void Interact(Action callback)
    {
        StartCoroutine(PickMeUp());
        callback?.Invoke();

    }

    IEnumerator PickMeUp()
    {
        yield return new WaitForSeconds(1);
        GetComponent<AudioSource>().Play();
        Debug.Log("You win");
        Destroy(gameObject);
    }
}
