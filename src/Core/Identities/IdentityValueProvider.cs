using System;

namespace BuildItEasy.Identities
{
    public class IdentityValueProvider<T> : ValueProvider<T>
    {
        public IIdentity<T> Identity { get; }

        public IdentityValueProvider(IIdentity<T> identity)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public override T GetValue() => Identity.GetNextValue();
    }
}
