using System;
using System.Collections;
using UnityEngine;

public class Key : MonoBehaviour, IInteractableGridObject
{
    Door _door;

    public Door Door
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
        Door.Unlock();
        yield return new WaitForSeconds(GetComponent<AudioSource>().clip.length);
        Destroy(gameObject);
    }
}
