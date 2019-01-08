namespace BuildItEasy
{
    public delegate void ValuesCustomizer<TItem>(IValues<ValueProvider<TItem>> customizer);
    
    public delegate void ValuesCustomizer<TItem, in TItemBuilder>(IValues<Customizer<TItemBuilder>> customizer)
        where TItemBuilder : IBuilder<TItem>;
}
