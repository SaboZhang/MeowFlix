using System.Text.RegularExpressions;

namespace MeowFlix.Naming
{
    public static partial class SeriesResolver
    {
        [GeneratedRegex(@"((?<a>[^\._]{2,})[\._]*)|([\._](?<b>[^\._]{2,}))")]
        private static partial Regex SeriesNameRegex();

        public static SeriesInfo Resolve(NamingOptions options, string path)
        {
            string seriesName = Path.GetFileName(path);

            SeriesPathParserResult result = SeriesPathParser.Parse(options, path);
            if (result.Success)
            {
                if (!string.IsNullOrEmpty(result.SeriesName))
                {
                    seriesName = result.SeriesName;
                }
            }

            if (!string.IsNullOrEmpty(seriesName))
            {
                seriesName = SeriesNameRegex().Replace(seriesName, "${a} ${b}").Trim();
            }

            return new SeriesInfo(path)
            {
                Name = seriesName
            };
        }
    }
}
