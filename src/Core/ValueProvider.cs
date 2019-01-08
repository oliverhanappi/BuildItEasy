using System;
using BuildItEasy.Identities;

namespace BuildItEasy
{
    public abstract class ValueProvider<T>
    {
        public static implicit operator ValueProvider<T>(T value) => new ConstantValueProvider<T>(value);
        public static implicit operator ValueProvider<T>(Func<T> factory) => new FactoryValueProvider<T>(factory);

        public abstract T GetValue();
    }
}
