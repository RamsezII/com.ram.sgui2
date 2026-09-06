using _ARK_;
using UnityEngine;

namespace _SGUI2_
{
    public sealed partial class SguiEditor : ArkComponent1
    {

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Util.InstantiateOrCreate<SguiEditor>();
        }

        //--------------------------------------------------------------------------------------------------------------


    }
}