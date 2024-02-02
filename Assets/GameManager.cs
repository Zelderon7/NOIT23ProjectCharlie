using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct Object {
    public int id;
    public string objectName;
    public GameObject prefab;

    public Object(int id, string objectName, GameObject prefab)
    {
        this.id = id;
        this.objectName = objectName;
        this.prefab = prefab;
    }
}

public class ScriptableObjectData {
    public int ID
    {
        get;
    }
    public int FacingDirection
    {
        get;
    }
    public int ReferenceID
    {
        get;
    }
    public BlockTypes[] CodeBlocks
    {
        get;
    }

    public ScriptableObjectData(int id, int facingDirection, int referenceID, BlockTypes[] codeBlocks)
    {
        ID = id;
        FacingDirection = facingDirection;
        ReferenceID = referenceID;
        CodeBlocks = codeBlocks;
    }
}

public class GameManager : MonoBehaviour
{

    [SerializeField]
    List<Object> gridObjects = new List<Object>();

    [SerializeField]
    List<Object> scriptableObjects = new List<Object>();

   

    #region Singleton pattern
    private static GameManager instance;

    string seed = "0,0,0,1,0/2,2,2,2,0/2,2,2,2,0/2,2,2,2,0/2,2,2,2,4/;{1-1-1:([0,1],[1,-1],[2,-1],[3,-1])},0,0,0,0/0,0,0,0,0/0,0,0,0,0/0,0,0,0,0/0,0,0,0,0/";
    ScriptableObjectData[] scriptableObjectDataArray;

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
                    //DontDestroyOnLoad(instance);
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
    [SerializeField] public AnimationCurve EaseInOutQuad = new AnimationCurve();


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
        get { return _gridHeight; }
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

    #region UIManagment

    public GameOverWindow GameOverWindowScreen;
    public VictoryWindow VictoryWindowScreen;
    public bool IsGameOver { get; private set; } = false;

    #endregion

