using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDrag : MonoBehaviour {
    #region Variables

    private Vector3 _origin;
    private Vector3 _difference;

    private Camera _mainCamera;

    private bool _isDragging;

    private Bounds _cameraBounds;
    private Vector3 _targetPosition;

    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

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

    private void Awake() => _mainCamera = Camera.main;

    private void Start()
    {
        // Initialization of camera bounds
        InitializeCameraBounds();
    }

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            Debug.Log("DRAGGING");
            _origin = GetMousePosition;
            _isDragging = true;
        } else if (ctx.canceled)
        {
            _isDragging = false;
        }
    }

    public void OnScroll(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            // Recalculate camera bounds based on the new camera size
            RecalculateCameraBounds();

            // Get the scroll value from the callback context
            float scrollValue = ctx.ReadValue<Vector2>().y;

            // Log the scroll value to the console
            Debug.Log("Scroll Value: " + scrollValue);

            // Handle zooming using Unity's new Input System
            float newSize = _mainCamera.orthographicSize - scrollValue * zoomSpeed;
            newSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            _mainCamera.orthographicSize = newSize;
        }
    }

    private void RecalculateCameraBounds()
    {
        InitializeCameraBounds(); // Recalculate and set camera bounds
    }

   

    private void LateUpdate()
    {
        if (!_isDragging)
            return;
        _difference = GetMousePosition - transform.position;

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

    private Vector3 GetMousePosition => _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
}