using System;
using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractableGridObject {

    float _openAnimationDuration;
    bool _isLocked;

    [SerializeField] AnimationCurve _animationCurve;
    [SerializeField] Sprite _locked;
    [SerializeField] Sprite _unlocked;
    [SerializeField] GameObject _mySpriteMask;

    SpriteRenderer _mySpriteRenderer;
    AudioSource _myAudioSource;
    AudioClip OpenDoorClip;

    

    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            _isLocked = value;
            _mySpriteRenderer.sprite = IsLocked ? _locked : _unlocked;
            GetComponent<GridObjectDataSheet>().CanWalkOver = !IsLocked;
        }
    }
    public void Awake()
    {
        _mySpriteMask = GetComponentInChildren<SpriteMask>().gameObject;
        _mySpriteRenderer = GetComponent<SpriteRenderer>();
        _mySpriteRenderer.sprite = IsLocked ? _locked : _unlocked;
        GetComponent<GridObjectDataSheet>().CanWalkOver = !IsLocked;
        _myAudioSource = GetComponent<AudioSource>();
        OpenDoorClip = _myAudioSource.clip;
        _openAnimationDuration = OpenDoorClip.length;
    }

    public void Interact(Action callback)
    {
        if (IsLocked)
            return;
        StartCoroutine(OpenDoor(callback));
    }

    IEnumerator OpenDoor(Action callback)
    {
        _myAudioSource.Play();
        float elapsedTime = 0f;
        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = transform.localPosition + Vector3.down * transform.localScale.y;

        while (elapsedTime < _openAnimationDuration)
        {
            float t = elapsedTime / _openAnimationDuration;

            float easedT = _animationCurve.Evaluate(t);

            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, easedT);

            _mySpriteMask.transform.localPosition = startPosition - transform.localPosition;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = targetPosition;

        _mySpriteMask.transform.localPosition = startPosition - transform.localPosition;



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