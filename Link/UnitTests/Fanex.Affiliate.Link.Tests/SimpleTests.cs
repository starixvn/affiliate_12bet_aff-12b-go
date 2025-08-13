using System;
using Fanex.Affiliate.Link.Constants;
using Fanex.Affiliate.Link.Extensions;
using Fanex.Affiliate.Link.Helpers;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests;

public class SimpleTests
{
    [Fact]
    public void CacheKey_ShouldHaveCorrectValues()
    {
        // Act & Assert
        CacheKey.COMM_GROUP_CACHE_KEY.Should().Be("Aff.12B.Go.CommGroup");
        CacheKey.AFF_INFO_BY_AFF_CODE_CACHE_KEY.Should().Be("Aff.12B.Go.AffInfo.AffCode");
        CacheKey.MEDIA_LINK_CACHE_KEY.Should().Be("Aff.12B.Go.MediaLink");
    }

    [Fact]
    public void StringExtensions_ShouldWorkCorrectly()
    {
        // Arrange
        var value = "test";
        var strToMatch = "TEST";

        // Act
        var result = value.MatchWith(strToMatch);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CommonHelper_ShouldReturnCorrectOsName()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36";

        // Act
        var result = Common.GetOsName(userAgent);

        // Assert
        result.Should().Be("Windows 7");
    }

    [Fact]
    public void CommonHelper_ShouldReturnCorrectBrowserName()
    {
        // Arrange
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";

        // Act
        var result = Common.GetBrowserName(userAgent);

        // Assert
        result.Should().Be("Chrome");
    }

    [Fact]
    public void CommonHelper_ShouldReturnCorrectDomainReferrer()
    {
        // Arrange
        var urlReferrer = "https://www.google.com/search?q=test";

        // Act
        var result = Common.GetDomainReferrer(urlReferrer);

        // Assert
        result.Should().Be("www.google.com");
    }

    [Fact]
    public void DateTimeHelper_ShouldReturnCorrectTimeSpan()
    {
        // Act
        var result = DateTimeHelper.GmtMinus4TimeSpan;

        // Assert
        result.Should().Be(TimeSpan.FromHours(-4));
    }

    [Fact]
    public void DateTimeHelper_ShouldCreateDateTimeCorrectly()
    {
        // Act
        var result = DateTimeHelper.CreateDateTime(2023, 1, 1, 12, 30, 45);

        // Assert
        result.Year.Should().Be(2023);
        result.Month.Should().Be(1);
        result.Day.Should().Be(1);
        result.Hour.Should().Be(12);
        result.Minute.Should().Be(30);
        result.Second.Should().Be(45);
    }
}



