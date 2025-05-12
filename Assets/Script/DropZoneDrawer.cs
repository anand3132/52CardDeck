using UnityEngine;

namespace RedGaint.Games.Core
{
    [RequireComponent(typeof(LineRenderer))]
    public class DropZoneDrawer : MonoBehaviour
    {
        public Vector2 size = new Vector2(2, 3);

        void Awake()
        {
            var lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.loop = true;
            lineRenderer.positionCount = 5;
            lineRenderer.widthMultiplier = 0.05f;
            lineRenderer.useWorldSpace = false;

            Vector3[] corners = new Vector3[5];
            float halfX = size.x / 2;
            float halfY = size.y / 2;

            corners[0] = new Vector3(-halfX, -halfY, 0);
            corners[1] = new Vector3(-halfX, halfY, 0);
            corners[2] = new Vector3(halfX, halfY, 0);
            corners[3] = new Vector3(halfX, -halfY, 0);
            corners[4] = corners[0];

            lineRenderer.SetPositions(corners);
        }
    }
}