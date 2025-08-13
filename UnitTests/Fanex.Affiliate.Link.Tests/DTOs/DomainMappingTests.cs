using Fanex.Affiliate.Link.DTOs;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.DTOs;

public class DomainMappingTests
{
    [Fact]
    public void DomainMapping_ShouldBeCreatableWithDefaultValues()
    {
        // Arrange & Act
        var domainMapping = new DomainMapping();

        // Assert
        domainMapping.Should().NotBeNull();
        domainMapping.AffDomain.Should().BeNull();
        domainMapping.LanguageCode.Should().BeNull();
        domainMapping.LanguageLink.Should().BeNull();
        domainMapping.RedirectDomain.Should().BeNull();
        domainMapping.IsSSL.Should().BeFalse();
        domainMapping.IsMobile.Should().BeFalse();
        domainMapping.IsExcluded.Should().BeFalse();
        domainMapping.DealIds.Should().BeNull();
    }

    [Fact]
    public void DomainMapping_ShouldBeCreatableWithAllProperties()
    {
        // Arrange
        var affDomain = "example.com";
        var languageCode = "en-US";
        var languageLink = "https://example.com/en";
        var redirectDomain = "redirect.example.com";
        var isSSL = true;
        var isMobile = false;
        var isExcluded = true;
        var dealIds = "123,456,789";

        // Act
        var domainMapping = new DomainMapping
        {
            AffDomain = affDomain,
            LanguageCode = languageCode,
            LanguageLink = languageLink,
            RedirectDomain = redirectDomain,
            IsSSL = isSSL,
            IsMobile = isMobile,
            IsExcluded = isExcluded,
            DealIds = dealIds
        };

        // Assert
        domainMapping.Should().NotBeNull();
        domainMapping.AffDomain.Should().Be(affDomain);
        domainMapping.LanguageCode.Should().Be(languageCode);
        domainMapping.LanguageLink.Should().Be(languageLink);
        domainMapping.RedirectDomain.Should().Be(redirectDomain);
        domainMapping.IsSSL.Should().Be(isSSL);
        domainMapping.IsMobile.Should().Be(isMobile);
        domainMapping.IsExcluded.Should().Be(isExcluded);
        domainMapping.DealIds.Should().Be(dealIds);
    }

    [Fact]
    public void DomainMapping_PropertiesShouldBeSettable()
    {
        // Arrange
        var domainMapping = new DomainMapping();

        // Act
        domainMapping.AffDomain = "updated.com";
        domainMapping.LanguageCode = "fr-FR";
        domainMapping.LanguageLink = "https://updated.com/fr";
        domainMapping.RedirectDomain = "new-redirect.com";
        domainMapping.IsSSL = true;
        domainMapping.IsMobile = true;
        domainMapping.IsExcluded = false;
        domainMapping.DealIds = "999,888,777";

        // Assert
        domainMapping.AffDomain.Should().Be("updated.com");
        domainMapping.LanguageCode.Should().Be("fr-FR");
        domainMapping.LanguageLink.Should().Be("https://updated.com/fr");
        domainMapping.RedirectDomain.Should().Be("new-redirect.com");
        domainMapping.IsSSL.Should().BeTrue();
        domainMapping.IsMobile.Should().BeTrue();
        domainMapping.IsExcluded.Should().BeFalse();
        domainMapping.DealIds.Should().Be("999,888,777");
    }

    [Theory]
    [InlineData("domain1.com", "en", "https://domain1.com/en", "redirect1.com", true, false, false, "123")]
    [InlineData("domain2.com", "es", "https://domain2.com/es", "redirect2.com", false, true, true, "456,789")]
    [InlineData("domain3.com", "fr", "", "redirect3.com", true, true, false, "")]
    [InlineData("", "", "", "", false, false, false, null)]
    public void DomainMapping_ShouldHandleVariousValues(string affDomain, string languageCode, string languageLink, 
        string redirectDomain, bool isSSL, bool isMobile, bool isExcluded, string dealIds)
    {
        // Arrange & Act
        var domainMapping = new DomainMapping
        {
            AffDomain = affDomain,
            LanguageCode = languageCode,
            LanguageLink = languageLink,
            RedirectDomain = redirectDomain,
            IsSSL = isSSL,
            IsMobile = isMobile,
            IsExcluded = isExcluded,
            DealIds = dealIds
        };

        // Assert
        domainMapping.AffDomain.Should().Be(affDomain);
        domainMapping.LanguageCode.Should().Be(languageCode);
        domainMapping.LanguageLink.Should().Be(languageLink);
        domainMapping.RedirectDomain.Should().Be(redirectDomain);
        domainMapping.IsSSL.Should().Be(isSSL);
        domainMapping.IsMobile.Should().Be(isMobile);
        domainMapping.IsExcluded.Should().Be(isExcluded);
        domainMapping.DealIds.Should().Be(dealIds);
    }

