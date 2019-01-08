using System;
using BuildItEasy.Utils.Graphs;

namespace BuildItEasy.States
{
    public class StateTransition<TEntity, TState, TContext> : Edge<TState>
    {
        public Action<TEntity, TContext> TransitionAction { get; }

        public StateTransition(TState sourceState, TState targetState, Action<TEntity, TContext> transitionAction)
            : base(sourceState, targetState)
        {
            TransitionAction = transitionAction ?? throw new ArgumentNullException(nameof(transitionAction));
        }
    }
}