using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour, IInteractableGridObject
{
    Door _door;

    public Door door
    {
        get => _door;
        set
        {
            if (_door != null)
                _door.IsLocked = false;
            _door = value;
            _door.IsLocked = true;
        }
    }

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
