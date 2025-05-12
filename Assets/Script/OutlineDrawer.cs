using UnityEngine;

namespace RedGaint.Games.Core
{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteAlways]
    public class OutlineDrawer : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private CardGroup cardGroup;

        void OnEnable()
        {
            Init();
            Subscribe();
            Redraw();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        private void Init()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                lineRenderer.loop = true;
                lineRenderer.positionCount = 5;
                lineRenderer.widthMultiplier = 0.05f;
                lineRenderer.useWorldSpace = false;

                var mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = Color.green;
                lineRenderer.material = mat;
            }

            cardGroup = GetComponent<CardGroup>();
        }

        private void Subscribe()
        {
            if (cardGroup != null)
                cardGroup.OnSizeChanged += Redraw;
        }

        private void Unsubscribe()
        {
            if (cardGroup != null)
                cardGroup.OnSizeChanged -= Redraw;
        }

        private void Redraw()
        {
            Vector2 size = cardGroup != null ? cardGroup.size : new Vector2(2, 2);

            float halfX = size.x / 2;
            float halfY = size.y / 2;

            Vector3[] corners = new Vector3[5];
            corners[0] = new Vector3(-halfX, -halfY, 0);
            corners[1] = new Vector3(-halfX, halfY, 0);
            corners[2] = new Vector3(halfX, halfY, 0);
            corners[3] = new Vector3(halfX, -halfY, 0);
            corners[4] = corners[0];

            lineRenderer.SetPositions(corners);
        }
    }
}