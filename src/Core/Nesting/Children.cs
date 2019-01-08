using System;

namespace BuildItEasy.Nesting
{
    public class Children<T, TBuilder, TParent> : ValuesBase<T, TBuilder, Customizer<TBuilder>, TParent>
        where TBuilder : IBuilder<T>
    {
        private readonly Func<TParent, int, TBuilder> _builderFactory;

        public Children(string name, int defaultCount, Func<TParent, int, TBuilder> builderFactory)
            : base(name, defaultCount)
        {
            _builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
        }

        protected override void ApplyCustomizer(TBuilder builder, Customizer<TBuilder> customizer)
        {
            customizer.Invoke(builder);
        }

        protected override TBuilder CreateBuilder(TParent parent, int index)
        {
            return _builderFactory.Invoke(parent, index);
        }

        protected override T BuildValue(TParent parent, TBuilder builder)
        {
            return builder.Build();
        }
    }
}
