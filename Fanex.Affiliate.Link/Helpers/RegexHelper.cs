using System.Text.RegularExpressions;

namespace Fanex.Affiliate.Link.Helpers
{
    public static class RegexHelper
    {
        private const int TimeOutSecond = 2;

        public static bool IsMatch(string input, string pattern, RegexOptions regexOptions)
        {
            return Regex.IsMatch(input, pattern, regexOptions, TimeSpan.FromSeconds(TimeOutSecond));
        }

        public static Match Match(string input, string pattern)
        {
            return Regex.Match(input, pattern, RegexOptions.None, TimeSpan.FromSeconds(TimeOutSecond));
        }
    }
}