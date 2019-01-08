namespace BuildItEasy
{
    /// <summary>
    /// Marker interface for all builders.
    /// </summary>
    public interface IBuilder
    {
    }

    public interface IBuilder<out TResult> : IBuilder
    {
        TResult Build();
    }
}
