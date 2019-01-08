namespace BuildItEasy
{
    public class ConstantValueProvider<T> : ValueProvider<T>
    {
        public T Value { get; }

        public ConstantValueProvider(T value)
        {
            Value = value;
        }

        public override T GetValue() => Value;
    }
}