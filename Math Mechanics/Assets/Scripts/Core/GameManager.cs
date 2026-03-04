using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

namespace MathMechanics
{
    public class GameManager : MonoBehaviour
    {
        private const int MAX_LEVEL = 20;
        private const float TIMED_DURATION = 60f;

        [Header("Scene References")]
        [SerializeField] private GameUIController gameUI;
        [SerializeField] private ResultsPanelUI resultsUI;

        [Header("Optional Fade (recommended)")]
        [SerializeField] private FadeController fader;

        private readonly QuestionService questionService = new QuestionService();
        private readonly AnswerValidator validator = new AnswerValidator();
        private readonly HintSystem hintSystem = new HintSystem();
        private readonly StreakSystem streakSystem = new StreakSystem();

        private GameState state = GameState.Idle;
        private Question currentQuestion;
        private int currentLevel;

        private bool inputLocked;
        private bool hintLocked;

        // Campaign step tracking
        private int stepIndex;

        // Timed mode tracking (simple score only)
        private float timeLeft;
        private int timedScore;

        private bool IsTimed => AppManager.Instance != null && AppManager.Instance.SelectedMode == GameMode.Timed;

        private void Start()
        {
            if (gameUI == null) gameUI = FindFirstObjectByType<GameUIController>();
            if (resultsUI == null) resultsUI = FindFirstObjectByType<ResultsPanelUI>();
            if (fader == null) fader = FindFirstObjectByType<FadeController>();

            if (gameUI == null || resultsUI == null)
            {
                Debug.LogError("GameManager missing UI references.");
                return;
            }

            gameUI.SubmitPressed += OnSubmitPressed;
            gameUI.HintPressed += OnHintPressed;
            gameUI.BackPressed += OnBackPressed;

            resultsUI.RetryPressed += OnRetryPressed;
            resultsUI.NextPressed += OnNextPressed;
            resultsUI.BackPressed += OnBackPressed;

            resultsUI.Hide();

            if (AppManager.Instance == null)
            {
                Debug.LogError("AppManager missing.");
                return;
            }

            // ✅ Start correct background loop for this mode
            Debug.Log("AudioManager.Instance = " + (AudioManager.Instance != null ? "OK" : "NULL"));
            Debug.Log("Audio: PlayLoopForMode -> " + AppManager.Instance.SelectedMode);
            AudioManager.Instance?.PlayLoopForMode(AppManager.Instance.SelectedMode);

            if (IsTimed)
            {
                timeLeft = TIMED_DURATION;
                timedScore = 0;

                gameUI.SetTimerVisible(true);
                gameUI.SetScoreVisible(true);
                gameUI.SetTimer(timeLeft);
                gameUI.SetScore(timedScore);

                // Hide step prompt in timed
                gameUI.SetStepPrompt("");

                // Streak not used in timed
                streakSystem.Set(0);
                gameUI.SetStreak(0);

                if (fader != null) fader.FadeOutIn(StartTimedRound);
                else StartTimedRound();
            }
            else
            {
                gameUI.SetTimerVisible(false);
                gameUI.SetScoreVisible(false);

                currentLevel = Mathf.Clamp(AppManager.Instance.SelectedLevelIndex, 1, MAX_LEVEL);
                streakSystem.Set(AppManager.Instance.CurrentStreak);

                if (fader != null) fader.FadeOutIn(() => StartLevel(currentLevel));
                else StartLevel(currentLevel);
            }
        }
        private void Update()
        {
            if (!IsTimed) return;
            if (state != GameState.Playing) return;

            timeLeft -= Time.deltaTime;
            if (timeLeft < 0f) timeLeft = 0f;

            gameUI.SetTimer(timeLeft);

            if (timeLeft <= 0f)
                EndTimedMode();
        }

        // ---------------- TIMED MODE ----------------

