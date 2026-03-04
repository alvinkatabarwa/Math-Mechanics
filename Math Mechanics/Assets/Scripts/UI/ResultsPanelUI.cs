using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathMechanics
{
    public class ResultsPanelUI : MonoBehaviour
    {
        [Header("Overlay + Card")]
        [SerializeField] private GameObject overlay;
        [SerializeField] private RectTransform card;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Text")]
        [SerializeField] private TMP_Text resultTitleTMP;
        [SerializeField] private TMP_Text resultDetailsTMP;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button backButton;

        [Header("Optional Next Label")]
        [SerializeField] private TMP_Text nextButtonLabelTMP;

        public event Action RetryPressed;
        public event Action NextPressed;
        public event Action BackPressed;

        [Header("Pop Animation")]
        [SerializeField] private float popDuration = 0.18f;
        [SerializeField] private float startScale = 0.85f;
        [SerializeField] private bool useFade = true;

        private Coroutine animCo;
        private bool nextIsFinishMode;

        private readonly Color32 correctColor = new Color32(46, 204, 113, 255);
        private readonly Color32 incorrectColor = new Color32(231, 76, 60, 255);
        private readonly Color32 neutralColor = new Color32(43, 43, 43, 255);

        private void Awake()
        {
            if (card == null) card = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            if (retryButton != null)
                retryButton.onClick.AddListener(() => RetryPressed?.Invoke());

            if (nextButton != null)
                nextButton.onClick.AddListener(() =>
                {
                    if (nextIsFinishMode) BackPressed?.Invoke();
                    else NextPressed?.Invoke();
                });

            if (backButton != null)
                backButton.onClick.AddListener(() => BackPressed?.Invoke());
        }

        public void Hide()
        {
            if (animCo != null) StopCoroutine(animCo);

            if (overlay != null) overlay.SetActive(false);
            gameObject.SetActive(false);

            if (card != null) card.localScale = Vector3.one;
            if (canvasGroup != null) canvasGroup.alpha = 1f;

            nextIsFinishMode = false;
        }

        public void Show(bool success, string details, bool nextEnabled, bool nextIsFinish)
        {
            nextIsFinishMode = nextIsFinish;

            if (overlay != null) overlay.SetActive(true);
            gameObject.SetActive(true);

            // Title
            if (resultTitleTMP != null)
            {
                resultTitleTMP.text = success ? "CORRECT!" : "INCORRECT!";
                resultTitleTMP.color = success ? correctColor : incorrectColor;
            }

            // Details (neutral)
            if (resultDetailsTMP != null)
            {
                resultDetailsTMP.text = details;
                resultDetailsTMP.color = neutralColor;
            }

            // ✅ UPDATED: Retry shows on failure OR when there is no Next (Timed mode)
            if (retryButton != null)
                retryButton.gameObject.SetActive(!success || !nextEnabled);

            // Next button only for campaign pass
            if (nextButton != null)
            {
                bool showNext = success && nextEnabled;
                nextButton.gameObject.SetActive(showNext);
                nextButton.interactable = showNext;
            }

            if (backButton != null)
                backButton.gameObject.SetActive(true);

            if (nextButtonLabelTMP != null)
                nextButtonLabelTMP.text = nextIsFinishMode ? "HOME" : "";

            if (animCo != null) StopCoroutine(animCo);
            animCo = StartCoroutine(PopIn());
        }

        private IEnumerator PopIn()
        {
            if (card == null) yield break;

            card.localScale = Vector3.one * startScale;

            if (useFade && canvasGroup != null)
                canvasGroup.alpha = 0f;

            float t = 0f;
            while (t < popDuration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / popDuration);
                float ease = 1f - Mathf.Pow(1f - u, 3f);

                float s = Mathf.Lerp(startScale, 1f, ease);
                card.localScale = new Vector3(s, s, s);

                if (useFade && canvasGroup != null)
                    canvasGroup.alpha = ease;

                yield return null;
            }

            card.localScale = Vector3.one;
            if (useFade && canvasGroup != null)
                canvasGroup.alpha = 1f;

            animCo = null;
        }
    }
}