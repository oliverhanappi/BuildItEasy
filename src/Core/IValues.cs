using System;

namespace BuildItEasy
{
    public interface IValues<in TCustomizer>
    {
        IValues<TCustomizer> Some();
        IValues<TCustomizer> None();
        
        IValues<TCustomizer> AtLeast(int minCount);
        IValues<TCustomizer> Exactly(int count);
        IValues<TCustomizer> AtMost(int maxCount);

        IValues<TCustomizer> WithAll(TCustomizer customizer);

        IValues<TCustomizer> With(int index, TCustomizer customizer);

        IValues<TCustomizer> WithExactly(params TCustomizer[] customizers);
    }
}
