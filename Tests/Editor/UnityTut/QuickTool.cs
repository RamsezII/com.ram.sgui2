using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace _SGUI2_.Editor.Tests
{
    internal class QuickTool : EditorWindow
    {
        [MenuItem("Assets/" + nameof(_SGUI2_) + "/" + nameof(QuickTool) + " " + nameof(ShowWindow) + " _%#T")]
        public static void ShowWindow()
        {
            var window = GetWindow<QuickTool>();
            window.titleContent = new GUIContent(typeof(QuickTool).FullName);
            window.minSize = new Vector2(280, 50);
        }

        //--------------------------------------------------------------------------------------------------------------

        public void CreateGUI()
        {
            var root = rootVisualElement;

            foreach (var code in Util.EGetEnumValues<PrimitiveType>())
            {
                var button = new Button(() =>
                {
                    Debug.Log(code, this);
                    var go = ObjectFactory.CreatePrimitive(code);
                    go.transform.position = Vector3.zero;
                });
                button.Add(new Label(code.ToString()));
                root.Add(button);
            }
        }
    }
}