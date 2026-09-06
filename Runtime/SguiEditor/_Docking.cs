using System;
using _SGUI2_.windows;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    public enum SguiDockSide : byte
    {
        Center,
        Left,
        Right,
        Top,
        Bottom,
    }

    partial class SguiEditor
    {
        VisualElement dockRoot;
        DockGroup activeGroup;

        // Opening an existing instance only selects it. Use DockWindow to move it.
        public void OpenWindow(SguiWindow window, SguiDockSide side = SguiDockSide.Center, float fixedPaneDimension = 300)
        {
            ValidateDock(window, side, fixedPaneDimension);
            var existing = GetWindowGroup(window);
            if (existing != null)
            {
                existing.SelectWindow(window);
                activeGroup = existing;
                return;
            }

            if (dockRoot == null)
            {
                activeGroup = new DockGroup(this);
                dockRoot = activeGroup;
                dockLayer.Add(dockRoot);
                activeGroup.AddWindow(window);
                return;
            }

            Dock(window, activeGroup ?? FindGroup(dockRoot), side, fixedPaneDimension);
        }

        public void DockWindow(SguiWindow window, SguiWindow targetWindow, SguiDockSide side = SguiDockSide.Center, float size = 300)
        {
            ValidateDock(window, side, size);
            var target = GetWindowGroup(targetWindow);
            if (target == null)
                throw new ArgumentException("The target window must be open in this editor.", nameof(targetWindow));

            // Docking relative to yourself has no useful destination.
            if (window == targetWindow)
            {
                target.SelectWindow(window);
                activeGroup = target;
                return;
            }

            Dock(window, target, side, size);
        }

        public void CloseWindow(SguiWindow window)
        {
            var group = GetWindowGroup(window);
            if (group == null)
                return;

            if (window == draggedWindow)
                CancelDrag();

            group.RemoveWindow(window);
            RemoveEmptyGroup(group);
            window.OnClosed();
        }

        DockGroup GetWindowGroup(SguiWindow window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            var group = window.GetFirstAncestorOfType<DockGroup>();
            if (group != null && !dockLayer.Contains(group))
                throw new ArgumentException("The window belongs to another editor.", nameof(window));
            return group;
        }

        static void ValidateDock(SguiWindow window, SguiDockSide side, float size)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            if (side < SguiDockSide.Center || side > SguiDockSide.Bottom)
                throw new ArgumentOutOfRangeException(nameof(side));
            if (side != SguiDockSide.Center && (float.IsNaN(size) || float.IsInfinity(size) || size <= 0))
                throw new ArgumentOutOfRangeException(nameof(size));
        }

        void Dock(SguiWindow window, DockGroup target, SguiDockSide side, float size)
        {
            var source = GetWindowGroup(window);
            if (source == target && side == SguiDockSide.Center)
            {
                target.SelectWindow(window);
                activeGroup = target;
                return;
            }

            source?.RemoveWindow(window);
            var destination = target;
            if (side != SguiDockSide.Center)
            {
                destination = new DockGroup(this);
                bool before = side is SguiDockSide.Left or SguiDockSide.Top;
                var orientation = side is SguiDockSide.Left or SguiDockSide.Right
                    ? TwoPaneSplitViewOrientation.Horizontal
                    : TwoPaneSplitViewOrientation.Vertical;
                var split = new DockSplit(before ? 0 : 1, size, orientation);

                // Replace before reparenting target, preserving its position in the parent split.
                ReplaceNode(target, split);
                split.Add(before ? destination : target);
                split.Add(before ? target : destination);
            }

            destination.AddWindow(window);
            if (source != null)
                RemoveEmptyGroup(source);
            activeGroup = destination;
        }

        void ReplaceNode(VisualElement node, VisualElement replacement)
        {
            var parent = node.parent;
            int index = parent.IndexOf(node);
            node.RemoveFromHierarchy();
            replacement?.RemoveFromHierarchy();
            if (replacement != null)
                parent.Insert(index, replacement);
            if (dockRoot == node)
                dockRoot = replacement;
        }

        void RemoveEmptyGroup(DockGroup group)
        {
            if (group.childCount != 0)
                return;

            var split = group.GetFirstAncestorOfType<DockSplit>();
            if (split == null)
                ReplaceNode(group, null);
            else
            {
                var remaining = split[0] == group ? split[1] : split[0];
                group.RemoveFromHierarchy();
                ReplaceNode(split, remaining);
            }

            if (activeGroup == group)
                activeGroup = FindGroup(dockRoot);
        }

        static DockGroup FindGroup(VisualElement node)
        {
            while (node is DockSplit split)
                node = split[0];
            return node as DockGroup;
        }
    }
}
