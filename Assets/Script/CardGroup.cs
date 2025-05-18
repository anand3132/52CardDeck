using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
using Random = System.Random;

namespace RedGaint.Games.Core
{
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider2D))]
    public class CardGroup : MonoBehaviour
    {
        public Vector2 size = new Vector2(5f, 2f);
        public Vector2 slotSpacing = new Vector2(0.3f, 0.0f);
        public int maxSlots = 54;
        public SpriteRenderer defaultSpriteRenderer;
        private OutlineDrawer outlineDrawer;
        private BoxCollider2D boxCollider;
        [SerializeField] private  List<Card> SelectedCards = new List<Card>();
        
        private void Start()
        {
            outlineDrawer = GetComponent<OutlineDrawer>();
            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            UpdateColliderSize();

        }

        private void OnEnable()
        {
            GameEventSystem.Subscribe<RequestRearrangeCardEvent>(RearrangeCardsOnEvent);
        }

        private void OnDisable()
        {
            GameEventSystem.Unsubscribe<RequestRearrangeCardEvent>(RearrangeCardsOnEvent);
        }
        private  void RearrangeCardsOnEvent(RequestRearrangeCardEvent eventData)
        {
            if(eventData.Group == this)
                RearrangeCards();
        }


        public void AddSelectedCard(Card card)
        {
            if (card == null)
                return;
            if( SelectedCards.Contains(card))
                return;
            
            SelectedCards.Add(card);
        }
        public void RemoveSelectedCard(Card card)
        {
            if (card == null || !SelectedCards.Contains(card))
                return;
        
            SelectedCards.Remove(card);
    
            // Proper check for empty group
            var cardsInGroup = GetComponentsInChildren<Card>();
            if (cardsInGroup.Length == 0)
            {
                GameEventSystem.Trigger(new RequestGroupDestroyEvent { Group = this ,Immediate = false});
            }
        }
        public List<Card> GetSelectedCards()
        {
            Debug.Log("<color=red>GetSelectedCards:count  </color>"+SelectedCards.Count);
            return SelectedCards;
        }
        public void ClearSelectedCards()
        {
            SelectedCards.Clear();
        }
        public Vector3 GetNextCardPosition()
        {
            List<Vector3> allSlots = GetAllSlotPositions();
            int index = Mathf.Min(transform.childCount, allSlots.Count - 1);
            return allSlots[index];
        }
        private List<Vector3> GetAllSlotPositions()
        {
            List<Vector3> slots = new List<Vector3>();
            float totalWidth = (maxSlots - 1) * slotSpacing.x;

            Vector2 startOffset = Vector2.zero;
            for (int i = 0; i < maxSlots; i++)
            {
                Vector3 slot = new Vector3(
                    startOffset.x + i * slotSpacing.x,
                    startOffset.y,
                    0f
                );
                slots.Add(slot);
            }

            return slots;
        }
        private Rect GetWorldRect()
        {
            Vector2 center = transform.localPosition;
            return new Rect(center - size * 0.5f, size);
        }
        public bool ContainsPoint(Vector2 worldPoint)
        {
            return GetWorldRect().Contains(worldPoint);
        }
        private void RearrangeCards()
        {
            List<Vector3> slotPositions = GetAllSlotPositions();
            int count = Mathf.Min(transform.childCount, slotPositions.Count);

            for (int i = 0; i < count; i++)
            {
                Transform child = transform.GetChild(i);
                Vector3 slot = slotPositions[i];

                // Apply slot position and z-offset
                slot.z = -i * 0.01f;
                child.localPosition = slot;

                // Update SortingGroup order
                var sortingGroup = child.GetComponent<SortingGroup>();
                if (sortingGroup != null)
                {
                    sortingGroup.sortingOrder = i + 1;
                }
            }

            ReDrawOutline();
            UpdateColliderSize();
        }
        public void RearrangeCardsFromSelection()
        {
            List<Vector3> slotPositions = GetAllSlotPositions();
            int count = Mathf.Min(SelectedCards.Count, slotPositions.Count);

            for (int i = 0; i < count; i++)
            {
                Card card = SelectedCards[i];
                Vector3 slot = slotPositions[i];
                slot.z = -i * 0.01f;
                card.transform.localPosition = slot;

                var sortingGroup = card.GetComponent<SortingGroup>();
                if (sortingGroup != null)
                {
                    sortingGroup.sortingOrder = i + 1;
                }
            }

            ReDrawOutline();
            UpdateColliderSize();
        }
        public List<string> GetCardIDs()
        {
            List<string> cardIDs = new List<string>();
    
            // Get all Card components from children
            Card[] cards = GetComponentsInChildren<Card>();
    
            foreach (Card card in cards)
            {
                if (!string.IsNullOrEmpty(card.CardID))
                {
                    cardIDs.Add(card.CardID);
                }
            }
    
            return cardIDs;
        }
        public List<string> GetSelectedCardIDs()
        {
            List<string> cardIDs = new List<string>();
    
            foreach (Card card in SelectedCards)
            {
                if (!string.IsNullOrEmpty(card.CardID))
                {
                    cardIDs.Add(card.CardID);
                }
            }
    
            return cardIDs;
        }
        private void ReDrawOutline()
        {
            if (outlineDrawer != null)
            {
                outlineDrawer.RedrawFromCard();
            }
        }
        private void UpdateColliderSize()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider2D>();

            if (defaultSpriteRenderer != null)
            {
                Bounds bounds = defaultSpriteRenderer.bounds;

                Vector2 localSize = transform.InverseTransformVector(bounds.size);
                Vector2 localCenter = (Vector2)transform.InverseTransformPoint(bounds.center);

                boxCollider.size = localSize;
                boxCollider.offset = localCenter;
            }
            else
            {
                int cardCount = Mathf.Max(transform.childCount, 1);
                float width = Mathf.Min(cardCount, maxSlots) * slotSpacing.x;
                float height = size.y;

                boxCollider.size = new Vector2(width, height);
                float offsetX = width / 2f;
 
                boxCollider.offset = new Vector2(offsetX, 0f);
            }
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other != null && other.transform.parent != this.transform)
            {
                other.transform.SetParent(transform);
                RearrangeCards();
            }
        }

        public bool IsGroupEmpty()
        {
            Debug.Log("IsGroupEmpty: " + transform.childCount + "");
            return transform.childCount == 0;
        }
    }
}
