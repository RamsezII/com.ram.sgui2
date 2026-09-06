using UnityEngine;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    partial class SguiEditor
    {
        VisualElement
            dockLayer,
            floatingLayer,
            modalLayer,
            popupLayer;

        //--------------------------------------------------------------------------------------------------------------

        void Setup()
        {
            dockLayer = CreateLayer();
            floatingLayer = CreateLayer();
            modalLayer = CreateLayer();
            popupLayer = CreateLayer();

            static VisualElement CreateLayer()
            {
                var e = new VisualElement();

                e.style.position = Position.Absolute;
                e.style.left = 0;
                e.style.right = 0;
                e.style.top = 0;
                e.style.bottom = 0;

                e.pickingMode = PickingMode.Ignore;

                return e;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            this.root = root;

            Debug.Log($"{this}.{nameof(OnUIReload)}({nameof(version)}: {version}".ToSubLog(), this);

            if (version == uiVersion)
                return;
            uiVersion = version;

            root.Clear();
            root.style.flexGrow = 1;
            root.style.backgroundColor = new Color(0, 0, 0, .75f);

            root.Add(dockLayer);
            root.Add(floatingLayer);
            root.Add(modalLayer);
            root.Add(popupLayer);

            OnToggleVisual();
        }
    }
}