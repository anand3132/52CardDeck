using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace RedGaint.Games.Core
{
    public class CardSpawner : MonoBehaviour
    {
        [Header("Paths")]
        public string jsonPath = "Assets/Config/card_data.json";
        public string spriteFolder = "Art/CardDeck"; 

        [Header("Settings")]
        public GameObject cardPrefab;
        public Vector2 spacing = new Vector2(0.5f, 0f);
        public Vector2 startOffset = Vector2.zero;
        public int sortingOrderBase = 0;
        public string sortingLayerName = "Cards";

        private Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
        private List<string> cardCodesFromJson = new List<string>();

        private void Start()
        {
            LoadAllSprites();
            LoadJsonData();
            SpawnValidatedCards();
        }

        private void LoadAllSprites()
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(spriteFolder);
            
            foreach (Sprite sprite in sprites)
            {
                string key = sprite.name;
                if (!loadedSprites.ContainsKey(key))
                {
                    loadedSprites.Add(key, sprite);
                }
            }
            Debug.Log($"Loaded {sprites.Length} sprites from Resources/{spriteFolder}");
        }

        private void LoadJsonData()
        {
            string fullPath = Path.Combine(Application.dataPath, jsonPath.Replace("Assets/", ""));
            
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"JSON file not found at: {fullPath}");
                return;
            }

            string json = File.ReadAllText(fullPath);
            CardDataWrapper wrapper = JsonUtility.FromJson<CardDataWrapper>(json);
            cardCodesFromJson = wrapper.data.deck;
            Debug.Log($"Loaded {cardCodesFromJson.Count} card codes from JSON");
        }

        private void SpawnValidatedCards()
        {
            if (cardPrefab == null || GroupManager.Instance == null)
            {
                Debug.LogError("Missing required references (prefab or GroupManager)");
                return;
            }

            var group = GroupManager.Instance.CreateEmptyGroup();
            if (group == null)
            {
                Debug.LogError("Failed to create card group");
                return;
            }

            int spawnedCount = 0;
            int missingCount = 0;

            for (int i = 0; i < cardCodesFromJson.Count; i++)
            {
                string cardCode = cardCodesFromJson[i];
                
                if (loadedSprites.TryGetValue(cardCode, out Sprite sprite))
                {
                    SpawnCard(cardCode, sprite, i, group.transform);
                    spawnedCount++;
                }
                else
                {
                    Debug.LogError($"Missing sprite for card: {cardCode}");
                    missingCount++;
                }
            }

            Debug.Log($"Spawned {spawnedCount} cards, {missingCount} missing sprites");
            GameEventSystem.Trigger(new RequestRearrangeCardEvent{Group = group,Immediate=false});
        }

        private void SpawnCard(string cardCode, Sprite sprite, int index, Transform parent)
        {
            GameObject card = Instantiate(cardPrefab, parent);
            Vector3 localOffset = new Vector3(index * spacing.x, index * spacing.y, 0);
            card.transform.position = parent.position + (Vector3)startOffset + localOffset;
            card.name = $"Card_{cardCode}";

            SetCardVisual(card, sprite, index);
        }

        private void SetCardVisual(GameObject card, Sprite sprite, int order)
        {
            Card cardComponent = card.GetComponent<Card>();
            if (cardComponent?.cardVisualTransform == null)
            {
                Debug.LogError("Invalid card prefab setup");
                return;
            }

            SpriteRenderer renderer = cardComponent.cardVisualTransform.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogError("Missing SpriteRenderer on card");
                return;
            }

            renderer.sprite = sprite;
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrderBase + order;
        }

        [System.Serializable]
        private class CardDataWrapper
        {
            public DeckData data;
        }

        [System.Serializable]
        private class DeckData
        {
            public List<string> deck;
        }
    }
}