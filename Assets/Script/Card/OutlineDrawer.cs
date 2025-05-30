﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedGaint.Games.Core
{
    [RequireComponent(typeof(LineRenderer))]
    // [ExecuteAlways]
    public class OutlineDrawer : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        public SpriteRenderer defaultSpriteRenderer;
        bool initialized = false;

        private void OnEnable()
        {
            RedrawFromCard();
        }

        private void Init()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                lineRenderer.loop = false;
                lineRenderer.useWorldSpace = false;
                lineRenderer.widthMultiplier = 0.05f;

                var mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = Color.green;
                lineRenderer.material = mat;
            }
            initialized = true;
        }


        private List<Renderer> GetVisualRenderers()
        {
            List<Renderer> visualRenderers = new List<Renderer>();

            // Get all children, including inactive
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);

            foreach (Transform child in allChildren)
            {
                if (child.name == "Visual")
                {
                    Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);
                    visualRenderers.AddRange(renderers);
                }
            }
            return visualRenderers;
        }
        private Bounds GetCombinedBounds(List<Renderer> renderers)
        {
            if (renderers == null || renderers.Count == 0)
            {
                // Return an empty bounds centered at origin
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            Bounds combinedBounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Count; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            return combinedBounds;
        }
        public void RedrawFromCard()
        {
            if (!initialized)
                Init();

            List<Renderer> renderers = GetVisualRenderers();

            if (renderers == null || renderers.Count == 0)
            {
                DrawDefault1();
                return;
            }

            Bounds combinedBounds = GetCombinedBounds(renderers);
            DrawOutline(combinedBounds);
        }
        
        private void DrawDefault1()
        {
            if (defaultSpriteRenderer == null || defaultSpriteRenderer.sprite == null)
            {
                Debug.LogWarning("No defaultSpriteRenderer or sprite found.");
                return;
            }

            // Get the local size of the sprite
            Vector2 size = defaultSpriteRenderer.sprite.bounds.size;
            Vector3 halfSize = new Vector3(size.x / 2f, size.y / 2f, 0);

            // Define corners in local space
            Vector3 topLeft = new Vector3(-halfSize.x, halfSize.y, 0);
            Vector3 topRight = new Vector3(halfSize.x, halfSize.y, 0);
            Vector3 bottomRight = new Vector3(halfSize.x, -halfSize.y, 0);
            Vector3 bottomLeft = new Vector3(-halfSize.x, -halfSize.y, 0);
            Vector3 center = Vector3.zero;

            // Define all line points: rectangle + diagonals
            Vector3[] points = new Vector3[]
            {
                topLeft,
                topRight,
                bottomRight,
                bottomLeft
                ,topLeft
            };

            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

      private void DrawOutline(Bounds worldBounds)
        {
            Vector3 localCenter = transform.InverseTransformPoint(worldBounds.center);
            Vector3 size = worldBounds.size;

            float halfX = size.x / 2f;
            float halfY = size.y / 2f;

            Vector3[] corners = new Vector3[5];
            corners[0] = localCenter + new Vector3(-halfX, -halfY, 0);
            corners[1] = localCenter + new Vector3(-halfX, halfY, 0);
            corners[2] = localCenter + new Vector3(halfX, halfY, 0);
            corners[3] = localCenter + new Vector3(halfX, -halfY, 0);
            corners[4] = corners[0];

            lineRenderer.SetPositions(corners);
        }
    }
}
