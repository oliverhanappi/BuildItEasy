using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildItEasy.Graphs;

namespace BuildItEasy.States
{
    public class StateHelper<TEntity, TState, TContext>
    {
        private readonly StateDefinition<TEntity, TState, TContext> _stateDefinition;

        private readonly List<string> _conditions;
        private readonly HashSet<TState> _possibleDesiredStates;
        private readonly HashSet<TState> _requiredStates;

        public StateHelper(StateDefinition<TEntity, TState, TContext> stateDefinition)
        {
            _stateDefinition = stateDefinition ?? throw new ArgumentNullException(nameof(stateDefinition));

            _conditions = new List<string>();
            _possibleDesiredStates = new HashSet<TState>(_stateDefinition.GetPossibleStates(), _stateDefinition.GetStateEqualityComparer());
            _requiredStates = new HashSet<TState>(_stateDefinition.GetStateEqualityComparer());
        }

        public void RequireAtLeast(TState state)
        {
            var possibleStates = _stateDefinition.StateGraph.GetReachableNodes(state);
            RestrictDesiredState($"at least {state}", state, possibleStates);
        }

        public void RequireAtMost(TState state)
        {
            var possibleStates = _stateDefinition.StateGraph.GetReachingNodes(state);
            RestrictDesiredState($"at most {state}", state, possibleStates);
        }

        public void RequireExactly(TState state)
        {
            var possibleStates = new[] {state};
            RestrictDesiredState($"exactly {state}", state, possibleStates);
        }

        public void EnsureNot(TState state)
        {
            _conditions.Add($"ensure not {state}");
            _possibleDesiredStates.Remove(state);
            
            AssertHasAnyPossibleDesiredState();
        }

        private void RestrictDesiredState(string condition, TState requestedState, IEnumerable<TState> possibleStates)
        {
            _conditions.Add(condition);
            _requiredStates.Add(requestedState);

            _possibleDesiredStates.IntersectWith(possibleStates);

            AssertHasAnyPossibleDesiredState();
        }

        private void AssertHasAnyPossibleDesiredState()
        {
            if (_possibleDesiredStates.Count == 0)
            {
                var message = new StringBuilder();
                message.AppendLine("A conflict in state conditions has occurred such that no state is valid.");
                foreach (var recordedCondition in _conditions)
                    message.AppendLine($"  - {recordedCondition}");

                throw new Exception(message.ToString().TrimEnd());
            }
        }

        public void Transition(TEntity entity, TContext context)
        {
            var path = GetPath(entity, context);

            AssertEntityState(entity, path.StartNode);

            foreach (var stateTransition in path.Edges)
            {
                AssertEntityState(entity, stateTransition.StartNode);

                stateTransition.TransitionAction.Invoke(entity, context);

                AssertEntityState(entity, stateTransition.EndNode);
            }

            AssertEntityState(entity, path.EndNode);
        }

        private void AssertEntityState(TEntity entity, TState expectedState)
        {
            var currentState = _stateDefinition.GetState(entity);
            if (!_stateDefinition.GetStateEqualityComparer().Equals(currentState, expectedState))
                throw new Exception($"Expected {entity} to be in state {expectedState}, but was in state {currentState}.");
        }

        private Path<TState, StateTransition<TEntity, TState, TContext>> GetPath(TEntity entity, TContext context)
        {
            var initialState = _stateDefinition.GetInitialState(entity, context);

            foreach (var state in _stateDefinition.StateGraph.GetTopologicallySortedNodes())
            {
                if (!_possibleDesiredStates.Contains(state))
                    continue;

                var paths = _stateDefinition.StateGraph.GetPaths(initialState, state)
                    .OrderBy(p => p.Edges.Count);

                foreach (var path in paths)
                {
                    if (_requiredStates.All(path.Nodes.Contains))
                        return path;
                }
            }

            var message = new StringBuilder();
            message.AppendLine("Failed to determine path.");
            message.AppendLine();

            var states = String.Join(", ", _stateDefinition.StateGraph.GetTopologicallySortedNodes());
            message.AppendLine($"Topologically sorted states: {states}");
            message.AppendLine();
            
            message.AppendLine("Possible desired states:");
            foreach (var possibleDesiredState in _possibleDesiredStates)
                message.AppendLine($"  - {possibleDesiredState}");
            message.AppendLine();

            message.AppendLine("Conditions:");
            foreach (var condition in _conditions)
                message.AppendLine($"  - {condition}");

            throw new Exception(message.ToString().TrimEnd());
        }
    }
}
