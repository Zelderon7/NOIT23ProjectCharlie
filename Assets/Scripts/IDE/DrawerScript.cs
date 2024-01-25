using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerScript : MonoBehaviour
{
    public bool Opened { get; private set; } = false;
    bool inMotion = false;
    private void Start()
    {
        
    }

    IEnumerator OnOpenCoroutine()
    {
        inMotion = true;
        yield return StartCoroutine(MoveTransform(transform.parent, new Vector3(-.388f, 0, 0), 1f));
        Opened = true;
        inMotion = false;
    }

    IEnumerator OnCloseCoroutine()
    {
        inMotion = true;
        yield return StartCoroutine(MoveTransform(transform.parent, new Vector3(-.615f, 0, 0), 1f));
        Opened = false;
        inMotion = false;
    }

    private void OnMouseDown()
    {
        if (inMotion) return;
        
        if(Opened)
            StartCoroutine(OnCloseCoroutine());
        else
            StartCoroutine(OnOpenCoroutine());
    }

    public IEnumerator MoveTransform(Transform targetTransform, Vector3 targetPosition, float duration)
    {
        // Store the initial position of the transform
        Vector3 startPosition = targetTransform.localPosition;

        // Variable to track elapsed time
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the interpolation factor between 0 and 1 based on elapsed time and duration
            float t = elapsedTime / duration;

            float easedT = GameManager.Instance.EaseInOutQuad.Evaluate(t); // Use the easing function

            // Use Vector3.Lerp to interpolate between the start and target positions
            targetTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, easedT);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure that the transform reaches the exact target position
        targetTransform.localPosition = targetPosition;
    }
}
