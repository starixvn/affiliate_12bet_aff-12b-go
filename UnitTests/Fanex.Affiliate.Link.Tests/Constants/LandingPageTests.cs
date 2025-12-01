using Fanex.Affiliate.Link.Constants;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Constants;

public class LandingPageTests
{
    [Fact]
    public void SupportedPageCodeOnV1_ShouldContainExpectedPages()
    {
        // Act
        var supportedPages = LandingPage.SupportedPageCodeOnV1;

        // Assert
        supportedPages.Should().ContainKeys(
            "signup", "home", "12sports/cricket", "exchange", "sportsbook",
            "promotion", "livecasino", "casino", "playtech", "numbergame",
            "keno", "lottery", "esports", "arcade", "p2p", "12sports/virtualsports",
            "mktsignup", "inrsignup"
        );
        supportedPages.Should().HaveCount(18);
    }

    [Theory]
    [InlineData("signup", "join")]
    [InlineData("mktsignup", "join")]
    [InlineData("inrsignup", "join")]
    [InlineData("home", "home")]
    [InlineData("12sports/cricket", "home")]
    [InlineData("exchange", "home")]
    [InlineData("12sports/virtualsports", "home")]
    [InlineData("sportsbook", "sports")]
    [InlineData("promotion", "promotion")]
    [InlineData("livecasino", "live-casino")]
    [InlineData("casino", "slots")]
    [InlineData("playtech", "slots")]
    [InlineData("numbergame", "numberGame")]
    [InlineData("keno", "keno")]
    [InlineData("lottery", "lottery")]
    [InlineData("esports", "eSport")]
    [InlineData("arcade", "game")]
    [InlineData("p2p", "game")]
    public void SupportedPageCodeOnV1_ShouldMapCorrectly(string pageCode, string expectedMapping)
    {
        // Act
        var result = LandingPage.SupportedPageCodeOnV1[pageCode];

        // Assert
        result.Should().Be(expectedMapping);
    }

    [Fact]
    public void SupportedPageCodeOnV1_ShouldBeReadOnly()
    {
        // Act & Assert
        LandingPage.SupportedPageCodeOnV1.Should().BeAssignableTo<IReadOnlyDictionary<string, string>>();
    }

    [Fact]
    public void SignupPages_ShouldMapToJoin()
    {
        // Act
        var signupPages = new[] { "signup", "mktsignup", "inrsignup" };

        // Assert
        foreach (var page in signupPages)
        {
            LandingPage.SupportedPageCodeOnV1[page].Should().Be("join", $"Page '{page}' should map to 'join'");
        }
    }

    [Fact]
    public void HomePages_ShouldMapToHome()
    {
        // Act
        var homePages = new[] { "home", "12sports/cricket", "exchange", "12sports/virtualsports" };

        // Assert
        foreach (var page in homePages)
        {
            LandingPage.SupportedPageCodeOnV1[page].Should().Be("home", $"Page '{page}' should map to 'home'");
        }
    }

    [Fact]
    public void CasinoPages_ShouldMapToSlots()
    {
        // Act
        var casinoPages = new[] { "casino", "playtech" };

        // Assert
        foreach (var page in casinoPages)
        {
            LandingPage.SupportedPageCodeOnV1[page].Should().Be("slots", $"Page '{page}' should map to 'slots'");
        }
    }

    [Fact]
    public void GamePages_ShouldMapToGame()
    {
        // Act
        var gamePages = new[] { "arcade", "p2p" };

        // Assert
        foreach (var page in gamePages)
        {
            LandingPage.SupportedPageCodeOnV1[page].Should().Be("game", $"Page '{page}' should map to 'game'");
        }
    }

    [Fact]
    public void JoinPageCode_ShouldBePublicConstant()
    {
        // Act & Assert
        LandingPage.JOIN_PAGE_CODE.Should().Be("join");
    }

    [Fact]
    public void SportsPages_ShouldIncludeSportsbook()
    {
        // Act
        var sportsPages = LandingPage.SupportedPageCodeOnV1
            .Where(kvp => kvp.Value == "sports")
            .Select(kvp => kvp.Key);

        // Assert
        sportsPages.Should().Contain("sportsbook");
    }

    [Fact]
    public void MarketingSignupPages_ShouldHaveComments()
    {
        // Act & Assert
        // These page codes should exist as they are marketing-specific
        LandingPage.SupportedPageCodeOnV1.Should().ContainKey("mktsignup");
        LandingPage.SupportedPageCodeOnV1.Should().ContainKey("inrsignup");

        // Both should map to join
        LandingPage.SupportedPageCodeOnV1["mktsignup"].Should().Be("join");
        LandingPage.SupportedPageCodeOnV1["inrsignup"].Should().Be("join");
    }

    [Fact]
    public void SportsRelatedPages_ShouldExist()
    {
        // Act
        var sportsRelatedPages = new[] { "12sports/cricket", "12sports/virtualsports", "sportsbook", "esports" };

        // Assert
        foreach (var page in sportsRelatedPages)
        {
            LandingPage.SupportedPageCodeOnV1.Should().ContainKey(page, $"Sports-related page '{page}' should be supported");
        }
    }

    [Fact]
    public void CasinoRelatedPages_ShouldExist()
    {
        // Act
        var casinoRelatedPages = new[] { "livecasino", "casino", "playtech" };

        // Assert
        foreach (var page in casinoRelatedPages)
        {
            LandingPage.SupportedPageCodeOnV1.Should().ContainKey(page, $"Casino-related page '{page}' should be supported");
        }
    }

    [Fact]
    public void GamblingRelatedPages_ShouldExist()
    {
        // Act
        var gamblingRelatedPages = new[] { "numbergame", "keno", "lottery" };

        // Assert
        foreach (var page in gamblingRelatedPages)
        {
            LandingPage.SupportedPageCodeOnV1.Should().ContainKey(page, $"Gambling-related page '{page}' should be supported");
        }
    }

    [Fact]
    public void AllPageCodes_ShouldMapToValidTargets()
    {
        // Arrange
        var validTargets = new[] { "join", "home", "sports", "promotion", "live-casino", "slots", "numberGame", "keno", "lottery", "eSport", "game" };

        // Act & Assert
        foreach (var mapping in LandingPage.SupportedPageCodeOnV1.Values)
        {
            validTargets.Should().Contain(mapping, $"Page mapping '{mapping}' should be a valid target");
        }
    }

    [Fact]
    public void PageCodeKeys_ShouldNotContainSpecialCharacters()
    {
        // Act
        var invalidCharacters = new[] { " ", "@", "#", "$", "%", "^", "&", "*", "(", ")", "+", "=", "[", "]", "{", "}", "\\", "|", ";", ":", "\"", "'", "?", ".", "," };

        // Assert
        foreach (var pageCode in LandingPage.SupportedPageCodeOnV1.Keys)
        {
            foreach (var invalidChar in invalidCharacters)
            {
                if (invalidChar != "/" && invalidChar != "-") // Allow slash and dash
                {
                    pageCode.Should().NotContain(invalidChar, $"Page code '{pageCode}' should not contain '{invalidChar}'");
                }
            }
        }
    }
}
