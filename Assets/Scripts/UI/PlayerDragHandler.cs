using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    public sealed class PlayerDragHandler
    {
        private readonly VisualElement root;
        private readonly VisualElement source;
        private readonly Action<Vector2> onDrop;

        private Vector2 startPosition;

        private bool pointerDown;
        private bool dragging;

        public PlayerDragHandler(
            VisualElement root,
            VisualElement source,
            Action<Vector2> onDrop)
        {
            this.root = root;
            this.source = source;
            this.onDrop = onDrop;

            source.RegisterCallback<PointerDownEvent>(
                OnPointerDown);

            source.RegisterCallback<PointerMoveEvent>(
                OnPointerMove);

            source.RegisterCallback<PointerUpEvent>(
                OnPointerUp);
        }

        private void OnPointerDown(
            PointerDownEvent evt)
        {
            pointerDown = true;
            dragging = false;

            startPosition =
                evt.position;

            source.CapturePointer(
                evt.pointerId);

            evt.StopPropagation();
        }

        private void OnPointerMove(
            PointerMoveEvent evt)
        {
            if (!pointerDown)
                return;

            float distance =
                Vector2.Distance(
                    startPosition,
                    evt.position);

            if (!dragging &&
                distance > 8f)
            {
                dragging = true;

                source.AddToClassList(
                    "dragging-player");
            }

            if (dragging)
            {
                evt.StopPropagation();
            }
        }

        private void OnPointerUp(
            PointerUpEvent evt)
        {
            if (!pointerDown)
                return;

            if (dragging)
            {
                onDrop(
                    evt.position);
            }

            pointerDown = false;
            dragging = false;

            source.RemoveFromClassList(
                "dragging-player");

            if (source.HasPointerCapture(
                    evt.pointerId))
            {
                source.ReleasePointer(
                    evt.pointerId);
            }

            evt.StopPropagation();
        }
    }
}