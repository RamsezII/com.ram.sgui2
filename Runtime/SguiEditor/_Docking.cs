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
        DockNode dockRootNode;

        //--------------------------------------------------------------------------------------------------------------

        public void OpenWindow(in SguiWindow window, in SguiDockSide side = SguiDockSide.Center, in float fixedPaneDimension = 300)
        {
            var group = new DockGroup(window);

            if (dockRootNode == null)
                dockRootNode = group;
            else if (side == SguiDockSide.Center)
                FirstGroup(dockRootNode).windows.Add(window);
            else
                SplitNode(dockRootNode, group, side, fixedPaneDimension);

            ReloadDock();
        }

        void SplitNode(in DockNode target, in DockGroup incoming, in SguiDockSide side, in float fixedPaneDimension)
        {
            var previousParent = target.parent;

            DockNode first, second;

            TwoPaneSplitViewOrientation orientation;
            int fixedPaneIndex;

            switch (side)
            {
                case SguiDockSide.Left:
                    first = incoming;
                    second = target;
                    orientation = TwoPaneSplitViewOrientation.Horizontal;
                    fixedPaneIndex = 0;
                    break;

                case SguiDockSide.Right:
                    first = target;
                    second = incoming;
                    orientation = TwoPaneSplitViewOrientation.Horizontal;
                    fixedPaneIndex = 1;
                    break;

                case SguiDockSide.Top:
                    first = incoming;
                    second = target;
                    orientation = TwoPaneSplitViewOrientation.Vertical;
                    fixedPaneIndex = 0;
                    break;

                case SguiDockSide.Bottom:
                    first = target;
                    second = incoming;
                    orientation = TwoPaneSplitViewOrientation.Vertical;
                    fixedPaneIndex = 1;
                    break;

                default:
                    return;
            }

            var split = new DockSplit
            {
                parent = previousParent,
                first = first,
                second = second,
                orientation = orientation,
                fixedPaneIndex = fixedPaneIndex,
                fixedPaneDimension = fixedPaneDimension,
            };

            first.parent = split;
            second.parent = split;

            if (previousParent == null)
                dockRootNode = split;
            else if (previousParent.first == target)
                previousParent.first = split;
            else
                previousParent.second = split;
        }

        static DockGroup FirstGroup(in DockNode node)
        {
            if (node is DockGroup group)
                return group;
            return FirstGroup(((DockSplit)node).first);
        }

        void ReloadDock()
        {
            dockLayer.Clear();
            if (dockRootNode != null)
                dockLayer.Add(CreateDockVisual(dockRootNode));
        }

        static VisualElement CreateDockVisual(in DockNode node)
        {
            if (node is DockGroup group)
                return CreateDockGroupVisual(group);
            return CreateDockSplitVisual((DockSplit)node);
        }

        static VisualElement CreateDockGroupVisual(in DockGroup group)
        {
            var tabView = new TabView
            {
                reorderable = true,
            };

            tabView.style.flexGrow = 1;

            foreach (var window in group.windows)
            {
                var tab = new Tab(window.title);
                tab.Add(window);
                tabView.Add(tab);
            }

            return tabView;
        }

        static VisualElement CreateDockSplitVisual(in DockSplit split)
        {
            var splitView = new TwoPaneSplitView(
                fixedPaneIndex: split.fixedPaneIndex,
                fixedPaneStartDimension: split.fixedPaneDimension,
                orientation: split.orientation
            );

            splitView.style.flexGrow = 1;

            splitView.Add(CreateDockVisual(split.first));
            splitView.Add(CreateDockVisual(split.second));

            return splitView;
        }
    }
}