    [Fact]
    public void DomainMapping_ShouldHandleNullValues()
    {
        // Arrange
        var domainMapping = new DomainMapping
        {
            AffDomain = "test.com",
            LanguageCode = "en",
            LanguageLink = "https://test.com",
            RedirectDomain = "redirect.com",
            IsSSL = true,
            IsMobile = false,
            IsExcluded = false,
            DealIds = "123"
        };

        // Act
        domainMapping.AffDomain = null;
        domainMapping.LanguageCode = null;
        domainMapping.LanguageLink = null;
        domainMapping.RedirectDomain = null;
        domainMapping.DealIds = null;

        // Assert
        domainMapping.AffDomain.Should().BeNull();
        domainMapping.LanguageCode.Should().BeNull();
        domainMapping.LanguageLink.Should().BeNull();
        domainMapping.RedirectDomain.Should().BeNull();
        domainMapping.IsSSL.Should().BeTrue();
        domainMapping.IsMobile.Should().BeFalse();
        domainMapping.IsExcluded.Should().BeFalse();
        domainMapping.DealIds.Should().BeNull();
    }

    [Fact]
    public void DomainMapping_ShouldHandleEmptyStringValues()
    {
        // Arrange & Act
        var domainMapping = new DomainMapping
        {
            AffDomain = "",
            LanguageCode = "",
            LanguageLink = "",
            RedirectDomain = "",
            IsSSL = false,
            IsMobile = true,
            IsExcluded = true,
            DealIds = ""
        };

        // Assert
        domainMapping.AffDomain.Should().Be("");
        domainMapping.LanguageCode.Should().Be("");
        domainMapping.LanguageLink.Should().Be("");
        domainMapping.RedirectDomain.Should().Be("");
        domainMapping.IsSSL.Should().BeFalse();
        domainMapping.IsMobile.Should().BeTrue();
        domainMapping.IsExcluded.Should().BeTrue();
        domainMapping.DealIds.Should().Be("");
    }

    [Fact]
    public void DomainMapping_ShouldBeAbleToCreateMultipleInstances()
    {
        // Arrange & Act
        var mapping1 = new DomainMapping
        {
            AffDomain = "domain1.com",
            LanguageCode = "en",
            LanguageLink = "https://domain1.com/en",
            RedirectDomain = "redirect1.com",
            IsSSL = true,
            IsMobile = false,
            IsExcluded = false,
            DealIds = "123"
        };

        var mapping2 = new DomainMapping
        {
            AffDomain = "domain2.com",
            LanguageCode = "es",
            LanguageLink = "https://domain2.com/es",
            RedirectDomain = "redirect2.com",
            IsSSL = false,
            IsMobile = true,
            IsExcluded = true,
            DealIds = "456"
        };

        // Assert
        mapping1.Should().NotBeSameAs(mapping2);
        mapping1.AffDomain.Should().Be("domain1.com");
        mapping2.AffDomain.Should().Be("domain2.com");
        mapping1.IsSSL.Should().BeTrue();
        mapping2.IsSSL.Should().BeFalse();
    }

    [Fact]
    public void DomainMapping_ShouldHandleLongDomainNames()
    {
        // Arrange
        var longDomain = new string('a', 100) + ".com";

        // Act
        var domainMapping = new DomainMapping
        {
            AffDomain = longDomain,
            LanguageCode = "en",
            LanguageLink = "https://test.com",
            RedirectDomain = "redirect.com",
            IsSSL = false,
            IsMobile = false,
            IsExcluded = false,
            DealIds = "test"
        };

        // Assert
        domainMapping.AffDomain.Should().Be(longDomain);
        domainMapping.AffDomain.Should().HaveLength(104); // 100 'a' + ".com"
    }

    [Fact]
    public void DomainMapping_ShouldHandleSpecialCharactersInDomains()
    {
        // Arrange
        var specialDomain = "test-domain_123.com";

        // Act
        var domainMapping = new DomainMapping
        {
            AffDomain = specialDomain,
            LanguageCode = "en",
            LanguageLink = "https://test.com",
            RedirectDomain = "redirect.com",
            IsSSL = true,
            IsMobile = false,
            IsExcluded = false,
            DealIds = "special"
        };

        // Assert
        domainMapping.AffDomain.Should().Be(specialDomain);
    }

