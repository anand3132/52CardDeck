using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

namespace RedGaint.Games.Core
{
    public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector3 offset;
        private Vector3 originalLocalPosition;
        private bool isSelected = false;

        private Camera mainCam;
        private Transform originalParent;
        private int originalSortingOrder;

        private SpriteRenderer spriteRenderer;

        private CardGroup currentGroup;

        public CardGroup CurrentGroup
        {
            get => currentGroup;
            private set
            {
                if (currentGroup != value)
                {
                    currentGroup = value;
                    // Optionally trigger group change events here
                }
            }
        }

        private void Awake()
        {
            mainCam = Camera.main;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            CurrentGroup = GetComponentInParent<CardGroup>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("<color=red>OnPointerClick</color>");
            if (!isSelected)
            {
                originalLocalPosition = transform.localPosition;
                transform.localPosition += Vector3.up * 0.3f; // slight visual lift
                isSelected = true;
            }
            else
            {
                transform.localPosition = originalLocalPosition;
                isSelected = false;
            }
        }

        private Vector3 originalWorldPosition; // NEW

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("<color=red>OnBeginDrag</color>");
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);

            offset = transform.position - worldPos;
            originalWorldPosition = transform.position; // ✅ Store world position
            originalParent = transform.parent;

            // Bring to front
            if (spriteRenderer != null)
            {
                originalSortingOrder = spriteRenderer.sortingOrder;
                spriteRenderer.sortingOrder = 9999;
            }

            transform.SetParent(null); // temporarily detach
        }
        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("<color=red>OnDrag</color>");
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            transform.position = worldPos + offset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("<color=red>OnEndDrag</color>");
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            var allGroups = GameObject.FindObjectsOfType<CardGroup>();
            CardGroup dropTarget = allGroups.FirstOrDefault(g => g.ContainsPoint(worldPos));

            if (dropTarget != null)
            {
                Debug.Log($"Dropped onto group: {dropTarget.name}");

                // Get next local position inside the group
                Vector3 localSnapPos = dropTarget.GetNextCardPosition();
                transform.SetParent(dropTarget.transform, false);
                transform.localPosition = localSnapPos;
                CurrentGroup = dropTarget;

                if (spriteRenderer != null)
                {
                    spriteRenderer.sortingOrder = dropTarget.transform.childCount;
                }
            }
            else
            {
                transform.SetParent(originalParent, false);
                transform.position = originalWorldPosition; 
                CurrentGroup = originalParent?.GetComponent<CardGroup>();

                if (spriteRenderer != null)
                {
                    spriteRenderer.sortingOrder = originalSortingOrder;
                }
            }
        }
    }
}
