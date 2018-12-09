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
            return property.GetValue().ValueUnsafe();
        }

        private readonly List<Func<T, bool>> _validators = new List<Func<T, bool>>();
        private readonly ValueProvider<T> _defaultValueProvider;
        private ValueProvider<T> _overridingValueProvider;

        public ValueState State { get; private set; }
        public DefaultValuePreference DefaultValuePreference { get; }

        public Property(ValueProvider<T> defaultValueProvider, DefaultValuePreference defaultValuePreference)
        {
            _defaultValueProvider = defaultValueProvider ?? throw new ArgumentNullException(nameof(defaultValueProvider));

            State = ValueState.Default;
            DefaultValuePreference = defaultValuePreference;
        }

        public Property<T> Validate(Func<T, bool> validator)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            _validators.Add(validator);

            return this;
        }

        public void NoValue()
        {
            if (State == ValueState.ValueRequired)
                throw new InvalidOperationException("A property value has already been set.");

            State = ValueState.ValueForbidden;
        }

        public void SetValue(ValueProvider<T> valueProvider)
        {
            if (valueProvider == null)
                throw new ArgumentNullException(nameof(valueProvider));

            if (_overridingValueProvider != null)
                throw new InvalidOperationException("Another value has already been set before.");

            if (State == ValueState.ValueForbidden)
                throw new InvalidOperationException("The property has already been set to have no value.");

            _overridingValueProvider = valueProvider;
            State = ValueState.ValueRequired;
        }

        public void EnsureValue()
        {
            if (State == ValueState.ValueForbidden)
                throw new InvalidOperationException("The property has already been set to have no value.");

            State = ValueState.ValueRequired;
        }

        public Option<T> GetValue()
        {
            if (State == ValueState.ValueForbidden || (State == ValueState.Default && DefaultValuePreference == DefaultValuePreference.NoValue))
                return None;

            if (State == ValueState.ValueRequired || (State == ValueState.Default && DefaultValuePreference == DefaultValuePreference.Value))
            {
                var effectiveValueProvider = _overridingValueProvider ?? _defaultValueProvider;
                var value = effectiveValueProvider.GetValue();

                if (ReferenceEquals(value, null))
                    throw new Exception("Expected value but got null.");

                if (_validators.Any(validator => !validator.Invoke(value)))
                    throw new Exception($"The value {value} is invalid.");

                return value;
            }

            throw new InvalidOperationException($"Unreachable state: {State} {DefaultValuePreference}");
        }
    }
}
