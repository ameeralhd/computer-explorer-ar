# Technical architecture

## Runtime services (blueprint §5.1)

`AppManager` is created automatically before the first scene loads
(`[RuntimeInitializeOnLoadMethod]`). Because of that, **any scene can be played directly in the Editor**. It persists
across scenes and owns the core services. These are the only global singletons (§21):

```
[AppManager]  (DontDestroyOnLoad, single AudioListener, ContentLibrary)
├── AccessibilityManager   global settings + Changed event, persisted to accessibility.json
├── ProgressManager        learner progress, persisted to progress.json (SaveSystem)
├── AudioManager           music / UI / narration / instruction / feedback sources + NarrationController
├── GameStateManager       session state that survives scene changes (lesson, scenario, selected hardware)
├── SceneLoader            fade transitions (instant with reduced motion)
├── NavigationController   back history, Android back button
├── LearningManager        ModuleManager + LessonController
├── ModalController        bottom-sheet dialogs
├── PopupController        toasts
└── EventSystem            Input System or legacy module, chosen by define
```

## Scenes

| Scene | Root object → component | Depends on |
|---|---|---|
| 00_Boot | `SplashScreen` | core services → Welcome |
| 01_Welcome | `WelcomeScreen` | Progress (first run) |
| 02_MainMenu | `MainMenuController` | Learning, Progress |
| 03_LearningModules | `LearningModulesScreen` | ModuleManager |
| 04_ModuleDetail | `ModuleDetailScreen` | GameState.CurrentModuleId |
| 05_ARScanner | `ARCamera` (+`VuforiaBehaviour`), `ARSessionController`, `ARManager`, `ARCanvas`→`ARScannerScreen` | GameState.SelectedHardwareId, ARReturnScene |
| 06_HardwareExplorer | `HardwareExplorerScreen` | Content |
| 07_VonNeumann | `VonNeumannScreen` | Content (CPU / Von Neumann hotspots) |
| 08_ContextualLearning | `ContextualLearningScreen` | GameState.ActiveScenario |
| 09_Practice | `PracticeScreen` | QuizManager.Active |
| 10_Reflection | `ReflectionScreen` | GameState.ReflectionContextId |
| 11_Progress | `ProgressScreen` | Progress |
| 12_Accessibility | `AccessibilityScreen` | AccessibilityManager |
| 13_Help | `HelpScreen` | — |
| 14_TeacherGuide | `TeacherGuideScreen` | Content, Progress |

## UI architecture

- **`DesignTokens`** holds every size: the 8-pt spacing grid, radii, touch targets, header height and max content
  width. Colours come from **`ContrastController`** (Standard / High palettes). The type scale comes from
  **`TextSizeController`**.
- **`UIKit`** is the component library: Button (primary / secondary / tonal / ghost / danger / success × normal /
  pressed / disabled / loading / completed), IconButton, Chip, Badge, Card, Callout, ProgressBar, ToggleRow,
  Segmented, Slider, TextInput and IconTile. It uses anchors and layout groups only, so content reflows.
