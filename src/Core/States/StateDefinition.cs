using System;
using System.Collections.Generic;
using BuildItEasy.Graphs;

namespace BuildItEasy.States
{
    public abstract class StateDefinition<TEntity, TState, TContext>
    {
        private Graph<TState, StateTransition<TEntity, TState, TContext>> _stateGraph;

        public IReadOnlyGraph<TState, StateTransition<TEntity, TState, TContext>> StateGraph
        {
            get
            {
                if (_stateGraph == null)
                {
                    InitializeStateGraph();
                }

                return _stateGraph;
            }
        }

        private void InitializeStateGraph()
        {
            _stateGraph = new Graph<TState, StateTransition<TEntity, TState, TContext>>(GetStateEqualityComparer());

            foreach (var possibleState in GetPossibleStates())
                _stateGraph.AddNode(possibleState);

            ConfigureTransitions();
        }

        public abstract TState GetInitialState(TEntity entity, TContext context);

        public abstract IEnumerable<TState> GetPossibleStates();

        public abstract TState GetState(TEntity entity);

        public virtual IEqualityComparer<TState> GetStateEqualityComparer() => EqualityComparer<TState>.Default;
        
        protected abstract void ConfigureTransitions();

        protected void Transition(TState sourceState, TState targetState, Action<TEntity> transitionAction)
        {
            if (transitionAction == null)
                throw new ArgumentNullException(nameof(transitionAction));
            
            Transition(sourceState, targetState, (e, c) => transitionAction.Invoke(e));
        }

        protected void Transition(TState sourceState, TState targetState, Action<TEntity, TContext> transitionAction)
        {
            if (transitionAction == null)
                throw new ArgumentNullException(nameof(transitionAction));
            
            var stateTransition = new StateTransition<TEntity, TState, TContext>(sourceState, targetState, transitionAction);
            _stateGraph.AddEdge(stateTransition);
        }
    }
}
