using System;
using LanguageExt;

namespace BuildItEasy
{
    public class ValueConfigurator<TBuilderResult, TValue> : ValueConfiguratorBase<TBuilderResult, TValue, Value<TValue>, Unit, ValueConfigurator<TBuilderResult, TValue>>
    {
        public static implicit operator Value<TValue>(ValueConfigurator<TBuilderResult, TValue> configurator) => configurator.Value;

        private readonly ValueProvider<TValue> _defaultValueProvider;

        public ValueConfigurator(string name, Option<Func<TBuilderResult, TValue>> valueGetter, ValueProvider<TValue> defaultValueProvider)
            : base(name, valueGetter)
        {
            _defaultValueProvider = defaultValueProvider ?? throw new ArgumentNullException(nameof(defaultValueProvider));
        }

        protected override Value<TValue> CreateValue(string name, DefaultValuePreference defaultValuePreference)
        {
            return new Value<TValue>(name, defaultValuePreference, _defaultValueProvider);
        }

        protected override Unit GetContext(TBuilderResult result)
        {
            return Unit.Default;
        }
    }
}
