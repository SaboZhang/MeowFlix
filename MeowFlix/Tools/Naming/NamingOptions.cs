using System.Text.RegularExpressions;

namespace MeowFlix.Naming;

public class NamingOptions
{
    public NamingOptions()
    {
        VideoFlagDelimiters =
            [
                '(',
                ')',
                '-',
                '.',
                '_',
                '[',
                ']'
            ];
        CleanDateTimes =
            [
                @"(.+[^_\,\.\(\)\[\]\-])[_\.\(\)\[\]\-](19[0-9]{2}|20[0-9]{2})(?![0-9]+|\W[0-9]{2}\W[0-9]{2})([ _\,\.\(\)\[\]\-][^0-9]|).*(19[0-9]{2}|20[0-9]{2})*",
                @"(.+[^_\,\.\(\)\[\]\-])[ _\.\(\)\[\]\-]+(19[0-9]{2}|20[0-9]{2})(?![0-9]+|\W[0-9]{2}\W[0-9]{2})([ _\,\.\(\)\[\]\-][^0-9]|).*(19[0-9]{2}|20[0-9]{2})*"
            ];

        CleanStrings =
            [
                @"^\s*(?<cleaned>.+?)[ _\,\.\(\)\[\]\-](3d|sbs|tab|hsbs|htab|mvc|HDR|HDC|UHD|UltraHD|4k|ac3|dts|custom|dc|divx|divx5|dsr|dsrip|dutch|dvd|dvdrip|dvdscr|dvdscreener|screener|dvdivx|cam|fragment|fs|hdtv|hdrip|hdtvrip|internal|limited|multi|subs|ntsc|ogg|ogm|pal|pdtv|proper|repack|rerip|retail|cd[1-9]|r5|bd5|bd|se|svcd|swedish|german|read.nfo|nfofix|unrated|ws|telesync|ts|telecine|tc|brrip|4KH265版|V2版|bdrip|480p|480i|576p|576i|720p|720i|1080p|1080i|2160p|hrhd|hrhdtv|hddvd|bluray|blu-ray|x264|x265|h264|h265|xvid|xvidvd|xxx|www.www|AAC|DTS|\[.*\])([ _\,\.\(\)\[\]\-]|$)",
                @"^(?<cleaned>.+?)(\[.*\])",
                @"^\s*(?<cleaned>.+?)\WE[0-9]+(-|~)E?[0-9]+(\W|$)",
                @"^\s*\[[^\]]+\](?!\.\w+$)\s*(?<cleaned>.+)",
                @"^\s*(?<cleaned>.+?)\s+-\s+[0-9]+\s*$",
                @"^\s*(?<cleaned>.+?)(([-._ ](trailer|sample))|-(scene|clip|behindthescenes|deleted|deletedscene|featurette|short|interview|other|extra))$"
            ];

        EpisodeExpressions = new[]
            {
                // *** Begin Kodi Standard Naming
                // <!-- foo.s01.e01, foo.s01_e01, S01E02 foo, S01 - E02 -->
                new EpisodeExpression(@".*(\\|\/)(?<seriesname>((?![Ss]([0-9]+)[][ ._-]*[Ee]([0-9]+))[^\\\/])*)?[Ss](?<seasonnumber>[0-9]+)[][ ._-]*[Ee](?<epnumber>[0-9]+)([^\\/]*)$")
                {
                    IsNamed = true
                },
                // <!-- foo.ep01, foo.EP_01 -->
                new EpisodeExpression(@"[\._ -]()[Ee][Pp]_?([0-9]+)([^\\/]*)$"),
                // <!-- foo.E01., foo.e01. -->
                new EpisodeExpression(@"[^\\/]*?()\.?[Ee]([0-9]+)\.([^\\/]*)$"),
                new EpisodeExpression("(?<year>[0-9]{4})[._ -](?<month>[0-9]{2})[._ -](?<day>[0-9]{2})", true)
                {
                    DateTimeFormats = new[]
                    {
                        "yyyy.MM.dd",
                        "yyyy-MM-dd",
                        "yyyy_MM_dd",
                        "yyyy MM dd"
                    }
                },
                new EpisodeExpression("(?<day>[0-9]{2})[._ -](?<month>[0-9]{2})[._ -](?<year>[0-9]{4})", true)
                {
                    DateTimeFormats = new[]
                    {
                        "dd.MM.yyyy",
                        "dd-MM-yyyy",
                        "dd_MM_yyyy",
                        "dd MM yyyy"
                    }
                },

                // This isn't a Kodi naming rule, but the expression below causes false episode numbers for
                // Title Season X Episode X naming schemes.
                // "Series Season X Episode X - Title.avi", "Series S03 E09.avi", "s3 e9 - Title.avi"
                new EpisodeExpression(@".*[\\\/]((?<seriesname>[^\\/]+?)\s)?[Ss](?:eason)?\s*(?<seasonnumber>[0-9]+)\s+[Ee](?:pisode)?\s*(?<epnumber>[0-9]+).*$")
                {
                    IsNamed = true
                },

                // Not a Kodi rule as well, but the expression below also causes false positives,
                // so we make sure this one gets tested first.
                // "Foo Bar 889"
                new EpisodeExpression(@".*[\\\/](?![Ee]pisode)(?<seriesname>[\w\s]+?)\s(?<epnumber>[0-9]{1,4})(-(?<endingepnumber>[0-9]{2,4}))*[^\\\/x]*$")
                {
                    IsNamed = true
                },

                new EpisodeExpression(@"[\\\/\._ \[\(-]([0-9]+)x([0-9]+(?:(?:[a-i]|\.[1-9])(?![0-9]))?)([^\\\/]*)$")
                {
                    SupportsAbsoluteEpisodeNumbers = true
                },

                // Not a Kodi rule as well, but below rule also causes false positives for triple-digit episode names
                // [bar] Foo - 1 [baz] special case of below expression to prevent false positives with digits in the series name
                new EpisodeExpression(@".*[\\\/]?.*?(\[.*?\])+.*?(?<seriesname>[-\w\s]+?)[\s_]*-[\s_]*(?<epnumber>[0-9]+).*$")
                {
                    IsNamed = true
                },

                // /server/anything_102.mp4
                // /server/james.corden.2017.04.20.anne.hathaway.720p.hdtv.x264-crooks.mkv
                // /server/anything_1996.11.14.mp4
                new EpisodeExpression(@"[\\/._ -](?<seriesname>(?![0-9]+[0-9][0-9])([^\\\/_])*)[\\\/._ -](?<seasonnumber>[0-9]+)(?<epnumber>[0-9][0-9](?:(?:[a-i]|\.[1-9])(?![0-9]))?)([._ -][^\\\/]*)$")
                {
                    IsOptimistic = true,
                    IsNamed = true,
                    SupportsAbsoluteEpisodeNumbers = false
                },
                new EpisodeExpression(@"[\/._ -]p(?:ar)?t[_. -]()([ivx]+|[0-9]+)([._ -][^\/]*)$")
                {
                    SupportsAbsoluteEpisodeNumbers = true
                },

                // *** End Kodi Standard Naming

                // "Episode 16", "Episode 16 - Title"
                new EpisodeExpression(@"[Ee]pisode (?<epnumber>[0-9]+)(-(?<endingepnumber>[0-9]+))?[^\\\/]*$")
                {
                    IsNamed = true
                },

                new EpisodeExpression(@".*(\\|\/)[sS]?(?<seasonnumber>[0-9]+)[xX](?<epnumber>[0-9]+)[^\\\/]*$")
                {
                    IsNamed = true
                },

                new EpisodeExpression(@".*(\\|\/)[sS](?<seasonnumber>[0-9]+)[x,X]?[eE](?<epnumber>[0-9]+)[^\\\/]*$")
                {
                    IsNamed = true
                },

                new EpisodeExpression(@".*(\\|\/)(?<seriesname>((?![sS]?[0-9]{1,4}[xX][0-9]{1,3})[^\\\/])*)?([sS]?(?<seasonnumber>[0-9]{1,4})[xX](?<epnumber>[0-9]+))[^\\\/]*$")
                {
                    IsNamed = true
                },

                new EpisodeExpression(@".*(\\|\/)(?<seriesname>[^\\\/]*)[sS](?<seasonnumber>[0-9]{1,4})[xX\.]?[eE](?<epnumber>[0-9]+)[^\\\/]*$")
                {
                    IsNamed = true
                },

                // "01.avi"
                new EpisodeExpression(@".*[\\\/](?<epnumber>[0-9]+)(-(?<endingepnumber>[0-9]+))*\.\w+$")
                {
                    IsOptimistic = true,
                    IsNamed = true
                },

                // "1-12 episode title"
                new EpisodeExpression("([0-9]+)-([0-9]+)"),

                // "01 - blah.avi", "01-blah.avi"
                new EpisodeExpression(@".*(\\|\/)(?<epnumber>[0-9]{1,3})(-(?<endingepnumber>[0-9]{2,3}))*\s?-\s?[^\\\/]*$")
                {
                    IsOptimistic = true,
                    IsNamed = true
                },

                // "01.blah.avi"
                new EpisodeExpression(@".*(\\|\/)(?<epnumber>[0-9]{1,3})(-(?<endingepnumber>[0-9]{2,3}))*\.[^\\\/]+$")
                {
                    IsOptimistic = true,
                    IsNamed = true
                },

                // "blah - 01.avi", "blah 2 - 01.avi", "blah - 01 blah.avi", "blah 2 - 01 blah", "blah - 01 - blah.avi", "blah 2 - 01 - blah"
                new EpisodeExpression(@".*[\\\/][^\\\/]* - (?<epnumber>[0-9]{1,3})(-(?<endingepnumber>[0-9]{2,3}))*[^\\\/]*$")
                {
                    IsOptimistic = true,
                    IsNamed = true
                },

                // "01 episode title.avi"
                new EpisodeExpression(@"[Ss]eason[\._ ](?<seasonnumber>[0-9]+)[\\\/](?<epnumber>[0-9]{1,3})([^\\\/]*)$")
                {
                    IsOptimistic = true,
                    IsNamed = true
                },

                // Series and season only expression
                // "the show/season 1", "the show/s01"
                new EpisodeExpression(@"(.*(\\|\/))*(?<seriesname>.+)\/[Ss](eason)?[\. _\-]*(?<seasonnumber>[0-9]+)")
                {
                    IsNamed = true
                },

                // Series and season only expression
                // "the show S01", "the show season 1"
                new EpisodeExpression(@"(.*(\\|\/))*(?<seriesname>.+)[\. _\-]+[sS](eason)?[\. _\-]*(?<seasonnumber>[0-9]+)")
                {
                    IsNamed = true
                },
            };

        MultipleEpisodeExpressions = new[]
            {
                @".*(\\|\/)[sS]?(?<seasonnumber>[0-9]{1,4})[xX](?<epnumber>[0-9]{1,3})((-| - )[0-9]{1,4}[eExX](?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$",
                @".*(\\|\/)[sS]?(?<seasonnumber>[0-9]{1,4})[xX](?<epnumber>[0-9]{1,3})((-| - )[0-9]{1,4}[xX][eE](?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$",
                @".*(\\|\/)[sS]?(?<seasonnumber>[0-9]{1,4})[xX](?<epnumber>[0-9]{1,3})((-| - )?[xXeE](?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$",
                @".*(\\|\/)[sS]?(?<seasonnumber>[0-9]{1,4})[xX](?<epnumber>[0-9]{1,3})(-[xE]?[eE]?(?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$",
                @".*(\\|\/)(?<seriesname>((?![sS]?[0-9]{1,4}[xX][0-9]{1,3})[^\\\/])*)?([sS]?(?<seasonnumber>[0-9]{1,4})[xX](?<epnumber>[0-9]{1,3}))((-| - )[0-9]{1,4}[xXeE](?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$",
                @".*(\\|\/)(?<seriesname>((?![sS]?[0-9]{1,4}[xX][0-9]{1,3})[^\\\/])*)?([sS]?(?<seasonnumber>[0-9]{1,4})[xX](?<epnumber>[0-9]{1,3}))((-| - )[0-9]{1,4}[xX][eE](?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$",
                @".*(\\|\/)(?<seriesname>((?![sS]?[0-9]{1,4}[xX][0-9]{1,3})[^\\\/])*)?([sS]?(?<seasonnumber>[0-9]{1,4})[xX](?<epnumber>[0-9]{1,3}))((-| - )?[xXeE](?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$",
                @".*(\\|\/)(?<seriesname>((?![sS]?[0-9]{1,4}[xX][0-9]{1,3})[^\\\/])*)?([sS]?(?<seasonnumber>[0-9]{1,4})[xX](?<epnumber>[0-9]{1,3}))(-[xX]?[eE]?(?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$",
                @".*(\\|\/)(?<seriesname>[^\\\/]*)[sS](?<seasonnumber>[0-9]{1,4})[xX\.]?[eE](?<epnumber>[0-9]{1,3})((-| - )?[xXeE](?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$",
                @".*(\\|\/)(?<seriesname>[^\\\/]*)[sS](?<seasonnumber>[0-9]{1,4})[xX\.]?[eE](?<epnumber>[0-9]{1,3})(-[xX]?[eE]?(?<endingepnumber>[0-9]{1,3}))+[^\\\/]*$"
            }.Select(i => new EpisodeExpression(i)
            {
                IsNamed = true
            }).ToArray();

        Compile();
    }

    public char[] VideoFlagDelimiters { get; set; }

    public string[] SubtitleFileExtensions { get; set; }

    public string[] CleanDateTimes { get; set; }

    public string[] CleanStrings { get; set; }

    public EpisodeExpression[] EpisodeExpressions { get; set; }

    public EpisodeExpression[] MultipleEpisodeExpressions { get; set; }

    public Regex[] CleanStringRegexes { get; private set; } = Array.Empty<Regex>();

    public Regex[] CleanDateTimeRegexes { get; private set; } = Array.Empty<Regex>();

    public void Compile()
    {
        CleanDateTimeRegexes = CleanDateTimes.Select(Compile).ToArray();
        CleanStringRegexes = CleanStrings.Select(Compile).ToArray();
    }

    private Regex Compile(string exp)
    {
        return new Regex(exp, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
