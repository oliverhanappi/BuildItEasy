namespace BuildItEasy
{
    public delegate void CollectionCustomizer<TItemBuilder>(ICollectionCustomizer<TItemBuilder> customizer)
        where TItemBuilder : IBuilder;
}
