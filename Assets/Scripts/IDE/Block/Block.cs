using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    public ICodeable Owner;

    [SerializeField]
    private GameObject inpConnector = null;
    Block connectedIn = null;

    [SerializeField]
    private GameObject outConnectorsHolder;
    public bool ConnectableHere = false;
    internal OutputConnectionScript[] outConnectorsScripts = null;

    [SerializeField]
    private GameObject argsholder = null;
    private GameObject[] args = null;//TODO: GameObject => DataBlock
    private Vector2 dragOffset = Vector2.zero;
    private Vector2 lastPos = Vector2.zero;
    

    internal virtual void Awake()
    {
        outConnectorsScripts = outConnectorsHolder?.GetComponentsInChildren<OutputConnectionScript>();
        lastPos = transform.parent.position;
        Owner = IDEManager.Instance.CurrentlyProgramed;
    }

    private void Start()
    {
        IDEManager.Instance.OnBlockCreation(transform.parent.gameObject);
    }

    public virtual void RunBlock()
    {

    }

    private void OnMouseDown()
    {
        PrepDragAlong();
    }

    private void DisconnectCur()
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
                //Debug.Log("Disconnected sucsesfully");
            }
        }
    }

    public void PrepDragAlong()
    {
        dragOffset = GetMousePos() - (Vector2)transform.parent.transform.position;

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.PrepDragAlong();
        }
    }

    /// <summary>
    /// Should only be called from the MouseUp() or on the MouseUp() on connected which is dragging the current
    /// Checks for possible connection and connects to it.
    /// </summary>
    public void ConnectIfPossible()
    {
        foreach (var item in outConnectorsScripts)
        {
            if (item.Connect())
                item.FixPositionInStack();
        }

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.ConnectIfPossible();
        }

        if (inpConnector == null)
            return;

        if(inpConnector.GetComponent<InputConnector>().forConnection != null)
        {
            if (inpConnector.GetComponent<InputConnector>().forConnection.Connect())
            {
                inpConnector.GetComponent<InputConnector>().Connected.FixPositionInStack();
            }
        }
    }

    private void OnMouseUp()
    {
        PutDownBlock();
    }

    public void PutDownBlock()
    {
        if (CheckColliderOutOfScreenSpace(1.3f))
        {
            transform.parent.position = lastPos;
            return;
        }

        DisconnectCur();
        ConnectIfPossible();

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.ConnectIfPossible();
        }

        lastPos = transform.parent.position;
    }

    private void OnMouseDrag()
    {
        MoveWithMouse();
    }

    /// <summary>
    /// Should only be called from the MouseDrag() or on the MouseDrag() on connected which is dragging the current
    /// Moves the block based of the cursor's position and the <see cref="dragOffset"/>
    /// </summary>
    public void MoveWithMouse()
    {
        transform.parent.transform.position = GetMousePos() - dragOffset;

        foreach (var outConnector in outConnectorsScripts)
        {
            outConnector?.connected?.myBlock.MoveWithMouse();
        }
    }

    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint( Input.mousePosition );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if it's out of bounds</returns>
    bool CheckColliderOutOfScreenSpace(float tollerance)
    {
        Collider2D collider = GetComponent<BoxCollider2D>();

        if (collider != null)
        {
            // Calculate the screen bounds in world space
            Vector3 screenBoundsMin = Camera.main.ScreenToWorldPoint(Vector3.zero);
            Vector3 screenBoundsMax = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

            // Calculate the bounds of the collider in world space
            Bounds colliderBounds = collider.bounds;

            // Check if the collider bounds are even slightly out of the screen
            return !IsBoundsWithinScreen(colliderBounds, screenBoundsMin, screenBoundsMax, tollerance);
        }
        else
        {
            Debug.LogError("Collider component not found!");
            return false;
        }
    }

    bool IsBoundsWithinScreen(Bounds bounds, Vector3 screenBoundsMin, Vector3 screenBoundsMax, float tollerance)
    {
        // Check if the bounds are within the screen bounds
        //Debug.Log($"Bounds: {bounds.max.x}X, {bounds.max.y}Y");
        return bounds.min.x + tollerance >= screenBoundsMin.x && bounds.max.x - tollerance <= screenBoundsMax.x;
    }
}
