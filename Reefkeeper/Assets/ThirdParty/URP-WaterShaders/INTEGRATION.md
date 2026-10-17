# URP-WaterShaders integration

Upstream: https://github.com/aniruddhahar/URP-WaterShaders

Author: aniruddhahar. License: MIT; see LICENSE.txt.

Pinned upstream commit: `1d18105ab76f894e3187d32bb4e412dbcb35df2c`.

The complete Graphs folder is vendored with its original .meta files, preserving
the Caustics subgraph GUID references. Upstream recommends unitypackage export
to preserve these dependencies; this import preserves the same dependency files
and GUIDs directly. Demo scenes, models, materials and project settings are not imported.

Reefkeeper uses WaterUnder on static sand, rock and coral materials, plus
WaterDistortion through Unity 6's built-in FullScreenPassRendererFeature with
color fetch and depth input. Water level is 15. Fullscreen distortion is reduced
to 0.002 for the desktop first-person prototype. The water ceiling, grass sway,
fish and light shafts remain Reefkeeper shaders. WaterSurface is included as an
upstream reference but is not assigned in the scene.

Generated material settings and the renderer feature are configured by
Assets/Reefkeeper/Editor/ReefSceneBuilder.cs. No upstream graph edits are required.
XR rendering and headset comfort have not been validated.
