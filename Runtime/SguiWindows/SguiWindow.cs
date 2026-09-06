using System;
using UnityEngine.UIElements;

namespace _SGUI2_.windows
{
    public abstract partial class SguiWindow : VisualElement
    {
        string _title;
        public Action onClosed;
        public string title
        {
            get => _title;
            set
            {
                _title = value;
                var tab = GetFirstAncestorOfType<Tab>();
                if (tab != null)
                    tab.label = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        protected SguiWindow(string title)
        {
            this.title = title;
            style.flexGrow = 1;
            style.minWidth = 0;
            style.minHeight = 0;
        }

        //--------------------------------------------------------------------------------------------------------------

        // Called only when closed, never when moved between groups or when the UI reloads.
        protected internal virtual void OnClosed()
        {
            onClosed?.Invoke();
        }
    }
}
