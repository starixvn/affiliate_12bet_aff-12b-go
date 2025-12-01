using Fanex.Affiliate.Link.Constants;
using FluentAssertions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Constants;

public class RouteConstraintsTests
{
    [Theory]
    [InlineData("123", true)]
    [InlineData("0", true)]
    [InlineData("999999", true)]
    [InlineData("12345", true)]
    [InlineData("abc", false)]
    [InlineData("12a3", false)]
    [InlineData("", false)]
    [InlineData("12.3", false)]
    [InlineData("-123", false)]
    [InlineData("+123", false)]
    public void NumericRegexPattern_ShouldValidateCorrectly(string input, bool expectedResult)
    {
        // Arrange
        var regex = new Regex(RouteConstraints.NumericRegexPattern);

        // Act
        var result = regex.IsMatch(input);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("home", true)]
    [InlineData("sports", true)]
    [InlineData("live-casino", true)]
    [InlineData("sports_betting", true)]
    [InlineData("Casino123", true)]
    [InlineData("12bet", true)]
    [InlineData("test-page_code", true)]
    [InlineData("", false)]
    [InlineData("page code", false)] // Contains space
    [InlineData("page.code", false)] // Contains dot
    [InlineData("page@code", false)] // Contains @
    [InlineData("page#code", false)] // Contains #
    [InlineData("page$code", false)] // Contains $
    public void PageCodeRegexPattern_ShouldValidateCorrectly(string input, bool expectedResult)
    {
        // Arrange
        var regex = new Regex(RouteConstraints.PageCodeRegexPattern, RegexOptions.IgnoreCase);

        // Act
        var result = regex.IsMatch(input);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("en", true)]
    [InlineData("bd", true)]
    [InlineData("bn", true)]
    [InlineData("hien", true)]
    [InlineData("hitg", true)]
    [InlineData("id", true)]
    [InlineData("jp", true)]
    [InlineData("ko", true)]
    [InlineData("th", true)]
    [InlineData("vn", true)]
    [InlineData("cs", true)]
    [InlineData("mycs", true)]
    [InlineData("ch", true)]
    [InlineData("kh", true)]
    [InlineData("eneuro", true)]
    [InlineData("xx", false)]
    [InlineData("", false)]
    [InlineData("english", false)]
    [InlineData("EN", true)] // Case insensitive
    [InlineData("BD", true)] // Case insensitive
    public void LanguageRegexPattern_ShouldValidateCorrectly(string input, bool expectedResult)
    {
        // Arrange
        var pattern = $"^{RouteConstraints.LanguageRegexPattern}$";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        // Act
        var result = regex.IsMatch(input);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void LanguageRegexPattern_ShouldIncludeAllSupportedLanguages()
    {
        // Arrange
        var allLanguages = Language.SupportedLanguageCodes.Keys
            .Concat(Language.RemovedLanguageCodes.Keys)
            .Distinct()
            .ToList();

        var pattern = $"^{RouteConstraints.LanguageRegexPattern}$";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        // Act & Assert
        foreach (var language in allLanguages)
        {
            regex.IsMatch(language).Should().BeTrue($"Language '{language}' should be supported");
        }
    }

    [Fact]
    public void LanguageRegexPattern_ShouldPrioritizeLongerPatterns()
    {
        // Arrange
        var pattern = RouteConstraints.LanguageRegexPattern;

        // Assert
        // The pattern should have longer languages first to avoid partial matches
        // For example, "eneuro" should come before "en" to avoid "eneuro" matching as "en"
        pattern.Should().Contain("eneuro");
        pattern.Should().Contain("hien");
        pattern.Should().Contain("hitg");
        pattern.Should().Contain("mycs");

        // Verify that longer patterns appear before shorter ones in the pattern
        var enPosition = pattern.IndexOf("|en|", StringComparison.Ordinal);
        var eneuroPosition = pattern.IndexOf("eneuro", StringComparison.Ordinal);

        if (enPosition >= 0 && eneuroPosition >= 0)
        {
            eneuroPosition.Should().BeLessThan(enPosition, "eneuro should appear before en in the pattern");
        }
    }

    [Fact]
    public void LanguageRegexPattern_ShouldNotBeNullOrEmpty()
    {
        // Act
        var pattern = RouteConstraints.LanguageRegexPattern;

        // Assert
        pattern.Should().NotBeNullOrEmpty();
        pattern.Should().Contain("|"); // Should be a regex alternation pattern
        pattern.Should().StartWith("(");
        pattern.Should().EndWith(")");
    }

    [Fact]
    public void NumericRegexPattern_ShouldBeValidRegex()
    {
        // Act
        var creating = () => new Regex(RouteConstraints.NumericRegexPattern);

        // Assert
        creating.Should().NotThrow<ArgumentException>();
    }

    [Fact]
    public void PageCodeRegexPattern_ShouldBeValidRegex()
    {
        // Act
        var creating = () => new Regex(RouteConstraints.PageCodeRegexPattern);

        // Assert
        creating.Should().NotThrow<ArgumentException>();
    }
}
