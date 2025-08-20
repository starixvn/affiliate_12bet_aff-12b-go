using Fanex.Affiliate.Link.Constants;
using FluentAssertions;
using Xunit;
using System;

namespace Fanex.Affiliate.Link.Tests.Constants;

public class RegexPatternsTests
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
    [InlineData("123abc", false)]
    [InlineData("abc123", false)]
    public void Numeric_ShouldValidateCorrectly(string input, bool expectedResult)
    {
        // Act
        var result = RegexPatterns.Numeric().IsMatch(input);

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
    [InlineData("HOME", true)] // Case insensitive
    [InlineData("Sports", true)] // Case insensitive
    [InlineData("", false)]
    [InlineData("page code", false)] // Contains space
    [InlineData("page.code", false)] // Contains dot
    [InlineData("page@code", false)] // Contains @
    [InlineData("page#code", false)] // Contains #
    [InlineData("page$code", false)] // Contains $
    public void PageCode_ShouldValidateCorrectly(string input, bool expectedResult)
    {
        // Act
        var result = RegexPatterns.PageCode().IsMatch(input);

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
    [InlineData("phen", true)]
    [InlineData("ch", true)]
    [InlineData("kh", true)]
    [InlineData("eneuro", true)]
    [InlineData("EN", true)] // Case insensitive
    [InlineData("BD", true)] // Case insensitive
    [InlineData("xx", false)]
    [InlineData("", false)]
    [InlineData("english", false)]
    [InlineData("enenuro", false)] // Typo
    [InlineData("cs_", false)] // Extra character
    public void Language_ShouldValidateCorrectly(string input, bool expectedResult)
    {
        // Act
        var result = RegexPatterns.Language.IsMatch(input);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Numeric_ShouldBeCompiledRegex()
    {
        // Act
        var regex = RegexPatterns.Numeric();

        // Assert
        regex.Options.Should().HaveFlag(System.Text.RegularExpressions.RegexOptions.Compiled);
    }

    [Fact]
    public void PageCode_ShouldBeCompiledAndIgnoreCase()
    {
        // Act
        var regex = RegexPatterns.PageCode();

        // Assert
        regex.Options.Should().HaveFlag(System.Text.RegularExpressions.RegexOptions.Compiled);
        regex.Options.Should().HaveFlag(System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    [Fact]
    public void Language_ShouldBeCompiledAndIgnoreCase()
    {
        // Act
        var regex = RegexPatterns.Language;

        // Assert
        regex.Options.Should().HaveFlag(System.Text.RegularExpressions.RegexOptions.Compiled);
        regex.Options.Should().HaveFlag(System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    [Fact]
    public void Language_ShouldHaveTimeoutSet()
    {
        // Act
        var regex = RegexPatterns.Language;

        // Assert
        regex.MatchTimeout.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Numeric_ShouldReturnSameInstance()
    {
        // Act
        var regex1 = RegexPatterns.Numeric();
        var regex2 = RegexPatterns.Numeric();

        // Assert
        regex1.Should().BeSameAs(regex2, "Generated regex should return the same instance");
    }

    [Fact]
    public void PageCode_ShouldReturnSameInstance()
    {
        // Act
        var regex1 = RegexPatterns.PageCode();
        var regex2 = RegexPatterns.PageCode();

        // Assert
        regex1.Should().BeSameAs(regex2, "Generated regex should return the same instance");
    }

    [Theory]
    [InlineData("123456789", true)]
    [InlineData("1", true)]
    [InlineData("00000", true)]
    [InlineData("01234", true)]
    public void Numeric_ShouldAcceptLongNumbers(string input, bool expectedResult)
    {
        // Act
        var result = RegexPatterns.Numeric().IsMatch(input);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("Page_123", true)]
    [InlineData("page-code", true)]
    [InlineData("PageCode123", true)]
    [InlineData("123PageCode", true)]
    [InlineData("_PageCode", true)]
    [InlineData("-PageCode", true)]
    [InlineData("PageCode_", true)]
    [InlineData("PageCode-", true)]
    public void PageCode_ShouldAcceptValidCombinations(string input, bool expectedResult)
    {
        // Act
        var result = RegexPatterns.PageCode().IsMatch(input);

        // Assert
        result.Should().Be(expectedResult);
    }
}