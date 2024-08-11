namespace MeowFlix.Naming
{
    public readonly struct CleanDateTimeResult
    {
        public CleanDateTimeResult(string name, int? year = null)
        {
            Name = name;
            Year = year;
        }

        public string Name { get; }

        public int? Year { get; }
    }
}
