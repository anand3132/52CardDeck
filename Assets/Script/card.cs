using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace RedGaint.Games.Core
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SortingGroup))]
    public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Camera mainCam;
        private Transform originalParent;
        private CardGroup currentGroup;
        private CardGroup lastTriggeredGroup = null;

        [SerializeField] private Transform visual;
        [SerializeField] private bool onDrag = false;
        [SerializeField] private bool isSelected = false;

        private Vector3 originalVisualLocalPosition;
        private Vector3 originalWorldPosition;

        private Dictionary<Card, Vector3> dragOffsets = new();

        private void Start()
        {
            mainCam = Camera.main;
            currentGroup = GetComponentInParent<CardGroup>();
            originalVisualLocalPosition = visual.localPosition;

            var visualRenderer = visual.GetComponent<SpriteRenderer>();
            var collider = GetComponent<BoxCollider2D>();

            if (visualRenderer && collider)
            {
                collider.size = visualRenderer.bounds.size;
                collider.offset = visual.localPosition;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onDrag) return;

            isSelected = !isSelected;
            visual.localPosition = isSelected ? originalVisualLocalPosition + Vector3.up * 0.3f : originalVisualLocalPosition;

            if (isSelected)
                currentGroup.AddSelectedCard(this);
            else
                currentGroup.RemoveSelectedCard(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            onDrag = true;
            Vector3 worldPos = GetWorldPosition(eventData.position);

            dragOffsets.Clear();

            if (isSelected)
                SetupDragOffsets(currentGroup.GetSelectedCards(), worldPos);
            else
                SetupDragOffsets(new[] { this }, worldPos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag = true;
            Vector3 worldPos = GetWorldPosition(eventData.position);
            foreach (var (card, offset) in dragOffsets)
                card.transform.position = worldPos + offset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onDrag = false;
            Vector3 worldPos = GetWorldPosition(eventData.position);

            var allGroups = GameObject.FindObjectsOfType<CardGroup>();
            CardGroup dropTarget = allGroups.FirstOrDefault(g => g.ContainsPoint(worldPos)) ?? lastTriggeredGroup;

            dragOffsets.Clear();

            if (dropTarget != null)
                HandleDropTarget(dropTarget, worldPos);
            else
                HandleNoDropTarget();

            lastTriggeredGroup = null;
        }

        public void Reset()
        {
            isSelected = false;
            onDrag = false;
            dragOffsets.Clear();
            lastTriggeredGroup = null;
            visual.localPosition = Vector3.zero;
        }

        private void HandleDropTarget(CardGroup dropTarget, Vector3 worldPos)
        {
            bool sameGroup = dropTarget == currentGroup;

            if (isSelected)
                HandleSelectedCardsTransfer(dropTarget, exclude: this);

            Reset();

            if (sameGroup)
            {
                transform.SetParent(dropTarget.transform, true);
                transform.position = originalWorldPosition;
            }
            else
            {
                transform.SetParent(dropTarget.transform, false);
                transform.localPosition = dropTarget.GetNextCardPosition();
                currentGroup = dropTarget;
            }

            dropTarget.RearrangeCards();
            originalParent = dropTarget.transform;
            originalWorldPosition = transform.position;
        }

        private void HandleNoDropTarget()
        {
            transform.SetParent(originalParent, false);
            transform.position = originalWorldPosition;

            if (isSelected)
                HandleSelectedCardsTransfer(null, exclude: this);

            Reset();
            currentGroup = originalParent?.GetComponent<CardGroup>();
            currentGroup.RearrangeCards();
        }

        private void HandleSelectedCardsTransfer(CardGroup newGroup, Card exclude)
        {
            foreach (var card in currentGroup.GetSelectedCards())
            {
                if (card != exclude)
                {
                    if (newGroup != null)
                        card.currentGroup = newGroup;
                    card.Reset();
                }
            }

            currentGroup.ClearSelectedCards();
        }

        private void SetupDragOffsets(IEnumerable<Card> cards, Vector3 worldPos)
        {
            foreach (var card in cards)
            {
                dragOffsets[card] = card.transform.position - worldPos;
                card.originalWorldPosition = card.transform.position;
                card.originalParent = card.transform.parent;
            }
        }

        private Vector3 GetWorldPosition(Vector2 screenPos)
        {
            var screenWorld = new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z);
            var worldPos = mainCam.ScreenToWorldPoint(screenWorld);
            worldPos.z = 0;
            return worldPos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out CardGroup group))
                lastTriggeredGroup = group;
        }
    }
}
