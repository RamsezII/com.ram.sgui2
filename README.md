# SGUI2

Runtime UI Toolkit docking. The visual hierarchy is the layout: `SguiWindow` is a
`VisualElement`, `DockGroup` is a `TabView`, and `DockSplit` is a `TwoPaneSplitView`.
`SguiEditor` owns the open/move/close operations; windows only build their content.

## Usage

```csharp
var editor = SguiEditor.instance;
var hierarchy = new SguiHierarchy();
var inspector = new SguiInspector();

editor.OpenWindow(hierarchy);
editor.DockWindow(inspector, hierarchy, SguiDockSide.Right, 300);
editor.DockWindow(inspector, hierarchy); // Move into the same tab group.
editor.CloseWindow(inspector);
```

- `OpenWindow(window)` opens in the active group. Calling it again with the same
  instance selects its existing tab. Distinct instances may have the same type.
- `DockWindow(window, targetWindow, side, size)` opens or moves a window relative
  to an already open target. `Center` adds a tab; the other sides create a split.
  `size` is the new pane's starting dimension in UI Toolkit layout units.
- `OpenWindow(window, side, fixedPaneDimension)` also supports opening beside the
  active group. Use `DockWindow` when the destination must be explicit.
- `CloseWindow(window)` and the native tab close button remove the window and
  collapse any now-redundant split. Closing an already closed window does nothing.
- Assigning `window.title` also updates its current tab label.
- Override `OnClosed` to release subscriptions when a window closes. Moving a
  window or reattaching the UI does not call it. If you reuse a closed instance,
  restore its subscriptions yourself before opening it again.

Use the editor API to move or remove windows; calling `RemoveFromHierarchy`
from window code bypasses layout cleanup.

## Layout rules

Every group has at least one tab; every tab contains one window; every split has
exactly two panes. `ReplaceNode` preserves sibling order, and `RemoveEmptyGroup`
replaces an empty group's parent split with the surviving pane. There is no
separate layout model or window registry to synchronize.

## Drag

- Drag the entire tab label to reorder. The blue line shows the insertion point,
  including when moving to another group's tab bar.
- Drop near a group's edge to split it; drop in its interior to add a tab.
- Drop outside a group or press **Escape** to cancel. A click without dragging
  simply selects the tab. The native close button keeps its behavior.

`_Drag.cs` handles only tab dragging. It computes a destination (group, side,
index and preview rectangle) and commits the move on release through the docking
operations. Ordering uses `TabView.ReorderTab`; there is no separate tab model.

Pages (one layout and its windows per page) and layout persistence are deferred.
