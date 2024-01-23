using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DrawerBlock : MonoBehaviour
{
    public TextMeshPro text;
    public int count;
    public GameObject MePrefab;

    private void OnMouseUpAsButton()
    {
        if (count != 0)
        {
            count--;
            GameObject temp = Instantiate(MePrefab, parent: IDEManager.Instance.gameObject.transform);
            temp.transform.position = new Vector3(0, transform.parent.parent.position.y, 0);
        }
        RefreshText();
    }

    public void RefreshText()
    {
        if (count < 0)
        {
            text.text = "x99";
        }
        else
        {
            text.text = "x" + count;
        }
    }
}
