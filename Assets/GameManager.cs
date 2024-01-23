using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridObject {
    public int id;
    public string objectName;
    public Sprite sprite; // The prefab to instantiate
    // Add other properties specific to your game object

    public GridObject(int id, string objectName, Sprite sprite)
    {
        this.id = id;
        this.objectName = objectName;
        this.sprite = sprite;
    }
}

public class GameManager : MonoBehaviour
{

    [SerializeField]
     List<GridObject> gridObjects = new List<GridObject> ();


    #region Singleton pattern
    private static GameManager instance;

    string seed = "2,2,2,1,2/2,2,2,1,2/2,2,2,1,2/2,2,1,1,2/2,0,2,0,2";
    string levelName;
    string authorName;
    
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    instance = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(instance);
                }
            }

            return instance;
        }
    }

    #endregion

    #region Menus

    public enum Menus
    {
        Game,
        IDE,
        IDEDrawer,
        Pause,
        Settings,
    }


    [SerializeField]
    private GameObject _IDEScreen;
    public GameObject IDEScreen {
        get
        {
            return _IDEScreen;
        }
    }
    [SerializeField]
    private GameObject _GameScreen;
    public GameObject GameScreen
    {
        get
        {
            return _GameScreen;
        }
    }
    [SerializeField] AnimationCurve EaseInOutQuad = new AnimationCurve();


    public Menus CurrentMenu
    {
        get { return _currentMenu; }
        set
        {
            if (!_curMenuChangable)
                return;
            if (_currentMenu != value)
            {
                ChangeMenu(value);
            }

        }
    }

    public Dictionary<Menus, Action> OnMenusOpen { get; private set; } = new Dictionary<Menus, Action>();
    public Dictionary<Menus, Action> OnMenusClose { get; private set; } = new Dictionary<Menus, Action>();
    private bool _curMenuChangable = true;
    private Menus _currentMenu = Menus.Game;


    #endregion

    #region Grid Variables

    [SerializeField] private GameObject tilePrefab;
    /// <summary>
    /// The parent transform of every grid cell
    /// </summary>
    [SerializeField] public GameObject GridParent;

    public int GridWidth
    {
        get { return _gridWidth; }
        set
        {
            _gridWidth = value;
        }
    }

    public int GridHeight
    {
        get { return _gridWidth; }
        set
        {
            _gridHeight = value;
        }
    }

    private List<GameObject> grid = new List<GameObject>();
    [SerializeField] private float gridPadding = 0;

    [SerializeField] public float gridSpacing = 0.2f;

    [SerializeField]
    private int _gridWidth, _gridHeight;
    private float gridXRepos = 2.15f;
    private float gridYRepos = 1.2f;
    public float cellSize { get; private set; }

    [SerializeField]
    private List<Sprite> GridTileSprites;

