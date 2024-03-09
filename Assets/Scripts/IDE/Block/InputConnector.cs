using UnityEngine;

public class InputConnector : MonoBehaviour
{
    public Block Block { get; private set; }
    public OutputConnectionScript Connected = null;
    public OutputConnectionScript ForConnection = null;

    private void Awake()
    {
        Block = transform.parent.parent.gameObject.GetComponentInChildren<Block>();
        if (Block == null)
            Debug.LogError("BlockNotFound");
    }
    public void CallMe()
    {
        Block.RunBlock(this);
    }

    private void OnDestroy()
    {
        Connected?.Disconnect();
    }
}
