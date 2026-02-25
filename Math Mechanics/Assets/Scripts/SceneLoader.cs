using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private ScreenFader fader;
    [SerializeField] private string homeSceneName = "Home";
    [SerializeField] private string arenaSceneName = "Arena";

    private void Awake()
    {
        if (fader == null)
            fader = FindFirstObjectByType<ScreenFader>();
    }

    public void LoadArena()
    {
        Debug.Log("✅ Campaign button pressed → LoadArena()");
        LoadWithFade(arenaSceneName);
    }

    public void LoadHome()
    {
        Debug.Log("✅ Back button pressed → LoadHome()");
        LoadWithFade(homeSceneName);
    }

    // Quick test button option (bypasses fade completely)
    public void LoadArenaImmediate()
    {
        Debug.Log("⚡ LoadArenaImmediate() (no fade)");
        SceneManager.LoadScene(arenaSceneName);
    }

    public void LoadHomeImmediate()
    {
        Debug.Log("⚡ LoadHomeImmediate() (no fade)");
        SceneManager.LoadScene(homeSceneName);
    }

    private void LoadWithFade(string sceneName)
    {
        Debug.Log($"➡️ LoadWithFade('{sceneName}'), fader = {(fader == null ? "NULL" : "FOUND")}");

        if (fader == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        fader.FadeOutAndThen(() => SceneManager.LoadScene(sceneName));

    }
}
