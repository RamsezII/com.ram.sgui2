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

        void SetupLayers()
        {
            dockLayer = CreateLayer("sgui-editor__dock-layer");
            modalLayer = CreateLayer("sgui-editor__modal-layer");
            popupLayer = CreateLayer("sgui-editor__popup-layer");

            static VisualElement CreateLayer(string className)
            {
                var e = new VisualElement();

                e.AddToClassList("sgui-editor__layer");
                e.AddToClassList(className);

                e.pickingMode = PickingMode.Ignore;

                return e;
            }
        }

        void SetupDrag()
        {
            dragLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            dragLayer.AddToClassList("sgui-editor__layer");
            dragLayer.AddToClassList("sgui-editor__drag-layer");
            dropPreview = new VisualElement { pickingMode = PickingMode.Ignore };
            dropPreview.AddToClassList("sgui-editor__drop-preview");
            dragLayer.Add(dropPreview);
            dragLayer.style.display = DisplayStyle.None;
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
            root.AddToClassList("sgui-editor");
            if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
                root.styleSheets.Add(styleSheet);

            root.Add(dockLayer);
            root.Add(modalLayer);
            root.Add(popupLayer);
            root.Add(dragLayer);

            OnToggleVisual();
        }
    }
}
