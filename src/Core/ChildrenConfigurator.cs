using System;
using System.Collections.Generic;
using LanguageExt;

namespace BuildItEasy
{
    public class ChildrenConfigurator<TBuilderResult, TValue, TValueBuilder, TValues> : ValuesConfiguratorBase<TBuilderResult, TValue, TValueBuilder, Customizer<TValueBuilder>, TValues, TBuilderResult, ChildrenConfigurator<TBuilderResult, TValue, TValueBuilder, TValues>>
        where TValues : Children<TValue, TValueBuilder, TBuilderResult>
        where TValueBuilder : IBuilder<TValue>
    {
        public static implicit operator TValues(ChildrenConfigurator<TBuilderResult, TValue, TValueBuilder, TValues> configurator) => configurator.Values;

        private readonly Func<string, int, TValues> _valuesFactory;

        public ChildrenConfigurator(string name, Func<string, int, TValues> valuesFactory,
            Option<Func<TBuilderResult, IEnumerable<TValue>>> valuesGetter)
            : base(name, valuesGetter)
        {
            _valuesFactory = valuesFactory ?? throw new ArgumentNullException(nameof(valuesFactory));
        }

        protected override TValues CreateValues(string name, int defaultCount)
        {
            return _valuesFactory.Invoke(name, defaultCount);
        }

        protected override TBuilderResult GetContext(TBuilderResult builderResult)
        {
            return builderResult;
        }
    }
}
