using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    /// <summary>
    /// UI Toolkit drag detector for player cards/buttons.
    /// Uses the left mouse button and keeps the drag alive while the pointer
    /// moves across other UI elements. A normal left-button release completes
    /// the drop; right-click never participates in the operation.
    /// </summary>
    public sealed class PlayerDragHandler : PointerManipulator
    {
        private readonly Action onDragStarted;
        private readonly Action<Vector2> onDrop;
        private readonly Action onDragCancelled;

        private Vector2 startPosition;
        private int pointerId = -1;
        private bool active;
        private bool dragging;

        public PlayerDragHandler(
            VisualElement root,
            VisualElement source,
            Action<Vector2> onDrop,
            Action onDragStarted = null,
            Action onDragCancelled = null)
        {
            this.onDrop = onDrop;
            this.onDragStarted = onDragStarted;
            this.onDragCancelled = onDragCancelled;
            target = source;

            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse
            });

            source.AddManipulator(this);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (active || evt.button != (int)MouseButton.LeftMouse)
                return;

            active = true;
            dragging = false;
            pointerId = evt.pointerId;
            startPosition = evt.position;

            target.CapturePointer(pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!active || evt.pointerId != pointerId)
                return;

            if (!dragging && Vector2.Distance(startPosition, evt.position) >= 8f)
            {
                dragging = true;
                target.AddToClassList("dragging-player");
                onDragStarted?.Invoke();
            }

            if (dragging)
                evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!active || evt.pointerId != pointerId || evt.button != (int)MouseButton.LeftMouse)
                return;

            Vector2 dropPosition = evt.position;
            bool wasDragging = dragging;

            active = false;
            dragging = false;
            target.RemoveFromClassList("dragging-player");

            if (target.HasPointerCapture(pointerId))
                target.ReleasePointer(pointerId);

            pointerId = -1;

            if (wasDragging)
            {
                onDrop?.Invoke(dropPosition);
                evt.StopPropagation();
            }
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            CancelDrag();
        }

        private void CancelDrag()
        {
            bool wasDragging = dragging;

            active = false;
            dragging = false;
            target.RemoveFromClassList("dragging-player");

            if (pointerId >= 0 && target.HasPointerCapture(pointerId))
                target.ReleasePointer(pointerId);

            pointerId = -1;

            if (wasDragging)
                onDragCancelled?.Invoke();
        }
    }
}
