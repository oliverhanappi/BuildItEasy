using System;

namespace BuildItEasy
{
    public class OverridableValueProvider<T> : ValueProvider<T>
    {
        private readonly string _name;
        private readonly ValueProvider<T> _defaultValueProvider;
        private ValueProvider<T> _overridingValueProvider;

        public OverridableValueProvider(string name, ValueProvider<T> defaultValueProvider)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _defaultValueProvider = defaultValueProvider ?? throw new ArgumentNullException(nameof(defaultValueProvider));
        }

        public override T GetValue()
        {
            var effectiveValueProvider = _overridingValueProvider ?? _defaultValueProvider;
            return effectiveValueProvider.GetValue();
        }

        public void Override(ValueProvider<T> overridingValueProvider)
        {
            if (_overridingValueProvider != null)
                throw new InvalidOperationException($"Another value has already been set for property {_name}.");
                
            _overridingValueProvider = overridingValueProvider ?? throw new ArgumentNullException(nameof(overridingValueProvider));
        }
    }
}