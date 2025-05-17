using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedGaint.Games.Core
{
    public enum RummySequenceType
    {
        Invalid,
        PureSequence,    // Consecutive same suit, no jokers
        ImpureSequence,  // Consecutive same suit with jokers
        Set,             // Same value, different suits
        MixedSet,        // Same value with jokers
        JokerSet         // All jokers
    }

    public class RummySequenceAnalyzer : MonoBehaviour
    {
        public static RummySequenceAnalyzer Instance { get; private set; }

        [SerializeField] private string jokerCardID = "JK"; // Configurable joker ID

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public List<RummySequenceType> AnalyzeSequences(List<string> cardIDs)
        {
            List<RummySequenceType> results = new List<RummySequenceType>();
            List<Card> cards = ConvertToCards(cardIDs);

            // Check all possible valid sequences
            if (IsPureSequence(cards)) results.Add(RummySequenceType.PureSequence);
            if (IsImpureSequence(cards)) results.Add(RummySequenceType.ImpureSequence);
            if (IsSet(cards)) results.Add(RummySequenceType.Set);
            if (IsMixedSet(cards)) results.Add(RummySequenceType.MixedSet);
            if (IsJokerSet(cards)) results.Add(RummySequenceType.JokerSet);

            return results.Count > 0 ? results : new List<RummySequenceType> { RummySequenceType.Invalid };
        }

        public List<List<string>> FindAllPossibleSets(List<string> cardIDs)
        {
            List<List<string>> allSets = new List<List<string>>();
            List<Card> cards = ConvertToCards(cardIDs);

            // Group by card value
            var valueGroups = cards
                .Where(c => !IsJoker(c.CardID))
                .GroupBy(c => c.CardValue)
                .OrderByDescending(g => g.Count());

            foreach (var group in valueGroups)
            {
                // Get all cards with this value
                List<string> set = group.Select(c => c.CardID).ToList();

                // Add jokers if available
                int jokerCount = cards.Count(c => IsJoker(c.CardID));
                if (jokerCount > 0)
                {
                    set.AddRange(cards
                        .Where(c => IsJoker(c.CardID))
                        .Take(jokerCount)
                        .Select(c => c.CardID));
                }

                if (set.Count >= 3) // Minimum cards for a set
                {
                    allSets.Add(set);
                }
            }

            return allSets;
        }

        #region Sequence Validation Methods
        private bool IsPureSequence(List<Card> cards)
        {
            if (cards.Count < 3) return false;
            if (cards.Any(c => IsJoker(c.CardID))) return false;

            string suit = cards[0].CardSuit;
            var sortedCards = cards
                .Where(c => c.CardSuit == suit)
                .OrderBy(c => c.CardValue)
                .ToList();

            return CheckConsecutiveValues(sortedCards, 0);
        }

        private bool IsImpureSequence(List<Card> cards)
        {
            if (cards.Count < 3) return false;

            // Separate jokers and normal cards
            var jokers = cards.Where(c => IsJoker(c.CardID)).ToList();
            var normalCards = cards.Where(c => !IsJoker(c.CardID)).ToList();

            if (normalCards.Count == 0) return false;

            // Group by suit
            var suitGroups = normalCards.GroupBy(c => c.CardSuit);
            
            foreach (var group in suitGroups)
            {
                var sortedCards = group.OrderBy(c => c.CardValue).ToList();
                int requiredJokers = CalculateRequiredJokers(sortedCards);

                if (jokers.Count >= requiredJokers && 
                    (sortedCards.Count + jokers.Count) >= 3)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsSet(List<Card> cards)
        {
            if (cards.Count < 3) return false;
            if (cards.Any(c => IsJoker(c.CardID))) return false;

            int firstValue = cards[0].CardValue;
            return cards.All(c => c.CardValue == firstValue) && 
                   cards.Select(c => c.CardSuit).Distinct().Count() == cards.Count;
        }

        private bool IsMixedSet(List<Card> cards)
        {
            if (cards.Count < 3) return false;

            var normalCards = cards.Where(c => !IsJoker(c.CardID)).ToList();
            if (normalCards.Count == 0) return false;

            int firstValue = normalCards[0].CardValue;
            bool sameValue = normalCards.All(c => c.CardValue == firstValue);
            bool uniqueSuits = normalCards.Select(c => c.CardSuit).Distinct().Count() == normalCards.Count;

            return sameValue && uniqueSuits;
        }

        private bool IsJokerSet(List<Card> cards)
        {
            return cards.Count >= 3 && cards.All(c => IsJoker(c.CardID));
        }
        #endregion

        #region Helper Methods
        private List<Card> ConvertToCards(List<string> cardIDs)
        {
            List<Card> cards = new List<Card>();
            foreach (string id in cardIDs)
            {
                Card tempCard = new GameObject("TempCard").AddComponent<Card>();
                tempCard.InitializeCard(id);
                cards.Add(tempCard);
                // Note: These temporary GameObjects should be destroyed after analysis
                Destroy(tempCard.gameObject);
            }
            return cards;
        }

        private bool IsJoker(string cardID)
        {
            return cardID.Equals(jokerCardID, System.StringComparison.OrdinalIgnoreCase);
        }

        private bool CheckConsecutiveValues(List<Card> cards, int allowedGaps)
        {
            if (cards.Count < 3) return false;

            int gapCount = 0;
            for (int i = 1; i < cards.Count; i++)
            {
                int expectedValue = cards[i-1].CardValue + 1;
                int actualValue = cards[i].CardValue;

                if (actualValue != expectedValue)
                {
                    gapCount += (actualValue - expectedValue);
                    if (gapCount > allowedGaps) return false;
                }
            }

            return true;
        }

        private int CalculateRequiredJokers(List<Card> sortedCards)
        {
            int requiredJokers = 0;
            for (int i = 1; i < sortedCards.Count; i++)
            {
                int diff = sortedCards[i].CardValue - sortedCards[i-1].CardValue - 1;
                requiredJokers += diff;
            }
            return requiredJokers;
        }
        #endregion
    }
}