using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    /// <summary>
    /// UI Toolkit drag detector that coexists with Button.clicked.
    /// PointerDown is registered during trickle-down because Button's own
    /// Clickable manipulator intentionally stops the event at the target.
    /// </summary>
    public sealed class PlayerDragHandler : PointerManipulator
    {
        private readonly Action<Vector2> onDrop;

        private Vector2 startPosition;
        private int pointerId = -1;
        private bool active;
        private bool dragging;

        public PlayerDragHandler(
            VisualElement root,
            VisualElement source,
            Action<Vector2> onDrop)
        {
            this.onDrop = onDrop;
            target = source;

            activators.Add(
                new ManipulatorActivationFilter
                {
                    button = MouseButton.LeftMouse
                });

            source.AddManipulator(this);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(
                OnPointerDown,
                TrickleDown.TrickleDown);

            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(
                OnPointerDown,
                TrickleDown.TrickleDown);

            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (active || !CanStartManipulation(evt))
                return;

            active = true;
            dragging = false;
            pointerId = evt.pointerId;
            startPosition = evt.position;

            target.CapturePointer(pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!active ||
                !target.HasPointerCapture(pointerId))
            {
                return;
            }

            if (!dragging &&
                Vector2.Distance(startPosition, evt.position) >= 8f)
            {
                dragging = true;
                target.AddToClassList("dragging-player");
            }

            if (dragging)
                evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!active ||
                !target.HasPointerCapture(pointerId) ||
                !CanStopManipulation(evt))
            {
                return;
            }

            bool wasDragging = dragging;
            Vector2 dropPosition = evt.position;

            active = false;
            dragging = false;
            target.RemoveFromClassList("dragging-player");
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

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (active)
                CancelDrag();
        }

        private void CancelDrag()
        {
            active = false;
            dragging = false;
            target.RemoveFromClassList("dragging-player");

            if (pointerId >= 0 &&
                target.HasPointerCapture(pointerId))
            {
                target.ReleasePointer(pointerId);
            }

            pointerId = -1;
        }
    }
}