using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LanguageExt;

namespace BuildItEasy.Validation
{
    public class ValidationResult
    {
        public static readonly ValidationResult Valid = new ValidationResult(new string[0]);

        public static ValidationResult Merge(IEnumerable<ValidationResult> validationResults)
        {
            return validationResults.Aggregate(Valid, (x, y) => new ValidationResult(x.Errors.Concat(y.Errors)));
        }
        
        public bool IsValid => Errors.Count == 0;
        public IReadOnlyCollection<string> Errors { get; }

        public ValidationResult(IEnumerable<string> errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            Errors = errors.ToList();
        }

        public void AssertValid(Option<string> context = default)
        {
            if (!IsValid)
            {
                throw new ValidationException(this, context);
            }
        }
    }
}
