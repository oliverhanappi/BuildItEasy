using System;
using LanguageExt;

namespace BuildItEasy
{
    public class ChildBuilderConfigurator<TParent, TParentBuilder, TChild, TChildBuilder, TResult>
        where TParentBuilder : Builder<TParent>
        where TChildBuilder : Builder<TChild>
        where TResult : ChildBuilder<TParent, TParentBuilder, TChild, TChildBuilder>
    {
        public static implicit operator TResult(
            ChildBuilderConfigurator<TParent, TParentBuilder, TChild, TChildBuilder, TResult> configurator)
            => configurator.Builder;

        private readonly string _name;
        private readonly Func<DefaultValuePreference, TResult> _builderFactory;

        private readonly Lazy<TResult> _builder;
        
        private Option<DefaultValuePreference> _defaultValuePreference;
        private Option<bool> _required;

        public TResult Builder => _builder.Value;

        public ChildBuilderConfigurator(string name, Func<DefaultValuePreference, TResult> builderFactory)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
            _builder = new Lazy<TResult>(Build);
        }

        public ChildBuilderConfigurator<TParent, TParentBuilder, TChild, TChildBuilder, TResult> Required()
        {
            AssertNotBuilt();

            _required.IfSome(required =>
            {
                if (!required)
                    throw new InvalidOperationException($"Child {_name} must not be required.");
            });

            _required = true;

            return this;
        }

        public ChildBuilderConfigurator<TParent, TParentBuilder, TChild, TChildBuilder, TResult> OnlyIfNecessary()
        {
            AssertNotBuilt();

            _required.IfSome(required =>
            {
                if (required)
                    throw new InvalidOperationException($"Child {_name} is required.");
            });

            _defaultValuePreference.IfSome(defaultValuePreference =>
            {
                if (defaultValuePreference != DefaultValuePreference.NoValue)
                    throw new InvalidOperationException($"Child {_name} has default value preference {defaultValuePreference}.");
            });

            _required = false;
            _defaultValuePreference = DefaultValuePreference.NoValue;

            return this;
        }

        private void AssertNotBuilt()
        {
            if (_builder.IsValueCreated)
                throw new InvalidOperationException($"Child builder {_name} has already been built.");
        }

        private TResult Build()
        {
            var defaultValuePreference = _defaultValuePreference.IfNone(DefaultValuePreference.Value);
            var required = _required.IfNone(false);

            var childBuilder = _builderFactory.Invoke(defaultValuePreference);
            
            if (required)
                childBuilder.EnsureChild();

            return childBuilder;
        }
    }
}
