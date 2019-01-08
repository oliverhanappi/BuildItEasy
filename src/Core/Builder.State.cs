using BuildItEasy.States;

namespace BuildItEasy
{
    public abstract partial class Builder<TResult, TSelf>
    {
        protected static StateHelper<TResult, TState, TSelf> State<TState, TStateDefinition>()
            where TStateDefinition : StateDefinition<TResult, TState, TSelf>, new()
        {
            return new StateHelper<TResult, TState, TSelf>(new TStateDefinition());
        }

        protected void Transition<TState>(TResult result, StateHelper<TResult, TState, TSelf> stateHelper)
        {
            stateHelper.Transition(result, (TSelf) this);
        }
    }
}
