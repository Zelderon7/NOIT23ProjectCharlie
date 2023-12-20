using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputConnectionScript : MonoBehaviour
{
    public InputConnector connected { get; private set; } = null;
    InputConnector forConnection = null;
    [SerializeField] Block myBlock;
    public Action GoNext;

    public bool Connect()
    {
        if (forConnection == null)
            return false;

        Vector3 tPos = forConnection.gameObject.transform.position;
        Vector3 myPos = gameObject.transform.position;
        myBlock.transform.position += new Vector3(tPos.x - myPos.x, tPos.y - myPos.y, 0);

        connected = forConnection;
        forConnection = null;

        connected.Connected = myBlock;
        GoNext += connected.CallMe;

        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (connected != null)
            return;

        if(collision.tag == "inputConnector")
        {
            myBlock.ConnectibleHere = true;
            forConnection = collision.GetComponent<InputConnector>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "inputConnector")
        {
            myBlock.ConnectibleHere = false;
            forConnection = null;
        }
    }
}
