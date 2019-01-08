using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using BuildItEasy.Reflection;
using LanguageExt;

namespace BuildItEasy
{
    public abstract partial class Builder<TResult, TSelf>
    {
        protected class Children<TChild, TChildBuilder> : Children<TChild, TChildBuilder, TResult>
            where TChildBuilder : IBuilder<TChild>
        {
            public Children(string name, int defaultCount, Func<TResult, int, TChildBuilder> builderFactory)
                : base(name, defaultCount, builderFactory)
            {
            }
        }
        
        protected ChildrenConfigurator<TResult, TValue, TValueBuilder, Children<TValue, TValueBuilder>> AddChildren<TValue, TValueBuilder>(
            Expression<Func<TResult, IEnumerable<TValue>>> expression, Func<TResult, int, TValueBuilder> builderFactory)
            where TValueBuilder : IBuilder<TValue>
        {
            var memberName = ExpressionUtils.GetMemberName(expression);
            var childrenGetter = expression.Compile();

            return AddChildren<TValue, TValueBuilder>(memberName, builderFactory, childrenGetter);
        }
        protected ChildrenConfigurator<TResult, TValue, TValueBuilder, Children<TValue, TValueBuilder>> AddChildren<TValue, TValueBuilder>(string name,
            Func<TResult, int, TValueBuilder> builderFactory, Option<Func<TResult, IEnumerable<TValue>>> childrenGetter = default)
            where TValueBuilder : IBuilder<TValue>
        {
            Children<TValue, TValueBuilder> CreateChildren(string childrenName, int defaultCount)
            {
                return new Children<TValue, TValueBuilder>(childrenName, defaultCount, builderFactory);
            }
            
            var childrenConfigurator = new ChildrenConfigurator<TResult, TValue, TValueBuilder, Children<TValue, TValueBuilder>>(name, CreateChildren, childrenGetter);
            
            _validators.Add(childrenConfigurator);
            _resettables.Add(childrenConfigurator);

            return childrenConfigurator;
        }

        protected TSelf CustomizeChildren<TProperty, TBuilder>(Children<TProperty, TBuilder, TResult> children,
            ValuesCustomizer<TProperty, TBuilder> customizer)
            where TBuilder : IBuilder<TProperty>
        {
            if (children == null)
                throw new ArgumentNullException(nameof(children));
            if (customizer == null)
                throw new ArgumentNullException(nameof(customizer));

            customizer.Invoke(children);
            
            return (TSelf) this;
        }

        protected IReadOnlyList<TChild> BuildChildren<TChild, TChildBuilder>(TResult result,
            Children<TChild, TChildBuilder, TResult> children)
            where TChildBuilder : IBuilder<TChild>
        {
            if (children == null)
                throw new ArgumentNullException(nameof(children));
            
            return children.GetValues(result);
        }
    }
}
