using System;
using System.Linq.Expressions;
using BuildItEasy.Reflection;
using LanguageExt;

namespace BuildItEasy
{
    public abstract partial class Builder<TResult, TSelf>
    {
        protected ValueConfigurator<TResult, TProperty> Value<TProperty>(
            Expression<Func<TResult, TProperty>> expression, ValueProvider<TProperty> defaultValueProvider)
        {
            var memberName = ExpressionUtils.GetMemberName(expression);
            var valueGetter = expression.Compile();

            return Value(memberName, defaultValueProvider, valueGetter);
        }

        protected ValueConfigurator<TResult, TProperty> Value<TProperty>(string name,
            ValueProvider<TProperty> defaultValueProvider, Option<Func<TResult, TProperty>> valueGetter = default)
        {
            var propertyConfigurator =
                new ValueConfigurator<TResult, TProperty>(name, valueGetter, defaultValueProvider);

            _validators.Add(propertyConfigurator);
            return propertyConfigurator;
        }
        
        protected TSelf SetValue<TProperty>(Value<TProperty> value, ValueProvider<TProperty> valueProvider)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (valueProvider == null)
                throw new ArgumentNullException(nameof(valueProvider));

            value.SetValue(valueProvider);
            return (TSelf) this;
        }

        protected TSelf NoValue<TProperty>(Value<TProperty> value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            value.NoValue();
            return (TSelf) this;
        }
    }
}
