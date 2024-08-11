using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MeowFlix.Naming
{
    /// <summary>
    /// <see href="http://kodi.wiki/view/Advancedsettings.xml#video" />.
    /// </summary>
    public static class CleanStringParser
    {
        public static bool TryClean([NotNullWhen(true)] string? name, IReadOnlyList<Regex> expressions, out string newName)
        {
            if (string.IsNullOrEmpty(name))
            {
                newName = string.Empty;
                return false;
            }

            // Iteratively apply the regexps to clean the string.
            bool cleaned = false;
            for (int i = 0; i < expressions.Count; i++)
            {
                if (TryClean(name, expressions[i], out newName))
                {
                    cleaned = true;
                    name = newName;
                }
            }

            newName = cleaned ? name : string.Empty;
            return cleaned;
        }

        private static bool TryClean(string name, Regex expression, out string newName)
        {
            var match = expression.Match(name);
            if (match.Success && match.Groups.TryGetValue("cleaned", out var cleaned))
            {
                newName = cleaned.Value;
                return true;
            }

            newName = string.Empty;
            return false;
        }
    }
}
