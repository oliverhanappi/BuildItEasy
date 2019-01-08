using System;
using System.Text;
using LanguageExt;

namespace BuildItEasy.Validation
{
    public class ValidationException : Exception
    {
        public ValidationResult ValidationResult { get; }

        public ValidationException(ValidationResult validationResult, Option<string> context)
            : base(CreateMessage(validationResult, context))
        {
            ValidationResult = validationResult ?? throw new ArgumentNullException(nameof(validationResult));
        }

        private static string CreateMessage(ValidationResult validationResult, Option<string> context)
        {
            if (validationResult == null)
                throw new ArgumentNullException(nameof(validationResult));
            
            var message = new StringBuilder();

            message.AppendLine($"Validation failed{context.Match(c => $" for {c}:", () => ":")}");

            foreach (var error in validationResult.Errors)
                message.AppendLine($"  - {error}");

            throw new Exception(message.ToString().TrimEnd());
        }
    }
}
