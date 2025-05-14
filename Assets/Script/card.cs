using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Rendering;

namespace RedGaint.Games.Core
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SortingGroup))]
    public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector3 offset;
        private Vector3 originalVisualLocalPosition;
        private bool isSelected = false;

        private Camera mainCam;
        private Transform originalParent;
        private int originalSortingOrder;

        public Transform visual;
        private SpriteRenderer spriteRenderer;
        private SortingGroup sortingGroup;

        private Vector3 originalWorldPosition;
        private bool onDrag = false;

        private CardGroup currentGroup;
        public CardGroup CurrentGroup
        {
            get => currentGroup;
            private set
            {
                if (currentGroup != value)
                    currentGroup = value;
            }
        }

        private void Awake()
        {
            mainCam = Camera.main;

            if (visual != null)
                spriteRenderer = visual.GetComponent<SpriteRenderer>();

            sortingGroup = GetComponent<SortingGroup>();
        }

        private void Start()
        {
            CurrentGroup = GetComponentInParent<CardGroup>();
            originalVisualLocalPosition = visual.localPosition;

            var visualRenderer = visual.GetComponent<SpriteRenderer>();
            var collider = GetComponent<BoxCollider2D>();
            if (visualRenderer != null && collider != null)
            {
                collider.size = visualRenderer.bounds.size;
                collider.offset = visual.localPosition;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onDrag)
                return;

            if (!isSelected)
            {
                originalVisualLocalPosition = visual.localPosition;
                visual.localPosition += Vector3.up * 0.3f;
                isSelected = true;
            }
            else
            {
                visual.localPosition = originalVisualLocalPosition;
                isSelected = false;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            onDrag = true;

            if (isSelected)
            {
                visual.localPosition = originalVisualLocalPosition;
                isSelected = false;
            }

            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);

            offset = transform.position - worldPos;
            originalWorldPosition = transform.position;
            originalParent = transform.parent;

            if (sortingGroup != null)
            {
                originalSortingOrder = sortingGroup.sortingOrder;
                sortingGroup.sortingOrder = 9999;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag = true;

            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            transform.position = worldPos + offset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onDrag = false;

            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, -mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            var allGroups = GameObject.FindObjectsOfType<CardGroup>();
            CardGroup dropTarget = allGroups.FirstOrDefault(g => g.ContainsPoint(worldPos));

            if (dropTarget == null && lastTriggeredGroup != null)
            {
                Debug.Log("Fallback: Using last triggered group instead of pointer position.");
                dropTarget = lastTriggeredGroup;
            }

            if (dropTarget != null)
                HandleDropTargetFound(dropTarget, worldPos);
            else
                HandleNoDropTargetFound();

            lastTriggeredGroup = null;
        }


        private void HandleDropTargetFound(CardGroup dropTarget, Vector3 worldPos)
        {
            Debug.Log($"Dropped onto group: {dropTarget.name}");

            if (dropTarget == CurrentGroup)
            {
                // Just reset position within same group
                transform.SetParent(dropTarget.transform, true);
                transform.position = originalWorldPosition;
            }
            else
            {
                // Move to new group
                Vector3 localSnapPos = dropTarget.GetNextCardPosition();
                transform.SetParent(dropTarget.transform, false);
                transform.localPosition = localSnapPos;
                CurrentGroup = dropTarget;
            }

            if (sortingGroup != null)
                sortingGroup.sortingOrder = dropTarget.transform.childCount;

            dropTarget.RearrangeCards();

            originalParent = dropTarget.transform;
            originalWorldPosition = transform.position;
        }


        private void HandleNoDropTargetFound()
        {
            transform.SetParent(originalParent, false);
            transform.position = originalWorldPosition;
            CurrentGroup = originalParent?.GetComponent<CardGroup>();

            if (sortingGroup != null)
                sortingGroup.sortingOrder = originalSortingOrder;

            CurrentGroup?.RearrangeCards();
        }
        
        private CardGroup lastTriggeredGroup = null;

        
        private void OnTriggerEnter2D(Collider2D other)
        {
            var group = other.GetComponent<CardGroup>();
            if (group != null)
            {
                lastTriggeredGroup = group;
            }
        }

    }
}
