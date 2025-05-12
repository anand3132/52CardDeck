using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedGaint.Games.Core
{
    public enum GroupPivot
    {
        Center,
        Left,
        Right
    }

    [ExecuteAlways]
    public class CardGroup : MonoBehaviour
    {
        public Vector2 size = new Vector2(5f, 2f);
        public Vector2 slotSpacing = new Vector2(0.6f, 1.0f); // spacing between cards
        public int maxSlots = 10;
        public GroupPivot pivot = GroupPivot.Left;

        public event Action OnSizeChanged;

#if UNITY_EDITOR
        private Vector2 _previousSize;

        private void Update()
        {
            if (!Application.isPlaying && _previousSize != size)
            {
                _previousSize = size;
                TriggerRedraw();
            }
        }
#endif

        public Vector3 GetNextCardPosition()
        {
            List<Vector3> allSlots = GetAllSlotPositions();
            var usedPositions = transform.Cast<Transform>().Select(t => t.localPosition).ToList();

            foreach (var slot in allSlots)
            {
                bool taken = usedPositions.Any(pos => Vector3.Distance(pos, slot) < 0.01f);
                if (!taken)
                {
                    return slot;
                }
            }

            // fallback if all slots are taken
            return allSlots.Last();
        }

        public List<Vector3> GetAllSlotPositions()
        {
            List<Vector3> slots = new List<Vector3>();
            float totalWidth = maxSlots * slotSpacing.x;

            Vector2 startOffset;
            switch (pivot)
            {
                case GroupPivot.Left:
                    startOffset = Vector2.zero;
                    break;
                case GroupPivot.Right:
                    startOffset = new Vector2(-totalWidth, 0f);
                    break;
                case GroupPivot.Center:
                default:
                    startOffset = new Vector2(-totalWidth / 2f, 0f);
                    break;
            }

            for (int i = 0; i < maxSlots; i++)
            {
                Vector3 slot = new Vector3(
                    startOffset.x + i * slotSpacing.x + slotSpacing.x / 2f,
                    startOffset.y,
                    0f
                );
                slots.Add(slot);
            }

            return slots;
        }

        public void TriggerRedraw()
        {
            OnSizeChanged?.Invoke();
        }

        public Rect GetWorldRect()
        {
            Vector2 center = transform.position;
            return new Rect(center - size * 0.5f, size);
        }

        public bool ContainsPoint(Vector2 worldPoint)
        {
            return GetWorldRect().Contains(worldPoint);
        }

        public Vector3 GetSnapPosition(Vector3 dropPosition)
        {
            var allSlots = GetAllSlotPositions();
            Vector3 localDrop = transform.InverseTransformPoint(dropPosition);

            float closestDist = float.MaxValue;
            Vector3 closestSlot = allSlots[0];

            foreach (var slot in allSlots)
            {
                float dist = Vector3.Distance(localDrop, slot);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestSlot = slot;
                }
            }

            return closestSlot;
        }
    }
}
