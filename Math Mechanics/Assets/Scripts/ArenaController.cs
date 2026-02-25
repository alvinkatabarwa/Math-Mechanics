using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArenaController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI levelText;        // Drag "LEVEL 1" TMP here
    public TextMeshProUGUI equationText;     // Equation TMP
    public TextMeshProUGUI feedbackText;     // "Make your move" TMP
    public TMP_InputField inputField;        // TMP Input Field (integer)

    [Header("Buttons / Counters")]
    public Button hintButton;                // Drag Hint button here
    public TextMeshProUGUI hintCounterText;  // Drag a TMP text like "Hints: 3/3" here
    public Button undoButton;                // Optional: drag Undo button here (so we can disable when empty)

    // ---------------- Equation + Level ----------------
    // Current equation: ax + b = c
    private int a, b, c;

    private int currentLevelIndex = 0;
    private bool isSolved = false;

    // Tap operation first, then enter number, then Confirm/Enter
    private enum Op { None, Add, Subtract, Multiply, Divide }
    private Op pendingOp = Op.None;

    [System.Serializable]
    private struct LevelData
    {
        public int a, b, c; // ax + b = c
        public LevelData(int a, int b, int c) { this.a = a; this.b = b; this.c = c; }
    }

    // Add more levels anytime
    private LevelData[] levels = new LevelData[]
    {
        new LevelData(2, 3, 11),   // x=4
        new LevelData(3, -6, 9),   // x=5
        new LevelData(5, 10, 0),   // x=-2
        new LevelData(4, -8, 20),  // x=7
    };

    [Header("Auto Next Level")]
    [SerializeField] private bool autoAdvance = true;
    [SerializeField] private float nextLevelDelay = 0.8f;

    // ---------------- Undo System ----------------
    private struct Snapshot
    {
        public int a, b, c;
        public bool isSolved;
        public Op pendingOp;
        public int levelIndex;
        public int hintsUsed;

        public Snapshot(int a, int b, int c, bool solved, Op op, int lvl, int hintsUsed)
        {
            this.a = a; this.b = b; this.c = c;
            this.isSolved = solved;
            this.pendingOp = op;
            this.levelIndex = lvl;
            this.hintsUsed = hintsUsed;
        }
    }

    private readonly Stack<Snapshot> history = new Stack<Snapshot>();

    // ---------------- Hint System ----------------
    [Header("Hints")]
    [SerializeField] private int maxHints = 3;
    private int hintsUsed = 0;

    // ---------------- Colors + FX ----------------
    [Header("Feedback Colors (Hex)")]
    [SerializeField] private string efficientHex = "#22C55E";   // green
    [SerializeField] private string inefficientHex = "#EF4444"; // red
    [SerializeField] private string invalidHex = "#9CA3AF";     // gray
    [SerializeField] private string neutralHex = "#FFFFFF";     // white

    [Header("FX Settings")]
    [SerializeField] private float fadeInDuration = 0.12f;
    [SerializeField] private float shakeDuration = 0.18f;
    [SerializeField] private float shakeStrength = 10f;
    [SerializeField] private int shakeVibrato = 14;
    [SerializeField] private float glowDuration = 0.28f;
    [SerializeField] private float glowScaleUp = 1.08f;
    [SerializeField] private float glowOutlineMax = 0.35f;

    private Color efficientColor, inefficientColor, invalidColor, neutralColor;

    private RectTransform feedbackRT;
    private Vector2 feedbackStartPos;
    private Vector3 feedbackStartScale;

    private Coroutine fadeRoutine;
    private Coroutine shakeRoutine;
    private Coroutine glowRoutine;
    private Coroutine nextLevelRoutine;

    // ---------------- Unity Lifecycle ----------------
    private void Awake()
    {
        ColorUtility.TryParseHtmlString(efficientHex, out efficientColor);
        ColorUtility.TryParseHtmlString(inefficientHex, out inefficientColor);
        ColorUtility.TryParseHtmlString(invalidHex, out invalidColor);
        ColorUtility.TryParseHtmlString(neutralHex, out neutralColor);

        feedbackRT = feedbackText.GetComponent<RectTransform>();
        feedbackStartPos = feedbackRT.anchoredPosition;
        feedbackStartScale = feedbackRT.localScale;

        // Outline pulse for "glow"
        feedbackText.fontMaterial.EnableKeyword("OUTLINE_ON");
        feedbackText.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        feedbackText.outlineWidth = 0f;
    }

    private void Start()
    {
        LoadLevel(0);
        SetFeedback("Tap an operation, then enter a number", neutralColor, doGlow: false, doShake: false);
        UpdateHintUI();
        UpdateUndoUI();
    }

    // ---------------- Level System ----------------
    private void LoadLevel(int index)
    {
        if (index < 0) index = 0;
        if (index >= levels.Length) index = levels.Length - 1;

        currentLevelIndex = index;

        a = levels[index].a;
        b = levels[index].b;
        c = levels[index].c;

        isSolved = false;
        pendingOp = Op.None;
        inputField.text = "";

        history.Clear();           // fresh history per level
        hintsUsed = 0;             // reset hints per level
        UpdateHintUI();
        UpdateUndoUI();

        if (levelText != null) levelText.text = $"LEVEL {index + 1}";
        UpdateEquationText();
    }

    public void NextLevel()
    {
        if (currentLevelIndex + 1 >= levels.Length)
        {
            SetFeedback("All levels complete! 🎉", efficientColor, doGlow: true, doShake: false);
            return;
        }

        LoadLevel(currentLevelIndex + 1);
        SetFeedback("New level! Tap an operation…", neutralColor, doGlow: false, doShake: false);
    }

    // Restart same level (start from beginning of equation)
    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex);
        SetFeedback("Restarted. Tap an operation…", neutralColor, doGlow: false, doShake: false);
    }

    // ---------------- Equation Display ----------------
    private void UpdateEquationText()
    {
        // When solved show X=4 (not 1x=4)
        if (a == 1 && b == 0)
        {
            equationText.text = $"X={c}";
            return;
        }

        string left = FormatAx(a);
        if (b > 0) left += $" + {b}";
        else if (b < 0) left += $" - {Mathf.Abs(b)}";

        equationText.text = $"{left} = {c}";
    }

    private string FormatAx(int coef)
    {
        if (coef == 1) return "x";
        if (coef == -1) return "-x";
        return $"{coef}x";
    }

    private int GetComplexity() => Mathf.Abs(a) + Mathf.Abs(b) + Mathf.Abs(c);

    // ---------------- Operation Selection (Tap First) ----------------
    public void SelectAdd() => SelectOperation(Op.Add, "+");
    public void SelectSubtract() => SelectOperation(Op.Subtract, "−");
    public void SelectMultiply() => SelectOperation(Op.Multiply, "×");
    public void SelectDivide() => SelectOperation(Op.Divide, "÷");

    private void SelectOperation(Op op, string symbol)
    {
        if (isSolved) return;

        pendingOp = op;
        SetFeedback($"Selected {symbol}. Enter a number then press OK", neutralColor, doGlow: false, doShake: false);
        inputField.ActivateInputField();
        inputField.Select();
    }

    // ---------------- OK / Confirm ----------------
    public void Confirm()
    {
        if (isSolved)
        {
            if (!autoAdvance) NextLevel();
            return;
        }

        if (pendingOp == Op.None)
        {
            SetFeedback("Pick an operation first (+, −, ×, ÷)", invalidColor, doGlow: false, doShake: true);
            return;
        }

        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            SetFeedback("Enter a number, then tap OK", invalidColor, doGlow: false, doShake: true);
            inputField.ActivateInputField();
            inputField.Select();
            return;
        }

        ApplyPendingOperation();
    }

    public void ApplyPendingOperation()
    {
        if (isSolved) return;
        if (!TryGetInputValue(out int v)) return;

        // SAVE snapshot for Undo BEFORE applying changes
        history.Push(new Snapshot(a, b, c, isSolved, pendingOp, currentLevelIndex, hintsUsed));
        UpdateUndoUI();

        int prevComplexity = GetComplexity();

        switch (pendingOp)
        {
            case Op.Add: b += v; c += v; break;
            case Op.Subtract: b -= v; c -= v; break;
            case Op.Multiply: a *= v; b *= v; c *= v; break;

            case Op.Divide:
                if (v == 0 || a % v != 0 || b % v != 0 || c % v != 0)
                {
                    // Undo the history push because the move didn't apply
                    history.Pop();
                    UpdateUndoUI();

                    SetFeedback("Invalid move (won't divide cleanly)", invalidColor, doGlow: false, doShake: true);
                    return;
                }
                a /= v; b /= v; c /= v;
                break;
        }

        FinishMove(prevComplexity);

        pendingOp = Op.None;
        inputField.text = "";
    }

    private bool TryGetInputValue(out int value)
    {
        value = 0;

        if (pendingOp == Op.None)
        {
            SetFeedback("Pick an operation first (+, −, ×, ÷)", invalidColor, doGlow: false, doShake: true);
            return false;
        }

        if (!int.TryParse(inputField.text, out value))
        {
            SetFeedback("Invalid number", invalidColor, doGlow: false, doShake: true);
            return false;
        }

        return true;
    }

    // ---------------- Undo (Redo last move) ----------------
    public void UndoLastMove()
    {
        if (history.Count == 0)
        {
            SetFeedback("Nothing to undo", invalidColor, doGlow: false, doShake: true);
            UpdateUndoUI();
            return;
        }

        Snapshot s = history.Pop();
        a = s.a; b = s.b; c = s.c;
        isSolved = s.isSolved;
        pendingOp = Op.None;
        // Keep same level index (but restore anyway)
        currentLevelIndex = s.levelIndex;
        hintsUsed = s.hintsUsed;

        inputField.text = "";

        UpdateEquationText();
        UpdateHintUI();
        UpdateUndoUI();

        SetFeedback("Undone", neutralColor, doGlow: false, doShake: false);
    }

    private void UpdateUndoUI()
    {
        if (undoButton != null)
            undoButton.interactable = history.Count > 0;
    }

    // ---------------- Hint System (Max 3) ----------------
    public void Hint()
    {
        if (isSolved)
        {
            SetFeedback("Already solved ", neutralColor, doGlow: false, doShake: false);
            return;
        }

        if (hintsUsed >= maxHints)
        {
            UpdateHintUI();
            SetFeedback("No hints left", invalidColor, doGlow: false, doShake: true);
            return;
        }

        hintsUsed++;
        UpdateHintUI();

        string hint = GenerateHint();
        SetFeedback(hint, neutralColor, doGlow: false, doShake: false);
    }

    private string GenerateHint()
    {
        // Simple, useful hints for ax + b = c:
        // 1) Remove b first
        if (b != 0)
        {
            if (b > 0) return $"Hint: Subtract {b} from both sides (tap − then type {b}).";
            else return $"Hint: Add {Mathf.Abs(b)} to both sides (tap + then type {Mathf.Abs(b)}).";
        }

        // 2) Then deal with a
        if (a != 1)
        {
            if (a == -1) return "Hint: Multiply both sides by −1 (tap × then type -1).";

            // Prefer clean division if possible
            if (a != 0 && c % a == 0)
                return $"Hint: Divide both sides by {a} (tap ÷ then type {a}).";

            // If not divisible, still suggest divide as a concept (your engine requires clean divide)
            return $"Hint: Try to make division by {a} possible (aim for right side divisible by {a}).";
        }

        // If a==1 and b==0 we'd be solved, but just in case:
        return "Hint: You're very close — aim for x on its own.";
    }

    private void UpdateHintUI()
    {
        int remaining = Mathf.Max(0, maxHints - hintsUsed);

        if (hintCounterText != null)
            hintCounterText.text = $"Hints: {remaining}/{maxHints}";

        if (hintButton != null)
            hintButton.interactable = remaining > 0;
    }

    // ---------------- Finish Move + Feedback ----------------
    private void FinishMove(int previousComplexity)
    {
        int newComplexity = GetComplexity();

        // Solved condition: x = c  => a==1 and b==0
        if (a == 1 && b == 0)
        {
            isSolved = true;
            UpdateEquationText(); // shows X=...
            SetFeedback("Solved!", efficientColor, doGlow: true, doShake: false);

            if (autoAdvance)
            {
                if (nextLevelRoutine != null) StopCoroutine(nextLevelRoutine);
                nextLevelRoutine = StartCoroutine(AdvanceAfterDelay());
            }
            else
            {
                SetFeedback("Solved! Tap OK to continue", efficientColor, doGlow: true, doShake: false);
            }

            return;
        }

        if (newComplexity < previousComplexity)
            SetFeedback("Efficient move", efficientColor, doGlow: true, doShake: false);
        else if (newComplexity > previousComplexity)
            SetFeedback("Inefficient move", inefficientColor, doGlow: false, doShake: true);
        else
            SetFeedback("Neutral move", neutralColor, doGlow: false, doShake: false);

        UpdateEquationText();
    }

    private IEnumerator AdvanceAfterDelay()
    {
        yield return new WaitForSecondsRealtime(nextLevelDelay);
        NextLevel();
    }

    // ---------------- Feedback FX (Fade + Shake + Glow) ----------------
    private void SetFeedback(string message, Color color, bool doGlow, bool doShake)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        if (glowRoutine != null) StopCoroutine(glowRoutine);

        feedbackRT.anchoredPosition = feedbackStartPos;
        feedbackRT.localScale = feedbackStartScale;
        feedbackText.outlineWidth = 0f;

        feedbackText.text = message;
        feedbackText.color = new Color(color.r, color.g, color.b, 0f);

        fadeRoutine = StartCoroutine(FadeInFeedback(color));
        if (doShake) shakeRoutine = StartCoroutine(ShakeFeedback());
        if (doGlow) glowRoutine = StartCoroutine(GlowFeedback());
    }

    private IEnumerator FadeInFeedback(Color targetColor)
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float a01 = Mathf.Clamp01(t / fadeInDuration);
            feedbackText.color = new Color(targetColor.r, targetColor.g, targetColor.b, a01);
            yield return null;
        }
        feedbackText.color = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);
    }

    private IEnumerator ShakeFeedback()
    {
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / shakeDuration);
            float damper = 1f - progress;

            float x = (Mathf.PerlinNoise(Time.unscaledTime * shakeVibrato, 0f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0f, Time.unscaledTime * shakeVibrato) - 0.5f) * 2f;

            feedbackRT.anchoredPosition = feedbackStartPos + new Vector2(x, y) * shakeStrength * damper;
            yield return null;
        }
        feedbackRT.anchoredPosition = feedbackStartPos;
    }

    private IEnumerator GlowFeedback()
    {
        float half = glowDuration * 0.5f;

        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / half);

            feedbackRT.localScale = Vector3.Lerp(feedbackStartScale, feedbackStartScale * glowScaleUp, k);
            feedbackText.outlineWidth = Mathf.Lerp(0f, glowOutlineMax, k);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / half);

            feedbackRT.localScale = Vector3.Lerp(feedbackStartScale * glowScaleUp, feedbackStartScale, k);
            feedbackText.outlineWidth = Mathf.Lerp(glowOutlineMax, 0f, k);
            yield return null;
        }

        feedbackRT.localScale = feedbackStartScale;
        feedbackText.outlineWidth = 0f;
    }
}