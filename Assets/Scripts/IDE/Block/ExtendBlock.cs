using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExtendBlock : MonoBehaviour
{
    [SerializeField]
    GameObject[] BottomBracketComponents;
    [SerializeField]
    GameObject LeftBracket;
    [SerializeField]
    Block MyBlock;

    private List<Block> currentlyIn = new List<Block>();

    private float[] _defaultBottomBraketrPositions;

    private void Awake()
    {
        _defaultBottomBraketrPositions = new float[BottomBracketComponents.Length];
        for (int i = 0; i < BottomBracketComponents.Length; i++)
        {
            _defaultBottomBraketrPositions[i] = BottomBracketComponents[i].transform.localPosition.y;
        }
    }

    /// <summary>
    /// Retracts the extend block, adjusting its size and position accordingly.
    /// </summary>
    private void Retract()
    {
        // Calculate the target size
        float targetSize = 0;
        if (MyBlock.outConnectorsScripts[0].Connected != null)
        {
            targetSize += MyBlock.outConnectorsScripts[0].Connected.Block.MySize;
            targetSize += MyBlock.outConnectorsScripts[0].Connected.Block.StackSize;
        }

        // Ensure the target size is not less than 1
        targetSize = Mathf.Max(targetSize, 1);

        // Adjust the position of the bottom bracket components
        for(int i = 0;i < BottomBracketComponents.Length;i++) 
        {
            BottomBracketComponents[i].transform.localPosition = new Vector2(BottomBracketComponents[i].transform.localPosition.x, _defaultBottomBraketrPositions[i] - targetSize + 1); 
        };

        // Adjust the scale and position of the left bracket
        LeftBracket.transform.localScale = new Vector2(0.3f, targetSize > 1 ? targetSize : 1);
        LeftBracket.transform.localPosition = new Vector2(-2.68f, -1 - ((targetSize - 1) / 2));

        // Adjust the size and offset of the box collider
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        bc.size = new Vector2(bc.size.x, targetSize * 2);
        bc.offset = new Vector2(bc.offset.x, bc.size.y / 2);

        if (MyBlock.outConnectorsScripts[0].Connected != null)
        {
            MyBlock.outConnectorsScripts[0].Connected.Block.StackBottom.outConnectorsScripts[^1].Connect();
        }

        BottomBracketComponents
            .Where(x => x.TryGetComponent<OutputConnectionScript>(out _))
            .ToList()
            .ForEach(x => x.GetComponent<OutputConnectionScript>().Connected?.FixPositionInStack());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Block") && other.GetComponent<Block>().IsPickedUp)
        {
            //Makes sure that if a whole stack comes through it will only rescale once
            if (currentlyIn.Any(x => x.StackHead == other.GetComponent<Block>().StackHead))
            {
                currentlyIn.Add(other.GetComponent<Block>());
                return;
            }

            currentlyIn.Add(other.GetComponent<Block>());//Do not move this line above the if!!!

            float targetSize = 0;
            if(MyBlock.outConnectorsScripts[0].Connected != null)
            {
                targetSize += MyBlock.outConnectorsScripts[0].Connected.Block.MySize;
                targetSize += MyBlock.outConnectorsScripts[0].Connected.Block.StackSize;
            }

            targetSize += other.GetComponent<Block>().StackHead.MySize;
            targetSize += other.GetComponent<Block>().StackHead.StackSize;

            if (targetSize <= 1)
                return;

            Extend(targetSize);
        }
    }

    private void Extend(float targetSize)
    {
        MyBlock.inputConnectorsScripts[^1].Connected?.Disconnect();

        for (int i = 0; i < BottomBracketComponents.Length; i++)
        {
            BottomBracketComponents[i].transform.localPosition = new Vector2(BottomBracketComponents[i].transform.localPosition.x, _defaultBottomBraketrPositions[i] - targetSize + 1);
        };
        LeftBracket.transform.localScale = new Vector2(0.3f, targetSize > 1 ? targetSize : 1);
        LeftBracket.transform.localPosition = new Vector2(-2.68f, -1 - ((targetSize - 1) / 2));
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        bc.size = new Vector2(bc.size.x, targetSize * 2);
        bc.offset = new Vector2(bc.offset.x, bc.size.y / 2);

        BottomBracketComponents
            .Where(x => x.TryGetComponent<OutputConnectionScript>(out _))
            .ToList()
            .ForEach(x => x.GetComponent<OutputConnectionScript>().Connected?.FixPositionInStack());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Block") && other.GetComponent<Block>().IsPickedUp)
        {
            currentlyIn.Remove(other.GetComponent<Block>());

            if (!currentlyIn.Any(x => x.StackHead == other.GetComponent<Block>().StackHead))
                Retract();
        }
    }
}
