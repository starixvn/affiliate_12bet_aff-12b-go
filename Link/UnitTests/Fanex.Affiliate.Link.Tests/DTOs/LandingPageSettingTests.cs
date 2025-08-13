using Fanex.Affiliate.Link.DTOs;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.DTOs;

public class LandingPageSettingTests
{
    [Fact]
    public void LandingPageSetting_ShouldBeCreatableWithDefaultValues()
    {
        // Arrange & Act
        var landingPageSetting = new LandingPageSetting();

        // Assert
        landingPageSetting.Should().NotBeNull();
        landingPageSetting.PageCode.Should().BeNull();
        landingPageSetting.IsMobile.Should().BeFalse();
        landingPageSetting.IsExcluded.Should().BeFalse();
        landingPageSetting.DealIds.Should().BeNull();
    }

    [Fact]
    public void LandingPageSetting_ShouldBeCreatableWithAllProperties()
    {
        // Arrange
        var pageCode = "HOME";
        var isMobile = true;
        var isExcluded = false;
        var dealIds = "123,456,789";

        // Act
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = pageCode,
            IsMobile = isMobile,
            IsExcluded = isExcluded,
            DealIds = dealIds
        };

        // Assert
        landingPageSetting.Should().NotBeNull();
        landingPageSetting.PageCode.Should().Be(pageCode);
        landingPageSetting.IsMobile.Should().Be(isMobile);
        landingPageSetting.IsExcluded.Should().Be(isExcluded);
        landingPageSetting.DealIds.Should().Be(dealIds);
    }

    [Fact]
    public void LandingPageSetting_PropertiesShouldBeSettable()
    {
        // Arrange
        var landingPageSetting = new LandingPageSetting();

        // Act
        landingPageSetting.PageCode = "UPDATED_PAGE";
        landingPageSetting.IsMobile = true;
        landingPageSetting.IsExcluded = true;
        landingPageSetting.DealIds = "999,888,777";

        // Assert
        landingPageSetting.PageCode.Should().Be("UPDATED_PAGE");
        landingPageSetting.IsMobile.Should().BeTrue();
        landingPageSetting.IsExcluded.Should().BeTrue();
        landingPageSetting.DealIds.Should().Be("999,888,777");
    }

    [Theory]
    [InlineData("HOME", true, false, "123,456")]
    [InlineData("ABOUT", false, true, "789")]
    [InlineData("CONTACT", true, true, "")]
    [InlineData("", false, false, null)]
    public void LandingPageSetting_ShouldHandleVariousValues(string pageCode, bool isMobile, bool isExcluded, string dealIds)
    {
        // Arrange & Act
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = pageCode,
            IsMobile = isMobile,
            IsExcluded = isExcluded,
            DealIds = dealIds
        };

        // Assert
        landingPageSetting.PageCode.Should().Be(pageCode);
        landingPageSetting.IsMobile.Should().Be(isMobile);
        landingPageSetting.IsExcluded.Should().Be(isExcluded);
        landingPageSetting.DealIds.Should().Be(dealIds);
    }

    [Fact]
    public void LandingPageSetting_ShouldHandleNullValues()
    {
        // Arrange
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = "TEST",
            IsMobile = true,
            IsExcluded = false,
            DealIds = "123"
        };

        // Act
        landingPageSetting.PageCode = null;
        landingPageSetting.DealIds = null;

        // Assert
        landingPageSetting.PageCode.Should().BeNull();
        landingPageSetting.IsMobile.Should().BeTrue();
        landingPageSetting.IsExcluded.Should().BeFalse();
        landingPageSetting.DealIds.Should().BeNull();
    }

    [Fact]
    public void LandingPageSetting_ShouldHandleEmptyStringValues()
    {
        // Arrange & Act
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = "",
            IsMobile = false,
            IsExcluded = true,
            DealIds = ""
        };

        // Assert
        landingPageSetting.PageCode.Should().Be("");
        landingPageSetting.IsMobile.Should().BeFalse();
        landingPageSetting.IsExcluded.Should().BeTrue();
        landingPageSetting.DealIds.Should().Be("");
    }

    [Fact]
    public void LandingPageSetting_ShouldBeAbleToCreateMultipleInstances()
    {
        // Arrange & Act
        var setting1 = new LandingPageSetting
        {
            PageCode = "PAGE1",
            IsMobile = true,
            IsExcluded = false,
            DealIds = "123"
        };

        var setting2 = new LandingPageSetting
        {
            PageCode = "PAGE2",
            IsMobile = false,
            IsExcluded = true,
            DealIds = "456"
        };

        // Assert
        setting1.Should().NotBeSameAs(setting2);
        setting1.PageCode.Should().Be("PAGE1");
        setting2.PageCode.Should().Be("PAGE2");
        setting1.IsMobile.Should().BeTrue();
        setting2.IsMobile.Should().BeFalse();
    }

    [Fact]
    public void LandingPageSetting_ShouldHandleLongPageCode()
    {
        // Arrange
        var longPageCode = new string('A', 1000);

        // Act
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = longPageCode,
            IsMobile = false,
            IsExcluded = false,
            DealIds = "test"
        };

        // Assert
        landingPageSetting.PageCode.Should().Be(longPageCode);
        landingPageSetting.PageCode.Should().HaveLength(1000);
    }

    [Fact]
    public void LandingPageSetting_ShouldHandleSpecialCharactersInPageCode()
    {
        // Arrange
        var specialPageCode = "PAGE-CODE_123!@#$%^&*()";

        // Act
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = specialPageCode,
            IsMobile = true,
            IsExcluded = false,
            DealIds = "special"
        };

        // Assert
        landingPageSetting.PageCode.Should().Be(specialPageCode);
    }

    [Fact]
    public void LandingPageSetting_ShouldHandleUnicodeCharactersInDealIds()
    {
        // Arrange
        var unicodeDealIds = "123,456,789,中文,한국어,العربية";

        // Act
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = "UNICODE_PAGE",
            IsMobile = false,
            IsExcluded = true,
            DealIds = unicodeDealIds
        };

        // Assert
        landingPageSetting.DealIds.Should().Be(unicodeDealIds);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void LandingPageSetting_ShouldHandleAllBooleanCombinations(bool isMobile, bool isExcluded)
    {
        // Arrange & Act
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = "TEST_PAGE",
            IsMobile = isMobile,
            IsExcluded = isExcluded,
            DealIds = "test"
        };

        // Assert
        landingPageSetting.IsMobile.Should().Be(isMobile);
        landingPageSetting.IsExcluded.Should().Be(isExcluded);
    }

    [Fact]
    public void LandingPageSetting_ShouldHandleWhitespaceInPageCode()
    {
        // Arrange
        var pageCodeWithWhitespace = "  PAGE CODE  ";

        // Act
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = pageCodeWithWhitespace,
            IsMobile = false,
            IsExcluded = false,
            DealIds = "whitespace"
        };

        // Assert
        landingPageSetting.PageCode.Should().Be(pageCodeWithWhitespace);
    }

    [Fact]
    public void LandingPageSetting_ShouldHandleNumbersInPageCode()
    {
        // Arrange
        var numericPageCode = "PAGE123";

        // Act
        var landingPageSetting = new LandingPageSetting
        {
            PageCode = numericPageCode,
            IsMobile = true,
            IsExcluded = false,
            DealIds = "numeric"
        };

        // Assert
        landingPageSetting.PageCode.Should().Be(numericPageCode);
    }
}
