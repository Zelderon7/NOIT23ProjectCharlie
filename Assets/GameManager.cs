using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Object {
    public int Id;
    public string ObjectName;
    public GameObject Prefab;

    public Object(int id, string objectName, GameObject prefab)
    {
        Id = id;
        ObjectName = objectName;
        Prefab = prefab;
    }
}

public class ScriptableObjectData {

    public int Id
    {
        get;
    }

    public int FacingDirection
    {
        get;
    }

    public int ReferenceId
    {
        get;
    }

    public BlockTypes[] CodeBlocks
    {
        get;
    }

    public ScriptableObjectData(int id, int facingDirection, int referenceID, BlockTypes[] codeBlocks)
    {
        Id = id;
        FacingDirection = facingDirection;
        ReferenceId = referenceID;
        CodeBlocks = codeBlocks;
    }
}

public class GameManager : MonoBehaviour {

    internal struct GridObjectConnections {
        public int X, Y, X1, Y1;

        public GridObjectConnections(int x, int y, int x1, int y1)
        {
            X = x;
            Y = y;
            X1 = x1;
            Y1 = y1;
        }

        public override string ToString()
        {
            return $"KeyX: {X}; KeyY: {Y}; DoorX: {X1}; DoorY{Y1}";
        }
    }

    public AnimationCurve EaseInOutQuad = new AnimationCurve();

    public enum Menus {
        Game,
        IDE,
        Pause,
        Settings,
    }

    private Menus _currentMenu = Menus.Game;

    public GameObject IDEScreen
    {
        get
        {
            return _IDEScreen;
        }
    }

    public GameObject GameScreen
    {
        get
        {
            return _GameScreen;
        }
    }

    private bool _curMenuChangable = true;
    private string seed;
    private string levelName;
    private string authorName;
    private float _gridXRepos = 2.15f;
    private float _gridYRepos = 1.2f;
    public float gridSpacing = 0.2f;
    private bool FetchDataFlag = false;

    [SerializeField] private float gridPadding = 0;
    [SerializeField] private int _gridWidth, _gridHeight;

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject GridParent;
    [SerializeField] private GameObject _GameScreen;
    [SerializeField] private GameObject _IDEScreen;

    [SerializeField] private GameOverWindow GameOverWindowScreen;
    [SerializeField] private VictoryWindow VictoryWindowScreen;

    [SerializeField] private List<Sprite> GridTileSprites;
    [SerializeField] private List<Object> gridObjects = new List<Object>();
    [SerializeField] private List<Object> scriptableObjects = new List<Object>();

    [SerializeField] private Button StartCodeButton;

    private ScriptableObjectData[] _scriptableObjectDataArray;
    private List<GameObject> grid = new List<GameObject>();

    public int GridWidth
    {
        get
        {
            return _gridWidth;
        }
        set
        {
            _gridWidth = value;
        }
    }

    public int GridHeight
    {
        get
        {
            return _gridHeight;
        }
        set
        {
            _gridHeight = value;
        }
    }

    public float CellSize
    {
        get; private set;
    }

    private List<GridObjectConnections> _gridObjectsConnections = new List<GridObjectConnections>();
    private GameObject _robot;

    public bool IsGameOver { get; private set; } = false;

