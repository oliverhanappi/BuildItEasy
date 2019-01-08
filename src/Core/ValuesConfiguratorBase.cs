using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildItEasy.Validation;
using LanguageExt;

namespace BuildItEasy
{
    public abstract class ValuesConfiguratorBase<TBuilderResult, TValue, TValueBuilder, TCustomizer, TValues, TContext, TSelf> : IValidator<TBuilderResult>, IResettable
        where TValues : ValuesBase<TValue, TValueBuilder, TCustomizer, TContext>
        where TSelf : ValuesConfiguratorBase<TBuilderResult, TValue, TValueBuilder, TCustomizer, TValues, TContext, TSelf>
    {
        private readonly string _name;
        private readonly Option<Func<TBuilderResult, IEnumerable<TValue>>> _valuesGetter;
        private readonly Lazy<TValues> _values;

        private Option<bool> _required;
        private Option<int> _defaultCount;
        private IEqualityComparer<TValue> _equalityComparer;

        public TValues Values => _values.Value;

        public ValuesConfiguratorBase(string name, Option<Func<TBuilderResult, IEnumerable<TValue>>> valuesGetter)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _valuesGetter = valuesGetter;
            _equalityComparer = EqualityComparer<TValue>.Default;
            _values = new Lazy<TValues>(Build);
        }

        public TSelf DefaultCount(int defaultCount)
        {
            AssertNotBuilt();

            if (defaultCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_defaultCount), defaultCount,
                    $"Default count must not be negative but was {defaultCount}.");
            }

            _defaultCount.IfSome(existingDefaultCount =>
            {
                if (existingDefaultCount != defaultCount)
                {
                    throw new InvalidOperationException(
                        $"Default count of {_name} is already set to {existingDefaultCount}");
                }
            });

            _defaultCount = defaultCount;
            return (TSelf) this;
        }

        public TSelf EqualityComparer(IEqualityComparer<TValue> equalityComparer)
        {
            if (equalityComparer == null)
                throw new ArgumentNullException(nameof(equalityComparer));
            
            AssertNotBuilt();

            _equalityComparer = equalityComparer;
            return (TSelf) this;
        }

        public TSelf Required()
        {
            AssertNotBuilt();

            _required.IfSome(required =>
            {
                if (!required)
                    throw new InvalidOperationException($"{_name} must not be required.");
            });

            _defaultCount.IfSome(defaultCount =>
            {
                if (defaultCount == 0)
                    throw new InvalidOperationException($"{_name} has default count 0 but requires greater than 0.");
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

            _defaultCount.IfSome(defaultCount =>
            {
                if (defaultCount > 0)
                    throw new InvalidOperationException($"{_name} has default count {defaultCount} but requires 0.");
            });

            _defaultCount = 0;
            _required = false;
            
            return (TSelf) this;
        }

        private void AssertNotBuilt()
        {
            if (_values.IsValueCreated)
                throw new InvalidOperationException($"{_name} has already been built.");
        }
        
        private TValues Build()
        {
            var required = _required.IfNone(false);
            var defaultCount = _defaultCount.IfNone(() => _required.Match(r => r ? 1 : 0, () => 1));

            var values = CreateValues(_name, defaultCount);

            if (required)
                values.Some();

            return values;
        }

        protected abstract TValues CreateValues(string name, int defaultCount);

        ValidationResult IValidator<TBuilderResult>.Validate(TBuilderResult builderResult)
        {
            var errors = new List<string>();

            _valuesGetter.IfSome(valuesGetter =>
            {
                var actualValues = valuesGetter.Invoke(builderResult)?.ToList() ?? new List<TValue>();
                var expectedValues = Values.GetValues(GetContext(builderResult));

                if (ReferenceEquals(actualValues, expectedValues))
                    return;

                if (!actualValues.SequenceEqual(expectedValues, _equalityComparer))
                {
                    var expected = $"  Expected: {String.Join(", ", expectedValues)}";
                    var actual = $"  Actual:   {String.Join(", ", actualValues)}";

                    var error = new StringBuilder();
                    error.AppendLine($"{_name}: Expected and actual values differ:");
                    error.AppendLine(expected);
                    error.AppendLine(actual);
                    
                    errors.Add(error.ToString().TrimEnd());
                }
            });
            
            return new ValidationResult(errors);
        }

        protected abstract TContext GetContext(TBuilderResult builderResult);
        
        public void Reset()
        {
            if (_values.IsValueCreated)
            {
                _values.Value.Reset();
            }
        }
    }
}
