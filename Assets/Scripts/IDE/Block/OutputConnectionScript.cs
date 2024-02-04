using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OutputConnectionScript : MonoBehaviour
{
    public InputConnector connected { get; private set; } = null;
    InputConnector forConnection = null;
    [SerializeField] Block myBlock;
    public bool GoNext()
    {
        if (connected == null)
            return false;

        connected.CallMe();
        return true;
    }

    public bool Connect()
    {
        if (forConnection == null)
            return false;

        if (forConnection.Connected != null)
            return false;

        Vector3 tPos = forConnection.gameObject.transform.position;
        Vector3 myPos = gameObject.transform.position;
        myBlock.transform.parent.position += new Vector3(tPos.x - myPos.x, tPos.y - myPos.y, 0);

        connected = forConnection;
        forConnection = null;

        connected.Connected = this;

        RearrangeChildrenAfterTarget(connected.transform.parent);

        return true;
    }

    public void FixPositionInStack()
    {
        if(connected == null)
            return;

        Vector3 tPos = connected.gameObject.transform.position;
        Vector3 myPos = gameObject.transform.position;
        myBlock.transform.parent.position += new Vector3(tPos.x - myPos.x, tPos.y - myPos.y, 0);

        InputConnector myConnector = myBlock.transform.parent.gameObject.GetComponentInChildren<InputConnector>();
        if (myConnector == null)
            return;

        myConnector.Connected?.FixPositionInStack();
    }

    public void FixPosOnRescale(float newScale)
    {
        if (connected != null) return;
        if(myBlock.InputConnector != null)
        {
            if(myBlock.InputConnector.GetComponent<InputConnector>().Connected != null)
            {
                myBlock.InputConnector.GetComponent<InputConnector>().Connected.FixPositionInStack();
            }
        }
    }

    public void Disconnect()
    {
        if (connected == null) return;

        //forConnection = connected;
        connected.Connected = null;
        connected = null;
        //myBlock.ConnectableHere = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (connected != null)
            return;

        if(collision.tag == "inputConnector")
        {
            myBlock.ConnectableHere = true;
            forConnection = collision.GetComponent<InputConnector>();
            collision.GetComponent<InputConnector>().forConnection = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "inputConnector")
        {
            myBlock.ConnectableHere = false;
            forConnection = null;
            collision.GetComponent<InputConnector>().forConnection = null;
        }
    }

    void RearrangeChildrenAfterTarget(Transform target)
    {
        Transform parentTransform = transform.parent.parent.parent;
        int targetIndex = target.GetSiblingIndex();

        // Store the reference of the object to be moved
        Transform objectToMove = transform.parent.parent;

        target = target.parent.parent;

        // If the object to move is already after the target, no need to rearrange
        if (objectToMove.GetSiblingIndex() > targetIndex)
        {
            return;
        }

        // Detach the object to move from its parent temporarily
        objectToMove.SetParent(null);

        // Get the new index of the object to be moved after repositioning
        int newIndex = targetIndex;

        

        // Reattach the object to move back to its parent
        objectToMove.SetParent(parentTransform);
        
        // Reposition the object to be moved after the target
        objectToMove.SetSiblingIndex(newIndex);
    }

    void OnDestroy()
    {
        Block.OnResize -= FixPosOnRescale;
        Disconnect();
    }
}
