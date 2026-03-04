namespace MathMechanics
{
    public struct ValidationResult
    {
        public bool isValidNumber;
        public bool isCorrect;

        public ValidationResult(bool valid, bool correct)
        {
            isValidNumber = valid;
            isCorrect = correct;
        }
    }
}