using System;
using System.Collections.Generic;

namespace BuildItEasy.States
{
    public abstract class EnumStateDefinition<T, TState, TContext> : StateDefinition<T, TState, TContext>
    {
        public override IEnumerable<TState> GetPossibleStates()
        {
            return (TState[]) Enum.GetValues(typeof(TState));
        }
    }
}