using _SGUI2_.windows;
using UnityEngine;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    partial class SguiEditor
    {
        VisualElement dragLayer, dropPreview;
        SguiWindow draggedWindow;
        int dragPointer = -1;
        bool dragStarted;
        Vector2 dragStart;

        //--------------------------------------------------------------------------------------------------------------

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
            }
            else
            {
                element.UnregisterCallback<PointerMoveEvent>(OnDragMove, TrickleDown.TrickleDown);
                element.UnregisterCallback<PointerUpEvent>(OnDragUp, TrickleDown.TrickleDown);
                element.UnregisterCallback<PointerCancelEvent>(OnDragCancel);
                element.UnregisterCallback<PointerCaptureOutEvent>(OnDragCaptureOut);
                element.UnregisterCallback<KeyDownEvent>(OnDragKey, TrickleDown.TrickleDown);
                element.UnregisterCallback<DetachFromPanelEvent>(OnDragDetach);
            }
        }

        void BeginTabDrag(SguiWindow window, Tab tab, PointerDownEvent evt)
        {
            if (evt.button != 0 || dragPointer != -1 || root == null)
                return;
            // Leave the native close button alone.
            for (var element = evt.target as VisualElement; element != null && element != tab.tabHeader; element = element.parent)
                if (element.ClassListContains(Tab.closeButtonUssClassName))
                    return;

            draggedWindow = window;
            dragPointer = evt.pointerId;
            dragStart = evt.position;
            activeGroup = GetWindowGroup(window);
            activeGroup.SelectWindow(window);
            root.Focus();
            root.CapturePointer(dragPointer);
            // One gesture on the entire label, without the native handle competing for capture.
            evt.StopImmediatePropagation();
        }

        void OnDragMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != dragPointer)
                return;
            if (!dragStarted && ((Vector2)evt.position - dragStart).sqrMagnitude < 36)
                return;
            dragStarted = true;
            var (group, _, _, rect) = ResolveDrop(evt.position);
            dragLayer.style.display = group == null ? DisplayStyle.None : DisplayStyle.Flex;
            var local = dragLayer.WorldToLocal(rect.position);
            dropPreview.style.left = local.x;
            dropPreview.style.top = local.y;
            dropPreview.style.width = rect.width;
            dropPreview.style.height = rect.height;
            evt.StopPropagation();
        }

        // All coordinates here are panel coordinates. Only the preview converts to local space.
        (DockGroup group, SguiDockSide side, int index, Rect rect) ResolveDrop(Vector2 position)
        {
            var picked = root.panel.Pick(position);
            var group = picked as DockGroup ?? picked?.GetFirstAncestorOfType<DockGroup>();
            if (group == null || !dockLayer.Contains(group) || !group.worldBound.Contains(position))
                return default;

            var bounds = group.worldBound;
            var side = SguiDockSide.Center;
            if (group.contentViewport.worldBound.Contains(position))
            {
                int index = 0;
                float marker = group.contentViewport.worldBound.xMin;
                for (int i = 0; i < group.childCount; i++)
                {
                    var tab = group.GetTab(i);
                    if (tab.userData == draggedWindow) continue;
                    var header = tab.tabHeader.worldBound;
                    if (position.x < header.center.x) { marker = header.xMin; break; }
                    index++;
                    marker = header.xMax;
                }
                return (group, side, index, new Rect(marker, group.contentViewport.worldBound.yMin, 3, group.contentViewport.worldBound.height));
            }

            float edge = Mathf.Min(60, Mathf.Min(bounds.width, bounds.height) * .2f);
            if (position.x < bounds.xMin + edge) side = SguiDockSide.Left;
            else if (position.x > bounds.xMax - edge) side = SguiDockSide.Right;
            else if (position.y < bounds.yMin + edge) side = SguiDockSide.Top;
            else if (position.y > bounds.yMax - edge) side = SguiDockSide.Bottom;
            if (group == GetWindowGroup(draggedWindow) && group.childCount == 1)
                side = SguiDockSide.Center;

            var rect = bounds;
            if (side == SguiDockSide.Left) rect.width *= .5f;
            if (side == SguiDockSide.Right) { rect.x += bounds.width * .5f; rect.width *= .5f; }
            if (side == SguiDockSide.Top) rect.height *= .5f;
            if (side == SguiDockSide.Bottom) { rect.y += bounds.height * .5f; rect.height *= .5f; }
            return (group, side, -1, rect);
        }

        void OnDragUp(PointerUpEvent evt)
        {
            if (evt.pointerId != dragPointer || evt.button != 0)
                return;
            var window = draggedWindow;
            var drop = dragStarted ? ResolveDrop(evt.position) : default;
            CancelDrag();
            if (drop.group != null)
            {
                float size = drop.side is SguiDockSide.Left or SguiDockSide.Right
                    ? drop.group.layout.width / 2 : drop.group.layout.height / 2;
                Dock(window, drop.group, drop.side, Mathf.Max(1, size));
                if (drop.index >= 0)
                    drop.group.ReorderTab(drop.group.IndexOf(window.GetFirstAncestorOfType<Tab>()), Mathf.Min(drop.index, drop.group.childCount - 1));
            }
            evt.StopPropagation();
        }

        void CancelDrag()
        {
            int pointer = dragPointer;
            dragPointer = -1;
            draggedWindow = null;
            dragStarted = false;
            if (dragLayer != null) dragLayer.style.display = DisplayStyle.None;
            if (root != null && pointer >= 0 && root.HasPointerCapture(pointer))
                root.ReleasePointer(pointer);
        }

        void OnDragCancel(PointerCancelEvent evt) { if (evt.pointerId == dragPointer) CancelDrag(); }
        void OnDragCaptureOut(PointerCaptureOutEvent evt) { if (evt.pointerId == dragPointer) CancelDrag(); }
        void OnDragDetach(DetachFromPanelEvent evt) { if (evt.target == root) CancelDrag(); }
        void OnDragKey(KeyDownEvent evt)
        {
            if (dragPointer >= 0 && evt.keyCode == KeyCode.Escape) { CancelDrag(); evt.StopPropagation(); }
        }
    }
}
