using UnityEngine.UIElements;

namespace _SGUI2_
{
    partial class SguiEditor
    {
        static VisualElement CreateDesktop()
        {
            /*
             * ┌───────────┬────────────────────────┬──────────────┐
             * │ Hierarchy │ Scene / Game           │ Inspector    │
             * │           │                        │              │
             * │           ├────────────────────────┤              │
             * │           │ Console                │              │
             * └───────────┴────────────────────────┴──────────────┘
             */

            var rootSplit = new TwoPaneSplitView(
                fixedPaneIndex: 0,
                fixedPaneStartDimension: 250,
                orientation: TwoPaneSplitViewOrientation.Horizontal
            );

            rootSplit.style.flexGrow = 1;

            // LEFT ----------------------------------------------------------------

            rootSplit.Add(
                CreateTabs(
                    ("Hierarchy", new Label("Hierarchy content"))
                )
            );

            // RIGHT ----------------------------------------------------------------

            var rightSplit = new TwoPaneSplitView(
                1,
                300,
                TwoPaneSplitViewOrientation.Horizontal);

            rightSplit.style.flexGrow = 1;

            rootSplit.Add(rightSplit);

            // CENTER ---------------------------------------------------------------

            var centerSplit = new TwoPaneSplitView(
                1,
                200,
                TwoPaneSplitViewOrientation.Vertical);

            centerSplit.style.flexGrow = 1;

            rightSplit.Add(centerSplit);

            centerSplit.Add(
                CreateTabs(
                    ("Scene", new Label("Scene content")),
                    ("Game", new Label("Game content"))
                )
            );

            centerSplit.Add(
                CreateTabs(
                    ("Console", new Label("Console content")),
                    ("Terminal", new Label("Terminal content"))
                )
            );

            // RIGHT / INSPECTOR ----------------------------------------------------

            rightSplit.Add(
                CreateTabs(
                    ("Inspector", new Label("Inspector content"))
                )
            );

            return rootSplit;
        }

        //----------------------------------------------------------------------------------------------------------

        static TabView CreateTabs(params (string title, VisualElement content)[] entries)
        {
            var tabView = new TabView
            {
                reorderable = true
            };

            tabView.style.flexGrow = 1;

            foreach (var (title, content) in entries)
            {
                var tab = new Tab(title);

                content.style.flexGrow = 1;

                tab.Add(content);
                tabView.Add(tab);
            }

            return tabView;
        }
    }
}