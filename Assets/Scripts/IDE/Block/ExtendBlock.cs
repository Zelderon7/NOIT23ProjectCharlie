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

    public ExtendBlock outerExtendable;

    List<Block> CurrentlyIn = new List<Block>();

    float _lastSize = 1;

    public float TotalInnerSize
    {
        get
        {
            float ans = 0;
            CurrentlyIn.ConvertAll(x => x.StackHead).Distinct().ToList().ForEach(x => {
                ans += x.StackSize + x.MySize;
            });
            return Mathf.Max(ans, 1);
        }
    }

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
    /// <para></para>
    /// <para></para>
    /// <param name="currentlyExtracted"><paramref name="currentlyExtracted"/>: The stack head of a stack that is currently extracted from an inner expandable block<strong>ONLY ASSIGN FROM THIS CLASS!!!</strong></param>
    /// </summary>
    private void Retract()
    {
        float targetSize = 0;

        targetSize += CurrentlyIn
            .ConvertAll(x => x.StackHead)
            .Distinct()
            .Sum(x => x.MySize + x.StackSize);

        MyBlock.outConnectorsScripts[0].Connected?.Block
            .Where(x => x.transform.parent.GetComponentInChildren<ExtendBlock>() != null)
            .ToList()
            .ForEach(x => targetSize -= x.transform.parent.GetComponentInChildren<ExtendBlock>().CurrentlyIn
                .Where(y => CurrentlyIn
                    .Any(z => z.StackHead == y.StackHead))
                .Select(y => y.StackHead)
                .Distinct()
                .Sum(y => y.MySize + y.StackSize));

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

        if (_lastSize < targetSize)
            MyBlock.inputConnectorsScripts[^1].Connected?.Disconnect();
        else if (MyBlock.outConnectorsScripts[0].Connected != null)
        {
            MyBlock.outConnectorsScripts[0].Connected.Block.StackBottom.outConnectorsScripts[^1].Connect();
            if (MyBlock.outConnectorsScripts[0].Connected.Block.StackBottom.outConnectorsScripts[^1].Connected == null)
                MyBlock.outConnectorsScripts[0].Connected.Block.StackBottom.outConnectorsScripts[^1].ReconnectToPrevious();
        }

        _lastSize = targetSize;

        BottomBracketComponents
            .Where(x => x.TryGetComponent<OutputConnectionScript>(out _))
            .ToList()
            .ForEach(x => x.GetComponent<OutputConnectionScript>().Connected?.FixPositionInStack());

        outerExtendable?.Retract();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Block") && (other.GetComponent<Block>().IsPickedUp || MyBlock.IsPickedUp))
        {
            if(other.transform.parent.GetComponentInChildren<ExtendBlock>() != null)
            {
                if (other.transform.parent.GetComponentInChildren<ExtendBlock>().CurrentlyIn.Contains(MyBlock))
                    return;

                other.transform.parent.GetComponentInChildren<ExtendBlock>().outerExtendable = this;
            }

            //Makes sure that if a whole stack comes through it will only rescale once
            if (CurrentlyIn.Any(x => x.StackHead == other.GetComponent<Block>().StackHead))
            {
                CurrentlyIn.Add(other.GetComponent<Block>());
                return;
            }

            CurrentlyIn.Add(other.GetComponent<Block>());//Do not move this line above the if!!!

            //Extend(targetSize);
            Retract();
            //if(other.GetComponent<Block>().StackHead)
        }
    }

    /// <summary>
    /// If this block is inside of another extendable block, retract the outer one
    /// <paramref name="sizeReduction"/>
    /// </summary>
    public void RetractOuter()
    {
        if (outerExtendable != null)
        {
            outerExtendable.Retract();
            outerExtendable.RetractOuter();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Block") && (other.GetComponent<Block>().IsPickedUp || MyBlock.IsPickedUp))
        {
            CurrentlyIn.Remove(other.GetComponent<Block>());

            if(other.GetComponent<Block>().transform.parent.GetComponentInChildren<ExtendBlock>() != null )
                other.GetComponent<Block>().transform.parent.GetComponentInChildren<ExtendBlock>().outerExtendable = this.outerExtendable;

            //In case a whole stack is getting out do not retract until the last block is out
            if (!CurrentlyIn.Any(
                x => {
                    Block y = other.GetComponent<Block>().StackHead;
                    if (y == this)
                        y = MyBlock.outConnectorsScripts[0].Connected.Block;
                    return x.StackHead == y && y.IsPickedUp;
                }
            ))
            {
                Retract();
                RetractOuter();
            }
                
        }
    }
}
