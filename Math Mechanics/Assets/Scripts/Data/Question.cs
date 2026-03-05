using System;

namespace MathMechanics
{
    [Serializable]
    public struct Question
    {
        public int levelIndex;
        public QuestionTier tier;
        public string prompt;

        public string stepHint;

        public int[] correctAnswers;

        public string[] stepPrompts;
        public int[] stepExpectedInts;
        public string[] stepResultEquations;

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