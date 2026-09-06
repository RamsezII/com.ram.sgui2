using _SGUI2_.windows;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    partial class SguiEditor
    {
        sealed class DockGroup : TabView
        {
            public DockGroup(SguiEditor editor)
            {
                reorderable = true;
                style.flexGrow = 1;
                RegisterCallback<PointerDownEvent>(_ => editor.activeGroup = this, TrickleDown.TrickleDown);
                RegisterCallback<FocusInEvent>(_ => editor.activeGroup = this);
                activeTabChanged += (_, current) =>
                {
                    if (current != null)
                        editor.activeGroup = this;
                };
                tabClosed += (tab, _) =>
                {
                    var window = (SguiWindow)tab.userData;
                    window.RemoveFromHierarchy();
                    editor.RemoveEmptyGroup(this);
                    window.OnClosed();
                };
            }

            public void AddWindow(SguiWindow window)
            {
                var tab = new Tab(window.title) { closeable = true, userData = window };
                tab.Add(window);
                Add(tab);
                activeTab = tab;
            }

            public void RemoveWindow(SguiWindow window)
            {
                var tab = window.GetFirstAncestorOfType<Tab>();
                Remove(tab);
                window.RemoveFromHierarchy();
            }

            public void SelectWindow(SguiWindow window)
            {
                activeTab = window.GetFirstAncestorOfType<Tab>();
            }
        }

        sealed class DockSplit : TwoPaneSplitView
        {
            public DockSplit(int fixedPane, float size, TwoPaneSplitViewOrientation orientation)
                : base(fixedPane, size, orientation)
            {
                style.flexGrow = 1;
            }
        }
    }
}
