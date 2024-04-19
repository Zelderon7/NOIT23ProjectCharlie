using System.Linq;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class InputConnector : MonoBehaviour
{
    public Block Block { get; private set; }
    public OutputConnectionScript Connected = null;
    public OutputConnectionScript ForConnection = null;
    public OutputConnectionScript LastConnection = null;
    public bool IsPrimary
    {
        get { return transform.GetSiblingIndex() == 0; }
    }

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

    /// <summary>
    /// Fixes the position of every element in the stack below this one
    /// </summary>
    public void FixPositionInStack()
    {
        if (!IsPrimary)
            return;

        if(Connected != null)
        {
            Vector3 tPos = Connected.gameObject.transform.position;
            Vector3 myPos = gameObject.transform.position;
            Block.transform.parent.position += new Vector3(tPos.x - myPos.x, tPos.y - myPos.y, 0);
        }

        PropagadeFixPositionInStack();
    }

    void PropagadeFixPositionInStack()
    {
        if (Block.outConnectorsScripts.Length > 0)
        {
            foreach (OutputConnectionScript connector in Block.outConnectorsScripts)
            {
                connector.Connected?.FixPositionInStack();
            }
        }
    }

    /// <summary>
    /// SHOULD ONLY BE CALLED FROM OutputConnector.cs!!!!!!
    /// </summary>
    public void Disconnect()
    {
        if (Connected == null)
            return;

        LastConnection = Connected;
        Connected = null;
    }

    private void OnDestroy()
    {
        Connected?.Disconnect();
    }
}
