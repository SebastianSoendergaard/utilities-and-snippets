namespace Clone
{
    public class SensitiveInt : ISensitiveValue
    {
        public SensitiveInt(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public object ToMaskedValue() => 999999;

        public override string ToString() => Value.ToString();
    }
}

