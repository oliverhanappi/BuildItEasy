using System;
using System.Threading;

namespace BuildItEasy.Identities
{
    public class Identity<T> : IIdentity<T>
    {
        public static implicit operator ValueProvider<T>(Identity<T> identity)
            => new IdentityValueProvider<T>(identity);

        private readonly Func<int, T> _factory;
        private int _nextIdentity;

        public Identity(Func<int, T> factory, int initialIdentity = 1)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _nextIdentity = initialIdentity;
        }

        public T GetNextValue()
        {
            var nextIdentity = Interlocked.Increment(ref _nextIdentity);
            return _factory.Invoke(nextIdentity - 1);
        }
    }

    public class Identity : Identity<int>
    {
        public Identity(int initialIdentity = 1)
            : base(i => i, initialIdentity)
        {
        }
    }
}
