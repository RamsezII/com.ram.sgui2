using _SGUI2_.windows;
using UnityEngine;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    partial class SguiEditor
    {
        VisualElement dragLayer, dropPreview;
        Label dragLabel;
        readonly Label[] dockHints = new Label[5];
        SguiWindow draggedWindow;
        FloatingWindow draggedFrame;
        DockGroup dropGroup;
        SguiDockSide dropSide;
        int dragPointer = -1, dropIndex;
        bool dragStarted, resizingFrame, dropAtRoot;
        Vector2 dragStart;
        Rect frameStart, dropRect;

        void SetupDrag()
        {
            dragLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            dragLayer.style.position = Position.Absolute;
            dragLayer.style.left = dragLayer.style.right = dragLayer.style.top = dragLayer.style.bottom = 0;
            dropPreview = new VisualElement { pickingMode = PickingMode.Ignore };
            dropPreview.style.position = Position.Absolute;
            dropPreview.style.backgroundColor = new Color(.2f, .55f, 1, .3f);
            dragLabel = new Label { pickingMode = PickingMode.Ignore };
            dragLabel.style.position = Position.Absolute;
            dragLabel.style.backgroundColor = new Color(.1f, .1f, .1f, .95f);
            dragLabel.style.color = Color.white;
            dragLabel.style.paddingLeft = dragLabel.style.paddingRight = 6;
            dragLayer.Add(dropPreview);
            string[] captions = { "Tabs", "<", ">", "^", "v" };
            for (int i = 0; i < dockHints.Length; i++)
            {
                var hint = new Label(captions[i]) { pickingMode = PickingMode.Ignore };
                hint.style.position = Position.Absolute;
                hint.style.width = 36;
                hint.style.height = 28;
                hint.style.unityTextAlign = TextAnchor.MiddleCenter;
                hint.style.backgroundColor = new Color(.15f, .35f, .55f, .9f);
                hint.style.color = Color.white;
                dragLayer.Add(hint);
                dockHints[i] = hint;
            }
            dragLayer.Add(dragLabel);
            dragLayer.style.display = DisplayStyle.None;
        }

        void RegisterDragCallbacks(VisualElement element, bool register)
        {
            if (register)
            {
                element.focusable = true;
                element.RegisterCallback<PointerMoveEvent>(OnDragMove, TrickleDown.TrickleDown);
                element.RegisterCallback<PointerUpEvent>(OnDragUp, TrickleDown.TrickleDown);
                element.RegisterCallback<PointerCancelEvent>(OnDragCancel);
                element.RegisterCallback<PointerCaptureOutEvent>(OnDragCaptureOut);
                element.RegisterCallback<KeyDownEvent>(OnDragKey, TrickleDown.TrickleDown);
                element.RegisterCallback<DetachFromPanelEvent>(OnDragDetach);
                element.RegisterCallback<GeometryChangedEvent>(OnWorkspaceResize);
            }
            else
            {
                element.UnregisterCallback<PointerMoveEvent>(OnDragMove, TrickleDown.TrickleDown);
                element.UnregisterCallback<PointerUpEvent>(OnDragUp, TrickleDown.TrickleDown);
                element.UnregisterCallback<PointerCancelEvent>(OnDragCancel);
                element.UnregisterCallback<PointerCaptureOutEvent>(OnDragCaptureOut);
                element.UnregisterCallback<KeyDownEvent>(OnDragKey, TrickleDown.TrickleDown);
                element.UnregisterCallback<DetachFromPanelEvent>(OnDragDetach);
                element.UnregisterCallback<GeometryChangedEvent>(OnWorkspaceResize);
            }
        }

        void BeginTabDrag(SguiWindow window, Tab tab, PointerDownEvent evt)
        {
            // The close button must retain its native behavior.
            for (var element = evt.target as VisualElement; element != null && element != tab.tabHeader; element = element.parent)
                if (element.ClassListContains(Tab.closeButtonUssClassName))
                    return;
            if (!BeginDrag(evt))
                return;
            draggedWindow = window;
            var group = GetWindowGroup(window);
            group.SelectWindow(window);
            activeGroup = group;
            group.GetFirstAncestorOfType<FloatingWindow>()?.BringToFront();
        }

        void BeginFrameDrag(FloatingWindow frame, bool resize, PointerDownEvent evt)
        {
            if (!BeginDrag(evt))
                return;
            draggedFrame = frame;
            resizingFrame = resize;
            frameStart = frame.layout;
            frame.BringToFront();
        }

        bool BeginDrag(PointerDownEvent evt)
        {
            if (evt.button != 0 || dragPointer != -1 || root == null)
                return false;
            dragPointer = evt.pointerId;
            dragStart = floatingLayer.WorldToLocal(evt.position);
            root.Focus();
            root.CapturePointer(dragPointer);
            // Prevent the native handle dragger from competing for the same pointer.
            evt.StopImmediatePropagation();
            return true;
        }

        void OnDragMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != dragPointer)
                return;
            var position = floatingLayer.WorldToLocal(evt.position);
            if (!dragStarted && (position - dragStart).sqrMagnitude < 36)
                return;
            dragStarted = true;
            if (draggedFrame != null)
            {
                var rect = frameStart;
                if (resizingFrame) rect.size += position - dragStart;
                else rect.position += position - dragStart;
                SetFloatingRect(draggedFrame, rect);
            }
            else
            {
                ResolveDrop(evt.position, evt.altKey);
                ShowDrop(evt.position);
            }
            evt.StopPropagation();
        }

        void ResolveDrop(Vector2 position, bool forceFloat)
        {
            dropGroup = null;
            dropAtRoot = false;
            dropIndex = -1;
            var picked = root.panel.Pick(position);
            var group = picked as DockGroup ?? picked?.GetFirstAncestorOfType<DockGroup>();
            if (group != null && !dockLayer.Contains(group) && !floatingLayer.Contains(group))
                group = null;

            var bounds = group != null ? group.worldBound : dockLayer.worldBound;
            bool emptyWorkspace = dockRoot == null && picked is not FloatingWindow && picked?.GetFirstAncestorOfType<FloatingWindow>() == null;
            ShowDockHints(bounds, !forceFloat && bounds.Contains(position) && (group != null || emptyWorkspace));
            bool header = group != null && group.contentViewport.worldBound.Contains(position);
            float edge = Mathf.Min(60, Mathf.Min(bounds.width, bounds.height) * .2f);
            var center = new Rect(bounds.center - new Vector2(40, 40), new Vector2(80, 80));
            bool zone = bounds.Contains(position) && (header || center.Contains(position)
                || position.x < bounds.xMin + edge || position.x > bounds.xMax - edge
                || position.y < bounds.yMin + edge || position.y > bounds.yMax - edge);

            if (!forceFloat && zone && (group != null || emptyWorkspace))
            {
                dropGroup = group;
                dropAtRoot = group == null;
                dropSide = SguiDockSide.Center;
                if (!header)
                {
                    if (position.x < bounds.xMin + edge) dropSide = SguiDockSide.Left;
                    else if (position.x > bounds.xMax - edge) dropSide = SguiDockSide.Right;
                    else if (position.y < bounds.yMin + edge) dropSide = SguiDockSide.Top;
                    else if (position.y > bounds.yMax - edge) dropSide = SguiDockSide.Bottom;
                }
                // Splitting a group's only tab away from itself would just recreate the same group.
                if (group == GetWindowGroup(draggedWindow) && group.childCount == 1)
                    dropSide = SguiDockSide.Center;
                dropRect = bounds;
                if (dropSide == SguiDockSide.Left) dropRect.width *= .5f;
                if (dropSide == SguiDockSide.Right) { dropRect.x += bounds.width * .5f; dropRect.width *= .5f; }
                if (dropSide == SguiDockSide.Top) dropRect.height *= .5f;
                if (dropSide == SguiDockSide.Bottom) { dropRect.y += bounds.height * .5f; dropRect.height *= .5f; }
                if (header)
                {
                    dropIndex = 0;
                    float marker = group.contentViewport.worldBound.xMin;
                    for (int i = 0; i < group.childCount; i++)
                    {
                        var tab = group.GetTab(i);
                        if (tab.userData == draggedWindow) continue;
                        var rect = tab.tabHeader.worldBound;
                        if (position.x < rect.center.x) { marker = rect.xMin; break; }
                        dropIndex++;
                        marker = rect.xMax;
                    }
                    dropRect = new Rect(marker, group.contentViewport.worldBound.yMin, 3, group.contentViewport.worldBound.height);
                }
                return;
            }

            var local = floatingLayer.WorldToLocal(position);
            var floatingRect = ClampFloatingRect(new Rect(local - new Vector2(80, 12), new Vector2(480, 320)));
            dropRect = new Rect(floatingLayer.LocalToWorld(floatingRect.position), floatingRect.size);
        }

        void ShowDrop(Vector2 position)
        {
            dragLayer.style.display = DisplayStyle.Flex;
            var local = dragLayer.WorldToLocal(dropRect.position);
            dropPreview.style.left = local.x;
            dropPreview.style.top = local.y;
            dropPreview.style.width = dropRect.width;
            dropPreview.style.height = dropRect.height;
            var labelPosition = dragLayer.WorldToLocal(position) + new Vector2(16, 20);
            dragLabel.style.left = labelPosition.x;
            dragLabel.style.top = labelPosition.y;
            dragLabel.text = dropGroup != null || dropAtRoot
                ? $"{draggedWindow.title} — {dropSide} · Alt: float · Esc: cancel"
                : $"{draggedWindow.title} — Float · Esc: cancel";
        }

        void ShowDockHints(Rect bounds, bool visible)
        {
            for (int i = 0; i < dockHints.Length; i++)
            {
                var hint = dockHints[i];
                hint.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                var point = bounds.center;
                if (i == 1) point.x = bounds.xMin + 22;
                if (i == 2) point.x = bounds.xMax - 22;
                if (i == 3) point.y = bounds.yMin + 22;
                if (i == 4) point.y = bounds.yMax - 22;
                point = dragLayer.WorldToLocal(point);
                hint.style.left = point.x - 18;
                hint.style.top = point.y - 14;
            }
        }

        void OnDragUp(PointerUpEvent evt)
        {
            if (evt.pointerId != dragPointer || evt.button != 0)
                return;
            if (dragStarted && draggedWindow != null)
            {
                ResolveDrop(evt.position, evt.altKey);
                var window = draggedWindow;
                var group = dropGroup;
                var side = dropSide;
                var index = dropIndex;
                var atRoot = dropAtRoot;
                var rect = new Rect(floatingLayer.WorldToLocal(dropRect.position), dropRect.size);
                FinishDrag();
                if (atRoot) DockAtRoot(window);
                else if (group != null)
                {
                    float size = side is SguiDockSide.Left or SguiDockSide.Right ? group.layout.width / 2 : group.layout.height / 2;
                    Dock(window, group, side, Mathf.Max(1, size));
                    if (index >= 0)
                        group.ReorderTab(group.IndexOf(window.GetFirstAncestorOfType<Tab>()), Mathf.Min(index, group.childCount - 1));
                }
                else FloatWindow(window, rect);
            }
            else FinishDrag();
            evt.StopPropagation();
        }

        void FinishDrag()
        {
            int pointer = dragPointer;
            dragPointer = -1;
            draggedWindow = null;
            draggedFrame = null;
            dropGroup = null;
            dragStarted = false;
            if (dragLayer != null) dragLayer.style.display = DisplayStyle.None;
            if (root != null && pointer >= 0 && root.HasPointerCapture(pointer))
                root.ReleasePointer(pointer);
        }

        void CancelDrag()
        {
            if (draggedFrame != null)
                SetFloatingRect(draggedFrame, frameStart);
            FinishDrag();
        }

        void OnDragCancel(PointerCancelEvent evt) { if (evt.pointerId == dragPointer) CancelDrag(); }
        void OnDragCaptureOut(PointerCaptureOutEvent evt) { if (evt.pointerId == dragPointer) CancelDrag(); }
        void OnDragDetach(DetachFromPanelEvent evt) { if (evt.target == root) CancelDrag(); }
        void OnDragKey(KeyDownEvent evt)
        {
            if (dragPointer >= 0 && evt.keyCode == KeyCode.Escape) { CancelDrag(); evt.StopPropagation(); }
        }
        void OnWorkspaceResize(GeometryChangedEvent evt)
        {
            foreach (var element in floatingLayer.Children())
                if (element is FloatingWindow frame) SetFloatingRect(frame, frame.layout);
        }
    }
}
