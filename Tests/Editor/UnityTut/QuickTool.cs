using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace _SGUI2_.Editor.Tests
{
    internal class QuickTool : EditorWindow
    {
        [MenuItem("Assets/" + nameof(_SGUI2_) + "/" + nameof(QuickTool) + "/" + nameof(ShowWindow) + " _%#T")]
        public static void ShowWindow()
        {
            var window = GetWindow<QuickTool>();
            window.titleContent = new GUIContent(typeof(QuickTool).FullName);
            window.minSize = new Vector2(280, 50);
        }

        //--------------------------------------------------------------------------------------------------------------

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            Label label = new() { text = "Label", };
            root.Add(label);

            Button button = new() { text = "Button", };
            root.Add(button);
        }
    }
}