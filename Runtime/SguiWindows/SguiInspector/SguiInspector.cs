using UnityEngine.UIElements;

namespace _SGUI2_.windows
{
    public sealed partial class SguiInspector : SguiWindow
    {
        public ScrollView scroll;

        //--------------------------------------------------------------------------------------------------------------

        public SguiInspector() : base("Inspector")
        {
            scroll = new ScrollView();
            scroll.Add(new Label("Nothing selected"));
            Add(scroll);
        }
    }
}