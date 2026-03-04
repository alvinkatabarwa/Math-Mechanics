using System;

namespace MathMechanics
{
    [Serializable]
    public struct Question
    {
        public int levelIndex;
        public QuestionTier tier;
        public string prompt;

        // Short step hint (never reveals the answer)
        public string stepHint;

        // Final answer(s) used for normal validation + results display
        public int[] correctAnswers;

        // ✅ NEW: step-solving support (levels 1–10)
        // Each step expects an integer input from the player.
        // After each correct step, we update the displayed equation.
        public string[] stepPrompts;         // "Step 1/2: ..."
        public int[] stepExpectedInts;       // expected integer input per step
        public string[] stepResultEquations; // equation to display AFTER step i is correct

        public bool HasMultipleAnswers => correctAnswers != null && correctAnswers.Length > 1;

        public bool HasSteps =>
            stepExpectedInts != null &&
            stepPrompts != null &&
            stepResultEquations != null &&
            stepExpectedInts.Length > 0 &&
            stepPrompts.Length == stepExpectedInts.Length &&
            stepResultEquations.Length == stepExpectedInts.Length;
    }
}