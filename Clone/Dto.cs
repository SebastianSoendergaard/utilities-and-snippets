namespace Clone
{
    internal class Dto
    {
        public int Number { get; set; }
        public string Text { get; set; } = default!;
        [Mask]
        public SensitiveString SecretText { get; set; } = default!;
        public DateTimeOffset DateTime { get; set; }
        public SubDtoA SubDtoA { get; set; } = default!;
        public SubDtoB SubDtoB { get; set; } = default!;
        public IList<SubDtoA> SubList { get; set; } = new List<SubDtoA>();
        public IDictionary<string, SubDtoB> SubDic { get; set; } = new Dictionary<string, SubDtoB>();
    }

    internal class SubDtoA
    {
        [Mask]
        public SensitiveInt N { get; set; }
        public string T { get; set; } = default!;
    }

    internal record SubDtoB(int N, [Mask] SensitiveString T);
}
