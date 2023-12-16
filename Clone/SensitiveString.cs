namespace Clone
{
    public class SensitiveString : ISensitiveValue
    {
        private readonly string _value;

        public SensitiveString(string value)
        {
            _value = value;
        }

        public string GetValue()
        {
            return _value;
        }

        public object ToMaskedValue() => "***sensitive***";

        public override string ToString() => ToMaskedValue()?.ToString() ?? "";

    }
}
