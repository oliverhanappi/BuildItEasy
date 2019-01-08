using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using BuildItEasy.Validation;
using LanguageExt;

namespace BuildItEasy
{
    public abstract class ValueConfiguratorBase<TBuilderResult, TValue, TConfiguredValue, TContext, TSelf> : IValidator<TBuilderResult>, IResettable
        where TConfiguredValue : ValueBase<TValue, TContext>
        where TSelf : ValueConfiguratorBase<TBuilderResult, TValue, TConfiguredValue, TContext, TSelf> 
    {
        private readonly string _name;
        private readonly Option<Func<TBuilderResult, TValue>> _valueGetter;
        private readonly List<Validator<TValue>> _validators;
        private readonly Lazy<TConfiguredValue> _value;

        private Option<DefaultValuePreference> _defaultValuePreference;
        private Option<bool> _required;

        public TConfiguredValue Value => _value.Value;

        protected ValueConfiguratorBase(string name, Option<Func<TBuilderResult, TValue>> valueGetter)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _valueGetter = valueGetter;
            _validators = new List<Validator<TValue>>();
            _value = new Lazy<TConfiguredValue>(Build);
        }

        public TSelf Required()
        {
            AssertNotBuilt();

            _required.IfSome(required =>
            {
                if (!required)
                    throw new InvalidOperationException($"{_name} must not be required.");
            });

            _required = true;

            return (TSelf) this;
        }

        public TSelf OnlyIfNecessary()
        {
            AssertNotBuilt();

            _required.IfSome(required =>
            {
                if (required)
                    throw new InvalidOperationException($"{_name} is required.");
            });

            _defaultValuePreference.IfSome(defaultValuePreference =>
            {
                if (defaultValuePreference != DefaultValuePreference.NoValue)
                    throw new InvalidOperationException($"{_name} has default value preference {defaultValuePreference}.");
            });

            _required = false;
            _defaultValuePreference = DefaultValuePreference.NoValue;

            return (TSelf) this;
        }

        public TSelf Validate(Expression<Func<TValue, bool>> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            AssertNotBuilt();

            _validators.Add(new Validator<TValue>(validator.ToString(), validator.Compile()));
            return (TSelf) this;
        }

        private void AssertNotBuilt()
        {
            if (_value.IsValueCreated)
                throw new InvalidOperationException($"{_name} has already been built.");
        }
        
        private TConfiguredValue Build()
        {
            var defaultValuePreference = _defaultValuePreference.IfNone(DefaultValuePreference.Value);
            var required = _required.IfNone(false);

            var value = CreateValue(_name, defaultValuePreference);

            if (required)
                value.EnsureValue();

            foreach (var validator in _validators)
                value.AddValidator(validator);

            return value;
        }

        protected abstract TConfiguredValue CreateValue(string name, DefaultValuePreference defaultValuePreference);

        ValidationResult IValidator<TBuilderResult>.Validate(TBuilderResult result)
        {
            var errors = new List<string>();

            var context = GetContext(result);
            Value.GetValue(context).IfSome(configuredValue => _valueGetter.IfSome(valueGetter =>
            {
                var actualValue = valueGetter.Invoke(result);
                if (!Equals(actualValue, configuredValue))
                    errors.Add($"{_name}: Expected {configuredValue} but got {actualValue}.");
            }));
            
            return new ValidationResult(errors);
        }

        protected abstract TContext GetContext(TBuilderResult result);

        public void Reset()
        {
            if (_value.IsValueCreated)
            {
                _value.Value.Reset();
            }
        }
    }
}
