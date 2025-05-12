// using UnityEngine;
// using RedGaint.Games.Core;
//
// namespace RedGaint.Games.Core
// {
//     public class DraggableCard : MonoBehaviour
//     {
//         private Camera mainCamera;
//
//         private void Awake()
//         {
//             mainCamera = Camera.main;
//         }
//
//         public void OnEndDrag()
//         {
//             Vector2 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
//             CardGroup targetGroup = FindGroupUnderPoint(worldPos);
//
//             if (targetGroup != null)
//             {
//                 Debug.Log($"Dropped into group: {targetGroup.name}");
//                 //  parent the card under this group, or snap to grid !!...
//             }
//         }
//
//         private CardGroup FindGroupUnderPoint(Vector2 point)
//         {
//             CardGroup[] allGroups = FindObjectsOfType<CardGroup>();
//             foreach (CardGroup group in allGroups)
//             {
//                 if (group.ContainsPoint(point))
//                     return group;
//             }
//
//             return null;
//         }
//     }
//
// }