using UnityEngine;

public class OutputConnectionScript : MonoBehaviour
{
    public InputConnector Connected { get; private set; } = null;
    InputConnector _forConnection = null;
    InputConnector _lastConnection = null;

    public Block Block { get => myBlock; }

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

        if (_forConnection.IsPrimary)
        {
            Vector3 tPos = gameObject.transform.position;
            Vector3 myPos = _forConnection.gameObject.transform.position;
            _forConnection.Block.transform.parent.position += new Vector3(tPos.x - myPos.x, tPos.y - myPos.y, 0);
        }

        Connected = _forConnection;
        _forConnection = null;
        Connected.Connected = this;

        return true;
    }

    public void ReconnectToPrevious()
    {
        if(_lastConnection == null)
            return;

        _forConnection = _lastConnection;
        Connect();
    }

    public void Disconnect()
    {
        if (Connected == null) return;

        _lastConnection = Connected;
        Connected.Disconnect();
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
        Disconnect();
    }
}
