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
        // Inspector References
        [SerializeField] public Transform cardVisualTransform;
        
        // Systems
        private CardDragHandler dragHandler;
        private Camera mainCamera;
        
        // State
        public CardGroup ActiveCardGroup { get; set; }
        public CardGroup LastHoveredGroup { get; set; }
        public Transform PreviousParentGroup { get; set; }
        public Vector3 PreDragWorldPosition { get; set; }
        public bool IsSelected { get; private set; }
        public bool IsBeingDragged => dragHandler?.IsDragging ?? false;
        private Vector3 visualBaseLocalPosition;
        
        // Card Identification
        public string CardID { get; private set; }
        public int CardValue { get; private set; } 
        public string CardSuit { get; private set; } 

        #region Initialization
        private void Start()
        {
            mainCamera = Camera.main;
            ActiveCardGroup = GetComponentInParent<CardGroup>();
            visualBaseLocalPosition = cardVisualTransform.localPosition;
            dragHandler = new CardDragHandler(this, mainCamera);
            SetupCollider();
        }

        public void InitializeCard(string cardID)
        {
            CardID = cardID;
        
            // Parse suit (first character)
            if (cardID.Length > 0)
            {
                CardSuit = cardID[0].ToString().ToUpper();
            }

            // Parse value (remaining characters)
            string valuePart = cardID.Length > 1 ? cardID.Substring(1) : "";
            CardValue = ParseCardValue(valuePart);
        }
        private int ParseCardValue(string valuePart)
        {
            switch (valuePart.ToUpper())
            {
                case "A": return 1;
                case "J": return 11;
                case "Q": return 12;
                case "K": return 13;
                default:
                    if (int.TryParse(valuePart, out int numericValue))
                        return numericValue;
                    return 0; // Invalid value
            }
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
        #endregion

        #region Input Handlers
        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsBeingDragged) return;
            ToggleSelection();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            dragHandler.BeginDrag(worldPos);

            GameEventSystem.Trigger(new RequestRearrangeCardEvent{Group = ActiveCardGroup});
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            worldPos.z = 0;
            dragHandler.UpdateDrag(worldPos);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragHandler.EndDrag();
            
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            worldPos.z = 0;

            var dropTarget = FindDropTarget(worldPos);
            if (dropTarget != null)
                ProcessDrop(dropTarget, worldPos);
            else
                RevertToPreviousPosition();
        }
        #endregion

        #region Core Functionality
        public void StorePreDragState()
        {
            PreDragWorldPosition = transform.position;
            PreviousParentGroup = transform.parent;
        }

        private CardGroup FindDropTarget(Vector3 worldPos)
        {
            var allGroups = FindObjectsOfType<CardGroup>();
            return allGroups.FirstOrDefault(g => g.ContainsPoint(worldPos)) ?? LastHoveredGroup;
        }

        private void ProcessDrop(CardGroup dropTarget, Vector3 worldPos)
        {
            if (dropTarget == ActiveCardGroup)
                HandleSameGroupDrop(dropTarget);
            else
                HandleDifferentGroupDrop(dropTarget);
        }
        #endregion

        #region Selection Logic
        private void ToggleSelection()
        {
            if (!IsSelected) Select();
            else Deselect();
        }

        private void Select()
        {
            visualBaseLocalPosition = cardVisualTransform.localPosition;
            cardVisualTransform.localPosition += Vector3.up * 0.3f;
            IsSelected = true;
            if(ActiveCardGroup!=null)
                ActiveCardGroup.AddSelectedCard(this);
            else
            {
                Deselect();
            }
        }

        private void Deselect()
        {
            cardVisualTransform.localPosition = visualBaseLocalPosition;
            IsSelected = false;
            ActiveCardGroup.RemoveSelectedCard(this);
        }
        #endregion

        #region Drop Handling
        private void HandleSameGroupDrop(CardGroup dropTarget)
        {
            transform.SetParent(dropTarget.transform, true);
            transform.position = PreDragWorldPosition;

            if (IsSelected)
            {
                ResetAllSelectedCards();
                ActiveCardGroup.ClearSelectedCards();
            }

            ResetCardState();
            GameEventSystem.Trigger(new RequestRearrangeCardEvent{Group = ActiveCardGroup});
        }

        private void HandleDifferentGroupDrop(CardGroup dropTarget)
        {
            if (IsSelected)
            {
                ResetAllSelectedCards();
                MoveAllSelectedCardsToNewGroup(dropTarget);
                ActiveCardGroup.ClearSelectedCards();
            }

            ResetCardState();
            MoveToNewGroup(dropTarget);
            GameEventSystem.Trigger(new RequestRearrangeCardEvent{Group = ActiveCardGroup});

        }

        private void RevertToPreviousPosition()
        {
            transform.SetParent(PreviousParentGroup, false);
            transform.position = PreDragWorldPosition;

            if (IsSelected)
            {
                ResetAllSelectedCards();
                ActiveCardGroup = PreviousParentGroup?.GetComponent<CardGroup>();
                ActiveCardGroup.ClearSelectedCards();
            }

            ResetCardState();
            GameEventSystem.Trigger(new RequestRearrangeCardEvent{Group = ActiveCardGroup});
        }
        #endregion

        #region Helper Methods
        private Vector3 GetWorldPositionFromEventData(PointerEventData eventData)
        {
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCamera.transform.position.z);
            return mainCamera.ScreenToWorldPoint(screenPos);
        }

        public void ResetCardState()
        {
            IsSelected = false;
            cardVisualTransform.localPosition = Vector3.zero;
            LastHoveredGroup = null;
        }

        public void ResetAllSelectedCards()
        {
            Debug.Log(ActiveCardGroup.name);
            foreach (var card in ActiveCardGroup.GetSelectedCards())
                if (card != this) card.ResetCardState();
        }

        public void MoveAllSelectedCardsToNewGroup(CardGroup dropTarget)
        {
            foreach (var card in ActiveCardGroup.GetSelectedCards())
                if (card != this) card.MoveToNewGroup(dropTarget);
        }

        public void MoveToNewGroup(CardGroup dropTarget)
        {
            Vector3 localSnapPos = dropTarget.GetNextCardPosition();
            transform.SetParent(dropTarget.transform, false);
            transform.localPosition = localSnapPos;

            if (ActiveCardGroup.IsGroupEmpty())
            {
                GameEventSystem.Trigger(new RequestGroupDestroyEvent(){Group = ActiveCardGroup});
            }
            else
            {
                GameEventSystem.Trigger(new RequestRearrangeCardEvent{Group = ActiveCardGroup});
            }
            
            ActiveCardGroup = dropTarget;
            
            GameEventSystem.Trigger(new RequestRearrangeCardEvent{Group = dropTarget});
        }
        #endregion

        private void OnTriggerEnter2D(Collider2D other)
        {
            var group = other.GetComponent<CardGroup>();
            if (group != null) LastHoveredGroup = group;
        }
    }
}