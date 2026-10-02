# Skyline and Golden gameplay comparison

Both retained scenes were built from the same current scripts in Unity 6000.3.25f1, then tested as Windows players with D3D11. The comparison run used a fixed 60 Hz movement step at 1920x1080 and an AMD Radeon RX 7600. It drove the regular `PlayerMotor` and `CharacterController` across the opening gap, shifted to the overgrown world, landed on its first bridge, and reached the shared landing. It did not simulate human camera input.

| Check | SkylineRooftops | GoldenRooftops |
| --- | --- | --- |
| Build | Passed | Passed |
| Headless smoke | Passed: blocked/safe/midair shifts, material restoration, jump steps at 30/60/120/144 Hz, respawn and goal | Passed with the same checks, including after camera tuning |
| Opening route | Passed in 135 fixed steps; jump peak y=1.85 m | Passed in 135 fixed steps; jump peak y=1.85 m |
| Movement/animation values | Same as Golden | Same as Skyline |
| Camera before this comparison | 8 m distance, 2.3 m look-ahead, 1.45 m height, 66 degree field of view | 4.9 m, 0.55 m, 1.35 m, 60 degree |
| Camera for Unity review now | Unchanged | Matched to Skyline's four values |

The equal route results show that the opening collision and motor behavior are effectively the same in this scripted run. The original Golden camera gave less view of the upcoming landing. The wider tuned Golden view makes the crossing easier to read in the captured frames. This supports the camera-framing part of the reported feel difference; it does not prove subjective smoothness or full-level quality.

## Captures from the Windows players

| Point | Skyline | Golden before camera tuning | Golden after camera tuning |
| --- | --- | --- | --- |
| Start | [Skyline start](ArtDirection/Comparison/skyline-compare.png) | [Golden start before](ArtDirection/Comparison/golden-compare.png) | [Golden start after](ArtDirection/Comparison/golden-start-tuned.png) |
| First bridge exit | [Skyline bridge](ArtDirection/Comparison/skyline-first-bridge.png) | [Golden bridge before](ArtDirection/Comparison/golden-first-bridge.png) | [Golden bridge after](ArtDirection/Comparison/golden-first-bridge-tuned.png) |

The automated run was in batch mode. Its recorded host-loop intervals do **not** represent presented frame times or GPU cost, so no FPS comparison is claimed. No person has yet played the entire level, tested active camera input, or reviewed a continuous gameplay recording. The two scenes contain 1,425 and 1,393 MeshRenderers respectively; that count alone does not establish which scene renders faster.

## Review in Unity

Open `UnityProject` in Unity Hub, then open `Assets/Shiftbound/Scenes/GoldenRooftops.unity` and press Play. Open `SkylineRooftops.unity` afterward for direct comparison. Both scene assets are retained; `GoldenRooftops` remains the only enabled build scene. The comparison builds are local at `UnityProject/Builds/ComparisonSkylineRooftops/Shiftbound.exe` and `UnityProject/Builds/ComparisonGoldenRooftops/Shiftbound.exe`.

The comparison builds are local artifacts; GitHub contains their Unity source and capture images.
