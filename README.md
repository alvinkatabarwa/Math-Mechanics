# MathMechanics (Unity Android App)

**MathMechanics** is a Unity-based math game with two modes:
- **Campaign Mode**: Levels 1–20 with increasing difficulty tiers
- **Timed Mode**: 60-second challenge, score = correct answers

---

## Requirements
- Unity: **6000.2.12f1**
- Platform: **Android**
- Device tested: **Samsung A06**

---

## How to Run (Unity Editor)
1. Open the project in **Unity 6000.2.12f1**
2. Go to **File → Build Settings**
3. Ensure these scenes are added in this order:
   - **Home** (Scene 0)
   - **Campaign Mode** (Scene 1)
4. Open **Home** scene and press **Play**

---

## How to Install and Run (Android APK)
1. Locate the APK in this repo:
   - `Builds/Android/MathMechanics.apk`
2. Copy the APK to your Android phone (USB / Google Drive / email)
3. On your phone, enable **Install unknown apps** for your file manager (Files/Drive)
4. Tap the APK → **Install**
5. Open the app and test both modes:
   - **Campaign**
   - **Timed**

---

## Demo Video (5 minutes)
- Link: https://youtu.be/ykSul2jRNpU?si=1OeSheObHLxhokQu

---

## Deployment
- Deployment type: **Android APK**
- Build verified: ✅ Installed and run successfully on Android device
- Core features verified on device: Campaign loop, Timed countdown, Results panel, Retry/Next navigation

---

# Testing Results (Screenshots with demos)

All screenshots are stored here:
- `Testing/Screenshots/`

## A) Testing strategies
Demonstration of functionality under different strategies:

- Functional flow (Campaign pass + results):
  - `Testing/Screenshots/01_campaign_pass_results.png`

- Negative/invalid input handling (non-integer or empty):
  - `Testing/Screenshots/02_campaign_invalid_input.png`

- Boundary test (Timed mode ends at 00:00 and shows results):
  - `Testing/Screenshots/03_timed_end_results.png`

## B) Different data values
Demonstration using different question types and values:

- Easy (Lvl 1–5): `ax + b = 0`
  - `Testing/Screenshots/04_easy_equation.png`

- Medium (Lvl 6–10): `ax + b = cx + d`
  - `Testing/Screenshots/05_medium_equation.png`

- Intermediate (Lvl 11–15): `x² + bx + c = 0` (can have 2 solutions)
  - `Testing/Screenshots/06_intermediate_quadratic.png`

- Boss (Lvl 16–20): `(x+a)(x+b) = c`
  - `Testing/Screenshots/07_boss_equation.png`

## C) Different hardware / software specifications
Performance across different environments:

- Unity Editor run:
  - `Testing/Screenshots/08_editor_running.png`

- Android device run:
  - `Testing/Screenshots/09_phone_campaign_running.jpg`

(Optional extra)
- Timed scoring in progress:
  - `Testing/Screenshots/10_timed_scoring.png`

---

# Analysis (Objectives vs Results)

**Objective: Ship a stable, playable math game focused on clarity and flow (not polish).**

### Results achieved
- ✅ **Campaign Mode loop complete**: Home → Campaign → Solve → Results panel → Retry / Next / Home
- ✅ **Difficulty scaling implemented** using level tiers:
  - Lvl 1–5 Easy (`ax + b = 0`)
  - Lvl 6–10 Medium (`ax + b = cx + d`)
  - Lvl 11–15 Intermediate (quadratic)
  - Lvl 16–20 Boss (expanded product form)
- ✅ **Step-by-step solving implemented for Levels 1–10** (supports learning progression)
- ✅ **Hint system implemented** (guides solving steps without giving the answer)
- ✅ **Streak system implemented** with rule: hint usage prevents streak increase
- ✅ **Timed Mode implemented**: 60-second countdown + score based on correct answers
- ✅ **Stability**: No console errors; APK built and tested successfully on Android device

### Missed / intentionally postponed (future work)
- Audio feedback and deeper polish (planned but postponed for stability)
- Leaderboards / persistence UI enhancements
- Advanced difficulty adaptation in timed mode

---

# Discussion (Milestones and Impact)

- Building **Campaign Mode first** ensured a stable core gameplay loop and reduced bugs before adding Timed Mode.
- Reusing **one gameplay scene** for both modes reduced complexity and prevented scene-transfer issues.
- Implementing step-solving in early levels improved usability for beginners and aligned with educational goals.
- Testing in both **Unity Editor and Android hardware** ensured the final product works in the target environment.

---

# Recommendations / Future Work

- Add sound feedback (button click, correct/incorrect, timed-end) to increase engagement.
- Add player progress UI and saved statistics (best streak, best timed score).
- Add leaderboards for timed mode and improved difficulty ramp based on performance.
- Expand step-solving to later tiers (quadratics/boss) using structured guided steps.

---

## Submission Notes
Attempt 1 includes:
- This GitHub repo (README + screenshots + APK)
- Demo video link
- APK file in `Builds/Android/`

Attempt 2:
- Zip file of this repo exactly as submitted in Attempt 1
