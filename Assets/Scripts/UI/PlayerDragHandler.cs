using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    /// <summary>
    /// UI Toolkit drag detector for player buttons.
    /// The drag starts on the source element, but release is observed from the
    /// root so moving over another Button cannot swallow the pointer-up event.
    /// </summary>
    public sealed class PlayerDragHandler : PointerManipulator
    {
        private readonly VisualElement root;
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
            this.root = root;
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
            target.RegisterCallback<PointerDownEvent>(
                OnPointerDown,
                TrickleDown.TrickleDown);

            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);

            // PointerUp is also registered on the root. This is important when
            // the pointer moves over another Button while dragging.
            root.RegisterCallback<PointerUpEvent>(OnRootPointerUp,
                TrickleDown.TrickleDown);
            root.RegisterCallback<PointerCancelEvent>(OnRootPointerCancel,
                TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(
                OnPointerDown,
                TrickleDown.TrickleDown);

            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            root.UnregisterCallback<PointerUpEvent>(OnRootPointerUp,
                TrickleDown.TrickleDown);
            root.UnregisterCallback<PointerCancelEvent>(OnRootPointerCancel,
                TrickleDown.TrickleDown);
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
            if (!active || evt.pointerId != pointerId)
                return;

            if (!dragging &&
                Vector2.Distance(startPosition, evt.position) >= 8f)
            {
                dragging = true;
                target.AddToClassList("dragging-player");
                onDragStarted?.Invoke();
            }

            if (dragging)
                evt.StopPropagation();
        }

        private void OnRootPointerUp(PointerUpEvent evt)
        {
            if (!active || evt.pointerId != pointerId ||
                !CanStopManipulation(evt))
                return;

            Vector2 dropPosition = evt.position;
            bool wasDragging = dragging;

            FinishPointer();

            if (wasDragging)
            {
                onDrop?.Invoke(dropPosition);
                evt.StopPropagation();
            }
        }

        private void OnRootPointerCancel(PointerCancelEvent evt)
        {
            if (!active || evt.pointerId != pointerId)
                return;

            CancelDrag();
        }

        private void FinishPointer()
        {
            active = false;
            dragging = false;
            target.RemoveFromClassList("dragging-player");

            if (pointerId >= 0 && target.HasPointerCapture(pointerId))
                target.ReleasePointer(pointerId);

            pointerId = -1;
        }

        private void CancelDrag()
        {
            bool wasDragging = dragging;
            FinishPointer();

            if (wasDragging)
                onDragCancelled?.Invoke();
        }
    }
}
