using System;

namespace BuildItEasy.Validation
{
    public class Validator<T> : IValidator<T>
    {
        private readonly Func<T, bool> _validate;
        
        public string Name { get; }

        public Validator(string name, Func<T, bool> validate)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _validate = validate ?? throw new ArgumentNullException(nameof(validate));
        }

        public ValidationResult Validate(T value)
        {
            var isValid = _validate.Invoke(value);
            return isValid
                ? ValidationResult.Valid
                : new ValidationResult(new[] {$"{Name} is invalid"});
        }
    }
}