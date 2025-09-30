namespace Fanex.Affiliate.Link.Constants
{
    public static class Language
    {
        private const string ENGLISH_SHORTED_CODE = "en";
        public const string BENGALI_SHORTED_CODE = "bd";
        private const string INDIA_SHORTED_CODE = "te";
        private const string CHINA_SHORTED_CODE = "cs";

        public static readonly IReadOnlyDictionary<string, string> SupportedLanguageCodes = new Dictionary<string, string>
        {
            ["en"] = ENGLISH_SHORTED_CODE,
            ["mycs"] = CHINA_SHORTED_CODE,
            ["bd"] = BENGALI_SHORTED_CODE,
            ["bnen"] = ENGLISH_SHORTED_CODE,
            ["hien"] = ENGLISH_SHORTED_CODE,
            ["hitg"] = INDIA_SHORTED_CODE,
            ["id"] = "id",
            ["iden"] = ENGLISH_SHORTED_CODE,
            ["jp"] = ENGLISH_SHORTED_CODE,
            ["jpen"] = ENGLISH_SHORTED_CODE,
            ["ko"] = "ko",
            ["kren"] = ENGLISH_SHORTED_CODE,
            ["th"] = "th",
            ["then"] = ENGLISH_SHORTED_CODE,
            ["vn"] = "vn",
            ["vnen"] = ENGLISH_SHORTED_CODE,
            ["cs"] = CHINA_SHORTED_CODE,
            ["csen"] = ENGLISH_SHORTED_CODE,
        };

        public static readonly IReadOnlyDictionary<string, string> RemovedLanguageCodes = new Dictionary<string, string>
        {
            ["bn"] = BENGALI_SHORTED_CODE,
            ["ch"] = ENGLISH_SHORTED_CODE,
            ["kh"] = ENGLISH_SHORTED_CODE,
            ["eneuro"] = ENGLISH_SHORTED_CODE,
        };
    }
}