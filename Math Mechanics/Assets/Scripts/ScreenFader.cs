using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Start black, then fade into the scene
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        StartCoroutine(FadeTo(0f, blockRaycastsAtEnd: false));
    }

    public void FadeOutAndThen(System.Action onFadedOut)
    {
        StartCoroutine(FadeOutRoutine(onFadedOut));
    }

    private IEnumerator FadeOutRoutine(System.Action onFadedOut)
    {
        canvasGroup.blocksRaycasts = true;
        yield return FadeTo(1f, blockRaycastsAtEnd: true);
        onFadedOut?.Invoke();
    }

    private IEnumerator FadeTo(float target, bool blockRaycastsAtEnd)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
        canvasGroup.blocksRaycasts = blockRaycastsAtEnd;
    }
}
