using UnityEngine;
using System.Text;

namespace MathMechanics
{
    public class QuestionService
    {
        public Question GenerateForLevel(int levelIndex)
        {
            QuestionTier tier = GetTier(levelIndex);

            return tier switch
            {
                QuestionTier.Easy => GenerateEasy(levelIndex),
                QuestionTier.Medium => GenerateMedium(levelIndex),
                QuestionTier.Intermediate => GenerateIntermediate(levelIndex),
                QuestionTier.Boss => GenerateBoss(levelIndex),
                _ => GenerateEasy(levelIndex)
            };
        }

        public QuestionTier GetTier(int levelIndex)
        {
            if (levelIndex <= 5) return QuestionTier.Easy;
            if (levelIndex <= 10) return QuestionTier.Medium;
            if (levelIndex <= 15) return QuestionTier.Intermediate;
            return QuestionTier.Boss;
        }

        // -------------------------
        // EASY: ax + b = 0
        // Step mode:
        //  Step 1: type in (-b) so it becomes ax = -b
        //  Step 2: type in x
        // -------------------------
        private Question GenerateEasy(int levelIndex)
        {
            int a = RandomNonZero(-9, 9);
            int x = Random.Range(-10, 11);
            int b = -a * x;

            string prompt = CleanSpaces($"{a}x {FormatSigned(b)} = 0");

            int rhsAfterMove = -b; // ax = -b

            // Prompts are kept simple on purpose (no LHS/RHS wording)
            string[] stepPrompts =
            {
                $"Step 1/2: Fill the blank → {a}x = ?",
                "Step 2/2: Fill the blank → x = ?"
            };

            int[] stepExpected =
            {
                rhsAfterMove,
                x
            };

            string[] stepEquations =
            {
                CleanSpaces($"{a}x = {rhsAfterMove}"),
                $"x = {x}"
            };

            // Quick nudge without giving the answer away
            string stepHint = CleanSpaces($"Move {b} to the other side → {a}x = {-b}\nThen divide by {a}.");

            return new Question
            {
                levelIndex = levelIndex,
                tier = QuestionTier.Easy,
                prompt = prompt,
                stepHint = stepHint,
                correctAnswers = new[] { x },

                stepPrompts = stepPrompts,
                stepExpectedInts = stepExpected,
                stepResultEquations = stepEquations
            };
        }

        // -------------------------
        // MEDIUM: ax + b = cx + d
        // Step mode:
        //  Step 1: type in (d - b) so it becomes (a-c)x = (d-b)
        //  Step 2: type in x
        // -------------------------
        private Question GenerateMedium(int levelIndex)
        {
            int x = Random.Range(-12, 13);

            int a = RandomNonZero(-9, 9);
            int c = RandomNonZero(-9, 9);
            while (c == a) c = RandomNonZero(-9, 9);

            int b = Random.Range(-20, 21);
            int d = (a - c) * x + b;

            string left = $"{a}x {FormatSigned(b)}";
            string right = $"{c}x {FormatSigned(d)}";
            string prompt = CleanSpaces($"{left} = {right}");

            int coeff = a - c; // (a-c)x
            int rhs = d - b;   // d - b

            // Same idea as easy: keep the wording super direct
            string[] stepPrompts =
            {
                $"Step 1/2: Fill the blank → {coeff}x = ?",
                "Step 2/2: Fill the blank → x = ?"
            };

            int[] stepExpected =
            {
                rhs,
                x
            };

            string[] stepEquations =
            {
                CleanSpaces($"{coeff}x = {rhs}"),
                $"x = {x}"
            };

            string stepHint = CleanSpaces($"Bring x terms together → ({a}-{c})x\nMove constants → {coeff}x = {d}-{b}");

            return new Question
            {
                levelIndex = levelIndex,
                tier = QuestionTier.Medium,
                prompt = prompt,
                stepHint = stepHint,
                correctAnswers = new[] { x },

                stepPrompts = stepPrompts,
                stepExpectedInts = stepExpected,
                stepResultEquations = stepEquations
            };
        }

        // -------------------------
        // INTERMEDIATE: x^2 + bx + c = 0
        // Final-answer only for now (keeps this tier quick and predictable)
        // -------------------------
        private Question GenerateIntermediate(int levelIndex)
        {
            int r1 = Random.Range(-8, 9);
            int r2 = Random.Range(-8, 9);
            while (r2 == r1) r2 = Random.Range(-8, 9);

            int b = -(r1 + r2);
            int c = r1 * r2;

            string prompt = BuildQuadraticPrompt(b, c);
            string stepHint = CleanSpaces($"Try factoring\nLook for two numbers: sum {b}, product {c}.");

            return new Question
            {
                levelIndex = levelIndex,
                tier = QuestionTier.Intermediate,
                prompt = prompt,
                stepHint = stepHint,
                correctAnswers = new[] { r1, r2 },

                stepPrompts = null,
                stepExpectedInts = null,
                stepResultEquations = null
            };
        }

        // -------------------------
        // BOSS: (x + a)(x + b) = c
        // Final-answer only for now (no step mode here)
        // -------------------------
        private Question GenerateBoss(int levelIndex)
        {
            int a = Random.Range(-9, 10);
            int b = Random.Range(-9, 10);
            if (a == 0) a = 2;
            if (b == 0) b = -3;

            int x0 = Random.Range(-10, 11);
            int c = (x0 + a) * (x0 + b);

            int x1 = -(a + b) - x0;

            string prompt = CleanSpaces($"(x {FormatSigned(a)})(x {FormatSigned(b)}) = {c}");
            string stepHint = CleanSpaces($"Expand it, move {c} over\nThen solve the quadratic (factoring works a lot of the time).");

            return new Question
            {
                levelIndex = levelIndex,
                tier = QuestionTier.Boss,
                prompt = prompt,
                stepHint = stepHint,
                correctAnswers = new[] { x0, x1 },

                stepPrompts = null,
                stepExpectedInts = null,
                stepResultEquations = null
            };
        }

        // -------- Helpers --------

        private static int RandomNonZero(int minInclusive, int maxInclusive)
        {
            int v = Random.Range(minInclusive, maxInclusive + 1);
            while (v == 0) v = Random.Range(minInclusive, maxInclusive + 1);
            return v;
        }

        private static string FormatSigned(int v)
        {
            if (v < 0) return $"- {Mathf.Abs(v)}";
            return $"+ {v}";
        }

        private static string BuildQuadraticPrompt(int b, int c)
        {
            var sb = new StringBuilder();
            sb.Append("x²");

            if (b != 0)
            {
                if (b == 1) sb.Append(" + x");
                else if (b == -1) sb.Append(" - x");
                else sb.Append(" ").Append(FormatSigned(b)).Append("x");
            }

            if (c != 0)
            {
                sb.Append(" ").Append(FormatSigned(c));
            }

            sb.Append(" = 0");
            return CleanSpaces(sb.ToString());
        }

        private static string CleanSpaces(string s)
        {
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            s = s.Replace("+ -", "- ");
            s = s.Replace("(x + -", "(x - ");
            s = s.Replace("(x - -", "(x + ");
            return s.Trim();
        }
    }
}