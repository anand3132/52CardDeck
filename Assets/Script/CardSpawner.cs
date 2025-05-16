using UnityEngine;

namespace RedGaint.Games.Core
{
    public class CardSpawner : MonoBehaviour
    {
        [Header("Card Setup")]
        public GameObject cardPrefab;
        public int numberOfCards = 5;

        [Header("Spawn Settings")]
        public Vector2 spacing = new Vector2(0.5f, 0f); // Horizontal spacing between cards
        public Vector2 startOffset = Vector2.zero;

        [Header("Sorting Settings")]
        public string sortingLayerName = "Cards";

        private void Start()
        {
            if (cardPrefab == null || GroupManager.Instance == null)
            {
                Debug.LogWarning("CardSpawner is missing required references.");
                return;
            }

            SpawnCards();
        }

        public void SpawnCards()
        {
            // Create a new group using the GroupManager
            GameObject group = GroupManager.Instance.CreateEmptyGroup();
            if (group == null)
            {
                Debug.LogError("Failed to create initial card group.");
                return;
            }

            for (int i = 0; i < numberOfCards; i++)
            {
                GameObject card = Instantiate(cardPrefab, transform);
                Vector3 localOffset = new Vector3(i * spacing.x, i * spacing.y, 0);
                card.transform.position = group.transform.position + (Vector3)startOffset + localOffset;
                card.transform.SetParent(group.transform, true);
            }

            // Optional: Rearrange if your group prefab has logic
            group.GetComponent<CardGroup>()?.RearrangeCards();
        }
    }
}