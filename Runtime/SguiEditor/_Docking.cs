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

        //--------------------------------------------------------------------------------------------------------------

        public void OpenWindow(in SguiWindow window, in SguiDockSide side = SguiDockSide.Center, in float fixedPaneDimension = 300)
        {
            if (dockRoot == null)
            {
                var group = new DockGroup();
                group.AddWindow(window);

                dockRoot = group;
                dockLayer.Add(group);

                return;
            }
            ((DockGroup)dockRoot).AddWindow(window);
        }

        void Dock(
            in SguiWindow window,
            in DockGroup target,
            in SguiDockSide side,
            in float size = 300
            )
        {
            if (side == SguiDockSide.Center)
            {
                target.AddWindow(window);
                return;
            }

            var incoming = new DockGroup();
            incoming.AddWindow(window);

            var parent = target.parent;

            target.RemoveFromHierarchy();

            bool before =
                side == SguiDockSide.Left ||
                side == SguiDockSide.Top;

            var orientation =
                side is SguiDockSide.Left or SguiDockSide.Right
                    ? TwoPaneSplitViewOrientation.Horizontal
                    : TwoPaneSplitViewOrientation.Vertical;

            var split = new DockSplit(
                before ? incoming : target,
                before ? target : incoming,
                before ? 0 : 1,
                size,
                orientation
            );

            if (parent == dockLayer)
            {
                dockRoot = split;
                dockLayer.Add(split);
            }
            else
            {
                parent.Add(split);
            }
        }
    }
}