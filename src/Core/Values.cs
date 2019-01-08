using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace BuildItEasy
{
    public class Values<T> : ValuesBase<T, OverridableValueProvider<T>, ValueProvider<T>, Unit>
    {
        public static implicit operator T[](Values<T> values) => values.GetValues().ToArray();
        public static implicit operator List<T>(Values<T> values) => values.GetValues().ToList();

        private readonly Func<int, ValueProvider<T>> _defaultValueProviderFactory;

        public Values(string name, Func<int, ValueProvider<T>> defaultValueProviderFactory, int defaultCount)
            : base(name, defaultCount)
        {
            _defaultValueProviderFactory = defaultValueProviderFactory ?? throw new ArgumentNullException(nameof(defaultValueProviderFactory));
        }

        protected override void ApplyCustomizer(OverridableValueProvider<T> builder, ValueProvider<T> valueProvider)
        {
            builder.Override(valueProvider);
        }

        protected override OverridableValueProvider<T> CreateBuilder(Unit context, int index)
        {
            var name = $"{Name}[{index}]";
            var defaultValueProvider = _defaultValueProviderFactory.Invoke(index);
            
            return new OverridableValueProvider<T>(name, defaultValueProvider);
        }

        protected override T BuildValue(Unit context, OverridableValueProvider<T> builder)
        {
            return builder.GetValue();
        }

        public IReadOnlyList<T> GetValues() => GetValues(Unit.Default);
    }
}
