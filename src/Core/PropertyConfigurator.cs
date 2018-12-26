using System;
using System.Collections.Generic;
using LanguageExt;

namespace BuildItEasy
{
    public class PropertyConfigurator<TResult, TProperty> : IValidator<TResult>
    {
        public static implicit operator Property<TProperty>(PropertyConfigurator<TResult, TProperty> configurator) => configurator.Property;

        private readonly string _name;
        private readonly ValueProvider<TProperty> _defaultValueProvider;
        private readonly Option<Func<TResult, TProperty>> _valueGetter;
        private readonly List<Func<TProperty, bool>> _validators;
        private readonly Lazy<Property<TProperty>> _property;

        private Option<DefaultValuePreference> _defaultValuePreference;
        private Option<bool> _required;

        public Property<TProperty> Property => _property.Value;

        public PropertyConfigurator(string name, ValueProvider<TProperty> defaultValueProvider, Option<Func<TResult, TProperty>> valueGetter)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _defaultValueProvider = defaultValueProvider ?? throw new ArgumentNullException(nameof(defaultValueProvider));
            _valueGetter = valueGetter;
            _validators = new List<Func<TProperty, bool>>();
            _property = new Lazy<Property<TProperty>>(Build);
        }

        public PropertyConfigurator<TResult, TProperty> Required()
        {
            AssertNotBuilt();

            _required.IfSome(required =>
            {
                if (!required)
                    throw new InvalidOperationException($"Property {_name} must not be required.");
            });

            _required = true;

            return this;
        }

        public PropertyConfigurator<TResult, TProperty> OnlyIfNecessary()
        {
            AssertNotBuilt();

            _required.IfSome(required =>
            {
                if (required)
                    throw new InvalidOperationException($"Property {_name} is required.");
            });

            _defaultValuePreference.IfSome(defaultValuePreference =>
            {
                if (defaultValuePreference != DefaultValuePreference.NoValue)
                    throw new InvalidOperationException($"Property {_name} has default value preference {defaultValuePreference}.");
            });

            _required = false;
            _defaultValuePreference = DefaultValuePreference.NoValue;

            return this;
        }

        public PropertyConfigurator<TResult, TProperty> Validate(Func<TProperty, bool> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            AssertNotBuilt();

            _validators.Add(validator);
            return this;
        }

        private void AssertNotBuilt()
        {
            if (_property.IsValueCreated)
                throw new InvalidOperationException($"Property {_name} has already been built.");
        }
        
        private Property<TProperty> Build()
        {
            var defaultValuePreference = _defaultValuePreference.IfNone(DefaultValuePreference.Value);
            var required = _required.IfNone(false);

            var property = new Property<TProperty>(_name, _defaultValueProvider, defaultValuePreference);

            if (required)
                property.EnsureValue();

            foreach (var validator in _validators)
                property.Validate(validator);

            return property;
        }

        public ValidationResult Validate(TResult result)
        {
            var errors = new List<string>();

            Property.Value.IfSome(configuredValue => _valueGetter.IfSome(valueGetter =>
            {
                var actualValue = valueGetter.Invoke(result);
                if (!Equals(actualValue, configuredValue))
                    errors.Add($"Property {_name}: Expected {configuredValue} but got {actualValue}.");
            }));
            
            return new ValidationResult(errors);
        }
    }
}
