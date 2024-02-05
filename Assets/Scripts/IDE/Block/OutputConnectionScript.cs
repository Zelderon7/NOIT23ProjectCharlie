using UnityEngine;

public class OutputConnectionScript : MonoBehaviour
{
    public InputConnector Connected { get; private set; } = null;
    InputConnector _forConnection = null;

    [SerializeField] Block myBlock;
    public bool GoNext()
    {
        if (Connected == null)
            return false;

        Connected.CallMe();
        return true;
    }

    public bool Connect()
    {
        if (_forConnection == null)
            return false;

        if (_forConnection.Connected != null)
            return false;

        Vector3 tPos = _forConnection.gameObject.transform.position;
        Vector3 myPos = gameObject.transform.position;
        myBlock.transform.parent.position += new Vector3(tPos.x - myPos.x, tPos.y - myPos.y, 0);

        Connected = _forConnection;
        _forConnection = null;
        Connected.Connected = this;

        return true;
    }

    public void FixPositionInStack()
    {
        if(Connected == null)
            return;

        Vector3 tPos = Connected.gameObject.transform.position;
        Vector3 myPos = gameObject.transform.position;
        myBlock.transform.parent.position += new Vector3(tPos.x - myPos.x, tPos.y - myPos.y, 0);

        InputConnector myConnector = myBlock.transform.parent.gameObject.GetComponentInChildren<InputConnector>();
        if (myConnector == null)
            return;

        myConnector.Connected?.FixPositionInStack();
    }

    public void FixPosOnRescale(float newScale)
    {
        if (Connected != null) return;
        if(myBlock.InpConnector != null)
        {
            if(myBlock.InpConnector.GetComponent<InputConnector>().Connected != null)
            {
                myBlock.InpConnector.GetComponent<InputConnector>().Connected.FixPositionInStack();
            }
        }
    }

    public void Disconnect()
    {
        if (Connected == null) return;

        Connected.Connected = null;
        Connected = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Connected != null)
            return;

        if(collision.CompareTag("inputConnector"))
        {
            myBlock.ConnectableHere = true;
            _forConnection = collision.GetComponent<InputConnector>();
            collision.GetComponent<InputConnector>().ForConnection = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("inputConnector"))
        {
            myBlock.ConnectableHere = false;
            _forConnection = null;
            collision.GetComponent<InputConnector>().ForConnection = null;
        }
    }

    void OnDestroy()
    {
        Block.OnResize -= FixPosOnRescale;
        Disconnect();
    }
}
