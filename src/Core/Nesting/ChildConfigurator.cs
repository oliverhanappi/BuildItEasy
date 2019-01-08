using System;
using System.Collections.Generic;
using LanguageExt;

namespace BuildItEasy.Nesting
{
    public class ChildConfigurator<TBuilderResult, TValue, TValueBuilder, TConfigured> :
        ValueConfiguratorBase<TBuilderResult, TValue, TConfigured, TBuilderResult, ChildConfigurator<TBuilderResult, TValue, TValueBuilder, TConfigured>>
        where TValueBuilder : IBuilder<TValue>
        where TConfigured : Child<TValue, TValueBuilder, TBuilderResult>
    {
        public static implicit operator TConfigured(ChildConfigurator<TBuilderResult, TValue, TValueBuilder, TConfigured> configurator) => configurator.Value;
        
        private readonly Func<string, DefaultValuePreference, TConfigured> _valueFactory;
        private readonly List<Customizer<TValueBuilder>> _customizers;

        public ChildConfigurator(string name, Option<Func<TBuilderResult, TValue>> valueGetter, Func<string, DefaultValuePreference, TConfigured> valueFactory)
            : base(name, valueGetter)
        {
            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            _customizers = new List<Customizer<TValueBuilder>>();
        }

        public ChildConfigurator<TBuilderResult, TValue, TValueBuilder, TConfigured> Customize(Customizer<TValueBuilder> customizer)
        {
            if (customizer == null)
                throw new ArgumentNullException(nameof(customizer));

            _customizers.Add(customizer);
            return this;
        }

        protected override TConfigured CreateValue(string name, DefaultValuePreference defaultValuePreference)
        {
            var property = _valueFactory.Invoke(name, defaultValuePreference);
            
            foreach (var customizer in _customizers) 
                property.Customize(customizer);

            return property;
        }

        protected override TBuilderResult GetContext(TBuilderResult result)
        {
            return result;
        }
    }
}
