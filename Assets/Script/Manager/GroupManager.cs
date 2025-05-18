using System;
using System.Collections;
using System.Collections.Generic;
using RedGaint.Games.Core;
using UnityEngine;

public class GroupManager : MonoBehaviour
{
    public static GroupManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject groupPrefab;
    [SerializeField] private float groupSpacing = 3f;
    [SerializeField] private float blinkInterval = 0.3f;
    public Vector3 initialGroupPosition = new Vector3(-4, -2, 0);

    [Header("Debug")]
    [SerializeField] private bool logGroupEvents = true;

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

    private void OnEnable()
    {
        GameEventSystem.Subscribe<RequestGroupDestroyEvent>(RemoveGroupData);
    }

    private void OnDisable()
    {
        GameEventSystem.Unsubscribe<RequestGroupDestroyEvent>(RemoveGroupData);
    }

    private void RemoveGroupData(RequestGroupDestroyEvent destroyRequestGroup)
    {
        if (destroyRequestGroup.Group != null && destroyRequestGroup.Group.gameObject != null)
        {
            groups.Remove(destroyRequestGroup.Group.gameObject);
            Destroy(destroyRequestGroup.Group.gameObject);
            if (logGroupEvents) Debug.Log($"Group removed: {destroyRequestGroup.Group.gameObject.name}");
        }
    }

    public void ShowGroupPreview()
    {
        if (isPreviewActive) return;
        
        currentPreviewGroup = Instantiate(groupPrefab, GetSnappedGroupPosition(), Quaternion.identity);
        currentPreviewGroup.name = "PreviewGroup";
        isPreviewActive = true;
        blinkingCoroutine = StartCoroutine(BlinkGroupOutline());

        if (logGroupEvents) Debug.Log("Group preview shown");
    }

    public bool ConfirmGroupWithCards()
    {
        if (!isPreviewActive || currentPreviewGroup == null)
        {
            if (logGroupEvents) Debug.Log("No active preview group to confirm");
            return false;
        }

        // Check if group contains any cards
        int childCount = currentPreviewGroup.transform.childCount;
        if (childCount == 0)
        {
            if (logGroupEvents) Debug.Log("Preview group empty - confirmation failed");
            return false;
        }

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

        // Finalize the group
        currentPreviewGroup.name = $"Group_{groups.Count + 1}";
        groups.Add(currentPreviewGroup);
        
        if (logGroupEvents) Debug.Log($"Group confirmed with {childCount} cards");

        currentPreviewGroup = null;
        isPreviewActive = false;
        return true;
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
            if (logGroupEvents) Debug.Log("Preview group hidden");
            Destroy(currentPreviewGroup);
            currentPreviewGroup = null;
        }

        isPreviewActive = false;
    }

    public CardGroup CreateEmptyGroup()
    {
        GameObject newGroup = Instantiate(groupPrefab, GetSnappedGroupPosition(), Quaternion.identity);
        newGroup.name = $"Group_{groups.Count + 1}";
        groups.Add(newGroup);

        if (logGroupEvents) Debug.Log("New empty group created");
        return newGroup.GetComponent<CardGroup>();
    }

    private Vector3 GetSnappedGroupPosition()
    {
        if (groups.Count == 0)
            return initialGroupPosition;

        Vector3 lastPos = groups[groups.Count - 1].transform.position;
    
        // Only the second group goes up (when count == 1), others go right
        bool placeAbove = groups.Count == 1;
    
        return lastPos + new Vector3(
            placeAbove ? 0 : groupSpacing,  // X
            placeAbove ? groupSpacing : 0,  // Y
            0
        );
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

    public bool PreviewGroupHasCards()
    {
        return isPreviewActive && 
               currentPreviewGroup != null && 
               currentPreviewGroup.transform.childCount > 0;
    }
}