    [Fact]
    public void DomainMapping_ShouldHandleUnicodeCharactersInLanguageCode()
    {
        // Arrange
        var unicodeLanguageCode = "zh-CN";

        // Act
        var domainMapping = new DomainMapping
        {
            AffDomain = "unicode.com",
            LanguageCode = unicodeLanguageCode,
            LanguageLink = "https://unicode.com/zh",
            RedirectDomain = "redirect.com",
            IsSSL = false,
            IsMobile = true,
            IsExcluded = false,
            DealIds = "unicode"
        };

        // Assert
        domainMapping.LanguageCode.Should().Be(unicodeLanguageCode);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public void DomainMapping_ShouldHandleAllBooleanCombinations(bool isSSL, bool isMobile, bool isExcluded)
    {
        // Arrange & Act
        var domainMapping = new DomainMapping
        {
            AffDomain = "test.com",
            LanguageCode = "en",
            LanguageLink = "https://test.com",
            RedirectDomain = "redirect.com",
            IsSSL = isSSL,
            IsMobile = isMobile,
            IsExcluded = isExcluded,
            DealIds = "test"
        };

        // Assert
        domainMapping.IsSSL.Should().Be(isSSL);
        domainMapping.IsMobile.Should().Be(isMobile);
        domainMapping.IsExcluded.Should().Be(isExcluded);
    }

    [Fact]
    public void DomainMapping_ShouldHandleWhitespaceInDomains()
    {
        // Arrange
        var domainWithWhitespace = "  test.com  ";

        // Act
        var domainMapping = new DomainMapping
        {
            AffDomain = domainWithWhitespace,
            LanguageCode = "en",
            LanguageLink = "https://test.com",
            RedirectDomain = "redirect.com",
            IsSSL = false,
            IsMobile = false,
            IsExcluded = false,
            DealIds = "whitespace"
        };

        // Assert
        domainMapping.AffDomain.Should().Be(domainWithWhitespace);
    }

    [Fact]
    public void DomainMapping_ShouldHandleNumbersInDomains()
    {
        // Arrange
        var numericDomain = "test123.com";

        // Act
        var domainMapping = new DomainMapping
        {
            AffDomain = numericDomain,
            LanguageCode = "en",
            LanguageLink = "https://test123.com",
            RedirectDomain = "redirect123.com",
            IsSSL = true,
            IsMobile = false,
            IsExcluded = false,
            DealIds = "numeric"
        };

        // Assert
        domainMapping.AffDomain.Should().Be(numericDomain);
        domainMapping.RedirectDomain.Should().Be("redirect123.com");
    }

    [Fact]
    public void DomainMapping_ShouldHandleComplexUrlsInLanguageLink()
    {
        // Arrange
        var complexUrl = "https://example.com/en/path?param=value&another=123#fragment";

        // Act
        var domainMapping = new DomainMapping
        {
            AffDomain = "example.com",
            LanguageCode = "en",
            LanguageLink = complexUrl,
            RedirectDomain = "redirect.com",
            IsSSL = true,
            IsMobile = false,
            IsExcluded = false,
            DealIds = "complex"
        };

        // Assert
        domainMapping.LanguageLink.Should().Be(complexUrl);
    }

    [Fact]
    public void DomainMapping_ShouldHandleSubdomains()
    {
        // Arrange
        var subdomain = "sub.example.com";

        // Act
        var domainMapping = new DomainMapping
        {
            AffDomain = subdomain,
            LanguageCode = "en",
            LanguageLink = "https://sub.example.com/en",
            RedirectDomain = "sub.redirect.com",
            IsSSL = true,
            IsMobile = true,
            IsExcluded = false,
            DealIds = "subdomain"
        };

        // Assert
        domainMapping.AffDomain.Should().Be(subdomain);
        domainMapping.RedirectDomain.Should().Be("sub.redirect.com");
    }

    [Fact]
    public void DomainMapping_ShouldHandleInternationalizedDomainNames()
    {
        // Arrange
        var idnDomain = "täst.com";

        // Act
        var domainMapping = new DomainMapping
        {
            AffDomain = idnDomain,
            LanguageCode = "de",
            LanguageLink = "https://täst.com/de",
            RedirectDomain = "redirect.com",
            IsSSL = false,
            IsMobile = false,
            IsExcluded = true,
            DealIds = "idn"
        };

        // Assert
        domainMapping.AffDomain.Should().Be(idnDomain);
        domainMapping.LanguageCode.Should().Be("de");
    }
}
