using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DrawerScript : MonoBehaviour
{
    public bool IsOpen { get; private set; } = true;

    bool _inMotion = false;
    int _currentId = -1;

    Dictionary<int, List<GameObject>> _drawerBlocksDictionary = new Dictionary<int, List<GameObject>>();

    void OnMouseDown()
    {
        if (_inMotion)
            return;

        if (IsOpen)
            StartCoroutine(OnCloseCoroutine());
        else
            StartCoroutine(OnOpenCoroutine());
    }

    IEnumerator OnOpenCoroutine()
    {
        _inMotion = true;
        yield return StartCoroutine(MoveTransform(transform.parent, new Vector3(-.388f, 0, 0), 1f));
        IsOpen = true;
        _inMotion = false;
    }

    IEnumerator OnCloseCoroutine()
    {
        _inMotion = true;
        yield return StartCoroutine(MoveTransform(transform.parent, new Vector3(-.615f, 0, 0), 1f));
        IsOpen = false;
        _inMotion = false;
    }
    public IEnumerator MoveTransform(Transform targetTransform, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = targetTransform.localPosition;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float easedT = GameManager.Instance.EaseInOutQuad.Evaluate(t);

            targetTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, easedT);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        targetTransform.localPosition = targetPosition;
    }

    public void InstantiateCodeDrawer(BlockTypes[] blocks, int id)
    {
        float curPosY = 4.3f;
        for (int i = 0; i < blocks.Length; i++)
        {
            GameObject temp = Instantiate(IDEManager.Instance.BlockTypesPrefs[blocks[i].Id].Prefab, parent: gameObject.transform.parent.Find("DrawerBlocks"));

            if (!_drawerBlocksDictionary.ContainsKey(id))
                _drawerBlocksDictionary.Add(id, new List<GameObject>());
            _drawerBlocksDictionary[id].Add(temp);

            temp.GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(x => { x.sortingLayerName = "IDEScreen"; x.sortingOrder += 11; });
            var _tempTextRef = temp.GetComponentInChildren<TextMeshPro>();
            if (_tempTextRef != null)
            {
                _tempTextRef.sortingOrder += 11;
            }


            temp.transform.localScale = new Vector3(.5f, .5f, 1);
            temp.transform.localPosition = new Vector3(-.4f, curPosY, -2);
            curPosY -= temp.GetComponentInChildren<Block>().MySize/2 + .5f;
            temp.tag = "DrawerBlock";
            temp.GetComponentsInChildren<Transform>().ToList().ForEach(x => x.gameObject.tag = "DrawerBlock");
            temp.GetComponentsInChildren<OutputConnectionScript>().ToList().ForEach(x => Destroy(x));
            temp.GetComponentsInChildren<InputConnector>().ToList().ForEach(x => Destroy(x));
            GameObject targetParent = temp.GetComponentInChildren<Block>().gameObject;
            Destroy(targetParent.GetComponent<Block>());
            if(temp.GetComponentInChildren<ExtendBlock>() != null)
            {
                Destroy(temp.GetComponentInChildren<ExtendBlock>());
            }
            DrawerBlock scriptRef = targetParent.AddComponent<DrawerBlock>();
            GameObject TextObject = Instantiate(new GameObject("TextHolder"), parent: temp.transform);
            scriptRef.Text = TextObject.AddComponent<TextMeshPro>();
            scriptRef.Text.rectTransform.localPosition = Vector3.right * 3.8f;
            scriptRef.Text.rectTransform.sizeDelta = Vector2.one;
            scriptRef.Text.enableAutoSizing = true;
            scriptRef.Text.fontSizeMin = 0;
            scriptRef.Text.color = Color.black;
            scriptRef.Text.fontStyle = FontStyles.Bold;
            scriptRef.Count = blocks[i].Count;
            scriptRef.RefreshText();
            scriptRef.Prefab = IDEManager.Instance.BlockTypesPrefs[blocks[i].Id].Prefab;
        }
    }

    public void RefreshDrawer(int id)
    {
        if(id == _currentId || id < 0) return;

        if(_currentId != -1)
            _drawerBlocksDictionary[_currentId].ForEach(x => x.gameObject.SetActive(false));

        if (!_drawerBlocksDictionary.ContainsKey(id))
            InstantiateCodeDrawer(IDEManager.Instance.GetICodeableById(id).BlockTypes, id);
        _drawerBlocksDictionary[id].ForEach(x => x.gameObject.SetActive(true));

        _currentId = id;
    }
}
