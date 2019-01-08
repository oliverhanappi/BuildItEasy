using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using BuildItEasy.Utils.Reflection;
using LanguageExt;

namespace BuildItEasy
{
    public abstract partial class Builder<TResult, TSelf>
    {
        protected ValuesConfigurator<TResult, TValue> Values<TValue>(Expression<Func<TResult, IEnumerable<TValue>>> expression,
            ValueProvider<TValue> defaultValueProvider)
        {
            return Values(expression, _ => defaultValueProvider);
        }

        protected ValuesConfigurator<TResult, TValue> Values<TValue>(Expression<Func<TResult, IEnumerable<TValue>>> expression,
            Func<int, ValueProvider<TValue>> defaultValueProviderFactory)
        {
            var memberName = ExpressionUtils.GetMemberName(expression);
            var valuesGetter = expression.Compile();

            return Values(memberName, defaultValueProviderFactory, valuesGetter);
        }

        protected ValuesConfigurator<TResult, TValue> Values<TValue>(string name, ValueProvider<TValue> defaultValueProvider,
            Option<Func<TResult, IEnumerable<TValue>>> valuesGetter = default)
        {
            return Values(name, _ => defaultValueProvider, valuesGetter);
        }

        protected ValuesConfigurator<TResult, TValue> Values<TValue>(string name,
            Func<int, ValueProvider<TValue>> defaultValueProviderFactory,
            Option<Func<TResult, IEnumerable<TValue>>> valuesGetter = default)
        {
            var valuesConfigurator = new ValuesConfigurator<TResult, TValue>(name, defaultValueProviderFactory, valuesGetter);
            
            _validators.Add(valuesConfigurator);
            _resettables.Add(valuesConfigurator);

            return valuesConfigurator;
        }

        protected TSelf CustomizeValues<TProperty>(Values<TProperty> values, ValuesCustomizer<TProperty> customizer)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (customizer == null)
                throw new ArgumentNullException(nameof(customizer));

            customizer.Invoke(values);
            return (TSelf) this;
        }
    }
}
