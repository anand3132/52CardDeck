using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Rendering;

namespace RedGaint.Games.Core
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SortingGroup))]
    public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        //References
        private Camera mainCam;
        private Transform originalParent;
        private CardGroup currentGroup;        
        [SerializeField]private  Transform visual;
        // private SortingGroup sortingGroup;
        private CardGroup lastTriggeredGroup = null;

        //positions
        private Vector3 originalVisualLocalPosition;
        private Vector3 originalWorldPosition;
        private int originalSortingOrder;
        
        //toggles
        [SerializeField]private bool onDrag = false;
        [SerializeField]private bool isSelected = false;
        private Dictionary<Card, Vector3> dragOffsets = new Dictionary<Card, Vector3>();

        private void Start()
        {
            mainCam = Camera.main;
            currentGroup = GetComponentInParent<CardGroup>();
            originalVisualLocalPosition = visual.localPosition;
            var visualRenderer = visual.GetComponent<SpriteRenderer>();
            var collider = GetComponent<BoxCollider2D>();
            if (visualRenderer != null && collider != null)
            {
                collider.size = visualRenderer.bounds.size;
                collider.offset = visual.localPosition;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onDrag)
                return; 
            if (!isSelected)
            {
                originalVisualLocalPosition = visual.localPosition;
                visual.localPosition += Vector3.up * 0.3f;
                isSelected = true;
                currentGroup.AddSelectedCard(this);

            }
            else
            {
                visual.localPosition = originalVisualLocalPosition;
                isSelected = false;
                currentGroup.RemoveSelectedCard(this);
            }
            
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            onDrag = true;
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);

            dragOffsets.Clear();

            if (isSelected)
            {

                foreach (var card in currentGroup.GetSelectedCards())
                {
                    Vector3 offset = card.transform.position - worldPos;
                    dragOffsets[card] = offset;

                    card.originalWorldPosition = card.transform.position;
                    card.originalParent = card.transform.parent;

                }
            }
            else
            {
                // Not selected, only drag this card
                dragOffsets[this] = transform.position - worldPos;

                originalWorldPosition = transform.position;
                originalParent = transform.parent;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag = true;

            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            foreach (var kvp in dragOffsets)
            {
                var card = kvp.Key;
                var offset = kvp.Value;

                card.transform.position = worldPos + offset;
            }
        }

        public void Reset()
        {
            isSelected = false;
            onDrag = false;
            dragOffsets.Clear();
            lastTriggeredGroup = null;
            
            visual.localPosition=Vector3.zero;
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            onDrag = false;
            
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            var allGroups = GameObject.FindObjectsOfType<CardGroup>();
            CardGroup dropTarget = allGroups.FirstOrDefault(g => g.ContainsPoint(worldPos));

            if (dropTarget == null && lastTriggeredGroup != null)
            {
                // Debug.Log("Fallback: Using last triggered group instead of pointer position.");
                dropTarget = lastTriggeredGroup;
            }
            dragOffsets.Clear(); 

            if (dropTarget != null)
                HandleDropTargetFound(dropTarget, worldPos);
            else
                HandleNoDropTargetFound();

            lastTriggeredGroup = null;

        }


        private void HandleDropTargetFound(CardGroup dropTarget, Vector3 worldPos)
        {
            if (dropTarget == currentGroup)
            {
                // Just reset position within same group
                transform.SetParent(dropTarget.transform, true);
                transform.position = originalWorldPosition;
                if (isSelected)
                {
                    foreach (var card in currentGroup.GetSelectedCards())
                    {
                        if (card != this)
                        {
                            // card.currentGroup = dropTarget;
                            card.Reset();
                        }
                    }
                    currentGroup.ClearSelectedCards();
                }
                Reset();
                currentGroup.RearrangeCards();
            }
            else
            {
                if (isSelected)
                {
                    foreach (var card in currentGroup.GetSelectedCards())
                    {
                        if (card != this)
                        {
                            card.currentGroup = dropTarget;
                            card.Reset();
                        }
                    }
                    currentGroup.ClearSelectedCards();
                }
                Reset();
                // Move to new group
                Vector3 localSnapPos = dropTarget.GetNextCardPosition();
                transform.SetParent(dropTarget.transform, false);
                transform.localPosition = localSnapPos;
                currentGroup.RearrangeCards();
                currentGroup = dropTarget;
                dropTarget.RearrangeCards();

                
            }
            originalParent = dropTarget.transform;
            originalWorldPosition = transform.position;
        }


        private void HandleNoDropTargetFound()
        {
            transform.SetParent(originalParent, false);
            transform.position = originalWorldPosition;
            if (isSelected)
            {
                foreach (var card in currentGroup.GetSelectedCards())
                {
                    if (card != this)
                    {
                        card.Reset();
                    }
                }
                Reset();
                currentGroup.ClearSelectedCards();
                currentGroup = originalParent?.GetComponent<CardGroup>();
            }
            currentGroup.RearrangeCards();
        }
        
       

        
        private void OnTriggerEnter2D(Collider2D other)
        {
            CardGroup group = other.GetComponent<CardGroup>();
            if (group != null) {lastTriggeredGroup = group; }
        }

    }
}
