using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupManager : MonoBehaviour
{
    public static GroupManager Instance { get; private set; }

    [SerializeField] private GameObject groupPrefab;
    [SerializeField] private GameObject groupPreviewPrefab;
    [SerializeField] private float groupSpacing = 3f;

    public Vector3 initialGroupPosition=new Vector3(-4,-2,0);
    private List<GameObject> groups = new List<GameObject>();
    private GameObject currentPreview;
    private Coroutine blinkingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void ShowGroupPreviewAt(Vector3 position)
    {
        if (currentPreview == null)
        {
            currentPreview = Instantiate(groupPreviewPrefab);
            blinkingCoroutine = StartCoroutine(BlinkOutline(currentPreview));
        }

        currentPreview.transform.position = GetSnappedGroupPosition();
    }

    public void HideGroupPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;

            if (blinkingCoroutine != null)
            {
                StopCoroutine(blinkingCoroutine);
                blinkingCoroutine = null;
            }
        }
    }

    public void CreateGroupFromCards(List<GameObject> cards)
    {
        Vector3 newGroupPos = GetSnappedGroupPosition();
        GameObject newGroup = Instantiate(groupPrefab, newGroupPos, Quaternion.identity);
        groups.Add(newGroup);

        foreach (var card in cards)
        {
            card.transform.SetParent(newGroup.transform);
        }
    }

    private Vector3 GetSnappedGroupPosition()
    {
        if (groups.Count > 0)
        {
            Transform lastGroup = groups[groups.Count - 1].transform;
            return lastGroup.position + new Vector3(0, groupSpacing, 0);
        }
        else
        {
            return initialGroupPosition; 
        }
    }

    private IEnumerator BlinkOutline(GameObject outline)
    {
        var sr = outline.GetComponent<LineRenderer>();
        while (outline != null)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    public GameObject CreateEmptyGroup()
    {
        Vector3 newGroupPos = GetSnappedGroupPosition();
        GameObject newGroup = Instantiate(groupPrefab, newGroupPos, Quaternion.identity);
        groups.Add(newGroup);
        return newGroup;
    }

}
