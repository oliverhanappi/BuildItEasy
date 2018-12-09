using System;
using BuildItEasy.States;
using LanguageExt;

namespace BuildItEasy
{
    public abstract class Builder<TResult>
    {
        protected static Property<TProperty> Property<TProperty>(ValueProvider<TProperty> defaultValueProvider)
        {
            return new Property<TProperty>(defaultValueProvider, DefaultValuePreference.Value);
        }

        protected static Property<TProperty> RequiredProperty<TProperty>(ValueProvider<TProperty> defaultValueProvider)
        {
            var property = new Property<TProperty>(defaultValueProvider, DefaultValuePreference.Value);
            property.EnsureValue();

            return property;
        }

        protected static Property<TProperty> OptionalProperty<TProperty>(ValueProvider<TProperty> defaultValueProvider)
        {
            return new Property<TProperty>(defaultValueProvider, DefaultValuePreference.NoValue);
        }
        
        public abstract TResult Build();
    }
    
    public abstract class Builder<TResult, TSelf> : Builder<TResult>
        where TSelf : Builder<TResult, TSelf>
    {
        protected class ChildBuilder<TChild, TChildBuilder> : ChildBuilder<TResult, TSelf, TChild, TChildBuilder>
            where TChildBuilder : Builder<TChild>
        {
            public ChildBuilder(Func<TResult, TSelf, TChildBuilder> builderFactory, DefaultValuePreference defaultValuePreference)
                : base(builderFactory, defaultValuePreference)
            {
            }
        }
        
        public TSelf Customize(Action<TSelf> customize = null)
        {
            customize?.Invoke((TSelf) this);
            return (TSelf) this;
        }
        
        protected TSelf SetValue<TProperty>(Property<TProperty> property, ValueProvider<TProperty> valueProvider)
        {
            if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));

            property.SetValue(valueProvider);
            return (TSelf) this;
        }
        
        protected TSelf NoValue<TProperty>(Property<TProperty> property)
        {
            property.NoValue();
            return (TSelf) this;
        }

        protected static ChildBuilder<TChild, TChildBuilder> RequiredChild<TChild, TChildBuilder>(Func<TResult, TChildBuilder> builderFactory)
            where TChildBuilder : Builder<TChild>
        {
            return RequiredChild<TChild, TChildBuilder>((r, _) => builderFactory(r));
        }

        protected static ChildBuilder<TChild, TChildBuilder> RequiredChild<TChild, TChildBuilder>(Func<TResult, TSelf, TChildBuilder> builderFactory)
            where TChildBuilder : Builder<TChild>
        {
            var childBuilder = new ChildBuilder<TChild, TChildBuilder>(builderFactory, DefaultValuePreference.Value);
            childBuilder.EnsureChild();

            return childBuilder;
        }

        protected static ChildBuilder<TChild, TChildBuilder> Child<TChild, TChildBuilder>(Func<TResult, TChildBuilder> builderFactory)
            where TChildBuilder : Builder<TChild>
        {
            return Child<TChild, TChildBuilder>((r, _) => builderFactory(r));
        }

        protected static ChildBuilder<TChild, TChildBuilder> Child<TChild, TChildBuilder>(Func<TResult, TSelf, TChildBuilder> builderFactory)
            where TChildBuilder : Builder<TChild>
        {
            return new ChildBuilder<TChild, TChildBuilder>(builderFactory, DefaultValuePreference.Value);
        }

        protected static ChildBuilder<TChild, TChildBuilder> OptionalChild<TChild, TChildBuilder>(Func<TResult, TChildBuilder> builderFactory)
            where TChildBuilder : Builder<TChild>
        {
            return OptionalChild<TChild, TChildBuilder>((r, _) => builderFactory(r));
        }

        protected static ChildBuilder<TChild, TChildBuilder> OptionalChild<TChild, TChildBuilder>(Func<TResult, TSelf, TChildBuilder> builderFactory)
            where TChildBuilder : Builder<TChild>
        {
            return new ChildBuilder<TChild, TChildBuilder>(builderFactory, DefaultValuePreference.NoValue);
        }

        protected TSelf NoChild<TChild, TChildBuilder>(ChildBuilder<TResult, TSelf, TChild, TChildBuilder> childBuilder)
            where TChildBuilder : Builder<TChild>
        {
            childBuilder.NoChild();
            return (TSelf) this;
        }

        protected TSelf CustomizeChild<TChild, TChildBuilder>(ChildBuilder<TResult, TSelf, TChild, TChildBuilder> childBuilder, Action<TChildBuilder> customize)
            where TChildBuilder : Builder<TChild>
        {
            childBuilder.Customize((_, __, b) => customize.Invoke(b));
            return (TSelf) this;
        }

        protected Option<TChild> BuildChild<TChild, TChildBuilder>(TResult result, ChildBuilder<TResult, TSelf, TChild, TChildBuilder> childBuilder)
            where TChildBuilder : Builder<TChild>
        {
            return childBuilder.BuildChild(result, (TSelf) this);
        }

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
