using _ARK_;
using _UTIL_;
using UnityEngine;
using UnityEngine.UIElements;

namespace _SGUI2_
{
    [RequireComponent(typeof(PanelRenderer))]
    public sealed partial class SguiEditor : ArkComponent1
    {
        public static SguiEditor instance;

        [HideInInspector] public PanelRenderer panelRenderer;
        [HideInInspector] public VisualElement root;
        public int uiVersion = -1;

        public readonly ValueNotifier<bool> toggle = new();

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Util.InstantiateOrCreate<SguiEditor>();
        }

        //--------------------------------------------------------------------------------------------------------------

        protected override void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            panelRenderer = GetComponent<PanelRenderer>();
            panelRenderer.RegisterUIReloadCallback(OnUIReload);

            base.Awake();

#if ENABLE_INPUT_SYSTEM
            ArkShortcuts.AddShortcut_keyboard(
                shortcutName: typeof(SguiEditor).FullName,
                action: toggle.ToggleAuto,
                control: true,
                bindings: UnityEngine.InputSystem.Key.Y
            );
#endif
        }

        //--------------------------------------------------------------------------------------------------------------

        protected override void Start()
        {
            base.Start();

            toggle.AddListener(OnToggleVisual);
        }

        //--------------------------------------------------------------------------------------------------------------

        void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            this.root = root;

            if (version == uiVersion)
                return;
            uiVersion = version;

            root.Clear();
            root.style.flexGrow = 1;
            root.Add(CreateDesktop());

            OnToggleVisual();
        }

        void OnToggleVisual()
        {
            if (root == null)
                return;
            root.style.display = toggle._value ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}