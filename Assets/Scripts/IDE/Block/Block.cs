using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    public int Owner;
    public bool ConnectableHere = false;

    GameObject[] _args = null;//TODO: GameObject => DataBlock

    public DrawerBlock Parent;
    public static Action<float> OnResize = (x) => { };
    public Action OnPickup = () => { };

    [SerializeField] GameObject outConnectorsHolder;
    [SerializeField] GameObject inpConnectorsHolder;
    [SerializeField] GameObject argsholder = null;

    public OutputConnectionScript[] outConnectorsScripts { get; internal set; } = null;
    public InputConnector[] inputConnectorsScripts { get; private set; } = null;

    Vector2 dragOffset = Vector2.zero;
    Vector2 lastPos = Vector2.zero;

    Action ForDestroy = null;
    
    internal virtual void Awake()
    {
        if (transform.parent.parent.gameObject.name == "DrawerBlocks")
            return;

        outConnectorsScripts = outConnectorsHolder != null ? outConnectorsHolder.GetComponentsInChildren<OutputConnectionScript>(true) : new OutputConnectionScript[0];
        inputConnectorsScripts = inpConnectorsHolder != null ? inpConnectorsHolder.GetComponentsInChildren<InputConnector>(true) : new InputConnector[0];

        lastPos = transform.parent.position;
        Owner = IDEManager.Instance.CurrentlyProgramedId;
        IDEManager.Instance.OnBlockCreation(transform.parent.gameObject);
        OnResize += MyOnResize;
        outConnectorsScripts.ToList().ForEach(x => OnResize += x.FixPosOnRescale);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DrawerBlockTrasher"))
            ForDestroy = SelfDestruct;
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("DrawerBlockTrasher"))
            ForDestroy = null;
    }
    internal virtual void OnDestroy()
    {
        OnResize -= MyOnResize;
        if (transform.parent.parent.gameObject.name != "DrawerBlocks")
            IDEManager.Instance.OnBlockDestruction(this);
    }
    void OnMouseUp()
    {
        ForDestroy?.Invoke();
        PutDownBlock();
    }
    void OnMouseDown()
    {
        PrepDragAlong();
        OnPickup?.Invoke();
    }
    void OnMouseDrag()
    {
        MoveWithMouse();
    }

    void MyOnResize(float x) =>  transform.parent.localScale = Vector3.one * x;

    public virtual void RunBlock(InputConnector connectorInUse = null)
    {
        foreach (var script in outConnectorsScripts)
        {
            if (script.Connected != null)
                return;
        }

        StartCoroutine(Delay(1f, () => GameManager.Instance.GameOver()));
    }

    IEnumerator Delay(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }

    void DisconnectCurrent()
    {
        if (inputConnectorsScripts.Length == 0)
            return;

        InputConnector inputConnector = inputConnectorsScripts[0]?.GetComponent<InputConnector>();
        if (inputConnector != null)
        {
            OutputConnectionScript outputConnection = inputConnector.Connected;
            if (outputConnection != null)
            {
                outputConnection.Disconnect();
            }
        }
    }

    public void PrepDragAlong()
    {
        dragOffset = GetMousePos() - (Vector2)transform.parent.transform.position;

        foreach (var outConnector in outConnectorsScripts)
        {
            InputConnector connected = outConnector.Connected;
            if (connected != null && connected.Block.inputConnectorsScripts[0] == connected)
            {
                connected.Block.PrepDragAlong();
            }
        }
    }

    public void ConnectIfPossible()
    {
        foreach (var item in outConnectorsScripts)
        {
            if (item.Connect())
                item.FixPositionInStack();
        }

        foreach (var outConnector in outConnectorsScripts)
        {
            InputConnector connected = outConnector?.Connected;
            if (connected != null && connected.Block.inputConnectorsScripts[^1] != connected)
            {
                connected.Block.ConnectIfPossible();
            }
        }

        if (inputConnectorsScripts.Length == 0)
            return;
        foreach (var inpConnector in inputConnectorsScripts)
        {
            if(inpConnector.GetComponent<InputConnector>().ForConnection != null)
            {
                if (inpConnector.GetComponent<InputConnector>().ForConnection.Connect())
                {
                    inpConnector.GetComponent<InputConnector>().Connected.FixPositionInStack();
                }
            }
        }
        
    }

    void SelfDestruct()
    {
        Parent.Count++;
        foreach (var item in outConnectorsScripts)
        {
            if(item.Connected != null)
            {
                item.Connected.Block.SelfDestruct();
            }
        }

        Destroy(transform.parent.gameObject);
    }

    public void PutDownBlock()
    {
        if (CheckColliderOutOfScreenSpace(1.3f) || !IDEManager.Instance.CheckPlacingPositionY(this))
        {
            transform.parent.position = lastPos;
            return;
        }

        DisconnectCurrent();
        ConnectIfPossible();

        foreach (var outConnector in outConnectorsScripts)
        {
            if (outConnector != null)
            {
                if (outConnector.Connected != null)
                    outConnector.Connected.Block.ConnectIfPossible();
            }
        }

        lastPos = transform.parent.position;
        IDEManager.Instance.OnBlockRepositioning(this);
    }

    public void MoveWithMouse()
    {
        float tempZ = transform.parent.position.z;
        transform.parent.position = GetMousePos() - dragOffset;
        transform.parent.position = new Vector3(transform.parent.position.x, transform.parent.position.y, tempZ);

        foreach (var outConnector in outConnectorsScripts)
        {
            Block block = outConnector?.Connected?.Block;
            if (block != null && block.inputConnectorsScripts?[0] == outConnector.Connected)
                block.MoveWithMouse();
        }
    }

    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint( Input.mousePosition );
    }

    bool CheckColliderOutOfScreenSpace(float tolerance)
    {
        
        if (TryGetComponent<BoxCollider2D>(out var collider))
        {
            Vector3 screenBoundsMin = Camera.main.ScreenToWorldPoint(Vector3.zero);
            Vector3 screenBoundsMax = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

            Bounds colliderBounds = collider.bounds;

            return !IsBoundsWithinScreen(colliderBounds, screenBoundsMin, screenBoundsMax, tolerance);
        }
        else
        {
            Debug.LogError("Collider component not found!");
            return false;
        }
    }

    bool IsBoundsWithinScreen(Bounds bounds, Vector3 screenBoundsMin, Vector3 screenBoundsMax, float tollerance)
    {
        return bounds.min.x + tollerance >= screenBoundsMin.x && bounds.max.x - tollerance <= screenBoundsMax.x;
    }

    
}
