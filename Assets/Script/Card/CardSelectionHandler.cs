using UnityEngine;

namespace RedGaint.Games.Core
{
    public class CardSelectionHandler
    {
        private readonly Card card;
        private readonly Transform cardVisualTransform;
        private Vector3 visualBaseLocalPosition;
        
        public bool IsSelected { get; private set; }

        public CardSelectionHandler(Card card, Transform visualTransform)
        {
            this.card = card;
            this.cardVisualTransform = visualTransform;
            visualBaseLocalPosition = visualTransform.localPosition;
        }

        public void Select()
        {
            visualBaseLocalPosition = cardVisualTransform.localPosition;
            cardVisualTransform.localPosition += Vector3.up * 0.3f;
            IsSelected = true;
            card.ActiveCardGroup.AddSelectedCard(card);
        }

        public void Deselect()
        {
            cardVisualTransform.localPosition = visualBaseLocalPosition;
            IsSelected = false;
            card.ActiveCardGroup.RemoveSelectedCard(card);
        }

        public void ToggleSelection()
        {
            if (!IsSelected) Select();
            else Deselect();
        }
    }
}