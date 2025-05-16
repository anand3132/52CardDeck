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

            SetupCollider();
        }

        private void SetupCollider()
        {
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
            if (isBeingDragged) return;
            ToggleCardSelection();
        }

        private void ToggleCardSelection()
        {
            if (!isCardSelected)
            {
                SelectCard();
            }
            else
            {
                DeselectCard();
            }
        }

        private void SelectCard()
        {
            visualBaseLocalPosition = cardVisualTransform.localPosition;
            cardVisualTransform.localPosition += Vector3.up * 0.3f;
            isCardSelected = true;
            activeCardGroup.AddSelectedCard(this);
        }

        private void DeselectCard()
        {
            cardVisualTransform.localPosition = visualBaseLocalPosition;
            isCardSelected = false;
            activeCardGroup.RemoveSelectedCard(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isBeingDragged = true;
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            dragOffsetPerCard.Clear();
            if (isCardSelected)
            {
                Debug.Log("Selected, dragging multiple cards...");
                SetupMultiCardDrag(worldPos);
                activeCardGroup.RearrangeCardsFromSelection();
            }
            else
            {
                SetupSingleCardDrag(worldPos);
            }
        }

        private Vector3 GetWorldPositionFromEventData(PointerEventData eventData)
        {
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCamera.transform.position.z);
            return mainCamera.ScreenToWorldPoint(screenPos);
        }

        private void SetupMultiCardDrag(Vector3 worldPos)
        {
            foreach (var card in activeCardGroup.GetSelectedCards())
            {
                dragOffsetPerCard[card] = card.transform.position - worldPos;
                card.preDragWorldPosition = card.transform.position;
                card.previousParentGroup = card.transform.parent;
            }
        }

        private void SetupSingleCardDrag(Vector3 worldPos)
        {
            dragOffsetPerCard[this] = transform.position - worldPos;
            preDragWorldPosition = transform.position;
            previousParentGroup = transform.parent;
        }

        public void OnDrag(PointerEventData eventData)
        {
            isBeingDragged = true;
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            worldPos.z = 0;

            UpdateCardPositionsDuringDrag(worldPos);

            if (dragOffsetPerCard.Count > 1)
            {
                GroupManager.Instance.ShowGroupPreviewAt(worldPos);
            }
        }

        private void UpdateCardPositionsDuringDrag(Vector3 worldPos)
        {
            int index = 0;
            foreach (var kvp in dragOffsetPerCard)
            {
                var card = kvp.Key;
                Vector3 stackOffset = new Vector3(index * 0.2f, 0);
                card.transform.position = worldPos + stackOffset;
                index++;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {

            isBeingDragged = false;
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            worldPos.z = 0;

            CardGroup dropTarget = FindDropTarget(worldPos);
            dragOffsetPerCard.Clear();

            if (dropTarget != null)
                ProcessSuccessfulDrop(dropTarget, worldPos);
            else
                RevertToPreviousPosition();

            lastHoveredGroup = null;
        }

        private CardGroup FindDropTarget(Vector3 worldPos)
        {
            var allGroups = GameObject.FindObjectsOfType<CardGroup>();
            CardGroup dropTarget = allGroups.FirstOrDefault(g => g.ContainsPoint(worldPos));
            return dropTarget ?? lastHoveredGroup;
        }

        private void ProcessSuccessfulDrop(CardGroup dropTarget, Vector3 worldPos)
        {
            if (dropTarget == activeCardGroup)
            {
                HandleSameGroupDrop(dropTarget);
            }
            else
            {
                HandleDifferentGroupDrop(dropTarget);
            }

            previousParentGroup = dropTarget.transform;
            preDragWorldPosition = transform.position;
        }

        private void HandleSameGroupDrop(CardGroup dropTarget)
        {
            transform.SetParent(dropTarget.transform, true);
            transform.position = preDragWorldPosition;

            if (isCardSelected)
            {
                ResetAllSelectedCards();
                activeCardGroup.ClearSelectedCards();
            }

            ResetCardState();
            activeCardGroup.RearrangeCards();
        }

        private void HandleDifferentGroupDrop(CardGroup dropTarget)
        {
            if (isCardSelected)
            {
                MoveAllSelectedCardsToNewGroup(dropTarget);
                activeCardGroup.ClearSelectedCards();
            }

            ResetCardState();
            MoveToNewGroup(dropTarget);
        }

        private void ResetAllSelectedCards()
        {
            foreach (var card in activeCardGroup.GetSelectedCards())
            {
                if (card != this)
                {
                    card.ResetCardState();
                }
            }
        }

        private void MoveAllSelectedCardsToNewGroup(CardGroup dropTarget)
        {
            foreach (var card in activeCardGroup.GetSelectedCards())
            {
                if (card != this)
                {
                    card.activeCardGroup = dropTarget;
                    card.ResetCardState();
                }
            }
        }

        private void MoveToNewGroup(CardGroup dropTarget)
        {
            Vector3 localSnapPos = dropTarget.GetNextCardPosition();
            transform.SetParent(dropTarget.transform, false);
            transform.localPosition = localSnapPos;

            activeCardGroup.RearrangeCards();
            activeCardGroup = dropTarget;
            dropTarget.RearrangeCards();
        }

        private void RevertToPreviousPosition()
        {
            transform.SetParent(previousParentGroup, false);
            transform.position = preDragWorldPosition;

            if (isCardSelected)
            {
                ResetAllSelectedCards();
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
            CardGroup group = other.GetComponent<CardGroup>();
            if (group != null)
            {
                lastHoveredGroup = group;
            }
        }
    }
}