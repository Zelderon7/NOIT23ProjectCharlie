using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDrag : MonoBehaviour {
    #region Variables

    public bool ScriptEnabled { get; private set; } = true;

    private Vector3 _origin;
    private Vector3 _difference;

    private Camera _mainCamera;

    private bool _isDragging;

    private Bounds _cameraBounds;
    private Vector3 _targetPosition;



    [SerializeField] float[] zoomLevels;
    private int currentZoomIndex = 0;

    private float zoomSpeed = 2f;

    #endregion

    private void InitializeCameraBounds()
    {
        var height = _mainCamera.orthographicSize;
        var width = height * _mainCamera.aspect;

        var minX = Globals.WorldBounds.min.x + width;
        var maxX = Globals.WorldBounds.extents.x - width;

        var minY = Globals.WorldBounds.min.y + height;
        var maxY = Globals.WorldBounds.extents.y - height;

        _cameraBounds = new Bounds();
        _cameraBounds.SetMinMax(
            new Vector3(minX, minY, 0.0f),
            new Vector3(maxX, maxY, 0.0f)
        );
    }

        

    private void Awake()
    {
        _mainCamera = Camera.main;
        float j = 5f;
        zoomLevels = new float[20];
        for (int i = 0; i < 20; i++)
        {
            zoomLevels[i] = j;

            j -= 0.2f;
           
        }
    }

    private void OnMenuClose()
    {
        currentZoomIndex = 0;
        MoveCameraWithinBounds(zoomLevels[currentZoomIndex]);
        _mainCamera.orthographicSize = zoomLevels[currentZoomIndex];
        ScriptEnabled = false;
    }

    private void OnMenuOpen()
    {
        ScriptEnabled = true;
    }

    private void Start()
    {
        // Initialization of camera bounds
        InitializeCameraBounds();

        GameManager.Instance.OnMenusClose[GameManager.Menus.Game] += OnMenuClose;
        GameManager.Instance.OnMenusOpen[GameManager.Menus.Game] += OnMenuOpen;
    }

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        if (!ScriptEnabled)
            return;

        if (ctx.started)
        {
            //Debug.Log("DRAGGING");
            _origin = GetMousePosition();
            _isDragging = true;
        } else if (ctx.canceled)
        {
            _isDragging = false;
        }
    }

    public void OnScroll(InputAction.CallbackContext ctx)
    {
        if (!ScriptEnabled)
            return;

        if (ctx.performed)
        {
            // Recalculate camera bounds based on the new camera size
            RecalculateCameraBounds();

            // Get the scroll value from the callback context
            float scrollValue = ctx.ReadValue<Vector2>().y;

            // Log the current zoom level to the console
            //Debug.Log("Current Zoom Level: " + currentZoomIndex);

            // Calculate the new zoom index
            int newZoomIndex = currentZoomIndex;

            if (scrollValue > 0 && currentZoomIndex < zoomLevels.Length - 1)
            {
                // If scrolling up and not at the max zoom level
                newZoomIndex++;
            } else if (scrollValue < 0 && currentZoomIndex > 0)
            {
                // If scrolling down and not at the min zoom level
                newZoomIndex--;
            }

            //Debug.Log("New Zoom Index: " + newZoomIndex);

            // Check if the new zoom level goes outside bounds
            if (IsOutOfBounds(zoomLevels[newZoomIndex]))
            {
                // Move the camera to stay within bounds
                MoveCameraWithinBounds(zoomLevels[newZoomIndex]);
                //Debug.Log("Move camera within bounds");

                // Set the new zoom index
                currentZoomIndex = newZoomIndex;

                // Set the new zoom level
                _mainCamera.orthographicSize = zoomLevels[currentZoomIndex];

                // Recalculate the camera bounds with the new size
                RecalculateCameraBounds();
            } else
            {
                //Debug.Log("DIDN'T Move camera within bounds");

                // Set the new zoom index
                currentZoomIndex = newZoomIndex;

                // Set the new zoom level
                _mainCamera.orthographicSize = zoomLevels[currentZoomIndex];

                // Recalculate the camera bounds with the new size
                RecalculateCameraBounds();
            }
        }
    }



    private bool IsOutOfBounds(float newZoomLevel)
    {
        var height = newZoomLevel;
        var width = newZoomLevel * _mainCamera.aspect;

        var minX = Globals.WorldBounds.min.x + width;
        var maxX = Globals.WorldBounds.extents.x - width;

        var minY = Globals.WorldBounds.min.y + height;
        var maxY = Globals.WorldBounds.extents.y - height;

        return _targetPosition.x < minX || _targetPosition.x > maxX || _targetPosition.y < minY || _targetPosition.y > maxY;
    }

    private void MoveCameraWithinBounds(float newZoomLevel)
    {
        // Calculate the allowed range for the camera
        float minX = Globals.WorldBounds.min.x + newZoomLevel * _mainCamera.aspect;
        float maxX = Globals.WorldBounds.max.x - newZoomLevel * _mainCamera.aspect;

        float minY = Globals.WorldBounds.min.y + newZoomLevel;
        float maxY = Globals.WorldBounds.max.y - newZoomLevel;

        // Calculate the new position within bounds
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);

        // Calculate the delta movement to smoothly adjust the camera position
        float deltaX = clampedX - transform.position.x;
        float deltaY = clampedY - transform.position.y;

        // Update the camera position smoothly
        transform.position += new Vector3(deltaX, deltaY, 0f);

        // Update the _targetPosition
        _targetPosition = GetMousePosition();
    }






    private void RecalculateCameraBounds()
    {
        InitializeCameraBounds(); // Recalculate and set camera bounds
    }

    private void LateUpdate()
    {
        if (!ScriptEnabled)
            return;

        if (!_isDragging)
            return;
        _difference = GetMousePosition() - transform.position;

        _targetPosition = _origin - _difference;
        _targetPosition = GetCameraBounds();

        transform.position = _targetPosition;
    }

    private Vector3 GetCameraBounds()
    {
        return new Vector3(
            Mathf.Clamp(_targetPosition.x, _cameraBounds.min.x, _cameraBounds.max.x),
            Mathf.Clamp(_targetPosition.y, _cameraBounds.min.y, _cameraBounds.max.y),
            transform.position.z
        );
    }

    private Vector3 GetMousePosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        return _mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, _mainCamera.nearClipPlane));
    }
}
