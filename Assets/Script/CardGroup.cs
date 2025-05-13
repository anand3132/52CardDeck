using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace RedGaint.Games.Core
{
    public enum GroupPivot
    {
        Center,
        Left,
        Right
    }

    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider2D))]
    public class CardGroup : MonoBehaviour
    {
        public Vector2 size = new Vector2(5f, 2f);
        public Vector2 slotSpacing = new Vector2(0.6f, 1.0f);
        public int maxSlots = 10;
        public GroupPivot pivot = GroupPivot.Left;

        private OutlineDrawer outlineDrawer;
        private BoxCollider2D boxCollider;

        void Start()
        {
            outlineDrawer = GetComponent<OutlineDrawer>();
            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            UpdateColliderSize();
        }

        public Vector3 GetNextCardPosition()
        {
            List<Vector3> allSlots = GetAllSlotPositions();
            int index = Mathf.Min(transform.childCount, allSlots.Count - 1);
            return allSlots[index];
        }

        public List<Vector3> GetAllSlotPositions()
        {
            List<Vector3> slots = new List<Vector3>();
            float totalWidth = (maxSlots - 1) * slotSpacing.x;

            Vector2 startOffset = pivot switch
            {
                GroupPivot.Left => Vector2.zero,
                GroupPivot.Right => new Vector2(-totalWidth, 0f),
                GroupPivot.Center => new Vector2(-totalWidth / 2f, 0f),
                _ => Vector2.zero
            };

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

        public Rect GetWorldRect()
        {
            Vector2 center = transform.localPosition;
            return new Rect(center - size * 0.5f, size);
        }

        public bool ContainsPoint(Vector2 worldPoint)
        {
            return GetWorldRect().Contains(worldPoint);
        }

        public void RearrangeCards()
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

        public void ReDrawOutline()
        {
            if (outlineDrawer != null)
            {
                outlineDrawer.RedrawFromCard();
            }
        }
        public SpriteRenderer defaultSpriteRenderer;

        private void UpdateColliderSize()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider2D>();

            if (defaultSpriteRenderer != null)
            {
                Bounds bounds = defaultSpriteRenderer.bounds;

                // Convert world size to local size relative to parent
                Vector2 localSize = transform.InverseTransformVector(bounds.size);
                Vector2 localCenter = (Vector2)transform.InverseTransformPoint(bounds.center);

                boxCollider.size = localSize;
                boxCollider.offset = localCenter;
            }
            else
            {
                // fallback to calculated slot-based sizing
                int cardCount = Mathf.Max(transform.childCount, 1);
                float width = Mathf.Min(cardCount, maxSlots) * slotSpacing.x;
                float height = size.y;

                boxCollider.size = new Vector2(width, height);

                float offsetX = pivot switch
                {
                    GroupPivot.Left => width / 2f,
                    GroupPivot.Center => 0f,
                    GroupPivot.Right => -width / 2f,
                    _ => 0f
                };

                boxCollider.offset = new Vector2(offsetX, 0f);
            }
        }
    }
}
