using System.Collections;
using System.Collections.Generic;
using RedGaint.Games.Core;
using UnityEngine;

public class GroupManager : MonoBehaviour
{
    public static GroupManager Instance { get; private set; }

    [SerializeField] private GameObject groupPrefab;
    [SerializeField] private float groupSpacing = 3f;
    [SerializeField] private float blinkInterval = 0.3f;

    public Vector3 initialGroupPosition = new Vector3(-4, -2, 0);
    private List<GameObject> groups = new List<GameObject>();
    private GameObject currentPreviewGroup;
    private Coroutine blinkingCoroutine;
    private bool isPreviewActive;

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

    public void ShowGroupPreview()
    {
        if (isPreviewActive) return;
        
        currentPreviewGroup = Instantiate(groupPrefab, GetSnappedGroupPosition(), Quaternion.identity);
        isPreviewActive = true;
        blinkingCoroutine = StartCoroutine(BlinkGroupOutline());
    }

    public void HideGroupPreview()
    {
        if (!isPreviewActive) return;
        
        if (blinkingCoroutine != null)
        {
            StopCoroutine(blinkingCoroutine);
            blinkingCoroutine = null;
        }
        
        if (currentPreviewGroup != null)
        {
            Destroy(currentPreviewGroup);
            currentPreviewGroup = null;
        }
        
        isPreviewActive = false;
    }

    public bool ConfirmGroupWithCards(List<GameObject> cards)
    {
        if(cards.Count == 0) return false;
        
        if (!isPreviewActive || currentPreviewGroup == null)
            return false;
        
        // Stop blinking and make outline permanent
        if (blinkingCoroutine != null)
        {
            StopCoroutine(blinkingCoroutine);
            blinkingCoroutine = null;
        }
        
        var lineRenderer = currentPreviewGroup.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }

        // Parent all cards to the group
        foreach (var card in cards)
        {
            card.transform.SetParent(currentPreviewGroup.transform);
        }

        // Add to groups list and prepare for next group
        groups.Add(currentPreviewGroup);
        currentPreviewGroup = null;
        isPreviewActive = false;
        return true;
    }

    public GameObject CreateEmptyGroup()
    {
        GameObject newGroup = Instantiate(groupPrefab, GetSnappedGroupPosition(), Quaternion.identity);
        groups.Add(newGroup);
        return newGroup;
    }

    private Vector3 GetSnappedGroupPosition()
    {
        if (groups.Count > 0)
        {
            Transform lastGroup = groups[groups.Count - 1].transform;
            return lastGroup.position + new Vector3(0, groupSpacing, 0);
        }
        return initialGroupPosition;
    }

    private IEnumerator BlinkGroupOutline()
    {
        var lineRenderer = currentPreviewGroup.GetComponent<LineRenderer>();
        if (lineRenderer == null) yield break;

        while (currentPreviewGroup != null)
        {
            lineRenderer.enabled = !lineRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}