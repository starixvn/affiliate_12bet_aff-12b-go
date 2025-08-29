namespace Fanex.Affiliate.Link.Constants
{
    public static class LandingPage
    {
        private const string HOME_PAGE_CODE = "home";
        public const string JOIN_PAGE_CODE = "join";

        public static readonly IReadOnlyDictionary<string, string> SupportedPageCodeOnV1 = new Dictionary<string, string>
        {
            ["signup"] = JOIN_PAGE_CODE,
            ["home"] = HOME_PAGE_CODE,
            ["12sports/cricket"] = HOME_PAGE_CODE,
            ["exchange"] = HOME_PAGE_CODE,
            ["sportsbook"] = "sports",
            ["promotion"] = "promotion",
            ["livecasino"] = "live-casino",
            ["casino"] = "slots",
            ["playtech"] = "slots",
            ["numbergame"] = "numberGame",
            ["keno"] = "keno",
            ["lottery"] = "lottery",
            ["esports"] = "eSport",
            ["arcade"] = "game",
            ["p2p"] = "game",
            ["12sports/virtualsports"] = HOME_PAGE_CODE,
            ["mktsignup"] = JOIN_PAGE_CODE, //Marketing SignUp Page Code
            ["inrsignup"] = JOIN_PAGE_CODE, //Old Marketing SignUp Page Code
        };
    }
}