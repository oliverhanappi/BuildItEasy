using System.Collections.Generic;
using System.Linq;
using BuildItEasy.Validation;

namespace BuildItEasy
{
    public abstract partial class Builder<TResult, TSelf> : IBuilder<TResult>
        where TSelf : Builder<TResult, TSelf>
    {
        public static implicit operator ValueProvider<TResult>(Builder<TResult, TSelf> builder)
            => new BuilderValueProvider<TResult>(builder);
        
        private readonly ICollection<IValidator<TResult>> _validators;
        private readonly ICollection<IResettable> _resettables;

        protected Builder()
        {
            _validators = new List<IValidator<TResult>>();
            _resettables = new List<IResettable>();
        }

        public TResult Build()
        {
            foreach (var resettable in _resettables) 
                resettable.Reset();

            var result = BuildInternal();

            var validationResult = ValidationResult.Merge(_validators.Select(v => v.Validate(result)));
            validationResult.AssertValid();

            return result;
        }

        protected abstract TResult BuildInternal();

        public TSelf Customize(Customizer<TSelf> customizer = null)
        {
            customizer?.Invoke((TSelf) this);
            return (TSelf) this;
        }
    }
}
