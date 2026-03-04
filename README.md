# Math Mechanics

Unity math game with two modes:
- **Campaign Mode** (Levels 1–20, increasing difficulty)
- **Timed Mode** (60 seconds, score = correct answers)

## Requirements
- Unity: **6000.2.12f1**
- Android device for APK install

## How to run (Unity)
1. Open the project in Unity 6000.2.12f1
2. File → Build Settings → ensure scenes are added in this order:
   - **Home**
   - **Campaign Mode**
3. Open **Home** scene and press Play

## How to install (Android APK)
1. On your phone, enable **Install unknown apps** (for Files/Drive)
2. Copy the APK from: `Builds/Android/` onto your phone
3. Tap the APK → Install → Open

## Demo Video (5 minutes)
- Link: **PASTE LINK HERE**

## Deployment
- Target platform: Android
- Verified on: **(your phone model + Android version)**

## Testing Results (Screenshots)
### Testing strategies
- Campaign functional flow: `Testing/Screenshots/01_campaign_pass.png`
- Invalid input handling: `Testing/Screenshots/02_invalid_input.png`
- Timed mode end-of-timer: `Testing/Screenshots/03_timed_end.png`

### Different data values
- Easy (Lvl 1–5): `Testing/Screenshots/04_easy.png`
- Medium (Lvl 6–10): `Testing/Screenshots/05_medium.png`
- Intermediate (Lvl 11–15): `Testing/Screenshots/06_intermediate.png`
- Boss (Lvl 16–20): `Testing/Screenshots/07_boss.png`

### Different hardware/software specs
- Unity Editor run: `Testing/Screenshots/08_editor.png`
- Android device run: (use one of the phone screenshots above)

## Analysis (Objectives vs Results)
- Campaign mode loop achieved: level start → solve → results → retry/next/menu.
- Difficulty increases by level tiers (Easy/Medium/Intermediate/Boss).
- Timed mode achieved: countdown timer + score based on correct answers + results + retry.
- Stability achieved: no console errors; APK built and runs on Android.

## Discussion (Milestones + impact)
- Building Campaign loop first reduced bugs and ensured a stable core experience.
- Reusing one gameplay scene simplified mode switching and reduced scene-transfer complexity.
- Step-by-step solving in early levels improves learning by enforcing correct transformations.

## Recommendations / Future Work
- Add audio feedback + animations for more engagement.
- Add persistence of campaign progress UI and more detailed stats.
- Add leaderboards for timed mode.
- Improve adaptive difficulty based on accuracy over time.
