using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using static LanguageExt.Prelude;

namespace BuildItEasy
{
    public class Property<T>
    {
        public static implicit operator T(Property<T> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            return property.Value.ValueUnsafe();
        }

        private readonly List<Func<T, bool>> _validators = new List<Func<T, bool>>();
        private readonly ValueProvider<T> _defaultValueProvider;
        private ValueProvider<T> _overridingValueProvider;
        private readonly Lazy<Option<T>> _value;

        public string Name { get; }
        public ValueState State { get; private set; }
        public DefaultValuePreference DefaultValuePreference { get; }

        public Option<T> Value => _value.Value;

        public Property(string name, ValueProvider<T> defaultValueProvider, DefaultValuePreference defaultValuePreference)
        {
            _defaultValueProvider = defaultValueProvider ?? throw new ArgumentNullException(nameof(defaultValueProvider));

            State = ValueState.Default;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DefaultValuePreference = defaultValuePreference;
            
            _value = new Lazy<Option<T>>(CreateValue);
        }

        public Property<T> Validate(Func<T, bool> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            AssertValueNotCreated();
            _validators.Add(validator);

            return this;
        }

        public void NoValue()
        {
            AssertValueNotCreated();

            if (State == ValueState.ValueRequired)
                throw new InvalidOperationException($"A value has already been set for property {Name}.");

            State = ValueState.ValueForbidden;
        }

        public void SetValue(ValueProvider<T> valueProvider)
        {
            if (valueProvider == null)
                throw new ArgumentNullException(nameof(valueProvider));

            AssertValueNotCreated();

            if (_overridingValueProvider != null)
                throw new InvalidOperationException($"Another value has already been set for property {Name}.");

            if (State == ValueState.ValueForbidden)
                throw new InvalidOperationException($"The property {Name} has already been set to have no value.");

            _overridingValueProvider = valueProvider;
            State = ValueState.ValueRequired;
        }

        public void EnsureValue()
        {
            AssertValueNotCreated();

            if (State == ValueState.ValueForbidden)
                throw new InvalidOperationException($"The property {Name} has already been set to have no value.");

            State = ValueState.ValueRequired;
        }

        private void AssertValueNotCreated()
        {
            if (_value.IsValueCreated)
                throw new InvalidOperationException($"The value of property {Name} has already been created.");
        }
        
        private Option<T> CreateValue()
        {
            if (State == ValueState.ValueForbidden || (State == ValueState.Default && DefaultValuePreference == DefaultValuePreference.NoValue))
                return None;

            if (State == ValueState.ValueRequired || (State == ValueState.Default && DefaultValuePreference == DefaultValuePreference.Value))
            {
                var effectiveValueProvider = _overridingValueProvider ?? _defaultValueProvider;
                var value = effectiveValueProvider.GetValue();

                if (ReferenceEquals(value, null))
                    throw new Exception($"Expected value for property {Name} but got null.");

                if (_validators.Any(validator => !validator.Invoke(value)))
                    throw new Exception($"The value {value} of property {Name} is invalid.");

                return value;
            }

            throw new InvalidOperationException($"Unreachable state: {State} {DefaultValuePreference}");
        }
    }
}
