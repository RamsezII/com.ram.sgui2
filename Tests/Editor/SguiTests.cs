using _SGUI2_.windows;
using UnityEngine;

namespace _SGUI2_.Tests.Editor
{
    static partial class SguiTests
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            SguiEditor.instance.OpenWindow(new SguiInspector());
        }
    }
}