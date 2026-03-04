using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MathMechanics
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Home Buttons")]
        [SerializeField] private Button campaignButton;
        [SerializeField] private Button timedButton;

        [SerializeField] private int startLevel = 1;

        private void Start()
        {
            EnsureAppManagerExists();

            if (campaignButton != null)
            {
                campaignButton.onClick.RemoveAllListeners();
                campaignButton.onClick.AddListener(() =>
                {
                    AppManager.Instance.SelectedMode = GameMode.Campaign;
                    AppManager.Instance.SelectedLevelIndex = Mathf.Max(1, startLevel);
                    SceneManager.LoadScene("Campaign Mode");
                });
            }

            if (timedButton != null)
            {
                timedButton.onClick.RemoveAllListeners();
                timedButton.onClick.AddListener(() =>
                {
                    AppManager.Instance.SelectedMode = GameMode.Timed;
                    AppManager.Instance.SelectedLevelIndex = 1; // safe default
                    SceneManager.LoadScene("Campaign Mode");
                });
            }
        }

        private void EnsureAppManagerExists()
        {
            if (AppManager.Instance != null) return;

            var go = new GameObject("AppManager");
            go.AddComponent<AppManager>();
        }
    }
}