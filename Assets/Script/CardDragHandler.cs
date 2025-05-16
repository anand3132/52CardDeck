using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

namespace RedGaint.Games.Core
{
    public class CardDragHandler
    {
        private readonly Card card;
        private readonly Camera mainCamera;
        private Dictionary<Card, Vector3> dragOffsetPerCard = new Dictionary<Card, Vector3>();

        public bool IsDragging { get; private set; }

        public CardDragHandler(Card card, Camera mainCamera)
        {
            this.card = card;
            this.mainCamera = mainCamera;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("---------------- OnBeginDrag ----------------");
            
            IsDragging = true;
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);

            dragOffsetPerCard.Clear();

            if (card.IsCardSelected)
            {
                Debug.Log("Selected, dragging multiple cards...");
                SetupMultiCardDrag(worldPos);
                card.ActiveCardGroup.RearrangeCardsFromSelection();
            }
            else
            {
                SetupSingleCardDrag(worldPos);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            IsDragging = true;
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            worldPos.z = 0;

            UpdateCardPositionsDuringDrag(worldPos);

            if (dragOffsetPerCard.Count > 1)
            {
                GroupManager.Instance.ShowGroupPreviewAt(worldPos);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("---------------- OnEndDrag ----------------");

            IsDragging = false;
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            worldPos.z = 0;

            CardGroup dropTarget = FindDropTarget(worldPos);
            dragOffsetPerCard.Clear();

            if (dropTarget != null)
                card.ProcessSuccessfulDrop(dropTarget, worldPos);
            else
                card.RevertToPreviousPosition();

            card.LastHoveredGroup = null;
            Debug.Log("---------------- OnEndDrag Finished ----------------");
        }

        private Vector3 GetWorldPositionFromEventData(PointerEventData eventData)
        {
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCamera.transform.position.z);
            return mainCamera.ScreenToWorldPoint(screenPos);
        }

        private void SetupMultiCardDrag(Vector3 worldPos)
        {
            foreach (var selectedCard in card.ActiveCardGroup.GetSelectedCards())
            {
                dragOffsetPerCard[selectedCard] = selectedCard.transform.position - worldPos;
                selectedCard.PreDragWorldPosition = selectedCard.transform.position;
                selectedCard.PreviousParentGroup = selectedCard.transform.parent;
            }
        }

        private void SetupSingleCardDrag(Vector3 worldPos)
        {
            dragOffsetPerCard[card] = card.transform.position - worldPos;
            card.PreDragWorldPosition = card.transform.position;
            card.PreviousParentGroup = card.transform.parent;
        }

        private void UpdateCardPositionsDuringDrag(Vector3 worldPos)
        {
            int index = 0;
            foreach (var kvp in dragOffsetPerCard)
            {
                var draggedCard = kvp.Key;
                Vector3 stackOffset = new Vector3(index * 0.2f, 0);
                draggedCard.transform.position = worldPos + stackOffset;
                index++;
            }
        }

        private CardGroup FindDropTarget(Vector3 worldPos)
        {
            var allGroups = GameObject.FindObjectsOfType<CardGroup>();
            CardGroup dropTarget = allGroups.FirstOrDefault(g => g.ContainsPoint(worldPos));
            return dropTarget ?? card.LastHoveredGroup;
        }
    }
}