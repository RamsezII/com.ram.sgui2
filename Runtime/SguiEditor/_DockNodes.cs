using _SGUI2_.windows;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    partial class SguiEditor
    {
        sealed class DockGroup : TabView
        {
            public DockGroup()
            {
                reorderable = true;
                style.flexGrow = 1;
            }

            public void AddWindow(in SguiWindow window)
            {
                var tab = new Tab(window.title);
                tab.Add(window);
                Add(tab);
            }
        }

        sealed class DockSplit : TwoPaneSplitView
        {
            public DockSplit(
                in VisualElement first,
                in VisualElement second,
                in int fixedPane,
                in float size,
                in TwoPaneSplitViewOrientation orientation
                ) : base(
                    fixedPaneIndex: fixedPane,
                    fixedPaneStartDimension: size,
                    orientation: orientation
                    )
            {
                style.flexGrow = 1;
                Add(first);
                Add(second);
            }
        }
    }
}