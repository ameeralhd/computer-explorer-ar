# QA & testing checklist (blueprint §19, §20, §22)

Tick each item on **at least one small (≈5.5"), one standard (≈6.1") and one large (≈6.7"+) Android phone**, and
where possible on one tablet and one low-end device.

| # | Area | Test | Pass criteria | ✔ |
|---|---|---|---|---|
| 1 | Navigation | Tap every Main Menu card, the menu (☰) items, Help and Teacher Guide; press Back / Android back on each screen | Every action opens the intended screen; Back returns to the previous screen; on the Main Menu, back asks before exiting | ☐ |
| 2 | Vertical slice | Module 1 from Module Detail: Von Neumann → AR CPU → Scenario → Practice → Reflection | Each step unlocks "Lanjut" only after its activity; ends on Progress with "Modul selesai!" | ☐ |
| 3 | AR tracking | Print targets at 100 %; scan under normal classroom light at 20–40 cm | Target recognised in < 3 s; status chip shows "Target terdeteksi" | ☐ |
| 4 | 3D model | Move the phone slowly around the card | Model stays attached and stable; Reset restores rotation/zoom; Zoom cycles 1× / 1.5× / 2× | ☐ |
| 5 | Hotspots | Tap each hotspot, and pick each from the list | Correct title/description; "dilihat" label and green colour after viewing; counter increments | ☐ |
| 6 | Tracking loss | Cover the card for more than 4 s | Status "Target hilang"; recovery tips appear; model reappears when the card is visible again | ☐ |
| 7 | AR error | Deny camera permission / remove the license key | Clear error message and a working "Gunakan Mode Simulasi" button | ☐ |
| 8 | Audio | Dengarkan → Jeda → Lanjutkan → Putar ulang; change screen while speaking | Narration plays in Bahasa Indonesia; pause/replay work; narration stops on scene change; nothing overlaps | ☐ |
| 9 | Text size | Set each size; visit every screen | No clipped or overlapping text; grids switch to 1 column at Besar/Sangat Besar | ☐ |
| 10 | Contrast | High contrast on every screen, including AR | Black background, white text, yellow actions, visible outlines; status never relies on colour alone | ☐ |
| 11 | Reduced motion | Enable, then visit Splash, Welcome, AR, Von Neumann | No fades, bobbing or pulsing; the data flow advances in steps; functionality unchanged | ☐ |
| 12 | Guided mode | Toggle Guided / Standard | "Petunjuk" cards shown only in Guided mode on every screen that has one | ☐ |
| 13 | Large touch targets | Enable | All buttons ≥ 64 dp; AR hotspots easier to hit | ☐ |
| 14 | Persistence | Change settings, complete steps, force-close, reopen | Settings and progress restored; "Lanjutkan" shows the last module | ☐ |
| 15 | Quiz | Answer each question type right and wrong | Correct/incorrect logic and explanations match QuestionData; results summary is correct | ☐ |
| 16 | Scenario | Complete a scenario via AR, and another via "Baca info tanpa kamera" | Both paths unlock the task; feedback and reflection are saved; the scenario shows "Selesai" | ☐ |
| 17 | UI scaling | Portrait on all test devices, including notch / gesture-bar phones | Controls inside the safe area; nothing at the extreme edges; content ≤ 560 dp wide on tablets | ☐ |
| 18 | Performance | Profile the AR scene on a low-end device | Stable ≥ 30 fps; only focus targets active in lessons; no repeated instantiation | ☐ |

## Android delivery checklist (§20)

- [ ] Bundle id `com.computerexplorer.arlearning` and product name "Computer Explorer AR" are consistent
- [ ] Orientation: Portrait
- [ ] Camera permission prompt appears; Vuforia camera works
- [ ] Release build (Development Build unticked) used for final tests
- [ ] Tested on physical devices, not only the Editor
- [ ] All 15 scenes listed in Build Settings in order (00_Boot first)
- [ ] Vuforia license key set; the targets in `Resources/ARTargets` are included
- [ ] Indonesian TTS voice installed on test devices, or recorded narration assigned

## Accessibility test script (quick, 5 minutes)

1. Settings → Sangat Besar + Kontras Tinggi + Kurangi gerakan + Tombol besar.
2. Complete Module 1 using only the in-app narration and the hotspot list (without tapping 3D points).
3. Note any screen where you had to scroll sideways, text was cut off, or a control was hard to reach.
