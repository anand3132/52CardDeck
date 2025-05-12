using UnityEngine;

namespace RedGaint.Games.Core
{
    public class CardSpawner : MonoBehaviour
    {
        [Header("Card Setup")]
        public GameObject cardPrefab;
        public int numberOfCards = 5;

        [Header("Target Group")]
        public Transform initialGroup; // Group where cards will be spawned 

        [Header("Spawn Settings")]
        public Vector2 spacing = new Vector2(0.6f, 0f); // Horizontal spacing between cards
        public Vector2 startOffset = Vector2.zero;

        private void Start()
        {
            if (cardPrefab == null || initialGroup == null)
            {
                Debug.LogWarning("CardSpawner is missing references.");
                return;
            }

            SpawnCards();
        }

        private void SpawnCards()
        {
            for (int i = 0; i < numberOfCards; i++)
            {
                GameObject card = Instantiate(cardPrefab, transform); // temp parent
                Vector3 localOffset = new Vector3(i * spacing.x, i * spacing.y, 0);
                card.transform.position = initialGroup.position + (Vector3)startOffset + localOffset;
                card.transform.SetParent(initialGroup, true); // true = keep world pos

                // Set sorting order based on index
                SpriteRenderer renderer = card.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = i; // Higher index = on top
                }
            }
        }
    }
}