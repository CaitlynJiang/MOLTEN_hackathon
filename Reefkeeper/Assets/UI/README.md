# UI asset drop folder

Place UI artwork, sprite sheets, fonts and related license/source notes here.
The original shapes in Shapes and Nunito SemiBold/Bold in Fonts/Nunito are
assigned to ReefCleaningHUD on the nursery workshop. New files dropped here
are not automatically assigned.

Both arc sprites keep their complete 512 x 512 canvas and share one UI rect.
The custom UI Arc Progress material clips the fill and adds a round moving cap.
The hint mask uses nine-slice borders: left 80, bottom 52, right 80, top 52.
Its text is a separate UI object. Sprites use alpha, bilinear filtering, no
mipmaps and uncompressed imports.

F1 toggles controls; O folds objectives. The HUD renders after water effects so its text remains clear. It is intended
for the laptop demo; a headset will need an appropriately positioned XR UI.

ReefCleaningBuilder.PolishAndBuild reapplies these references and import settings
to the saved scene, then builds Windows. The original source art notes are
preserved in Shapes/README.txt; the Nunito license is in Fonts/Nunito/OFL.txt.