        private void StartTimedRound()
        {
            state = GameState.Playing;
            inputLocked = false;
            hintLocked = false;

            resultsUI.Hide();

            gameUI.SetInputInteractable(true);
            gameUI.SetHintButtonInteractable(true);
            gameUI.ClearHint();
            gameUI.ClearInput();

            // Difficulty ramp based on score (simple & stable)
            int pseudoLevel;
            if (timedScore < 3) pseudoLevel = 3;          // easy bucket
            else if (timedScore < 6) pseudoLevel = 8;     // medium bucket
            else if (timedScore < 9) pseudoLevel = 13;    // intermediate bucket
            else pseudoLevel = 18;                        // boss bucket

            currentQuestion = questionService.GenerateForLevel(pseudoLevel);
            hintSystem.ResetForNewQuestion(currentQuestion);

            // ✅ Clean banner for timed users (no fake LVL number)
            gameUI.SetHeader($"TIMED: {currentQuestion.tier.ToString().ToUpper()}");

            gameUI.SetQuestion(currentQuestion.prompt);
            gameUI.SetStepPrompt(""); // no step mode in timed
        }

        private void EndTimedMode()
        {
            state = GameState.ShowingResults;
            inputLocked = true;

            // ✅ Buzz sound when time ends
            AudioManager.Instance?.PlayBuzz();

            gameUI.SetInputInteractable(false);
            gameUI.SetHintButtonInteractable(false);

            var sb = new StringBuilder();
            sb.AppendLine($"Score: {timedScore}");
            sb.AppendLine("Time: 60s");
            sb.AppendLine();
            sb.AppendLine("Tap Retry to play again.");

            // nextEnabled=false => Retry shows in timed results
            resultsUI.Show(success: true, details: sb.ToString().Trim(), nextEnabled: false, nextIsFinish: false);
        }

        // ---------------- CAMPAIGN MODE ----------------

        private void StartLevel(int levelIndex)
        {
            state = GameState.Playing;
            inputLocked = false;
            hintLocked = false;

            currentLevel = Mathf.Clamp(levelIndex, 1, MAX_LEVEL);

            resultsUI.Hide();

            gameUI.SetInputInteractable(true);
            gameUI.SetHintButtonInteractable(true);
            gameUI.ClearHint();
            gameUI.ClearInput();

            currentQuestion = questionService.GenerateForLevel(currentLevel);
            hintSystem.ResetForNewQuestion(currentQuestion);

            stepIndex = 0;

            gameUI.SetLevel(currentLevel, currentQuestion.tier);
            gameUI.SetQuestion(currentQuestion.prompt);
            gameUI.SetStreak(streakSystem.CurrentStreak);

            if (currentQuestion.HasSteps)
                gameUI.SetStepPrompt(currentQuestion.stepPrompts[0]);
            else
                gameUI.SetStepPrompt("");
        }

        private void OnSubmitPressed(string input)
        {
            if (state != GameState.Playing) return;
            if (inputLocked) return;

            inputLocked = true;

            if (IsTimed)
            {
                ValidationResult result = validator.Validate(currentQuestion, input);
                if (result.isValidNumber && result.isCorrect)
                {
                    timedScore++;
                    gameUI.SetScore(timedScore);
                }

                inputLocked = false;
                StartTimedRound();
                return;
            }

            // Campaign step solving (levels 1–10)
            if (currentQuestion.HasSteps)
            {
                if (!int.TryParse(input, out int value))
                {
                    EndCampaignLevel(correct: false, validInput: false);
                    return;
                }

                int expected = currentQuestion.stepExpectedInts[stepIndex];
                if (value != expected)
                {
                    EndCampaignLevel(correct: false, validInput: true);
                    return;
                }

                // Step correct
                gameUI.SetQuestion(currentQuestion.stepResultEquations[stepIndex]);
                stepIndex++;

                if (stepIndex < currentQuestion.stepExpectedInts.Length)
                {
                    inputLocked = false;
                    gameUI.ClearInput();
                    gameUI.SetStepPrompt(currentQuestion.stepPrompts[stepIndex]);
                    return;
                }

                // Finished all steps => pass
                EndCampaignLevel(correct: true, validInput: true);
                return;
            }

            // Campaign final-answer mode (11–20)
            ValidationResult finalResult = validator.Validate(currentQuestion, input);
            EndCampaignLevel(correct: finalResult.isCorrect, validInput: finalResult.isValidNumber);
        }

