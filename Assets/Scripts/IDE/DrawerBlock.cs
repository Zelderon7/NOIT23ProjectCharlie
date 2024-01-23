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
            Instantiate(MePrefab, parent: IDEManager.Instance.gameObject.transform);
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
