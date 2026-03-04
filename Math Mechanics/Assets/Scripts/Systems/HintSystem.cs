namespace MathMechanics
{
    public class HintSystem
    {
        public bool HintUsedThisLevel { get; private set; }

        private Question current;

        public void ResetForNewQuestion(Question q)
        {
            current = q;
            HintUsedThisLevel = false;
        }

        public string GetHint()
        {
            HintUsedThisLevel = true;

            if (!string.IsNullOrEmpty(current.stepHint))
                return current.stepHint;

            return "Step: Rearrange the equation to isolate x, then solve.";
        }
    }
}