        private void EndCampaignLevel(bool correct, bool validInput)
        {
            state = GameState.ShowingResults;
            inputLocked = true;

            gameUI.SetInputInteractable(false);

            bool passed = validInput && correct;

            // ✅ Campaign result sounds
            if (passed) AudioManager.Instance?.PlayCorrect();
            else AudioManager.Instance?.PlayIncorrect();

            // Streak rules
            if (!validInput)
                streakSystem.OnWrong();
            else if (passed)
                streakSystem.OnCorrect(hintSystem.HintUsedThisLevel);
            else
                streakSystem.OnWrong();

            AppManager.Instance.CurrentStreak = streakSystem.CurrentStreak;
            gameUI.SetStreak(streakSystem.CurrentStreak);

            // Unlock / Next + Finish mode
            bool nextEnabled = false;
            bool nextIsFinish = false;

            if (passed)
            {
                if (currentLevel < MAX_LEVEL)
                {
                    int next = currentLevel + 1;
                    AppManager.Instance.UnlockLevel(next);
                    nextEnabled = next <= AppManager.Instance.UnlockedLevelIndex;
                }
                else
                {
                    nextEnabled = true;
                    nextIsFinish = true; // Level 20 -> Next becomes Home
                }
            }

            // Details text
            var sb = new StringBuilder();
            sb.AppendLine($"Streak: {streakSystem.CurrentStreak}");
            sb.AppendLine($"Hint used: {(hintSystem.HintUsedThisLevel ? "YES" : "NO")}");

            if (!validInput)
            {
                sb.AppendLine();
                sb.AppendLine("Enter a valid integer.");
            }
            else if (!passed)
            {
                sb.AppendLine();
                sb.AppendLine($"Correct answer: {FormatCorrectAnswers(currentQuestion.correctAnswers)}");
            }
            else
            {
                if (hintSystem.HintUsedThisLevel)
                {
                    sb.AppendLine();
                    sb.AppendLine("Hint used (streak did not increase).");
                }

                if (currentLevel == MAX_LEVEL)
                {
                    sb.AppendLine();
                    sb.AppendLine("Campaign complete!");
                }
            }

            resultsUI.Show(passed, sb.ToString().Trim(), nextEnabled, nextIsFinish);
        }

        private static string FormatCorrectAnswers(int[] answers)
        {
            if (answers == null || answers.Length == 0) return "";
            if (answers.Length == 1) return answers[0].ToString();
            return $"{answers[0]} or {answers[1]}";
        }

        private void OnHintPressed()
        {
            if (state != GameState.Playing) return;
            if (hintLocked) return;

            hintLocked = true;

            string hint = hintSystem.GetHint();
            gameUI.SetHint(hint);
            gameUI.SetHintButtonInteractable(false);
        }

        private void OnRetryPressed()
        {
            if (state != GameState.ShowingResults) return;

            if (IsTimed)
            {
                timeLeft = TIMED_DURATION;
                timedScore = 0;

                gameUI.SetTimer(timeLeft);
                gameUI.SetScore(timedScore);

                if (fader != null) fader.FadeOutIn(StartTimedRound);
                else StartTimedRound();

                return;
            }

            AppManager.Instance.SelectedLevelIndex = currentLevel;

            if (fader != null)
                fader.FadeOutIn(() => StartLevel(currentLevel));
            else
                StartLevel(currentLevel);
        }

        private void OnNextPressed()
        {
            if (state != GameState.ShowingResults) return;
            if (IsTimed) return;

            int next = currentLevel + 1;
            if (next > MAX_LEVEL) return;
            if (next > AppManager.Instance.UnlockedLevelIndex) return;

            AppManager.Instance.SelectedLevelIndex = next;

            if (fader != null)
                fader.FadeOutIn(() => StartLevel(next));
            else
                StartLevel(next);
        }

        private void OnBackPressed()
        {
            SceneManager.LoadScene("Home");
        }
    }
}