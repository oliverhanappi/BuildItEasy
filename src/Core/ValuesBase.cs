using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildItEasy
{
    public abstract class ValuesBase<T, TBuilder, TCustomizer, TContext> : IValues<TCustomizer>
    {
        private readonly int _defaultCount;
        private readonly IDictionary<TContext, IReadOnlyList<T>> _values;
        private readonly List<Action<int, TBuilder>> _customizers;
        
        private int _minCount = 0;
        private int _maxCount = Int32.MaxValue;

        public string Name { get; }

        protected ValuesBase(string name, int defaultCount, IEqualityComparer<TContext> contextEqualityComparer = null)
        {
            if (defaultCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultCount), defaultCount,
                    $"Default count must not be negative, but was {defaultCount}.");
            }

            contextEqualityComparer = contextEqualityComparer ?? EqualityComparer<TContext>.Default;

            Name = name ?? throw new ArgumentNullException(nameof(name));
            _defaultCount = defaultCount;
            _values = new Dictionary<TContext, IReadOnlyList<T>>(contextEqualityComparer);
            _customizers = new List<Action<int, TBuilder>>();
        }

        public IValues<TCustomizer> Some() => AtLeast(1);
        public IValues<TCustomizer> None() => Exactly(0);

        public IValues<TCustomizer> AtLeast(int minCount)
        {
            SetMinCount(minCount);
            return this;
        }

        public IValues<TCustomizer> Exactly(int count)
        {
            SetMinCount(count);
            SetMaxCount(count);
            return this;
        }

        public IValues<TCustomizer> AtMost(int maxCount)
        {
            SetMaxCount(maxCount);
            return this;
        }

        private void SetMinCount(int minCount)
        {
            if (minCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minCount), minCount,
                    $"Min count must not be negative but was {minCount}");
            }

            if (minCount > _maxCount)
                throw new InvalidOperationException($"Cannot set min count of {Name} to {minCount} because max count is {_maxCount}.");
            
            _minCount = Math.Max(_minCount, minCount);
        }

        private void SetMaxCount(int maxCount)
        {
            if (maxCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount,
                    $"Max count must not be negative but was {maxCount}");
            }

            if (maxCount < _minCount)
                throw new InvalidOperationException($"Cannot set max count of {Name} to {maxCount} because min count is {_minCount}.");

            _maxCount = Math.Min(_maxCount, maxCount);
        }

        public IValues<TCustomizer> WithAll(TCustomizer customizer)
        {
            _customizers.Add((_, builder) => ApplyCustomizer(builder, customizer));
            return this;
        }

        public IValues<TCustomizer> With(int index, TCustomizer customizer)
        {
            _customizers.Add((i, builder) =>
            {
                if (i == index)
                    ApplyCustomizer(builder, customizer);
            });

            return this;
        }

        protected abstract void ApplyCustomizer(TBuilder builder, TCustomizer customizer);

        public IValues<TCustomizer> WithExactly(params TCustomizer[] customizers)
        {
            Exactly(customizers.Length);

            for (var i = 0; i < customizers.Length; i++)
                With(i, customizers[i]);

            return this;
        }

        public IReadOnlyList<T> GetValues(TContext context)
        {
            if (!_values.TryGetValue(context, out var values))
            {
                values = CreateValues(context);
                _values.Add(context, values);
            }

            return values;
        }

        private IReadOnlyList<T> CreateValues(TContext context)
        {
            var count = Math.Min(Math.Max(_defaultCount, _minCount), _maxCount);

            var list = new List<TBuilder>(capacity: count);

            for (var i = 0; i < count; i++)
            {
                var builder = CreateBuilder(context, i);
                list.Add(builder);
            }

            foreach (var customizer in _customizers)
            {
                for (var i = 0; i < count; i++)
                {
                    customizer.Invoke(i, list[i]);
                }
            }

            return list.Select(b => BuildValue(context, b)).ToList().AsReadOnly();
        }

        protected abstract TBuilder CreateBuilder(TContext context, int index);

        protected abstract T BuildValue(TContext context, TBuilder builder);

        public void Reset()
        {
            _values.Clear();
        }
    }
}
