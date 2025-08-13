namespace Fanex.Affiliate.Link.Extensions
{
    public static class StringExtensions
    {
        public static bool MatchWith(this string value, string strToMatch)
        {
            return string.Equals(value, strToMatch, StringComparison.OrdinalIgnoreCase);
        }

        public static string ReplaceStr(this string orginalValue, string oldValue, string newValue, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return orginalValue.Replace(oldValue, newValue, comparison);
        }
    }
}
