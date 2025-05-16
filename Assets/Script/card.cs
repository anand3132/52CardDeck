using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace RedGaint.Games.Core
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SortingGroup))]
    public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // References
        private Camera mainCamera;
        [SerializeField] private Transform cardVisualTransform;

        // Positions
        private Vector3 visualBaseLocalPosition;

        // Drag handler
        private CardDragHandler dragHandler;

        // Properties for external access
        public Transform PreviousParentGroup { get; set; }
        public CardGroup ActiveCardGroup { get; set; }
        public CardGroup LastHoveredGroup { get; set; }
        public Vector3 PreDragWorldPosition { get; set; }
        public bool IsCardSelected { get; private set; }
        public bool IsBeingDragged => dragHandler?.IsDragging ?? false;

        private void Start()
        {
            mainCamera = Camera.main;
            ActiveCardGroup = GetComponentInParent<CardGroup>();
            visualBaseLocalPosition = cardVisualTransform.localPosition;
            dragHandler = new CardDragHandler(this, mainCamera);

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
            Debug.Log("---------------- OnPointerClick ----------------");

            if (IsBeingDragged) return;

            ToggleCardSelection();
        }

        private void ToggleCardSelection()
        {
            if (!IsCardSelected)
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
            IsCardSelected = true;
            ActiveCardGroup.AddSelectedCard(this);
        }

        private void DeselectCard()
        {
            cardVisualTransform.localPosition = visualBaseLocalPosition;
            IsCardSelected = false;
            ActiveCardGroup.RemoveSelectedCard(this);
        }

        public void ProcessSuccessfulDrop(CardGroup dropTarget, Vector3 worldPos)
        {
            Debug.Log("---------------- ProcessSuccessfulDrop ----------------");

            if (dropTarget == ActiveCardGroup)
            {
                HandleSameGroupDrop(dropTarget);
            }
            else
            {
                HandleDifferentGroupDrop(dropTarget);
            }

            PreviousParentGroup = dropTarget.transform;
            PreDragWorldPosition = transform.position;
        }

        public void RevertToPreviousPosition()
        {
            Debug.Log("---------------- RevertToPreviousPosition ----------------");

            transform.SetParent(PreviousParentGroup, false);
            transform.position = PreDragWorldPosition;

            if (IsCardSelected)
            {
                ResetAllSelectedCards();
                ResetCardState();
                ActiveCardGroup.ClearSelectedCards();
                ActiveCardGroup = PreviousParentGroup?.GetComponent<CardGroup>();
            }

            ActiveCardGroup.RearrangeCards();
        }

        public void ResetCardState()
        {
            IsCardSelected = false;
            dragHandler = new CardDragHandler(this, mainCamera);
            LastHoveredGroup = null;
            cardVisualTransform.localPosition = Vector3.zero;
        }

        private void HandleSameGroupDrop(CardGroup dropTarget)
        {
            transform.SetParent(dropTarget.transform, true);
            transform.position = PreDragWorldPosition;

            if (IsCardSelected)
            {
                ResetAllSelectedCards();
                ActiveCardGroup.ClearSelectedCards();
            }

            ResetCardState();
            ActiveCardGroup.RearrangeCards();
        }

        private void HandleDifferentGroupDrop(CardGroup dropTarget)
        {
            if (IsCardSelected)
            {
                MoveAllSelectedCardsToNewGroup(dropTarget);
                ActiveCardGroup.ClearSelectedCards();
            }

            ResetCardState();
            MoveToNewGroup(dropTarget);
        }

        private void ResetAllSelectedCards()
        {
            foreach (var selectedCard in ActiveCardGroup.GetSelectedCards())
            {
                if (selectedCard != this)
                {
                    selectedCard.ResetCardState();
                }
            }
        }

        private void MoveAllSelectedCardsToNewGroup(CardGroup dropTarget)
        {
            foreach (var selectedCard in ActiveCardGroup.GetSelectedCards())
            {
                if (selectedCard != this)
                {
                    selectedCard.ActiveCardGroup = dropTarget;
                    selectedCard.ResetCardState();
                }
            }
        }

        private void MoveToNewGroup(CardGroup dropTarget)
        {
            Vector3 localSnapPos = dropTarget.GetNextCardPosition();
            transform.SetParent(dropTarget.transform, false);
            transform.localPosition = localSnapPos;

            ActiveCardGroup.RearrangeCards();
            ActiveCardGroup = dropTarget;
            dropTarget.RearrangeCards();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("---------------- OnTriggerEnter2D ----------------");

            CardGroup group = other.GetComponent<CardGroup>();
            if (group != null)
            {
                LastHoveredGroup = group;
            }
        }

        // Implement drag interfaces by delegating to the drag handler
        public void OnBeginDrag(PointerEventData eventData) => dragHandler.OnBeginDrag(eventData);
        public void OnDrag(PointerEventData eventData) => dragHandler.OnDrag(eventData);
        public void OnEndDrag(PointerEventData eventData) => dragHandler.OnEndDrag(eventData);
    }
}