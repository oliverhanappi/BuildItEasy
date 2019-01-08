using System;
using System.Collections.Generic;
using System.Linq;
using BuildItEasy.Validation;
using LanguageExt;

namespace BuildItEasy
{
    public abstract class ValueBase<T, TContext>
    {
        private readonly List<Action<ValueProvider<T>>> _customizations = new List<Action<ValueProvider<T>>>();
        private readonly List<Validator<T>> _validators = new List<Validator<T>>();
        private readonly IDictionary<TContext, Option<T>> _values;

        public string Name { get; }
        public ValueState State { get; private set; }
        public DefaultValuePreference DefaultValuePreference { get; }

        protected ValueBase(string name, DefaultValuePreference defaultValuePreference, EqualityComparer<TContext> contextEqualityComparer = null)
        {
            State = ValueState.Default;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DefaultValuePreference = defaultValuePreference;

            contextEqualityComparer = contextEqualityComparer ?? EqualityComparer<TContext>.Default;
            _values = new Dictionary<TContext, Option<T>>(contextEqualityComparer);
        }

        protected abstract ValueProvider<T> CreateValueProvider(TContext context);

        public void AddValidator(Validator<T> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            AssertValueNotCreated();
            _validators.Add(validator);
        }

        public void NoValue()
        {
            AssertValueNotCreated();

            if (State == ValueState.ValueRequired)
                throw new InvalidOperationException($"A value has already been set for property {Name}.");

            State = ValueState.ValueForbidden;
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
            if (_values.Count > 0)
                throw new InvalidOperationException($"The value of property {Name} has already been created.");
        }

        protected void Customize(Action<ValueProvider<T>> customization)
        {
            if (customization == null)
                throw new ArgumentNullException(nameof(customization));
            
            EnsureValue();
            _customizations.Add(customization);
        }

        public Option<T> GetValue(TContext context)
        {
            if (!_values.TryGetValue(context, out var value))
            {
                value = CreateValue(context);
                _values.Add(context, value);
            }

            return value;
        }

        private Option<T> CreateValue(TContext context)
        {
            if (State == ValueState.ValueForbidden || (State == ValueState.Default && DefaultValuePreference == DefaultValuePreference.NoValue))
                return Prelude.None;

            if (State == ValueState.ValueRequired || (State == ValueState.Default && DefaultValuePreference == DefaultValuePreference.Value))
            {
                var valueProvider = CreateValueProvider(context);
                
                foreach (var customization in _customizations) 
                    customization.Invoke(valueProvider);

                var value = valueProvider.GetValue();

                if (ReferenceEquals(value, null))
                    throw new Exception($"Expected value for property {Name} but got null.");

                var validationResult = ValidationResult.Merge(_validators.Select(v => v.Validate(value)));
                validationResult.AssertValid($"property {Name}");

                return value;
            }

            throw new InvalidOperationException($"Unreachable state: {State} {DefaultValuePreference}");
        }

        public void Reset()
        {
            _values.Clear();
        }
    }
}
