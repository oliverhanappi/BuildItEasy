namespace BuildItEasy.Validation
{
    public interface IValidator<in T>
    {
        ValidationResult Validate(T value);
    }
}