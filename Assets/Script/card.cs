using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

namespace RedGaint.Games.Core
{
    public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector3 offset;
        private Vector3 originalPosition;
        private bool isSelected = false;

        private Camera mainCam;
        private Transform originalParent;

        private void Awake()
        {
            mainCam = Camera.main;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isSelected)
            {
                originalPosition = transform.position;
                transform.position += Vector3.up * 0.3f;
                isSelected = true;
            }
            else
            {
                transform.position = originalPosition;
                isSelected = false;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            offset = transform.position - mainCam.ScreenToWorldPoint(eventData.position);
            originalPosition = transform.position;
            originalParent = transform.parent;

            // Optional: bring to front during drag
            transform.SetParent(null);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 worldPos = mainCam.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;
            transform.position = worldPos + offset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector3 worldPos = mainCam.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;

            var allGroups = GameObject.FindObjectsOfType<CardGroup>();
            CardGroup dropTarget = allGroups.FirstOrDefault(g => g.ContainsPoint(worldPos));

            if (dropTarget != null)
            {
                Debug.Log($"Dropped onto group: {dropTarget.name}");
                transform.SetParent(dropTarget.transform);

                Vector3 snapPos = dropTarget.GetSnapPosition(worldPos);
                transform.position = snapPos;
            }
            else
            {
                transform.position = originalPosition;
                transform.SetParent(originalParent);
            }

            isSelected = false;
        }

    }
}