    public Menus CurrentMenu
    {
        get
        {
            return _currentMenu;
        }
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

    #region Singleton pattern

    private static GameManager instance;

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
                }
            }
            return instance;
        }
    }

    #endregion Singleton pattern

    private void Awake()
    {
        #region Singleton pattern

        if (instance == null)
        {
            instance = this;
        } else if (instance != this)
        {
            Destroy(gameObject);
        }

        #endregion Singleton pattern

        #region Menus

        OnMenusClose.Clear();
        OnMenusOpen.Clear();
        foreach (var item in Enum.GetValues(typeof(Menus)))
        {
            OnMenusClose.Add((Menus)item, new Action(() => { }));
            OnMenusOpen.Add((Menus)item, new Action(() => { }));
        }

        OnMenusOpen[Menus.IDE] += OnIDEOpen;
        OnMenusClose[Menus.IDE] += OnIDEClose;

        #endregion Menus

        InstantiateGrid();
        if (seed != "")
            ProcessSeedString(seed);
    }

    #region Grid Instantiation

    public void InstantiateGrid()
    {
        if (_robot != null)
            Destroy(_robot);

        grid.ForEach(item => { Destroy(item); });

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

        CellSize = CalculateCellSize(mainCamera);

        Vector3 offset = new Vector3(
            mainCamera.aspect * mainCamera.orthographicSize * -1 + gridPadding + (CellSize / 2),
            mainCamera.orthographicSize - gridPadding - (CellSize / 2),
            0f
        );

        grid.Clear();

        for (int row = 0; row < _gridHeight; row++)
        {
            for (int col = 0; col < GridWidth; col++)
            {
                float x = offset.x + col * (CellSize + gridSpacing);
                float y = offset.y - row * (CellSize + gridSpacing);

                Vector3 position = new Vector3(x, y, 0f);

                GameObject cell = Instantiate(tilePrefab, position, Quaternion.identity, GridParent.transform);
                cell.GetComponent<SpriteRenderer>().sprite = GetTileSprite(col, row);
                cell.name = col + " " + row;
                grid.Add(cell);
                cell.transform.localScale = new Vector3(CellSize, CellSize, 1f);
            }
        }
    }

    private Sprite GetTileSprite(int col, int row)
    {
        int x, y;
        if (col == 0)
            x = 0;
        else if (col == GridWidth - 1)
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

    private float CalculateCellSize(Camera camera)
    {
        float cameraHeight = 2f * camera.orthographicSize;
        float cameraWidth = cameraHeight * camera.aspect;

        cameraWidth -= _gridXRepos;
        cameraHeight -= _gridYRepos;

        float cellSizeY = (cameraHeight - (2 * gridPadding) - (gridSpacing * (_gridHeight - 1))) / _gridHeight;
        float cellSizeX = (cameraWidth - (2 * gridPadding) - (gridSpacing * (GridWidth - 1))) / GridWidth;

        return Mathf.Min(cellSizeX, cellSizeY);
    }

    #endregion Grid Instantiation

    #region SeedFetching

    public void FetchData(string data)
    {
        if (FetchDataFlag)
            return;
        FetchDataFlag = true;
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
            } else
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
        _gridObjectsConnections.Clear();
        Debug.Log($"Processing seed: {seed}");

        seed ??= this.seed;

        string[] subseeds = seed.Split(';', StringSplitOptions.RemoveEmptyEntries);

        string gridObjectSeed = subseeds[0];
        string[] gridObjectRows = gridObjectSeed.Split('/', StringSplitOptions.RemoveEmptyEntries);

        string scriptableObjectSeedWithCodeBlocks = subseeds[1];
        string[] scriptableObjectRows = scriptableObjectSeedWithCodeBlocks.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Process code blocks and scriptable objects
        List<ScriptableObjectData> scriptableObjectDataList = new List<ScriptableObjectData>();
        string[,] scriptableObjectData = new string[_gridHeight, GridWidth];

        foreach (string scriptableObjectRow in scriptableObjectRows)
        {
            string[] scriptableObjectElements = Regex.Split(scriptableObjectRow, @",(?!.*\])");

            foreach (string element in scriptableObjectElements)
            {
                if (element != "0")
                {
                    Match match = Regex.Match(element, @"^{(.+)}(?:,(\d+))*$");

                    if (match.Success)
                    {
                        string scriptableObject = match.Groups[1].Value.Split('#')[0].Trim();

                        string codeBlocks = match.Groups[1].Value.Split('#')[1].Trim();

                        string[] scriptableObjectComponents = scriptableObject.Split('-');

                        if (scriptableObjectComponents.Length >= 3)
                        {
                            int scriptableObjectID = int.Parse(scriptableObjectComponents[0]);
                            int facingDirection = int.Parse(scriptableObjectComponents[1]);
                            int referenceID = int.Parse(scriptableObjectComponents[2]);

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
                    scriptableObjectDataList.Add(null);
                }
            }
        }

        _scriptableObjectDataArray = scriptableObjectDataList.ToArray();

        GridWidth = Regex.Split(gridObjectRows[0], @",(?![^{]*\})").Length;
        GridHeight = gridObjectRows.Length;

        int[,] gridObjectData = new int[_gridHeight, GridWidth];

        for (int rowIndex = 0; rowIndex < _gridHeight; rowIndex++)
        {
            string[] gridObjectColumns = Regex.Split(gridObjectRows[rowIndex], @",(?![^{]*\})");
            string[] scriptableObjectColumns = Regex.Split(scriptableObjectRows[rowIndex], @",(?![^{}]*\})");

            scriptableObjectColumns = scriptableObjectColumns.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            for (int colIndex = 0; colIndex < GridWidth; colIndex++)
            {
                if (gridObjectColumns[colIndex] != null & Regex.IsMatch(gridObjectColumns[colIndex], @"^{3-\[\d+,\d+\]}$"))
                {
                    Match coordMatch = Regex.Match(gridObjectColumns[colIndex], @"{3-\[(\d+),(\d+)\]}");
                    if (coordMatch.Success)
                    {
                        int doorCoordX = int.Parse(coordMatch.Groups[1].Value);
                        int doorCoordY = int.Parse(coordMatch.Groups[2].Value);

                        _gridObjectsConnections.Add(new GridObjectConnections(colIndex, rowIndex, doorCoordX, doorCoordY));
                        gridObjectData[rowIndex, colIndex] = 3;
                    }
                } else
                {
                    if (int.TryParse(gridObjectColumns[colIndex], out int cellValue))
                    {
                        gridObjectData[rowIndex, colIndex] = cellValue;
                    } else
                    {
                        Debug.LogError("Failed to parse grid data at row " + rowIndex + ", column " + colIndex);
                    }
                }

                if (scriptableObjectColumns[colIndex] != "0")
                {
                    scriptableObjectData[rowIndex, colIndex] = scriptableObjectColumns[colIndex].Substring(1, 3);
                } else
                {
                    scriptableObjectData[rowIndex, colIndex] = "0";
                }
            }
        }
        InstantiateGridFromData(gridObjectData, scriptableObjectData);
    }

    private void DoorConnect()
    {
        foreach (var cur in _gridObjectsConnections)
        {
            Tile _keyTile = grid[cur.Y * GridWidth + cur.X].GetComponent<Tile>();
            Tile _doorTile = grid[cur.Y1 * GridWidth + cur.X1].GetComponent<Tile>();

            if (_keyTile.OccupyingObject == null)
            {
                throw new Exception("Invalid Key Coords " + cur);
            }
            if (!_keyTile.OccupyingObject.TryGetComponent(out Key _targetKey))
            {
                throw new Exception("Invalid Key Coords " + cur);
            }

            if (_doorTile.OccupyingObject == null)
            {
                throw new Exception("Invalid Door Coords " + cur);
            }
            if (!_doorTile.OccupyingObject.TryGetComponent(out Door _targetdoor))
            {
                throw new Exception("Invalid Door Coords " + cur);
            }

            _targetKey.Door = _targetdoor;
        }
    }

    private void InstantiateGridFromData(int[,] gridObjectData, string[,] scriptableObjectData)
    {
        for (int row = 0; row < _gridHeight; row++)
        {
            for (int col = 0; col < GridWidth; col++)
            {
                int gridObjectId = gridObjectData[row, col];
                GameObject gridObject = gridObjects.First(x => x.Id == gridObjectId).Prefab;

                if (gridObject != null && gridObjectId != 0)
                {
                    grid[row * GridWidth + col].GetComponent<Tile>().OccupyingObject = Instantiate(gridObject, grid[row * GridWidth + col].transform);

                    if (scriptableObjectData[row, col] != "0")
                    {
                        int scriptableObjectId = Convert.ToInt32(scriptableObjectData[row, col].Split('-')[0]);
                        int rotation = Convert.ToInt32(scriptableObjectData[row, col].Split('-')[1]);

                        GameObject scriptableObject = scriptableObjects.First(x => x.Id == scriptableObjectId).Prefab;
                        grid[row * GridWidth + col].GetComponent<Tile>().OccupyingObject = Instantiate(scriptableObject, (scriptableObjectId == 1 ? GridParent.transform.parent : grid[row * GridWidth + col].transform));
                    }
                }
                if (scriptableObjectData[row, col] != "0")
                {
                    int scriptableObjectId = Convert.ToInt32(scriptableObjectData[row, col].Split('-')[0]);
                    int rotation = Convert.ToInt32(scriptableObjectData[row, col].Split('-')[1]);

                    GameObject scriptableObject = scriptableObjects.First(x => x.Id == scriptableObjectId).Prefab;
                    GameObject temp;

                    if (scriptableObjectId == 1)
                    {
                        temp = Instantiate(scriptableObject, (GridParent.transform.parent));
                        _robot = temp;
                    } else
                    {
                        temp = Instantiate(scriptableObject, grid[row * GridWidth + col].transform);
                        grid[row * GridWidth + col].GetComponent<Tile>().OccupyingObject = temp;
                    }

                    if (_scriptableObjectDataArray[row * GridWidth + col] != null)
                    {
                        temp.GetComponent<ICodeable>().Id = _scriptableObjectDataArray[row * GridWidth + col].ReferenceId;
                        temp.GetComponent<ICodeable>().GridPosition = new Vector2(col, row);
                        temp.GetComponent<ICodeable>().GridRotation = _scriptableObjectDataArray[row * GridWidth + col].FacingDirection;
                        temp.GetComponent<ICodeable>().BlockTypes = _scriptableObjectDataArray[row * GridWidth + col].CodeBlocks;
                    }
                }
            }
        }

        DoorConnect();
    }

    private Object GetGridObjectById(int objectId)
    {
        return gridObjects.Find(obj => obj.Id == objectId);
    }

    #endregion SeedFetching

    #region Menus

    public void GameOver()
    {
        if (IsGameOver)
            return;
        IsGameOver = true;
        GameOverWindowScreen.gameObject.SetActive(true);
        Time.timeScale = 0;
        GameOverWindowScreen.TryAgain.onClick.AddListener(() => OnTryAgain(GameOverWindowScreen.TryAgain));
    }

    public void Victory()
    {
        if (IsGameOver)
            return;
        IsGameOver = true;
        StartCoroutine(VictoryCoroutine());
    }

    public void SwitchGameIDE()
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

    private void OnTryAgain(Button caller)
    {
        caller.transform.parent.gameObject.SetActive(false);
        InstantiateGrid();
        if (seed != null)
            ProcessSeedString(seed);
        StartCodeButton.GetComponent<Button>().enabled = true;
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
    }

    private void OnIDEClose()
    {
        IDEManager.Instance.OnClose();
        StartCoroutine(MoveTransform(IDEScreen.transform, new Vector3(-18f, 0, 0), 1.5f));
        //StartCoroutine(MoveTransform(GameScreen.transform, Vector3.zero, 1.5f));
    }

    private IEnumerator VictoryCoroutine()
    {
        yield return new WaitForSeconds(.5f);
        VictoryWindowScreen.gameObject.SetActive(true);
        CommunicationManager.SendDataMethod("Victory");
        VictoryWindowScreen.TryAgain.onClick.AddListener(() => OnTryAgain(VictoryWindowScreen.TryAgain));
    }

    #endregion Menus

    #region Grid Methods

    public Vector2 GetCellPos(int x, int y)
    {
        return grid[y * GridWidth + x].transform.position;
    }

    public bool IsCellWalkable(int x, int y)
    {
        if (x < 0 || y < 0 || x >= GridWidth || y >= _gridHeight)
            return false;
        if (!grid[y * GridWidth + x].TryGetComponent(out Tile _targetTile))
            throw new Exception($"grid {new Vector2(x, y)} has no Tile.cs");
        if (_targetTile.OccupyingObject == null)
            return true;
        if (!_targetTile.OccupyingObject.TryGetComponent(out GridObjectDataSheet _targetSheet))
            throw new Exception($"Grid Object {_targetTile.OccupyingObject}, at {new Vector2(x, y)} does not contains a GridObjectDataSheet");
        if (_targetSheet.CanWalkOver)
            return true;
        return false;
    }

    public void Interact(int x, int y, Action callback)
    {

        if (x < 0 || x >= GridWidth || y < 0 || y >= _gridHeight)
        {
            callback?.Invoke();
            return;
        }

        if (grid[y * GridWidth + x].GetComponent<Tile>().OccupyingObject == null)
        {
            callback?.Invoke();
            return;
        }

        GridObjectDataSheet _temp = grid[y * GridWidth + x].GetComponent<Tile>().OccupyingObject.GetComponent<GridObjectDataSheet>();


        if (_temp.IsAutoInteractable == false)
        {
            callback?.Invoke();
            return;
        }

        _temp.AutoInteract.Interact(callback);
    }

    #endregion Grid Methods
}