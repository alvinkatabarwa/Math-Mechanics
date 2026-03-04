using UnityEngine;

namespace MathMechanics
{
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; private set; }

        public GameMode SelectedMode = GameMode.Campaign;
        public int SelectedLevelIndex = 1;

        public int UnlockedLevelIndex = 1;
        public int CurrentStreak = 0;

        private const string PREF_UNLOCKED = "MM_UnlockedLevel";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            UnlockedLevelIndex = PlayerPrefs.GetInt(PREF_UNLOCKED, 1);
            UnlockedLevelIndex = Mathf.Max(1, UnlockedLevelIndex);
        }

        public void UnlockLevel(int levelIndex)
        {
            if (levelIndex > UnlockedLevelIndex)
            {
                UnlockedLevelIndex = levelIndex;
                PlayerPrefs.SetInt(PREF_UNLOCKED, UnlockedLevelIndex);
                PlayerPrefs.Save();
            }
        }
    }
}