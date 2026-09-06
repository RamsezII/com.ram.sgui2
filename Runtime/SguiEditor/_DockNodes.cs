using _SGUI2_.windows;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    partial class SguiEditor
    {
        abstract class DockNode
        {
            public DockSplit parent;
        }

        sealed class DockGroup : DockNode
        {
            public readonly List<SguiWindow> windows = new();
            public TabView tabView;
            public DockGroup(in SguiWindow window)
            {
                windows.Add(window);
            }
        }

        sealed class DockSplit : DockNode
        {
            public DockNode first, second;
            public TwoPaneSplitViewOrientation orientation;
            public int fixedPaneIndex;
            public float fixedPaneDimension;
        }
    }
}