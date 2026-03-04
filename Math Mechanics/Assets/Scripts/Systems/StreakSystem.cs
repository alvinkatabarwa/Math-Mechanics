namespace MathMechanics
{
    public class StreakSystem
    {
        public int CurrentStreak { get; private set; }

        public void Set(int value) => CurrentStreak = value;

        // Rule: streak increases only if correct AND hint not used
        public void OnCorrect(bool hintUsed)
        {
            if (hintUsed) return;
            CurrentStreak++;
        }

        public void OnWrong()
        {
            CurrentStreak = 0;
        }
    }
}