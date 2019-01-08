using System;

namespace BuildItEasy
{
    public class Child<T, TBuilder, TParent> : ValueBase<T, TParent>
        where TBuilder : IBuilder<T>
    {
        private readonly Func<TParent, TBuilder> _builderFactory;

        public Child(string name, DefaultValuePreference defaultValuePreference, Func<TParent, TBuilder> builderFactory)
            : base(name, defaultValuePreference)
        {
            _builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
        }

        public void Customize(Customizer<TBuilder> customizer)
        {
            if (customizer == null)
                throw new ArgumentNullException(nameof(customizer));
            
            base.Customize(valueProvider =>
            {
                var builderValueProvider = (BuilderValueProvider<T>) valueProvider;
                customizer.Invoke((TBuilder) builderValueProvider.Builder);
            });
        }

        protected override ValueProvider<T> CreateValueProvider(TParent parent)
        {
            var builder = _builderFactory.Invoke(parent);
            return new BuilderValueProvider<T>(builder);
        }
    }
}