#endregion

    void Awake()
    {
        #region Singleton pattern

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        #endregion

        #region Menus
        OnMenusClose.Clear();
        OnMenusOpen.Clear();
        foreach(var item in Enum.GetValues(typeof(Menus)))
        {
            OnMenusClose.Add((Menus)item, new Action(() => { }));
            OnMenusOpen.Add((Menus)item, new Action(() => { }));
        }

        OnMenusOpen[Menus.IDE] += OnIDEOpen;
        OnMenusClose[Menus.IDE] += OnIDEClose;

        #endregion
        ProcessSeedString(seed);
    }

    public void FetchData(string data)
    {
        // Split the data using the "//" delimiter
        string[] dataParts = data.Split(new string[] { "//" }, System.StringSplitOptions.RemoveEmptyEntries);

        // Process the data
        foreach (string part in dataParts)
        {
            // Split each part using ":" delimiter
            string[] keyValue = part.Split(':');

            // Ensure the part has at least two elements (key and value)
            if (keyValue.Length == 2)
            {
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();

                // Assign values based on the key
                switch (key)
                {
                    case "LEVEL_NAME":
                        levelName = value;
                        break;
                    case "AUTHOR_NAME":
                        authorName = value;
                        break;
                    case "SEED":
                        seed = value;
                        break;
                    default:
                        // Handle unknown key or ignore
                        break;
                }
            } 
            else
            {
                throw new ArgumentException("Invalid Data");
            }
        }

        ProcessSeedString(seed);

        // Now, you have the values in the levelName, authorName, and seed variables
        Debug.Log("Level Name: " + levelName);
        Debug.Log("Author Name: " + authorName);
        Debug.Log("Seed: " + seed);
    }

    void ProcessSeedString(string seed)
    {
        // Split the seed string by the row delimiter '/'
        string[] rows = seed.Split('/');

        // Set grid width and height based on seed
        _gridWidth = rows[0].Split(',').Length;
        _gridHeight = rows.Length;

        // Create a 2D array to store the grid data
        int[,] gridData = new int[_gridHeight, _gridWidth];

        // Iterate over each row
        for (int rowIndex = 0; rowIndex < _gridHeight; rowIndex++)
        {
            // Split the row string by commas to get individual column values
            string[] columns = rows[rowIndex].Split(',');

            // Iterate over each column
            for (int colIndex = 0; colIndex < _gridWidth; colIndex++)
            {
                // Parse the string value to an integer and assign it to the 2D array
                if (int.TryParse(columns[colIndex], out int cellValue))
                {
                    gridData[rowIndex, colIndex] = cellValue;
                } else
                {
                    Debug.LogError("Failed to parse grid data at row " + rowIndex + ", column " + colIndex);
                }
            }
        }

        // Now you have the grid data in the 'gridData' array, and you can use it to instantiate the grid
        InstantiateGridFromData(gridData);
    }

    void InstantiateGridFromData(int[,] gridData)
    {
        // Your existing grid instantiation code can be modified to use the provided grid data
        for (int row = 0; row < _gridHeight; row++)
        {
            for (int col = 0; col < _gridWidth; col++)
            {
                // Access gridData[row, col] to get the value for the current cell
                int objectId = gridData[row, col];

                // Use objectId to find the corresponding GridObject
                GridObject gridObject = GetGridObjectById(objectId);

                if (gridObject != null)
                {
                    // Instantiate an empty GameObject as the grid cell
                    GameObject gridCell = new GameObject("GridCell-" + col + "," + row);
                    gridCell.transform.parent = GridParent.transform;
                    gridCell.transform.position = new Vector3(col, row, 0f);

                    GameObject background = new GameObject("Background");
                    background.transform.parent = gridCell.transform;
                    background.transform.localPosition = Vector3.zero;
                    SpriteRenderer backgroundRenderer = background.AddComponent<SpriteRenderer>();
                    backgroundRenderer.sprite = GetTileSprite(col, row);
                    backgroundRenderer.sortingLayerName = "GameScreen";


                    GameObject cell = new GameObject("Cell");
                    cell.transform.parent = gridCell.transform;
                    cell.transform.localPosition = Vector3.zero;
                    SpriteRenderer cellRenderer = cell.AddComponent<SpriteRenderer>();
                    cellRenderer.sprite = gridObject.sprite;
                    cellRenderer.sortingLayerName = "GameScreen";


                }
            }
        }
    }






    private GridObject GetGridObjectById(int objectId)
    {
        return gridObjects.Find(obj => obj.id == objectId);
    }


    


    

    #region Grid Instantiation
    

    private Sprite GetTileSprite(int col, int row)
    {
        int x, y;
        if (col == 0)
            x = 0;
        else if (col == _gridWidth - 1)
            x = 2;
        else
            x = 1;

        if (row == 0)
            y = 0;
        else if (row == _gridHeight - 1)
            y = 2;
        else
            y = 1;

        return GridTileSprites[y*3 + x];
    }

    float CalculateCellSize(Camera camera)
    {
        float cameraHeight = 2f * camera.orthographicSize;
        float cameraWidth = cameraHeight * camera.aspect;

        cameraWidth -= gridXRepos;
        cameraHeight -= gridYRepos;

        float cellSizeY = (cameraHeight - (2 * gridPadding) - (gridSpacing * (_gridHeight - 1))) / _gridHeight;
        float cellSizeX = (cameraWidth - (2 * gridPadding) - (gridSpacing * (_gridWidth - 1))) / _gridWidth;

        return Mathf.Min(cellSizeX, cellSizeY);
    }

    #endregion

    #region Menus

    private bool OnMenuChangeSkipConditions(Menus value, Menus previous)
    {
        if (value == Menus.IDEDrawer && previous == Menus.IDE && _currentMenu == Menus.IDE)
            return true;
        if(previous == Menus.IDEDrawer && value == Menus.IDE && _currentMenu == Menus.IDE)
            return true;

        return false;
    }

    private void ChangeMenu(Menus value)
    {
        Menus prevValue = _currentMenu;
        if (!OnMenuChangeSkipConditions(value, prevValue))
            OnMenusClose[_currentMenu]?.Invoke();
        _currentMenu = value;
        if (!OnMenuChangeSkipConditions(value, prevValue))
            OnMenusOpen[value]?.Invoke();
        
    }

    private void OnIDEOpen()
    {
        StartCoroutine(MoveTransform(IDEScreen.transform, Vector3.zero, 1.5f));
        //StartCoroutine(MoveTransform(GameScreen.transform, new Vector3(18, 0, 0), 1.5f));
    }

    private void OnIDEClose()
    {
        IDEManager.Instance.PrepToClose();
        StartCoroutine(MoveTransform(IDEScreen.transform, new Vector3(-18f, 0, 0), 1.5f));
        //StartCoroutine(MoveTransform(GameScreen.transform, Vector3.zero, 1.5f));
    }

    public  void SwitchGameIDE()
    {
        CurrentMenu = CurrentMenu == Menus.Game ? Menus.IDE : Menus.Game;
    }

    public IEnumerator MoveTransform(Transform targetTransform, Vector3 targetPosition, float duration)
    {
        if (_curMenuChangable == false)//TODO: Test and Refactore if needed
            StopCoroutine("MoveTransform");

        _curMenuChangable = false;
        // Store the initial position of the transform
        Vector3 startPosition = targetTransform.position;

        // Variable to track elapsed time
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the interpolation factor between 0 and 1 based on elapsed time and duration
            float t = elapsedTime / duration;

            float easedT = EaseInOutQuad.Evaluate(t); // Use the easing function

            // Use Vector3.Lerp to interpolate between the start and target positions
            targetTransform.position = Vector3.Lerp(startPosition, targetPosition, easedT);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure that the transform reaches the exact target position
        targetTransform.position = targetPosition;
        _curMenuChangable = true;
    }

    #endregion

    #region Grid Methods

    public bool IsCellWalkable(int x, int y)
    {
        if (grid[y * GridWidth + x] == null)
            throw new Exception("Grid hasn't been instantiated properly");

        Tile targetTile = grid[y * GridWidth + x].GetComponent<Tile>();

        return targetTile.OccupyingObject == null || targetTile.OccupyingObject is ISteppableOver;//TODO: FIX THIS BULLSHIT
    }

    #endregion
}
