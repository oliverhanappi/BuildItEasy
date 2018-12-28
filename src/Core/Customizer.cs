namespace BuildItEasy
{
    public delegate void Customizer<in TBuilder>(TBuilder builder)
        where TBuilder : IBuilder;
}
