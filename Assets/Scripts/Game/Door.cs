using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour, IInteractableGridObject
{
    float openAnimationDuration;

    [SerializeField]
    AnimationCurve AnimationCurve;

    private GameObject mySpriteMask;
    private SpriteRenderer mySpriteRenderer;

    AudioSource myAudioSource;
    AudioClip OpenDoorClip;

    [SerializeField]
    Sprite Locked;
    [SerializeField]
    Sprite UnLocked;
    [SerializeField]
    private bool isLocked;

    public bool IsLocked { get => isLocked;
        private set 
        {
            isLocked = value;
            mySpriteRenderer.sprite = IsLocked ? Locked : UnLocked;
            GetComponent<GridObjectDataSheet>().CanWalkOver = !IsLocked;
        }
    }
    public void Awake()
    {
        mySpriteMask = GetComponentInChildren<SpriteMask>().gameObject;
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.sprite = IsLocked ? Locked : UnLocked;
        GetComponent<GridObjectDataSheet>().CanWalkOver = !IsLocked;
        myAudioSource = GetComponent<AudioSource>();
        OpenDoorClip = myAudioSource.clip;
        openAnimationDuration = OpenDoorClip.length;
    }

    public void Interact(Action callback)
    {
        if (IsLocked)
            return;
        StartCoroutine(OpenDoor(callback));
    }

    IEnumerator OpenDoor(Action callback)
    {
        myAudioSource.Play();
        float elapsedTime = 0f;
        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = transform.localPosition + Vector3.down * transform.localScale.y;

        while (elapsedTime < openAnimationDuration)
        {
            float t = elapsedTime / openAnimationDuration;

            float easedT = AnimationCurve.Evaluate(t);

            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, easedT);

            mySpriteMask.transform.localPosition = startPosition - transform.localPosition;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = targetPosition;

        mySpriteMask.transform.localPosition = startPosition - transform.localPosition;



        callback?.Invoke();
        Destroy(gameObject);
    }

    public void Unlock()
    {
        if (!IsLocked)
            throw new Exception("Door is already unlocked");
        IsLocked = false;
    }

}
