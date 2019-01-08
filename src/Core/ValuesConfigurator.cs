using System;
using System.Collections.Generic;
using LanguageExt;

namespace BuildItEasy
{
    public class ValuesConfigurator<TBuilderResult, TValue> : ValuesConfiguratorBase<TBuilderResult, TValue, OverridableValueProvider<TValue>, ValueProvider<TValue>, Values<TValue>, Unit, ValuesConfigurator<TBuilderResult, TValue>>
    {
        public static implicit operator Values<TValue>(ValuesConfigurator<TBuilderResult, TValue> configurator) => configurator.Values;

        private readonly Func<int, ValueProvider<TValue>> _defaultValueProviderFactory;

        public ValuesConfigurator(string name, Func<int, ValueProvider<TValue>> defaultValueProviderFactory, Option<Func<TBuilderResult, IEnumerable<TValue>>> valuesGetter)
            : base(name, valuesGetter)
        {
            _defaultValueProviderFactory = defaultValueProviderFactory ?? throw new ArgumentNullException(nameof(defaultValueProviderFactory));
        }

        protected override Values<TValue> CreateValues(string name, int defaultCount)
        {
            return new Values<TValue>(name, _defaultValueProviderFactory, defaultCount);
        }

        protected override Unit GetContext(TBuilderResult builderResult)
        {
            return Unit.Default;
        }
    }
}
