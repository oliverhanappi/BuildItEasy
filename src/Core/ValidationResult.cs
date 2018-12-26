using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildItEasy
{
    public class ValidationResult
    {
        public static readonly ValidationResult Valid = new ValidationResult(new string[0]);
        
        public bool IsValid => Errors.Count == 0;
        public IReadOnlyCollection<string> Errors { get; }

        public ValidationResult(IEnumerable<string> errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            Errors = errors.ToList();
        }

        public void AssertValid()
        {
            if (!IsValid)
            {
                var message = new StringBuilder();
                message.AppendLine("Validation failed:");

                foreach (var error in Errors)
                    message.AppendLine($"  - {error}");

                throw new Exception(message.ToString().TrimEnd());
            }
        }
    }
}