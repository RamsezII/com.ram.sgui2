using UnityEngine.UIElements;

namespace _SGUI2_.windows
{
    public abstract partial class SguiWindow : VisualElement
    {
        public string title;

        //--------------------------------------------------------------------------------------------------------------

        protected SguiWindow(in string title)
        {
            this.title = title;
        }
    }
}