# Reefkeeper — underwater prototype

Unity **6.3 LTS / 6000.3.19f1**, Universal Render Pipeline. Open the `Reefkeeper` folder in Unity Hub, then open `Assets/Reefkeeper/Scenes/ShallowReef.unity` and press Play. If the scene has not been generated yet, choose **Reefkeeper → Create or rebuild shallow reef** in the Editor menu.

![Milestone 01 — scene rendered by the Windows prototype](docs/previews/milestone-01-reef.png)

Milestone 01: Windows build succeeds; movement math and runtime movement/collision checks pass. Three offscreen scene renders were captured for visual review (HUD is not part of those renders). These are procedural prototype assets; underwater materials and art can be replaced independently.

## Controls

| Input | Action |
| --- | --- |
| Mouse | Look |
| WASD | Swim relative to view direction |
| Space / C | Rise / descend |
| Shift + movement | Sprint / stronger fin kick |
| Right mouse button or X | Active brake; overrides propulsion |
| Release movement | Coast with water drag |
| R | Reset to the starting point |
| Esc / left click | Release cursor / resume |

No gravity, head bob, camera roll or sprint FOV changes. Change the public settings on **Diver / desktop input → DiveMotor** to tune the feel. This is currently a desktop prototype, not a validated VR build. Glove capture and live input are not connected yet.

## Current scope

- A procedurally authored shallow reef with sand ripples, animated caustics, coral, limestone, a distant arch, fish, seagrass, particles and a water ceiling.
- View-relative swimming, boost, drag, active braking, collisions, a surface limit and an exploration boundary.
- Small depth/speed display and keyboard hints.
- Editable source assets and a reproducible scene builder. Generated assets live in `Assets/Reefkeeper/Generated`; save a copy of the scene before rebuilding if you have edited it by hand.

Next: cleaning coral racks with a brush and placing nursery fragments. Swimming motion capture is optional polish after the interaction loop works. A current-meter prop is an optional later interaction.

## Mocap capture while the loaner gloves are available

Keep the original Rokoko recordings and ask the hardware desk which export permission is available. Export animation with finger bones (FBX suitable for Unity when available), together with the actor/rig and calibration information. Record slow and natural-speed takes, with a neutral lead-in and lead-out.

Suggested clips: relaxed hand, empty-hand fist/open, brush grip, repeated brushing, brush release, coral-fragment pinch, place and release, optional instrument grip/button press. Use the same prop grip placement each take. Record left/right consistently; name takes with hand, action and speed. Glove finger capture does not imply tracked arm or world-position motion: preserve the distinction when reviewing assets.

Recorded clips should remain reusable without the gloves; interaction events will be separate from hardware adapters. Today's live glove input and any recorded animation playback must be identified honestly in the demonstration.

## Validation

**Reefkeeper → Validate swimming math** checks speed limits, diagonal normalization, coasting, brake priority and drag across frame rates. The command-line runtime smoke check also exercises actual CharacterController collision and the surface limit and captures three rendered views:

```text
Reefkeeper.exe -reef-smoke <absolute-output-directory> -screen-width 1600 -screen-height 900 -windowed
```

Build with `Reefkeeper.Editor.ReefSceneBuilder.BuildWindows` through Unity's `-executeMethod`, or the normal Windows Build Profiles workflow. Outputs are local under `Reefkeeper/Builds/Windows` and are not committed.

## Credits

Unity's bundled Universal 3D template supplies the URP render settings. Reef geometry, shaders, movement and interface were authored during this hackathon with Codex assistance. No downloaded third-party models, textures or sound clips are currently used.
