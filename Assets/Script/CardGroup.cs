using UnityEngine;
using System;

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
        public GroupPivot pivot = GroupPivot.Left; // default to Left for intuitive placement

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
            Rect worldRect = GetWorldRect();
            Vector2 basePos;

            switch (pivot)
            {
                case GroupPivot.Left:
                    basePos = new Vector2(worldRect.xMin, worldRect.center.y);
                    break;
                case GroupPivot.Right:
                    basePos = new Vector2(worldRect.xMax - (maxSlots * slotSpacing.x), worldRect.center.y);
                    break;
                case GroupPivot.Center:
                default:
                    float totalWidth = maxSlots * slotSpacing.x;
                    basePos = new Vector2(worldRect.center.x - totalWidth / 2f, worldRect.center.y);
                    break;
            }

            float closestDist = float.MaxValue;
            Vector3 closestSlot = basePos;

            for (int i = 0; i < maxSlots; i++)
            {
                Vector3 slotPos = new Vector3(
                    basePos.x + i * slotSpacing.x + slotSpacing.x / 2f,
                    basePos.y,
                    0f
                );

                float dist = Vector3.Distance(dropPosition, slotPos);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestSlot = slotPos;
                }
            }

            return closestSlot;
        }
    }
}