- **`ScreenBase`** builds the same frame on every screen: safe area, header (back or menu, title, "read this
  screen" narration, accessibility settings), guided-mode hint, lesson step bar, scrolling content and a bottom
  action bar. Screens implement `BuildContent` / `BuildFooter` and call `Render()` after a state change. An
  accessibility change re-renders the whole screen, so settings apply everywhere with no per-screen code (§9, §21).
- **`CanvasFactory`** sets up every canvas the same way: reference 1080 × 1920, Scale With Screen Size, match 0.5
  (1.0 in landscape).

## Learning flow

`ModuleData.lessonSteps` is data: `VonNeumann`, `AR (hardware)`, `HardwareExplorer`, `Scenario (scenario)`,
`Practice` and `Reflection`. `LessonController.OpenCurrentStep()` sets the context
(`SelectHardware`, `ActiveScenario`, and so on) and opens the scene. Each screen shows **"Lanjut ke langkah
berikutnya"** through `ScreenBase.AddLessonContinue`. The button is enabled only when the step's learning activity
is done:

| Step | Completion rule |
|---|---|
| Von Neumann | 4 components explored **or** the data flow played to the end |
| AR | at least 2 hotspots explored (or all, if the model has fewer) |
| Hardware Explorer | at least 3 of the module's devices viewed |
| Scenario | reflection submitted |
| Practice | all questions answered |
| Reflection | reflection submitted |

## AR pipeline (§7.3)

```
ARManager.Start ─► ARSessionController.Begin ─► Vuforia (VUFORIA_ENGINE) or Simulation
      │ Ready
      ▼
for each target (focus hardware, or all): ARTargetController.CreateVuforia / CreateSimulated
                                          ARObjectController.Create (prefab or HardwareModelFactory + ARHotspots)
TrackingChanged(true) ─► show model, status = Detected, feedback sound, progress.MarkHardwareViewed
tap ─► ARInteractionController (UI layer, Physics.Raycast) ─► ARManager.SelectHotspot
      ─► progress.MarkHotspotExplored ─► Changed ─► ARScannerScreen.Render (info panel + narration)
```

- Targets are created at runtime from `Resources/ARTargets/<arTargetName>.png` at `printedWidthMeters` (0.15 m).
  `AssetImportSettings` keeps these textures readable and in RGBA32, as Vuforia requires.
- `ARObjectController` scales the model to 80 % of the printed width. It corrects for the target's own transform
  scale, so the size is the same in Vuforia and in Simulation.
- Hotspots can also be chosen from a list in the info panel. This is an accessible alternative to tapping small
  3D points.

## How to add a new AR hardware item

Example: **GPU**. Only data and art change; no core system code is touched (§22).

1. **Target image.** Add `GPU_Target` to `TARGETS` in `tools/asset-gen/generate.mjs` and run `npm run generate`,
   or drop any feature-rich 1024 × 1024 PNG into `Assets/Resources/ARTargets/GPU_Target.png`.
2. **Content.** *Assets → Create → Computer Explorer → Hardware Data* in `Assets/ScriptableObjects/Hardware`:
   - `hardwareId = "gpu"`, names, category, descriptions, function, contextual example, `iconName`,
   - `arTargetName = "GPU_Target"`, `printedWidthMeters = 0.15`,
   - **Model Prefab**: your FBX prefab (about 1 unit wide, resting on y = 0), or leave it empty for a generic block,
   - **Hotspots**: id, label, description and `localPosition` in model units (optional narration clip each).
3. Optionally add it to a module (`ModuleData.hardware`, and an `AR` lesson step) and write questions or a
   scenario that reference it.
4. Run **Computer Explorer → Rebuild Content Database**.

The new item then appears in the Hardware Explorer, the AR scanner, the simulation card list and progress
totals.

## How to add a module, scenario or question

Create the asset (*Create → Computer Explorer → …*), fill in the fields, link it from a module, then run
**Rebuild Content Database**. The question answer format is documented on `QuestionData`.

## Persistence

`SaveSystem` writes JSON to `Application.persistentDataPath` (`progress.json`, `accessibility.json`,
`audio.json`), going through a temporary file. `ProgressData` is flat and id-based. To add a backend later, replace
`SaveSystem` / `ProgressManager` persistence; the UI only talks to `ProgressManager`.

## Narration

`NarrationController.Play(text, clip)`: uses the recorded `AudioClip` if one is assigned, otherwise Android TTS
(`id-ID`). In the Editor it simulates playback (logged and timed). Only one narration plays at a time; it stops on
scene change, and music is ducked while it plays. To use recorded narration, assign clips to
`HardwareData.narration`, `HotspotData.narration`, `QuestionData.audio` and `ScenarioData.audio`.
