using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour, IInteractableGridObject
{
    [SerializeField]
    float openAnimationDuration = 1.0f;

    private GameObject mySpriteMask;

    public void Awake()
    {
        mySpriteMask = GetComponentInChildren<SpriteMask>().gameObject;
    }

    public void Interact(Action callback)
    {
        StartCoroutine(OpenDoor(callback));
    }

    IEnumerator OpenDoor(Action callback)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = transform.localPosition + Vector3.up * transform.localScale.y;

        while (elapsedTime < openAnimationDuration)
        {
            float t = elapsedTime / openAnimationDuration;

            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);

            mySpriteMask.transform.localPosition = startPosition - transform.localPosition;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = targetPosition;

        mySpriteMask.transform.localPosition = startPosition - transform.localPosition;



        callback?.Invoke();
        //Destroy(gameObject);
    }

}
