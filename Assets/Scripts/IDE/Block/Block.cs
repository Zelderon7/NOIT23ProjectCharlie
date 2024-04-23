using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Block : MonoBehaviour, IEnumerable<Block>
{
    public int Owner = -1;
    public bool ConnectableHere = false;

    GameObject[] _args = null;//TODO: GameObject => DataBlock

    public DrawerBlock Parent;
    public static Action<float> OnResize = (x) => { };
    public Action OnPickup = () => { };
    public bool IsPickedUp { get; private set; }

    public virtual float MySize 
    { 
        get
        {
            //return IDEManager.Instance.BlockSize;
            return 1;
        } 
    }

    public float StackSize
    {
        get
        {
            if (outConnectorsScripts[^1].Connected != null && outConnectorsScripts[^1].Connected.IsPrimary)
                return outConnectorsScripts[^1].Connected.Block.MySize + outConnectorsScripts[^1].Connected.Block.StackSize;
            return 0;
        }
    }

    public Block StackHead 
    { 
        get
        {
            if (inputConnectorsScripts.Length > 0)
            {
                if (inputConnectorsScripts[0].Connected != null && inputConnectorsScripts[0].Connected == inputConnectorsScripts[0].Connected.Block.outConnectorsScripts[^1])
                {
                    return inputConnectorsScripts[0].Connected.Block.StackHead;
                }
            }
            return this;
        } 
    }

    public Block StackBottom
    {
        get
        {
            if (outConnectorsScripts.Length > 0)
            {
                if (outConnectorsScripts[^1].Connected != null && outConnectorsScripts[^1].Connected.IsPrimary)
                {
                    return outConnectorsScripts[^1].Connected.Block.StackBottom;
                }
            }
            return this;
        }
    }

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
        if(IDEManager.Instance != null)
        {
            Owner = IDEManager.Instance.CurrentlyProgramedId;
            IDEManager.Instance.OnBlockCreation(transform.parent.gameObject);
            OnResize += MyOnResize;
        }
    }
    internal void Start()
    {
        if(Owner == -1)
        {
            Owner = IDEManager.Instance.CurrentlyProgramedId;
            IDEManager.Instance.OnBlockCreation(transform.parent.gameObject);
            OnResize += MyOnResize;
        }
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
            IDEManager.Instance?.OnBlockDestruction(this);
    }
    void OnMouseUp()
    {
        ForDestroy?.Invoke();
        PutDownBlock();
    }
    void OnMouseDown()
    {
        DisconnectCurrent();
        DisconnectBottomBlock();
        StackHead.outConnectorsScripts[^1]?.Connected?.FixPositionInStack();
        PrepDragAlong();
        OnPickup?.Invoke();
    }
    void OnMouseDrag()
    {
        MoveWithMouse();
    }

    void MyOnResize(float x)
    {
        transform.parent.localScale = Vector3.one * x;
        StackHead.outConnectorsScripts.ToList().ForEach(x => { x.Connected?.FixPositionInStack(); });
    }

    public abstract void RunBlock(InputConnector connectorInUse = null);

    IEnumerator Delay(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }

    void DisconnectCurrent()
    {
        if (inputConnectorsScripts.Length == 0)
            return;

        InputConnector inputConnector = inputConnectorsScripts[0];
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
        // Calculate the drag offset for the current block
        CalculateDragOffset();

        IsPickedUp = true;

        // Propagate the PrepDragAlong call down the stack
        PropagatePrepDragAlong();
    }

    void CalculateDragOffset()
    {
        dragOffset = GetMousePos() - (Vector2)transform.parent.transform.position;
    }

    void DisconnectBottomBlock()
    {
        // Find the bottom block of the stack
        Block bottomBlock = this;
        if (bottomBlock.outConnectorsScripts.Length == 0)
            return;

        while (bottomBlock.outConnectorsScripts[^1].Connected != null && 
             bottomBlock.outConnectorsScripts[^1].Connected.IsPrimary)
        {
            bottomBlock = bottomBlock.outConnectorsScripts[^1].Connected.Block;
            if (bottomBlock.outConnectorsScripts.Length == 0)
                return;
        }

        // Disconnect the bottom output connector if it's connected
        bottomBlock.outConnectorsScripts[^1].Disconnect();
    }

    void PropagatePrepDragAlong()
    {
        // If this block is connected to another block below it, call PrepDragAlong on that block
        foreach (var outConnector in outConnectorsScripts)
        {
            if (outConnector.Connected != null && outConnector.Connected.Block.inputConnectorsScripts[0].Connected == outConnector)//The second check is to prevent stack overflow on case the block is placed inside of an extended block
            {
                outConnector.Connected.Block.PrepDragAlong();
            }
        }
    }

    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void ConnectIfPossible()
    {
        foreach (var item in outConnectorsScripts)
        {
            if (item.Connect())
                item.Connected.FixPositionInStack();
        }

        //propagates the ConnectIfPossible to the rest of the stack
        foreach (var outConnector in outConnectorsScripts)
        {
            InputConnector connected = outConnector.Connected;
            if (connected != null && connected.Block.inputConnectorsScripts[^1] != connected)//The second check is to prevent stack overflow on case the block is placed inside of an extended block
            {
                connected.Block.ConnectIfPossible();
            }
        }

        if (inputConnectorsScripts.Length == 0)
            return;

        foreach (var inpConnector in inputConnectorsScripts)
        {
            if(inpConnector.ForConnection != null)
            {
                if (inpConnector.ForConnection.Connect())
                    inpConnector.FixPositionInStack();
            }

        }
        
    }

    void SelfDestruct()
    {
        Parent.Count++;
        foreach (var item in outConnectorsScripts)
        {
            if(item.Connected != null && item.Connected.IsPrimary)
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
            Debug.LogError("TODO: Move in bounds");
            return;
        }

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
        IsPickedUp = false;
    }

    public void MoveWithMouse()
    {
        float tempZ = transform.parent.position.z;
        transform.parent.position = GetMousePos() - dragOffset;
        transform.parent.position = new Vector3(transform.parent.position.x, transform.parent.position.y, tempZ);

        foreach (var outConnector in outConnectorsScripts)
        {
            Block block = outConnector?.Connected?.Block;
            if (block != null && block.inputConnectorsScripts[0] == outConnector.Connected)//second check is to prevent stack overflow if placed inside an extended block.
                block.MoveWithMouse();
        }
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

    IEnumerator<Block> IEnumerable<Block>.GetEnumerator()
    {
        return new BlockEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
