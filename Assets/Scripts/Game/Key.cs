using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour, IInteractableGridObject
{
    public Door door;

    public void Interact(Action callback)
    {
        StartCoroutine(PickMeUp());
        callback?.Invoke();
    }

    IEnumerator PickMeUp()
    {
        yield return new WaitForSeconds(1);
        GetComponent<AudioSource>().Play();
        door.Unlock();
        Destroy(gameObject);
    }
}
