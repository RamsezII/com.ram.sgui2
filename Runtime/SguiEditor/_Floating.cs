using System;
using System.Collections.Generic;
using _SGUI2_.windows;
using UnityEngine;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    partial class SguiEditor
    {
        // Floating windows stay inside this runtime panel, not in separate OS windows.
        sealed class FloatingWindow : VisualElement
        {
            readonly VisualElement body;
            public override VisualElement contentContainer => body;

            public FloatingWindow(SguiEditor editor)
            {
                style.position = Position.Absolute;
                style.backgroundColor = new Color(.16f, .16f, .16f, 1);
                style.borderLeftWidth = style.borderRightWidth = style.borderTopWidth = style.borderBottomWidth = 1;
                style.borderLeftColor = style.borderRightColor = style.borderTopColor = style.borderBottomColor = new Color(.4f, .4f, .4f);

                var header = new VisualElement();
                header.style.height = 24;
                header.style.flexShrink = 0;
                header.style.flexDirection = FlexDirection.Row;
                header.style.backgroundColor = new Color(.22f, .22f, .22f);
                var title = new Label("Floating window");
                title.style.flexGrow = 1;
                title.style.paddingLeft = 6;
                title.RegisterCallback<PointerDownEvent>(evt => editor.BeginFrameDrag(this, false, evt));
                header.Add(title);
                header.Add(new Button(() => editor.CloseFloating(this)) { text = "×", tooltip = "Close this floating window" });
                hierarchy.Add(header);

                body = new VisualElement();
                body.style.flexGrow = 1;
                body.style.minHeight = 0;
                hierarchy.Add(body);

                var grip = new Label("◢") { tooltip = "Resize" };
                grip.style.position = Position.Absolute;
                grip.style.right = grip.style.bottom = 0;
                grip.style.width = grip.style.height = 18;
                grip.RegisterCallback<PointerDownEvent>(evt => editor.BeginFrameDrag(this, true, evt));
                hierarchy.Add(grip);
            }
        }

        public void FloatWindow(SguiWindow window, Rect rect = default)
        {
            var source = GetWindowGroup(window);
            if (rect == default)
                rect = new Rect(60, 60, 480, 320);
            if (!float.IsFinite(rect.x) || !float.IsFinite(rect.y) || !float.IsFinite(rect.width) || !float.IsFinite(rect.height) || rect.width <= 0 || rect.height <= 0)
                throw new ArgumentOutOfRangeException(nameof(rect));

            source?.RemoveWindow(window);
            var floating = new FloatingWindow(this);
            var group = new DockGroup(this);
            floating.Add(group);
            floatingLayer.Add(floating);
            SetFloatingRect(floating, rect);
            group.AddWindow(window);
            if (source != null)
                RemoveEmptyGroup(source);
            activeGroup = group;
        }

        void CloseFloating(FloatingWindow floating)
        {
            // Closing changes the tree, so collect windows before walking it.
            var windows = new List<SguiWindow>();
            floating.Query<SguiWindow>().ForEach(windows.Add);
            foreach (var window in windows)
                CloseWindow(window);
        }

        Rect ClampFloatingRect(Rect rect)
        {
            var bounds = floatingLayer.contentRect;
            float width = bounds.width > 0 ? bounds.width : 1920;
            float height = bounds.height > 0 ? bounds.height : 1080;
            rect.width = Mathf.Clamp(rect.width, Mathf.Min(180, width), width);
            rect.height = Mathf.Clamp(rect.height, Mathf.Min(120, height), height);
            rect.x = Mathf.Clamp(rect.x, 0, width - rect.width);
            rect.y = Mathf.Clamp(rect.y, 0, height - rect.height);
            return rect;
        }

        void SetFloatingRect(FloatingWindow floating, Rect rect)
        {
            rect = ClampFloatingRect(rect);
            floating.style.left = rect.x;
            floating.style.top = rect.y;
            floating.style.width = rect.width;
            floating.style.height = rect.height;
        }

        void DockAtRoot(SguiWindow window)
        {
            var source = GetWindowGroup(window);
            source?.RemoveWindow(window);
            var group = new DockGroup(this);
            dockRoot = group;
            dockLayer.Add(group);
            group.AddWindow(window);
            if (source != null)
                RemoveEmptyGroup(source);
            activeGroup = group;
        }
    }
}
