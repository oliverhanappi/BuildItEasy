using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BuildItEasy.Reflection;
using BuildItEasy.States;
using LanguageExt;

namespace BuildItEasy
{
    public abstract class Builder<TResult> : IBuilder
    {
        private readonly ICollection<IValidator<TResult>> _validators;

        protected Builder()
        {
            _validators = new List<IValidator<TResult>>();
        }

        protected PropertyConfigurator<TResult, TProperty> Property<TProperty>(
            Expression<Func<TResult, TProperty>> expression, ValueProvider<TProperty> defaultValueProvider)
        {
            var memberName = ExpressionUtils.GetMemberName(expression);
            var valueGetter = expression.Compile();

            return Property(memberName, defaultValueProvider, valueGetter);
        }

        protected PropertyConfigurator<TResult, TProperty> Property<TProperty>(string propertyName,
            ValueProvider<TProperty> defaultValueProvider, Option<Func<TResult, TProperty>> valueGetter = default)
        {
            var propertyConfigurator =
                new PropertyConfigurator<TResult, TProperty>(propertyName, defaultValueProvider, valueGetter);

            _validators.Add(propertyConfigurator);
            return propertyConfigurator;
        }

        public TResult Build()
        {
            var result = BuildInternal();

            var validationResult = _validators
                .Select(v => v.Validate(result))
                .Aggregate(ValidationResult.Valid, (x, y) => new ValidationResult(x.Errors.Concat(y.Errors)));

            validationResult.AssertValid();

            return result;
        }

        protected abstract TResult BuildInternal();
    }

    public abstract class Builder<TResult, TSelf> : Builder<TResult>
        where TSelf : Builder<TResult, TSelf>
    {
        protected class ChildBuilder<TChild, TChildBuilder> : ChildBuilder<TResult, TSelf, TChild, TChildBuilder>
            where TChildBuilder : Builder<TChild>
        {
            public ChildBuilder(Func<TResult, TSelf, TChildBuilder> builderFactory,
                DefaultValuePreference defaultValuePreference)
                : base(builderFactory, defaultValuePreference)
            {
            }
        }

        protected class ChildrenBuilder<TChild, TChildBuilder> : ChildrenBuilder<TResult, TSelf, TChild, TChildBuilder>
            where TChildBuilder : Builder<TChild>
        {
        }

        public TSelf Customize(Customizer<TSelf> customizer = null)
        {
            customizer?.Invoke((TSelf) this);
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

        protected ChildBuilderConfigurator<TResult, TSelf, TChild, TChildBuilder, ChildBuilder<TChild, TChildBuilder>> Child<TChild, TChildBuilder>(
            Func<TResult, TChildBuilder> builderFactory)
            where TChildBuilder : Builder<TChild>
        {
            return Child<TChild, TChildBuilder>((r, _) => builderFactory(r));
        }

        protected ChildBuilderConfigurator<TResult, TSelf, TChild, TChildBuilder, ChildBuilder<TChild, TChildBuilder>> Child<TChild, TChildBuilder>(
            Func<TResult, TSelf, TChildBuilder> builderFactory)
            where TChildBuilder : Builder<TChild>
        {
            return new ChildBuilderConfigurator<TResult, TSelf, TChild, TChildBuilder, ChildBuilder<TChild, TChildBuilder>>("TODO", dvp => new ChildBuilder<TChild, TChildBuilder>(builderFactory, dvp));
        }

        protected TSelf NoChild<TChild, TChildBuilder>(ChildBuilder<TResult, TSelf, TChild, TChildBuilder> childBuilder)
            where TChildBuilder : Builder<TChild>
        {
            childBuilder.NoChild();
            return (TSelf) this;
        }

        protected TSelf CustomizeChild<TChild, TChildBuilder>(
            ChildBuilder<TResult, TSelf, TChild, TChildBuilder> childBuilder, Customizer<TChildBuilder> customizer)
            where TChildBuilder : Builder<TChild>
        {
            childBuilder.Customize((_, __, b) => customizer.Invoke(b));
            return (TSelf) this;
        }

        protected Option<TChild> BuildChild<TChild, TChildBuilder>(TResult result,
            ChildBuilder<TResult, TSelf, TChild, TChildBuilder> childBuilder)
            where TChildBuilder : Builder<TChild>
        {
            return childBuilder.BuildChild(result, (TSelf) this);
        }

        protected TSelf CustomizeChildren<TChild, TChildBuilder>(
            ChildrenBuilder<TResult, TSelf, TChild, TChildBuilder> childrenBuilder, CollectionCustomizer<TChildBuilder> customizer)
            where TChildBuilder : Builder<TChild>
        {
            //TODO childrenBuilder.Customize((_, __, b) => customizer.Invoke(b));
            return (TSelf) this;
        }

        protected IReadOnlyList<TChild> BuildChildren<TChild, TChildBuilder>(TResult result,
            ChildrenBuilder<TResult, TSelf, TChild, TChildBuilder> childrenBuilder)
            where TChildBuilder : Builder<TChild>
        {
            return childrenBuilder.BuildChildren(result, (TSelf) this);
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
