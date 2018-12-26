namespace BuildItEasy
{
    public interface IValidator<in T>
    {
        ValidationResult Validate(T value);
    }
}