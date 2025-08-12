using Fanex.Affiliate.Link.Constants;
using FluentAssertions;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Constants;

public class RegexPatternsTests
{
    [Theory]
    [InlineData("123", true)]
    [InlineData("0", true)]
    [InlineData("999999", true)]
    [InlineData("abc", false)]
    [InlineData("12a", false)]
    [InlineData("a123", false)]
    [InlineData("123a", false)]
    [InlineData("", false)]
    [InlineData("12.34", false)]
    [InlineData("-123", false)]
    public void Numeric_ShouldMatchCorrectPatterns(string input, bool shouldMatch)
    {
        // Arrange
        var regex = RegexPatterns.Numeric();

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        isMatch.Should().Be(shouldMatch);
    }

    [Theory]
    [InlineData("page123", true)]
    [InlineData("page-123", true)]
    [InlineData("page_123", true)]
    [InlineData("PAGE123", true)]
    [InlineData("page 123", false)]
    [InlineData("page@123", false)]
    [InlineData("page#123", false)]
    [InlineData("page$123", false)]
    [InlineData("page/123", false)] // Forward slash not allowed in actual pattern
    [InlineData("", false)]
    [InlineData("page.123", false)]
    public void PageCode_ShouldMatchCorrectPatterns(string input, bool shouldMatch)
    {
        // Arrange
        var regex = RegexPatterns.PageCode();

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        isMatch.Should().Be(shouldMatch);
    }

    [Theory]
    [InlineData("en", true)]
    [InlineData("EN", true)]
    [InlineData("bd", true)]
    [InlineData("BD", true)]
    [InlineData("hien", true)]
    [InlineData("HIEN", true)]
    [InlineData("hitg", true)]
    [InlineData("HITG", true)]
    [InlineData("id", true)]
    [InlineData("ID", true)]
    [InlineData("jp", true)]
    [InlineData("JP", true)]
    [InlineData("ko", true)]
    [InlineData("KO", true)]
    [InlineData("th", true)]
    [InlineData("TH", true)]
    [InlineData("vn", true)]
    [InlineData("VN", true)]
    [InlineData("cs", true)]
    [InlineData("CS", true)]
    [InlineData("mycs", true)]
    [InlineData("MYCS", true)]
    [InlineData("bn", true)] // From RemovedLanguageCodes
    [InlineData("BN", true)]
    [InlineData("ch", true)] // From RemovedLanguageCodes
    [InlineData("CH", true)]
    [InlineData("kh", true)] // From RemovedLanguageCodes
    [InlineData("KH", true)]
    [InlineData("eneuro", true)] // From RemovedLanguageCodes
    [InlineData("ENEURO", true)]
    [InlineData("XX", false)] // Invalid language code
    [InlineData("", false)] // Empty string
    [InlineData("A", false)] // Single character
    [InlineData("ABC", false)] // Three characters
    [InlineData("vi", false)] // Not in supported codes
    [InlineData("VI", false)]
    [InlineData("cn", false)] // Not in supported codes
    [InlineData("CN", false)]
    [InlineData("my", false)] // Not in supported codes
    [InlineData("MY", false)]
    [InlineData("sg", false)] // Not in supported codes
    [InlineData("SG", false)]
    [InlineData("ph", false)] // Not in supported codes
    [InlineData("PH", false)]
    [InlineData("in", false)] // Not in supported codes
    [InlineData("IN", false)]
    [InlineData("pk", false)] // Not in supported codes
    [InlineData("PK", false)]
    [InlineData("lk", false)] // Not in supported codes
    [InlineData("LK", false)]
    [InlineData("np", false)] // Not in supported codes
    [InlineData("NP", false)]
    [InlineData("mm", false)] // Not in supported codes
    [InlineData("MM", false)]
    [InlineData("la", false)] // Not in supported codes
    [InlineData("LA", false)]
    [InlineData("tl", false)] // Not in supported codes
    [InlineData("TL", false)]
    public void Language_ShouldMatchCorrectPatterns(string input, bool shouldMatch)
    {
        // Act
        var isMatch = RegexPatterns.Language.IsMatch(input);

        // Assert
        isMatch.Should().Be(shouldMatch);
    }

    [Fact]
    public void Numeric_ShouldBeCompiledRegex()
    {
        // Act
        var regex = RegexPatterns.Numeric();

        // Assert
        regex.Options.Should().HaveFlag(RegexOptions.Compiled);
    }

    [Fact]
    public void PageCode_ShouldBeCompiledAndIgnoreCaseRegex()
    {
        // Act
        var regex = RegexPatterns.PageCode();

        // Assert
        regex.Options.Should().HaveFlag(RegexOptions.Compiled);
        regex.Options.Should().HaveFlag(RegexOptions.IgnoreCase);
    }

    [Fact]
    public void Language_ShouldBeCompiledAndIgnoreCaseRegex()
    {
        // Act
        var regex = RegexPatterns.Language;

        // Assert
        regex.Options.Should().HaveFlag(RegexOptions.Compiled);
        regex.Options.Should().HaveFlag(RegexOptions.IgnoreCase);
    }

    [Fact]
    public void Language_ShouldHaveTimeout()
    {
        // Act
        var regex = RegexPatterns.Language;

        // Assert
        regex.MatchTimeout.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("456", "456")]
    [InlineData("0", "0")]
    public void Numeric_ShouldExtractFullMatch(string input, string expectedMatch)
    {
        // Arrange
        var regex = RegexPatterns.Numeric();

        // Act
        var match = regex.Match(input);

        // Assert
        match.Success.Should().BeTrue();
        match.Value.Should().Be(expectedMatch);
    }

    [Theory]
    [InlineData("page123", "page123")]
    [InlineData("PAGE123", "PAGE123")]
    [InlineData("page-123", "page-123")]
    public void PageCode_ShouldExtractFullMatch(string input, string expectedMatch)
    {
        // Arrange
        var regex = RegexPatterns.PageCode();

        // Act
        var match = regex.Match(input);

        // Assert
        match.Success.Should().BeTrue();
        match.Value.Should().Be(expectedMatch);
    }
}
