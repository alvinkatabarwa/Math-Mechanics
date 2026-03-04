using System;
using System.Collections;
using UnityEngine;

namespace MathMechanics
{
    public class FadeController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeOutDuration = 0.25f;
        [SerializeField] private float fadeInDuration = 0.25f;

        private Coroutine running;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public void FadeOutIn(Action midAction)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(FadeOutInRoutine(midAction));
        }

        private IEnumerator FadeOutInRoutine(Action midAction)
        {
            // Fade OUT
            canvasGroup.blocksRaycasts = true; // block clicks during transition
            yield return FadeTo(1f, fadeOutDuration);

            // Do the level switch in the dark
            midAction?.Invoke();

            // Fade IN
            yield return FadeTo(0f, fadeInDuration);
            canvasGroup.blocksRaycasts = false;

            running = null;
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            float start = canvasGroup.alpha;
            float t = 0f;

            if (duration <= 0f)
            {
                canvasGroup.alpha = target;
                yield break;
            }

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                canvasGroup.alpha = Mathf.Lerp(start, target, k);
                yield return null;
            }

            canvasGroup.alpha = target;
        }
    }
}