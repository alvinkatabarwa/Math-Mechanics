using UnityEngine;

namespace MathMechanics
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Loop Tracks")]
        [SerializeField] private AudioClip campaignLoop;
        [SerializeField] private AudioClip timedLoop;

        [Header("SFX")]
        [SerializeField] private AudioClip correctSfx;
        [SerializeField] private AudioClip incorrectSfx;
        [SerializeField] private AudioClip buzzSfx;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource != null)
            {
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource != null)
            {
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        public void PlayLoopForMode(GameMode mode)
        {
            if (musicSource == null) return;

            AudioClip target = (mode == GameMode.Timed) ? timedLoop : campaignLoop;

            // If missing clip, stop music safely
            if (target == null)
            {
                musicSource.Stop();
                musicSource.clip = null;
                return;
            }

            // Don’t restart if already playing this clip
            if (musicSource.clip == target && musicSource.isPlaying)
                return;

            musicSource.clip = target;
            musicSource.Play();
        }

        public void StopLoop()
        {
            if (musicSource == null) return;
            musicSource.Stop();
            musicSource.clip = null;
        }

        public void PlayCorrect()
        {
            if (sfxSource == null || correctSfx == null) return;
            sfxSource.PlayOneShot(correctSfx);
        }

        public void PlayIncorrect()
        {
            if (sfxSource == null || incorrectSfx == null) return;
            sfxSource.PlayOneShot(incorrectSfx);
        }

        public void PlayBuzz()
        {
            if (sfxSource == null || buzzSfx == null) return;
            sfxSource.PlayOneShot(buzzSfx);
        }
    }
}