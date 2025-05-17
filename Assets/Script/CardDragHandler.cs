using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

namespace RedGaint.Games.Core
{
    public class CardDragHandler
    {
        // Dependencies
        private readonly Card card;
        private readonly Camera mainCamera;
        
        // State
        private Dictionary<Card, Vector3> dragOffsetPerCard = new Dictionary<Card, Vector3>();
        public bool IsDragging { get; private set; }
        public Vector3 CurrentDragPosition { get; private set; }

        public CardDragHandler(Card card, Camera mainCamera)
        {
            this.card = card;
            this.mainCamera = mainCamera;
        }

        public void BeginDrag(Vector3 pointerWorldPos)
        {
            IsDragging = true;
            dragOffsetPerCard.Clear();

            if (card.IsSelected)
            {
                foreach (var selectedCard in card.ActiveCardGroup.GetSelectedCards())
                {
                    StoreCardDragOffset(selectedCard, pointerWorldPos);
                    selectedCard.StorePreDragState();
                }
                card.ActiveCardGroup.RearrangeCardsFromSelection();
            }
            else
            {
                StoreCardDragOffset(card, pointerWorldPos);
                card.StorePreDragState();
            }
        }

        public void UpdateDrag(Vector3 pointerWorldPos)
        {
            CurrentDragPosition = pointerWorldPos;
            int index = 0;

            foreach (var kvp in dragOffsetPerCard)
            {
                var draggedCard = kvp.Key;
                Vector3 stackOffset = new Vector3(index * 0.2f, 0);
                draggedCard.transform.position = pointerWorldPos + stackOffset;
                index++;
            }
        }

        public void EndDrag()
        {
            IsDragging = false;
            dragOffsetPerCard.Clear();
        }

        private void StoreCardDragOffset(Card card, Vector3 pointerWorldPos)
        {
            dragOffsetPerCard[card] = card.transform.position - pointerWorldPos;
        }

        private Vector3 GetWorldPositionFromEventData(PointerEventData eventData)
        {
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCamera.transform.position.z);
            return mainCamera.ScreenToWorldPoint(screenPos);
        }
    }
}