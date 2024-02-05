using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDrag : MonoBehaviour {
    #region Variables

    public bool ScriptEnabled { get; private set; } = true;
    bool _isDragging;
    int _currentZoomIndex = 0;

    Vector3 _origin;
    Vector3 _difference;
    Vector3 _targetPosition;

    Camera _mainCamera;
    Bounds _cameraBounds;

    [SerializeField] float[] zoomLevels;
    #endregion

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

    private void Start()
    {
        CameraBounds();

        GameManager.Instance.OnMenusClose[GameManager.Menus.Game] += OnMenuClose;
        GameManager.Instance.OnMenusOpen[GameManager.Menus.Game] += OnMenuOpen;
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

    private void CameraBounds()
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

    private void OnMenuClose()
    {
        _currentZoomIndex = 0;
        MoveCameraWithinBounds(zoomLevels[_currentZoomIndex]);
        _mainCamera.orthographicSize = zoomLevels[_currentZoomIndex];
        ScriptEnabled = false;
    }

    private void OnMenuOpen()
    {
        ScriptEnabled = true;
    }

    

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        if (!ScriptEnabled)
            return;

        if (ctx.started)
        {
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
            CameraBounds();

            float scrollValue = ctx.ReadValue<Vector2>().y;

            int newZoomIndex = _currentZoomIndex;

            if (scrollValue > 0 && _currentZoomIndex < zoomLevels.Length - 1)
            {
                newZoomIndex++;
            } else if (scrollValue < 0 && _currentZoomIndex > 0)
            {
                newZoomIndex--;
            }


            if (IsOutOfBounds(zoomLevels[newZoomIndex]))
            {
                MoveCameraWithinBounds(zoomLevels[newZoomIndex]);
                _currentZoomIndex = newZoomIndex;
                _mainCamera.orthographicSize = zoomLevels[_currentZoomIndex];
                CameraBounds();

            } else
            {
                _currentZoomIndex = newZoomIndex;
                _mainCamera.orthographicSize = zoomLevels[_currentZoomIndex];
                CameraBounds();

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
        float minX = Globals.WorldBounds.min.x + newZoomLevel * _mainCamera.aspect;
        float maxX = Globals.WorldBounds.max.x - newZoomLevel * _mainCamera.aspect;

        float minY = Globals.WorldBounds.min.y + newZoomLevel;
        float maxY = Globals.WorldBounds.max.y - newZoomLevel;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);

        float deltaX = clampedX - transform.position.x;
        float deltaY = clampedY - transform.position.y;

        transform.position += new Vector3(deltaX, deltaY, 0f);
        _targetPosition = GetMousePosition();
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
