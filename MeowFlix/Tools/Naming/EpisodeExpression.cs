using System.Text.RegularExpressions;

namespace MeowFlix.Naming;

public class EpisodeExpression
{
    private string _expression;
    private Regex? _regex;

    public EpisodeExpression(string expression, bool byDate = false)
    {
        _expression = expression;
        IsByDate = byDate;
        DateTimeFormats = Array.Empty<string>();
        SupportsAbsoluteEpisodeNumbers = true;
    }

    public string Expression
    {
        get => _expression;
        set
        {
            _expression = value;
            _regex = null;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets property indicating if date can be find in expression.
    /// </summary>
    public bool IsByDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets property indicating if expression is optimistic.
    /// </summary>
    public bool IsOptimistic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets property indicating if expression is named.
    /// </summary>
    public bool IsNamed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets property indicating if expression supports episodes with absolute numbers.
    /// </summary>
    public bool SupportsAbsoluteEpisodeNumbers { get; set; }

    /// <summary>
    /// Gets or sets optional list of date formats used for date parsing.
    /// </summary>
    public string[] DateTimeFormats { get; set; }

    /// <summary>
    /// Gets a <see cref="Regex"/> expressions objects (creates it if null).
    /// </summary>
    public Regex Regex => _regex ??= new Regex(Expression, RegexOptions.IgnoreCase | RegexOptions.Compiled);
}
