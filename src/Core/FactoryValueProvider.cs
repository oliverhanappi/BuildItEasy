using System;

namespace BuildItEasy
{
    public class FactoryValueProvider<T> : ValueProvider<T>
    {
        private readonly Func<T> _factory;

        public FactoryValueProvider(Func<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public override T GetValue() => _factory.Invoke();
    }
}