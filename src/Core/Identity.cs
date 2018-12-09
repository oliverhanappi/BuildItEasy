using System;
using System.Threading;

namespace BuildItEasy
{
    public class Identity<T>
    {
        private readonly Func<int, T> _factory;
        private int _nextIdentity;

        public Identity(Func<int, T> factory, int initialIdentity = 1)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _nextIdentity = initialIdentity;
        }

        public T GetNextValue()
        {
            var identity = Interlocked.Increment(ref _nextIdentity);
            return _factory.Invoke(identity - 1);
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
