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

    private const float DEFAULTPOS = -1.7346f;

    private void Retract()
    {
        float targetSize = 0;
        if (MyBlock.outConnectorsScripts[0].Connected != null)
        {
            targetSize += MyBlock.outConnectorsScripts[0].Connected.Block.MySize;
            targetSize += MyBlock.outConnectorsScripts[0].Connected.Block.StackSize;
        }

        if(targetSize < 1)
            targetSize = 1;

        BottomBracketComponents.ToList().ForEach((x) => { x.transform.localPosition = new Vector2(x.transform.localPosition.x, DEFAULTPOS - targetSize + 1); });
        LeftBracket.transform.localScale = new Vector2(0.3f, targetSize > 1? targetSize : 1);
        LeftBracket.transform.localPosition = new Vector2(-2.68f, -1 - ((targetSize - 1) / 2));
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        bc.size = new Vector2(bc.size.x, targetSize * 2);
        bc.offset = new Vector2(bc.offset.x, bc.size.y / 2);
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

            BottomBracketComponents.ToList().ForEach((x) => { x.transform.localPosition = new Vector2(x.transform.localPosition.x, DEFAULTPOS - targetSize + 1); });
            LeftBracket.transform.localScale = new Vector2(0.3f, targetSize > 1 ? targetSize : 1);
            LeftBracket.transform.localPosition = new Vector2(-2.68f, -1 - ((targetSize-1) / 2));
            BoxCollider2D bc = GetComponent<BoxCollider2D>();
            bc.size = new Vector2(bc.size.x, targetSize * 2);
            bc.offset = new Vector2(bc.offset.x, bc.size.y/2);

            
        }
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
