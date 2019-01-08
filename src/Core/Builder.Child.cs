using System;
using System.Linq.Expressions;
using BuildItEasy.Nesting;
using BuildItEasy.Utils.Reflection;
using LanguageExt;

namespace BuildItEasy
{
    public abstract partial class Builder<TResult, TSelf>
    {
        protected class Child<TChild, TChildBuilder> : Child<TChild, TChildBuilder, TResult>
            where TChildBuilder : IBuilder<TChild>
        {
            public Child(string name, DefaultValuePreference defaultValuePreference, Func<TResult, TChildBuilder> builderFactory)
                : base(name, defaultValuePreference, builderFactory)
            {
            }
        }
        
        protected ChildConfigurator<TResult, TProperty, TBuilder, Child<TProperty, TBuilder>> AddChild<TProperty, TBuilder>(
            Expression<Func<TResult, TProperty>> expression, Func<TResult, TBuilder> builderFactory)
            where TBuilder : IBuilder<TProperty>
        {
            var memberName = ExpressionUtils.GetMemberName(expression);
            var childGetter = expression.Compile();

            return AddChild<TProperty, TBuilder>(memberName, builderFactory, childGetter);
        }

        protected ChildConfigurator<TResult, TProperty, TBuilder, Child<TProperty, TBuilder>> AddChild<TProperty, TBuilder>(string name,
            Func<TResult, TBuilder> builderFactory, Option<Func<TResult, TProperty>> childGetter = default)
            where TBuilder : IBuilder<TProperty>
        {
            var propertyConfigurator =
                new ChildConfigurator<TResult, TProperty, TBuilder, Child<TProperty, TBuilder>>(name, childGetter, CreateValue);

            Child<TProperty, TBuilder> CreateValue(string valueName, DefaultValuePreference defaultValuePreference)
            {
                return new Child<TProperty, TBuilder>(valueName, defaultValuePreference, builderFactory);
            }

            _validators.Add(propertyConfigurator);
            _resettables.Add(propertyConfigurator);

            return propertyConfigurator;
        }

        protected TSelf CustomizeChild<TProperty, TBuilder>(Child<TProperty, TBuilder, TResult> child, Customizer<TBuilder> customizer)
            where TBuilder : IBuilder<TProperty>
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (customizer == null)
                throw new ArgumentNullException(nameof(customizer));
            
            child.Customize(customizer);
            return (TSelf) this;
        }

        protected TSelf NoChild<TProperty, TBuilder>(Child<TProperty, TBuilder, TResult> child)
            where TBuilder : IBuilder<TProperty>
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            
            child.NoValue();
            return (TSelf) this;
        }

        protected Option<TChild> BuildChild<TChild, TChildBuilder>(TResult result, Child<TChild, TChildBuilder, TResult> child)
            where TChildBuilder : IBuilder<TChild>
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            
            return child.GetValue(result);
        }
    }
}
