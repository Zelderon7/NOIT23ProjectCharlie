using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderBall : MonoBehaviour
{
    [SerializeField] Transform _backRect;
    [SerializeField] int _steps;

    public Action<int> OnChange;

    private Vector3 _startPosition; // Initial position of the slider ball
    private Vector3 _endPosition;   // Final position of the slider ball

    private Vector3 _startPositionM; // Initial position of the slider ball
    private Vector3 _endPositionM;   // Final position of the slider ball

    private float _stepSize;
    private float _stepSizeM;

    private int _currentStep;

    public int CurrentStep
    {
        get => _currentStep;
        private set
        {
            if (value >= 0 && value <= _steps)
            {
                OnChange?.Invoke(value);
                _currentStep = value;
            }
        }
    }

    void Start()
    {
        // Calculate the start and end positions of the slider
        _startPosition = _backRect.localPosition - Vector3.right * (_backRect.localScale.x / 2f);
        _endPosition = _backRect.localPosition + Vector3.right * (_backRect.localScale.x / 2f);

        _startPositionM = _backRect.localPosition - Vector3.right * (_backRect.lossyScale.x / 2f);
        _endPositionM = _backRect.localPosition + Vector3.right * (_backRect.lossyScale.x / 2f);

        // Set up step size and initial step
        _stepSize = (_endPosition.x - _startPosition.x) / _steps;
        _stepSizeM = (_endPositionM.x - _startPositionM.x) / _steps;

        CurrentStep = _steps / 2;
        transform.localPosition = _startPosition + Vector3.right * CurrentStep * _stepSize;
    }

    void OnMouseDrag()
    {
        // Move the slider ball along the slider track
        Vector2 mousePosition = GetMouseWorldPosition();
        Debug.Log("Mouse X: " + mousePosition.x);
        int newPos = (int)Math.Clamp(Mathf.Round((mousePosition.x - _startPositionM.x) / _stepSizeM), 0, _steps);
        transform.localPosition = new Vector2(_startPosition.x + (float)newPos * _stepSize, transform.localPosition.y);
        if (CurrentStep != newPos)
            CurrentStep = newPos;
    }

    Vector3 GetMouseWorldPosition()
    {
        // Get the mouse position in screen coordinates
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Set the z position to the distance from the camera to the world plane
        mouseScreenPosition.z = Camera.main.nearClipPlane;

        // Convert the screen position to world position
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        return mouseWorldPosition;
    }
}
