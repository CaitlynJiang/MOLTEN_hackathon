Original UI shape drafts, drawn for Reefkeeper on 2026-10-17.
These are newly authored geometric artwork inspired by the supplied shape reference, not extracted game assets or downloaded third-party artwork.

arc_track.png / .svg: warm off-white arc, 512 x 512 transparent canvas.
arc_fill_white.png / .svg: white tintable full arc on the identical canvas.
arc_fill_green.png / .svg: green full arc on the identical canvas.
arc_example_50.png: composited 50% appearance, for visual review only.
The arc is a 116-degree circular segment, open to the LEFT, with round caps.
Preserve the transparent square canvas when overlaying the layers. Circle centre is (256,256), radius 180.
These are image assets, not a wired progress-bar component. Runtime fill and moving round caps must be driven by the UI implementation; simply changing opacity does not set progress.

hint_panel_navy.png / .svg: plain irregular, softly curved navy text panel, 1024 x 256.
hint_panel_white.png / .svg: identical white mask for tinting in Unity.
Text is NOT embedded in either panel image. The preview uses Segoe UI only as an installed display font; no font files are included.
Suggested Unity sprite borders: left 80, bottom 52, right 80, top 52 pixels. Set Image Type to Sliced, with appropriate text padding. Inspect at the final aspect ratio: shallow irregularities along the middle edges stretch with panel size. No Unity import settings or scene changes have been applied.

preview.png is a flat-colour review board, not an in-game screenshot.
build.ps1 regenerates all files using Windows System.Drawing with supersampling.
No third-party artwork or fonts are bundled. The user may use and modify these original assets in their project without a third-party asset licence or attribution requirement.
