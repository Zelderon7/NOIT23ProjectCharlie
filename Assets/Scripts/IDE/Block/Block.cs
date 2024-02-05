using System;
using System.Linq;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Owner;
    public bool ConnectableHere = false;

    GameObject[] _args = null;//TODO: GameObject => DataBlock

    public DrawerBlock Parent;
    public static Action<float> OnResize = (x) => { };
    public Action OnPickup = () => { };



    [SerializeField] GameObject outConnectorsHolder;
    [SerializeField] GameObject inpConnector = null;
    [SerializeField] GameObject argsholder = null;

    public GameObject InpConnector { get => inpConnector; }
    internal OutputConnectionScript[] _outConnectorsScripts = null;

    Vector2 dragOffset = Vector2.zero;
    Vector2 lastPos = Vector2.zero;

    Action ForDestroy = null;
    
    internal virtual void Awake()
    {
        if (transform.parent.parent.gameObject.name == "DrawerBlocks")
            return;

        _outConnectorsScripts = outConnectorsHolder?.GetComponentsInChildren<OutputConnectionScript>();
        lastPos = transform.parent.position;
        Owner = IDEManager.Instance.CurrentlyProgramedId;
        IDEManager.Instance.OnBlockCreation(transform.parent.gameObject);
        OnResize += MyOnResize;
        _outConnectorsScripts.ToList().ForEach(x => OnResize += x.FixPosOnRescale);
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
    void OnDestroy()
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

    public virtual void RunBlock()
    {
        foreach (var script in _outConnectorsScripts)
        {
            if (script.Connected != null)
                return;
        }
        GameManager.Instance.GameOver();
    }

    void DisconnectCurrent()
    {
        if(inpConnector == null)
            return;

        InputConnector inputConnector = inpConnector?.GetComponent<InputConnector>();
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

        foreach (var outConnector in _outConnectorsScripts)
        {
            outConnector?.Connected?.Block.PrepDragAlong();
        }
    }

    public void ConnectIfPossible()
    {
        foreach (var item in _outConnectorsScripts)
        {
            if (item.Connect())
                item.FixPositionInStack();
        }

        foreach (var outConnector in _outConnectorsScripts)
        {
            outConnector?.Connected?.Block.ConnectIfPossible();
        }

        if (inpConnector == null)
            return;

        if(inpConnector.GetComponent<InputConnector>().ForConnection != null)
        {
            if (inpConnector.GetComponent<InputConnector>().ForConnection.Connect())
            {
                inpConnector.GetComponent<InputConnector>().Connected.FixPositionInStack();
            }
        }
    }

    void SelfDestruct()
    {
        Parent.Count++;
        foreach (var item in _outConnectorsScripts)
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

        foreach (var outConnector in _outConnectorsScripts)
        {
            outConnector?.Connected?.Block.ConnectIfPossible();
        }

        lastPos = transform.parent.position;
        IDEManager.Instance.OnBlockRepositioning(this);
    }

    public void MoveWithMouse()
    {
        transform.parent.transform.position = GetMousePos() - dragOffset;

        foreach (var outConnector in _outConnectorsScripts)
        {
            outConnector?.Connected?.Block.MoveWithMouse();
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
