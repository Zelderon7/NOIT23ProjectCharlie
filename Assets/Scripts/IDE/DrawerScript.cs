using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DrawerScript : MonoBehaviour
{
    public bool Opened { get; private set; } = false;
    bool inMotion = false;
    private Dictionary<int, List<GameObject>> DrawerBlocksDictionary = new Dictionary<int, List<GameObject>>();
    private int curId = -1;
    private void Start()
    {
        
    }

    IEnumerator OnOpenCoroutine()
    {
        inMotion = true;
        yield return StartCoroutine(MoveTransform(transform.parent, new Vector3(-.388f, 0, 0), 1f));
        Opened = true;
        inMotion = false;
    }

    IEnumerator OnCloseCoroutine()
    {
        inMotion = true;
        yield return StartCoroutine(MoveTransform(transform.parent, new Vector3(-.615f, 0, 0), 1f));
        Opened = false;
        inMotion = false;
    }

    private void OnMouseDown()
    {
        if (inMotion) return;
        
        if(Opened)
            StartCoroutine(OnCloseCoroutine());
        else
            StartCoroutine(OnOpenCoroutine());
    }

    public IEnumerator MoveTransform(Transform targetTransform, Vector3 targetPosition, float duration)
    {
        // Store the initial position of the transform
        Vector3 startPosition = targetTransform.localPosition;

        // Variable to track elapsed time
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the interpolation factor between 0 and 1 based on elapsed time and duration
            float t = elapsedTime / duration;

            float easedT = GameManager.Instance.EaseInOutQuad.Evaluate(t); // Use the easing function

            // Use Vector3.Lerp to interpolate between the start and target positions
            targetTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, easedT);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure that the transform reaches the exact target position
        targetTransform.localPosition = targetPosition;
    }

    public void InstantiateCodeDrawer(BlockTypes[] blocks, int id)
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            GameObject temp = Instantiate(IDEManager.Instance.BlockTypesPrefs[blocks[i].id].Prefab, parent: gameObject.transform.parent.Find("DrawerBlocks"));

            if (!DrawerBlocksDictionary.ContainsKey(id))
                DrawerBlocksDictionary.Add(id, new List<GameObject>());
            DrawerBlocksDictionary[id].Add(temp);

            temp.GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(x => { x.sortingLayerName = "IDEScreen"; x.sortingOrder += 11; });
            var _tempTextRef = temp.GetComponentInChildren<TextMeshPro>();
            if (_tempTextRef != null)
            {
                _tempTextRef.sortingOrder += 11;
            }


            temp.transform.localScale = new Vector3(.5f, .5f, 1);
            temp.transform.localPosition = new Vector3(-.4f, 5 - i * (temp.transform.localScale.y * 1.8f) - temp.transform.localScale.y, -2);
            temp.tag = "DrawerBlock";
            temp.GetComponentsInChildren<Transform>().ToList().ForEach(x => x.gameObject.tag = "DrawerBlock");
            temp.GetComponentsInChildren<OutputConnectionScript>().ToList().ForEach(x => Destroy(x));
            temp.GetComponentsInChildren<InputConnector>().ToList().ForEach(x => Destroy(x));
            GameObject targetParent = temp.GetComponentInChildren<Block>().gameObject;
            Destroy(targetParent.GetComponent<Block>());
            DrawerBlock scriptRef = targetParent.AddComponent<DrawerBlock>();
            GameObject TextObject = Instantiate(new GameObject("TextHolder"), parent: temp.transform);
            scriptRef.text = TextObject.AddComponent<TextMeshPro>();
            scriptRef.text.rectTransform.localPosition = Vector3.right * 3.8f;
            scriptRef.text.rectTransform.sizeDelta = Vector2.one;
            scriptRef.text.enableAutoSizing = true;
            scriptRef.text.fontSizeMin = 0;
            scriptRef.text.color = Color.black;
            scriptRef.text.fontStyle = FontStyles.Bold;
            scriptRef.Count = blocks[i].count;
            scriptRef.RefreshText();
            scriptRef.MePrefab = IDEManager.Instance.BlockTypesPrefs[blocks[i].id].Prefab;
        }
    }

    public void RefreshDrawer(int id)
    {
        if(id == curId || id < 0) return;

        if(curId != -1)
            DrawerBlocksDictionary[curId].ForEach(x => x.gameObject.SetActive(false));

        if (!DrawerBlocksDictionary.ContainsKey(id))
            InstantiateCodeDrawer(IDEManager.Instance.GetICodeableById(id).MyBlockTypes, id);
        DrawerBlocksDictionary[id].ForEach(x => x.gameObject.SetActive(true));
    }
}
