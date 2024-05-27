using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowFlix.Naming;

public class SeriesPathParser
{
    public static SeriesPathParserResult Parse(NamingOptions options, string path)
    {
        SeriesPathParserResult? result = null;

        foreach (var expression in options.EpisodeExpressions)
        {
            var currentResult = Parse(path, expression);
            if (currentResult.Success)
            {
                result = currentResult;
                break;
            }
        }

        if (result is not null)
        {
            if (!string.IsNullOrEmpty(result.SeriesName))
            {
                result.SeriesName = result.SeriesName.Trim(' ', '_', '.', '-');
            }
        }

        return result ?? new SeriesPathParserResult();
    }

    private static SeriesPathParserResult Parse(string name, EpisodeExpression expression)
    {
        var result = new SeriesPathParserResult();

        var match = expression.Regex.Match(name);

        if (match.Success && match.Groups.Count >= 3)
        {
            if (expression.IsNamed)
            {
                result.SeriesName = match.Groups["seriesname"].Value;
                result.Success = !string.IsNullOrEmpty(result.SeriesName) && !match.Groups["seasonnumber"].ValueSpan.IsEmpty;
            }
        }

        return result;
    }
}
