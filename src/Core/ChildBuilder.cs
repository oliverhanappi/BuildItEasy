using System;
using System.Collections.Generic;
using LanguageExt;
using static LanguageExt.Prelude;

namespace BuildItEasy
{
    public class ChildBuilder<TParent, TParentBuilder, TChild, TChildBuilder>
        where TParentBuilder : Builder<TParent>
        where TChildBuilder : Builder<TChild>
    {
        private readonly Func<TParent, TParentBuilder, TChildBuilder> _builderFactory;
        private readonly List<Action<TParent, TParentBuilder, TChildBuilder>> _customizations;
        
        public ValueState State { get; private set; }
        public DefaultValuePreference DefaultValuePreference { get; }

        public ChildBuilder(Func<TParent, TParentBuilder, TChildBuilder> builderFactory, DefaultValuePreference defaultValuePreference)
        {
            _builderFactory = builderFactory ?? throw new ArgumentNullException(nameof(builderFactory));
            _customizations = new List<Action<TParent, TParentBuilder, TChildBuilder>>();

            State = ValueState.Default;
            DefaultValuePreference = defaultValuePreference;
        }

        public void NoChild()
        {
            if (State == ValueState.ValueRequired)
                throw new InvalidOperationException("The child builder requires a value.");

            State = ValueState.ValueForbidden;
        }

        public void EnsureChild()
        {
            if (State == ValueState.ValueForbidden)
                throw new InvalidOperationException("The child builder must not have a value.");

            State = ValueState.ValueRequired;
        }

        public void Customize(Action<TParent, TParentBuilder, TChildBuilder> customize)
        {
            if (customize == null)
                throw new ArgumentNullException(nameof(customize));
            
            EnsureChild();
            _customizations.Add(customize);
        }

        public Option<TChild> BuildChild(TParent parentEntity, TParentBuilder parentBuilder)
        {
            if (State == ValueState.ValueForbidden || (State == ValueState.Default && DefaultValuePreference == DefaultValuePreference.NoValue))
                return None;

            if (State == ValueState.ValueRequired || (State == ValueState.Default && DefaultValuePreference == DefaultValuePreference.Value))
            {
                var childBuilder = _builderFactory.Invoke(parentEntity, parentBuilder);
                
                foreach (var customization in _customizations) 
                    customization.Invoke(parentEntity, parentBuilder, childBuilder);

                var childEntity = childBuilder.Build();
                return childEntity;
            }

            throw new InvalidOperationException($"Unreachable state: {State} {DefaultValuePreference}");
        }
    }
}
