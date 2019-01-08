using System;

namespace BuildItEasy
{
    public class BuilderValueProvider<T> : ValueProvider<T>
    {
        public IBuilder<T> Builder { get; }

        public BuilderValueProvider(IBuilder<T> builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public override T GetValue() => Builder.Build();
    }
}
