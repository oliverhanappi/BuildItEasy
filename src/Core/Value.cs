using System;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;

namespace BuildItEasy
{
    public class Value<T> : ValueBase<T, Unit>
    {
        public static implicit operator T(Value<T> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return value.GetValue().ValueUnsafe();
        }

        private readonly ValueProvider<T> _defaultValueProvider;

        public Value(string name, DefaultValuePreference defaultValuePreference, ValueProvider<T> defaultValueProvider)
            : base(name, defaultValuePreference)
        {
            _defaultValueProvider = defaultValueProvider ?? throw new ArgumentNullException(nameof(defaultValueProvider));
        }
        
        public void SetValue(ValueProvider<T> overridingValueProvider)
        {
            if (overridingValueProvider == null)
                throw new ArgumentNullException(nameof(overridingValueProvider));
            
            Customize(valueProvider =>
            {
                var delegatedValueProvider = (OverridableValueProvider<T>) valueProvider;
                delegatedValueProvider.Override(overridingValueProvider);
            });
        }

        protected override ValueProvider<T> CreateValueProvider(Unit context)
        {
            return new OverridableValueProvider<T>(Name, _defaultValueProvider);
        }

        public Option<T> GetValue() => GetValue(Unit.Default);
    }
}
