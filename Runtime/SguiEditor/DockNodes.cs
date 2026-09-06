using System.Collections.Generic;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    abstract class DockNode { }

    class DockSplit : DockNode
    {
        public DockNode a;
        public DockNode b;

        public bool horizontal;
        public float ratio;
    }

    class DockTabs : DockNode
    {
        public List<DockWindow> windows;
        public int selected;
    }

    class DockWindow
    {
        public string title;
        public VisualElement content;
    }
}