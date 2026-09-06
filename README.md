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
editor.FloatWindow(inspector, new Rect(60, 60, 480, 320));
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

## Drag and floating windows

- Drag an entire tab label (or its native handle) to reorder it. The thin blue
  line shows the insertion position, including when moving to another group.
- Drop near a group's edge to split it, or on its central `Tabs` hint to merge.
- Drop outside those docking zones to float the tab. Hold **Alt** to force a
  floating window anywhere. **Escape** cancels the gesture.
- Drag a floating frame's title bar to move it, or its lower-right grip to resize.
  Escape restores the previous rectangle. Frames stay within the workspace.
- Drag its tabs back onto a group to dock them again. With an empty workspace,
  drop at the center or an edge to restore the docked root.
- The frame's close button closes all its tabs; each tab also keeps its own native
  close button. Moving, docking and floating do not invoke `OnClosed`/`onClosed`.

`FloatWindow(window, rect)` opens or detaches a window into a floating frame.
The rectangle uses coordinates local to the floating layer; omitting it uses
`(60, 60, 480, 320)`. Floating frames can also contain splits. They are runtime
VisualElements inside the panel, not separate operating-system windows.

`_Drag.cs` owns one pointer gesture at a time, previews the destination, and
commits the window move only on release. It intercepts the header gesture so
Unity's handle dragger does not compete for capture, but uses the public
`TabView.ReorderTab` operation for ordering. `_Floating.cs` contains the frame and
its layout operations. `_Docking.cs` remains the shared tree mutation path.

Layout persistence and dragging between separate OS windows are not implemented.

