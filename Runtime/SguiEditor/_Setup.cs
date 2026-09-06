using UnityEngine;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    partial class SguiEditor
    {
        VisualElement
            dockLayer,
            modalLayer,
            popupLayer;

        //--------------------------------------------------------------------------------------------------------------

        void Setup()
        {
            dockLayer = CreateLayer();
            modalLayer = CreateLayer();
            popupLayer = CreateLayer();
            SetupDrag();

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
            bool sameRoot = this.root == root;
            if (!sameRoot)
            {
                CancelDrag();
                if (this.root != null)
                    RegisterDragCallbacks(this.root, false);
                RegisterDragCallbacks(root, true);
            }
            this.root = root;

            Debug.Log($"{this}.{nameof(OnUIReload)}({nameof(version)}: {version}".ToSubLog(), this);

            if (sameRoot && version == uiVersion && dockLayer.parent == root)
                return;
            uiVersion = version;

            root.Clear();
            root.style.flexGrow = 1;
            root.style.backgroundColor = new Color(0, 0, 0, .85f);

            root.Add(dockLayer);
            root.Add(modalLayer);
            root.Add(popupLayer);
            root.Add(dragLayer);

            OnToggleVisual();
        }
    }
}