    void Awake()
    {
        #region Singleton pattern

        if (instance == null)
        {
            instance = this;
           // DontDestroyOnLoad(gameObject);
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
    #region Grid Instantiation
    public void InstantiateGrid()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

        cellSize = CalculateCellSize(mainCamera);

        Vector3 offset = new Vector3(
            mainCamera.aspect * mainCamera.orthographicSize * -1 + gridPadding + (cellSize / 2),
            mainCamera.orthographicSize - gridPadding - (cellSize / 2),
            0f
        );

        for (int row = 0; row < _gridHeight; row++)
        {
            for (int col = 0; col < _gridWidth; col++)
            {
                float x = offset.x + col * (cellSize + gridSpacing);
                float y = offset.y - row * (cellSize + gridSpacing);

                Vector3 position = new Vector3(x, y, 0f);

                GameObject cell = Instantiate(tilePrefab, position, Quaternion.identity, GridParent.transform);
                cell.GetComponent<SpriteRenderer>().sprite = GetTileSprite(col, row);
                cell.name = col + " " + row;
                grid.Add(cell);
                cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
            }
        }
    }

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

        return GridTileSprites[y * 3 + x];
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

    #region SeedFetching
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

    public void ProcessSeedString(string seed)
    {
        if (seed == null)
            seed = this.seed;

        string[] subseeds = seed.Split(';', StringSplitOptions.RemoveEmptyEntries);

        // Grid objects seed
        string gridObjectSeed = subseeds[0];
        string[] gridObjectRows = gridObjectSeed.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Scriptable objects with code blocks seed
        string scriptableObjectSeedWithCodeBlocks = subseeds[1];
        string[] scriptableObjectRows = scriptableObjectSeedWithCodeBlocks.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Process code blocks and scriptable objects
        List<ScriptableObjectData> scriptableObjectDataList = new List<ScriptableObjectData>();
        string[,] scriptableObjectData = new string[_gridHeight, _gridWidth];

        foreach (string scriptableObjectRow in scriptableObjectRows)
        {
            // Split the row string into individual elements using commas
            string[] scriptableObjectElements = Regex.Split(scriptableObjectRow, @",(?!.*\])");


            foreach (string element in scriptableObjectElements)
            {
                if (element != "0")
                {
                    // Use regular expressions to extract scriptableObject and codeBlocks
                    Match match = Regex.Match(element, @"^{(.+)}(?:,(\d+))*$");

                    if (match.Success)
                    {
                        string scriptableObject = match.Groups[1].Value.Split(':')[0].Trim();

                        string codeBlocks = match.Groups[1].Value.Split(':')[1].Trim();

                        // Split scriptableObject into its components: ID, facing direction, reference ID
                        string[] scriptableObjectComponents = scriptableObject.Split('-');

                        if (scriptableObjectComponents.Length >= 3)
                        {
                            int scriptableObjectID = int.Parse(scriptableObjectComponents[0]);
                            int facingDirection = int.Parse(scriptableObjectComponents[1]);
                            int referenceID = int.Parse(scriptableObjectComponents[2]);

                            // Extract code blocks for the scriptable object using regular expressions
                            MatchCollection codeBlockMatches = Regex.Matches(codeBlocks, @"\[(-?\d+),(-?\d+)\]");

                            List<BlockTypes> codeBlocksList = new List<BlockTypes>();

                            foreach (Match codeBlockMatch in codeBlockMatches)
                            {
                                if (codeBlockMatch.Groups.Count == 3 &&
                                    int.TryParse(codeBlockMatch.Groups[1].Value, out int codeBlockID) &&
                                    int.TryParse(codeBlockMatch.Groups[2].Value, out int count))
                                {
                                    codeBlocksList.Add(new BlockTypes(codeBlockID, count));
                                } else
                                {
                                    Debug.LogError($"Failed to parse code block ID or count from entry: {codeBlockMatch.Value}");
                                }
                            }

                            BlockTypes[] codeBlocksArray = codeBlocksList.ToArray();

                            // Add the scriptable object data along with code blocks to the list
                            scriptableObjectDataList.Add(new ScriptableObjectData(scriptableObjectID, facingDirection, referenceID, codeBlocksArray));
                        } else
                        {
                            Debug.LogError("Invalid format for scriptable object components: " + scriptableObject);
                        }
                    } else
                    {
                        Debug.LogError("Invalid format for scriptable object element: " + element);
                    }
                } else
                {
                    // No scriptable object in this element, add null to the list
                    scriptableObjectDataList.Add(null);
                }
            }
        }


        // Convert the list to an array
        scriptableObjectDataArray = scriptableObjectDataList.ToArray();

        // Set grid width and height based on seed
        _gridWidth = gridObjectRows[0].Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
        _gridHeight = gridObjectRows.Length;

        // Create a 2D array to store the grid data
        int[,] gridObjectData = new int[_gridHeight, _gridWidth];

        // Iterate over each row
        for (int rowIndex = 0; rowIndex < _gridHeight; rowIndex++)
        {
            // Split the row string by commas to get individual column values for gridObjectData
            string[] gridObjectColumns = gridObjectRows[rowIndex].Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Split the row string into scriptableObjectColumns using the modified Regex.Split
            string[] scriptableObjectColumns = Regex.Split(scriptableObjectRows[rowIndex], @",(?![^{}]*\})");


            // Remove any empty entries
            scriptableObjectColumns = scriptableObjectColumns.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            // Iterate over each column
            for (int colIndex = 0; colIndex < _gridWidth; colIndex++)
            {
                // Parse the string value to an integer and assign it to the 2D array for gridObjectData
                if (int.TryParse(gridObjectColumns[colIndex], out int cellValue))
                {
                    gridObjectData[rowIndex, colIndex] = cellValue;
                } else
                {
                    Debug.LogError("Failed to parse grid data at row " + rowIndex + ", column " + colIndex);
                }

                // Set scriptableObjectData[rowIndex, colIndex] to only the first 3 characters of scriptableObjectColumns[colIndex]
                if (scriptableObjectColumns[colIndex] != "0")
                {
                    scriptableObjectData[rowIndex, colIndex] = scriptableObjectColumns[colIndex].Substring(1, 3);

                } else
                {
                    scriptableObjectData[rowIndex, colIndex] = "0";
                }
                    
            }
        }

        // Now you have the grid data in the 'gridData' array, and you can use it to instantiate the grid
        InstantiateGrid();
        InstantiateGridFromData(gridObjectData, scriptableObjectData);
    }



    void InstantiateGridFromData(int[,] gridObjectData, string[,] scriptableObjectData)
    {

        // Your existing grid instantiation code can be modified to use the provided grid data
        for (int row = 0; row < _gridHeight; row++)
        {
            for (int col = 0; col < _gridWidth; col++)
            {
                // Access gridData[row, col] to get the value for the current cell

                int gridObjectId = gridObjectData[row, col];

                // Use objectId to find the corresponding GridObject
                GameObject gridObject = gridObjects.First(x => x.id == gridObjectId).prefab;//TODO: Add exception handling

                if (gridObject != null && gridObjectId != 0)
                {

                    grid[row * _gridWidth + col].GetComponent<Tile>().OccupyingObject = Instantiate(gridObject, grid[row * _gridWidth + col].transform);

                    if (scriptableObjectData[row, col] != "0")
                    {
                        int scriptableObjectId = Convert.ToInt32(scriptableObjectData[row, col].Split('-')[0]);
                        int rotation = Convert.ToInt32(scriptableObjectData[row, col].Split('-')[1]);

                        GameObject scriptableObject = scriptableObjects.First(x => x.id == scriptableObjectId).prefab;//TODO: Add exception handling
                        grid[row * _gridWidth + col].GetComponent<Tile>().OccupyingObject = Instantiate(scriptableObject, (scriptableObjectId == 1 ? GridParent.transform.parent : grid[row * _gridWidth + col].transform));

                    }
                }
                if (scriptableObjectData[row, col] != "0")
                {
                    int scriptableObjectId = Convert.ToInt32(scriptableObjectData[row, col].Split('-')[0]);
                    int rotation = Convert.ToInt32(scriptableObjectData[row, col].Split('-')[1]);

                    GameObject scriptableObject = scriptableObjects.First(x => x.id == scriptableObjectId).prefab;//TODO: Add exception handling
                    var temp = Instantiate(scriptableObject, (scriptableObjectId == 1 ? GridParent.transform.parent : grid[row * _gridWidth + col].transform));
                    grid[row * _gridWidth + col].GetComponent<Tile>().OccupyingObject = temp;
                    if (scriptableObjectDataArray[row * GridWidth+col] != null)
                    {
                        temp.GetComponent<ICodeable>().Id = scriptableObjectDataArray[row * GridWidth + col].ReferenceID;
                        temp.GetComponent<ICodeable>().GridPosition = new Vector2(col, row);
                        temp.GetComponent<ICodeable>().GridRotation = scriptableObjectDataArray[row * GridWidth + col].FacingDirection;
                        temp.GetComponent<ICodeable>().MyBlockTypes = scriptableObjectDataArray[row * GridWidth + col].CodeBlocks;
                        
                    }
                }
            }
        }
    }

    private Object GetGridObjectById(int objectId)
    {
        return gridObjects.Find(obj => obj.id == objectId);
    }

    #endregion

    #region Menus

    public void GameOver()
    {
        if (IsGameOver)
            return;
        IsGameOver = true;
        GameOverWindowScreen.gameObject.SetActive(true);
        Time.timeScale = 0;
        GameOverWindowScreen.TryAgain.onClick.AddListener(OnTryAgain);
    }

    private void OnTryAgain()
    {
        GameOverWindowScreen.gameObject.SetActive(false);
        Time.timeScale = 1;
        IsGameOver = false;
    }

    public void Victory()
    {
        if(IsGameOver) return;
        IsGameOver = true;
        VictoryWindowScreen.gameObject.SetActive(true);
        VictoryWindowScreen.NextLevel.onClick.AddListener(OnNextLevel);
    }

    private void OnNextLevel()
    {
        VictoryWindowScreen.gameObject.SetActive(false);
        Time.timeScale = 1;
        IsGameOver = false;
    }


    private void ChangeMenu(Menus value)
    {
        OnMenusClose[_currentMenu]?.Invoke();
        _currentMenu = value;
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

    public Vector2 GetCellPos(int x, int y)
    {
        return grid[y*_gridWidth + x].transform.position;
    }

    public bool IsCellWalkable(int x, int y)
    {
        if(x < 0 || y < 0 || x >= _gridWidth || y >= _gridHeight)
            return false;
        Tile _targetTile;
        if (!grid[y * _gridWidth + x].TryGetComponent<Tile>(out _targetTile))
            throw new Exception($"grid {new Vector2(x, y)} has no Tile.cs");
        if (_targetTile.OccupyingObject == null)
            return true;
        GridObjectDataSheet _targetSheet;
        if (!_targetTile.OccupyingObject.TryGetComponent<GridObjectDataSheet>(out _targetSheet))
            throw new Exception($"Grid Object {_targetTile.OccupyingObject}, at {new Vector2(x, y)} does not contains a GridObjectDataSheet");
        if (_targetSheet.CanWalkOver)
            return true;
        return false;
    }

    public void Interact(int x, int y, Action callback)
    {
        if (grid[y * _gridWidth + x].GetComponent<Tile>().OccupyingObject == null)
        {
            callback?.Invoke();
            return;
        }
            
        GridObjectDataSheet _temp = grid[y * _gridWidth + x].GetComponent<Tile>().OccupyingObject.GetComponent<GridObjectDataSheet>();
        if (_temp.IsAutoInteractable == false)
        {
            callback?.Invoke();
            return;
        }

        _temp.AutoInteract.Interact(callback);
    }

    #endregion
}
