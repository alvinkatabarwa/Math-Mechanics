namespace MathMechanics
{
    public class AnswerValidator
    {
        public ValidationResult Validate(Question q, string input)
        {
            if (!int.TryParse(input, out int value))
                return new ValidationResult(valid: false, correct: false);

            if (q.correctAnswers == null || q.correctAnswers.Length == 0)
                return new ValidationResult(valid: true, correct: false);

            for (int i = 0; i < q.correctAnswers.Length; i++)
            {
                if (value == q.correctAnswers[i])
                    return new ValidationResult(valid: true, correct: true);
            }

            return new ValidationResult(valid: true, correct: false);
        }
    }
}