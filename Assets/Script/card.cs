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
        // References
        private Camera mainCamera;
        private Transform previousParentGroup;
        [SerializeField] private CardGroup activeCardGroup;
        [SerializeField] private Transform cardVisualTransform;
        [SerializeField] private CardGroup lastHoveredGroup = null;

        // Positions
        private Vector3 visualBaseLocalPosition;
        private Vector3 preDragWorldPosition;

        // Toggles
        [SerializeField] private bool isBeingDragged = false;
        [SerializeField] private bool isCardSelected = false;

        private Dictionary<Card, Vector3> dragOffsetPerCard = new Dictionary<Card, Vector3>();

        private void Start()
        {
            mainCamera = Camera.main;
            activeCardGroup = GetComponentInParent<CardGroup>();
            visualBaseLocalPosition = cardVisualTransform.localPosition;

            var visualRenderer = cardVisualTransform.GetComponent<SpriteRenderer>();
            var collider = GetComponent<BoxCollider2D>();
            if (visualRenderer != null && collider != null)
            {
                collider.size = visualRenderer.bounds.size;
                collider.offset = cardVisualTransform.localPosition;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("---------------- OnPointerClick ----------------");

            if (isBeingDragged) return;

            if (!isCardSelected)
            {
                visualBaseLocalPosition = cardVisualTransform.localPosition;
                cardVisualTransform.localPosition += Vector3.up * 0.3f;
                isCardSelected = true;
                activeCardGroup.AddSelectedCard(this);
            }
            else
            {
                cardVisualTransform.localPosition = visualBaseLocalPosition;
                isCardSelected = false;
                activeCardGroup.RemoveSelectedCard(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("---------------- OnBeginDrag ----------------");

            isBeingDragged = true;
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

            dragOffsetPerCard.Clear();

            if (isCardSelected)
            {
                Debug.Log("Selected, dragging multiple cards...");
                foreach (var card in activeCardGroup.GetSelectedCards())
                {
                    Vector3 offset = card.transform.position - worldPos;
                    dragOffsetPerCard[card] = offset;
                    card.preDragWorldPosition = card.transform.position;
                    card.previousParentGroup = card.transform.parent;
                }
                activeCardGroup.RearrangeCardsFromSelection();
            }
            else
            {
                dragOffsetPerCard[this] = transform.position - worldPos;
                preDragWorldPosition = transform.position;
                previousParentGroup = transform.parent;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            isBeingDragged = true;

            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            int index = 0;
            foreach (var kvp in dragOffsetPerCard)
            {
                var card = kvp.Key;
                Vector3 stackOffset = new Vector3(index * 0.2f, 0);
                card.transform.position = worldPos + stackOffset;
                index++;
            }

            if (dragOffsetPerCard.Count > 1)
            {
                GroupManager.Instance.ShowGroupPreviewAt(worldPos);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("---------------- OnEndDrag ----------------");

            isBeingDragged = false;

            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            var allGroups = GameObject.FindObjectsOfType<CardGroup>();
            CardGroup dropTarget = allGroups.FirstOrDefault(g => g.ContainsPoint(worldPos));

            if (dropTarget == null && lastHoveredGroup != null)
            {
                dropTarget = lastHoveredGroup;
            }

            dragOffsetPerCard.Clear();

            if (dropTarget != null)
                ProcessSuccessfulDrop(dropTarget, worldPos);
            else
                RevertToPreviousPosition();

            lastHoveredGroup = null;

            Debug.Log("---------------- OnEndDrag Finished ----------------");
        }

        private void ProcessSuccessfulDrop(CardGroup dropTarget, Vector3 worldPos)
        {
            Debug.Log("---------------- ProcessSuccessfulDrop ----------------");

            if (dropTarget == activeCardGroup)
            {
                transform.SetParent(dropTarget.transform, true);
                transform.position = preDragWorldPosition;

                if (isCardSelected)
                {
                    foreach (var card in activeCardGroup.GetSelectedCards())
                    {
                        if (card != this)
                        {
                            card.ResetCardState();
                        }
                    }
                    activeCardGroup.ClearSelectedCards();
                }

                ResetCardState();
                activeCardGroup.RearrangeCards();
            }
            else
            {
                if (isCardSelected)
                {
                    foreach (var card in activeCardGroup.GetSelectedCards())
                    {
                        if (card != this)
                        {
                            card.activeCardGroup = dropTarget;
                            card.ResetCardState();
                        }
                    }

                    activeCardGroup.ClearSelectedCards();
                }

                ResetCardState();
                Vector3 localSnapPos = dropTarget.GetNextCardPosition();
                transform.SetParent(dropTarget.transform, false);
                transform.localPosition = localSnapPos;

                activeCardGroup.RearrangeCards();
                activeCardGroup = dropTarget;
                dropTarget.RearrangeCards();
            }

            previousParentGroup = dropTarget.transform;
            preDragWorldPosition = transform.position;
        }

        private void RevertToPreviousPosition()
        {
            Debug.Log("---------------- RevertToPreviousPosition ----------------");

            transform.SetParent(previousParentGroup, false);
            transform.position = preDragWorldPosition;

            if (isCardSelected)
            {
                foreach (var card in activeCardGroup.GetSelectedCards())
                {
                    if (card != this)
                    {
                        card.ResetCardState();
                    }
                }

                ResetCardState();
                activeCardGroup.ClearSelectedCards();

                activeCardGroup = previousParentGroup?.GetComponent<CardGroup>();
            }

            activeCardGroup.RearrangeCards();
        }

        public void ResetCardState()
        {
            isCardSelected = false;
            isBeingDragged = false;
            dragOffsetPerCard.Clear();
            lastHoveredGroup = null;
            cardVisualTransform.localPosition = Vector3.zero;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("---------------- OnTriggerEnter2D ----------------");

            CardGroup group = other.GetComponent<CardGroup>();
            if (group != null)
            {
                lastHoveredGroup = group;
            }
        }
    }
}
