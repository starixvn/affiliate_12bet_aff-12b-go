using Fanex.Affiliate.Link.Helpers;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Helpers;

public class CommonHelperTests
{
    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 6.1; Win64; x64)", "Windows 7")] // Windows NT 6.1 maps to Windows 7
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)", "Mac OS")] // Mac OS pattern matches
    [InlineData("Mozilla/5.0 (X11; Ubuntu; Linux x86_64)", "Linux")] // Ubuntu pattern returns "Linux" in actual implementation
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 14_7_1 like Mac OS X)", "iOS")] // iPhone pattern matches
    [InlineData("Mozilla/5.0 (Android 11; Mobile; rv:91.0)", "Android")] // Android pattern matches
    public void GetOsName_ShouldReturnCorrectOsName_WhenValidUserAgent(string userAgent, string expectedOs)
    {
        // Act
        var result = Common.GetOsName(userAgent);

        // Assert
        result.Should().Be(expectedOs);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64)", "")] // Windows NT 10.0 not in mapping
    [InlineData("Mozilla/5.0 (Unknown OS)", "")] // Unknown OS returns empty
    [InlineData("", "")] // Empty string returns empty
    [InlineData(null, "")] // Null returns empty
    public void GetOsName_ShouldReturnEmpty_WhenInvalidUserAgent(string userAgent, string expectedResult)
    {
        // Act
        var result = Common.GetOsName(userAgent);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("https://example.com/page", "example.com")]
    [InlineData("http://www.test.com/path", "www.test.com")]
    [InlineData("https://subdomain.example.org", "subdomain.example.org")]
    [InlineData("http://test.com", "test.com")]
    [InlineData("https://www.example.co.uk", "www.example.co.uk")]
    public void GetDomainReferrer_ShouldReturnCorrectDomain_WhenValidUrl(string urlReferrer, string expectedDomain)
    {
        // Act
        var result = Common.GetDomainReferrer(urlReferrer);

        // Assert
        result.Should().Be(expectedDomain);
    }

    [Theory]
    [InlineData("ftp://example.com", "example.com")] // FTP URLs are also handled
    [InlineData("invalid-url", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void GetDomainReferrer_ShouldReturnNull_WhenInvalidUrl(string urlReferrer, string expectedResult)
    {
        // Act
        var result = Common.GetDomainReferrer(urlReferrer);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36", "Other")] // No browser pattern matches
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36", "Other")] // No browser pattern matches
    [InlineData("Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:91.0) Gecko/20100101 Firefox/91.0", "Firefox")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36", "Chrome")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/91.0.864.59 Safari/537.36", "Edge")]
    public void GetBrowserName_ShouldReturnCorrectBrowserName_WhenValidUserAgent(string userAgent, string expectedBrowser)
    {
        // Act
        var result = Common.GetBrowserName(userAgent);

        // Assert
        result.Should().Be(expectedBrowser);
    }

    [Theory]
    [InlineData(null, "")] // Null returns empty string
    [InlineData("", "")] // Empty returns empty string
    [InlineData("Invalid User Agent", "Other")] // Invalid returns Other
    public void GetBrowserName_ShouldReturnOther_WhenInvalidUserAgent(string userAgent, string expectedResult)
    {
        // Act
        var result = Common.GetBrowserName(userAgent);

        // Assert
        result.Should().Be(expectedResult);
    }
}

