using System;

namespace BuildItEasy
{
    public class ValueProvider<T>
    {
        public static implicit operator ValueProvider<T>(T value) => new ValueProvider<T>(() => value);
        public static implicit operator ValueProvider<T>(Func<T> factory) => new ValueProvider<T>(factory);
        
        public static implicit operator ValueProvider<T>(Builder<T> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return new ValueProvider<T>(builder.Build);
        }

        public static implicit operator ValueProvider<T>(Identity<T> identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            return new ValueProvider<T>(identity.GetNextValue);
        }

        private readonly Func<T> _factory;

        public ValueProvider(Func<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public T GetValue() => _factory.Invoke();
    }
}
