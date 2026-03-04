using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MathMechanics
{
    public class GameUIController : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text levelText;     // top banner text
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text streakText;
        [SerializeField] private TMP_Text hintText;

        [Header("Step UI")]
        [SerializeField] private TMP_Text stepPromptText;

        [Header("Timed UI")]
        [SerializeField] private TMP_Text timerText; // TimerTMP
        [SerializeField] private TMP_Text scoreText; // ScoreTMP

        [Header("Input")]
        [SerializeField] private TMP_InputField answerInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button hintButton;

        [Header("Nav")]
        [SerializeField] private Button backToMenuButton;

        public event Action<string> SubmitPressed;
        public event Action HintPressed;
        public event Action BackPressed;

        private void Awake()
        {
            if (submitButton != null)
                submitButton.onClick.AddListener(() =>
                    SubmitPressed?.Invoke(answerInput != null ? answerInput.text : "")
                );

            if (hintButton != null)
                hintButton.onClick.AddListener(() => HintPressed?.Invoke());

            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(() => BackPressed?.Invoke());
        }

        // ✅ NEW: generic header setter
        public void SetHeader(string text)
        {
            if (levelText != null)
                levelText.text = text ?? "";
        }

        public void SetLevel(int levelIndex, QuestionTier tier)
        {
            SetHeader($"LVL {levelIndex}: {tier.ToString().ToUpper()}");
        }

        public void SetQuestion(string q)
        {
            if (questionText != null)
                questionText.text = q;
        }

        public void SetStepPrompt(string p)
        {
            if (stepPromptText != null)
                stepPromptText.text = p ?? "";
        }

        public void SetStreak(int streak)
        {
            if (streakText != null)
                streakText.text = streak.ToString();
        }

        public void SetHint(string hint)
        {
            if (hintText != null)
                hintText.text = hint;
        }

        public void ClearHint() => SetHint("");

        public void ClearInput()
        {
            if (answerInput == null) return;
            answerInput.text = "";
            answerInput.Select();
            answerInput.ActivateInputField();
        }

        public void SetInputInteractable(bool value)
        {
            if (answerInput != null) answerInput.interactable = value;
            if (submitButton != null) submitButton.interactable = value;
            if (hintButton != null) hintButton.interactable = value;
        }

        public void SetHintButtonInteractable(bool value)
        {
            if (hintButton != null) hintButton.interactable = value;
        }

        // -------- Timed UI --------

        public void SetTimerVisible(bool visible)
        {
            if (timerText != null) timerText.gameObject.SetActive(visible);
        }

        public void SetScoreVisible(bool visible)
        {
            if (scoreText != null) scoreText.gameObject.SetActive(visible);
        }

        public void SetTimer(float seconds)
        {
            if (timerText == null) return;

            int s = Mathf.CeilToInt(seconds);
            if (s < 0) s = 0;

            int m = s / 60;
            int r = s % 60;
            timerText.text = $"{m:00}:{r:00}";
        }

        public void SetScore(int score)
        {
            if (scoreText == null) return;
            scoreText.text = $"Score: {score}";
        }
    }
}