using UnityEngine;

namespace RedGaint.Games.Core
{
    public class CardDropHandler
    {
        private readonly Card card;
        
        public CardDropHandler(Card card)
        {
            this.card = card;
        }

        public void ProcessDrop(CardGroup dropTarget, Vector3 worldPos)
        {
            if (dropTarget == card.ActiveCardGroup)
                HandleSameGroupDrop(dropTarget);
            else
                HandleDifferentGroupDrop(dropTarget);
        }

        private void HandleSameGroupDrop(CardGroup dropTarget)
        {
            card.transform.SetParent(dropTarget.transform, true);
            card.transform.position = card.PreDragWorldPosition;

            if (card.IsSelected)
            {
                card.ResetAllSelectedCards();
                card.ActiveCardGroup.ClearSelectedCards();
            }

            card.ResetCardState();
            card.ActiveCardGroup.RearrangeCards();
        }

        private void HandleDifferentGroupDrop(CardGroup dropTarget)
        {
            if (card.IsSelected)
            {
                card.ResetAllSelectedCards();
                card.MoveAllSelectedCardsToNewGroup(dropTarget);
                card.ActiveCardGroup.ClearSelectedCards();
            }
            else
            {
                card.ResetCardState();
                card.MoveToNewGroup(dropTarget);
            }
        }
    }
}