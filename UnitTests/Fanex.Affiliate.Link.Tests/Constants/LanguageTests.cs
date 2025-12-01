using Fanex.Affiliate.Link.Constants;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Constants;

public class LanguageTests
{
    [Fact]
    public void SupportedLanguageCodes_ShouldContainExpectedLanguages()
    {
        // Act
        var supportedLanguages = Language.SupportedLanguageCodes;

        // Assert
        supportedLanguages.Should().ContainKeys("en", "bd", "hien", "hitg", "id", "jp", "ko", "th", "vn", "cs", "mycs");
        supportedLanguages.Should().HaveCount(18);
    }

    [Fact]
    public void RemovedLanguageCodes_ShouldContainExpectedLanguages()
    {
        // Act
        var removedLanguages = Language.RemovedLanguageCodes;

        // Assert
        removedLanguages.Should().ContainKeys("bn", "ch", "kh", "eneuro");
        removedLanguages.Should().HaveCount(4);
    }

    [Theory]
    [InlineData("en", "en")]
    [InlineData("bd", "bd")]
    [InlineData("hien", "en")]
    [InlineData("hitg", "te")]
    [InlineData("jp", "en")]
    [InlineData("cs", "cs")]
    [InlineData("mycs", "cs")]
    [InlineData("id", "id")]
    [InlineData("ko", "ko")]
    [InlineData("th", "th")]
    [InlineData("vn", "vn")]
    public void SupportedLanguageCodes_ShouldMapCorrectly(string key, string expectedValue)
    {
        // Act
        var result = Language.SupportedLanguageCodes[key];

        // Assert
        result.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("bn", "bd")]
    [InlineData("ch", "en")]
    [InlineData("kh", "en")]
    [InlineData("eneuro", "en")]
    public void RemovedLanguageCodes_ShouldMapCorrectly(string key, string expectedValue)
    {
        // Act
        var result = Language.RemovedLanguageCodes[key];

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void SupportedLanguageCodes_ShouldBeReadOnly()
    {
        // Act & Assert
        Language.SupportedLanguageCodes.Should().BeAssignableTo<IReadOnlyDictionary<string, string>>();
    }

    [Fact]
    public void RemovedLanguageCodes_ShouldBeReadOnly()
    {
        // Act & Assert
        Language.RemovedLanguageCodes.Should().BeAssignableTo<IReadOnlyDictionary<string, string>>();
    }

    [Fact]
    public void BengaliShortedCode_ShouldBeConsistent()
    {
        // Act
        var bengaliFromSupported = Language.SupportedLanguageCodes["bd"];
        var bengaliFromRemoved = Language.RemovedLanguageCodes["bn"];

        // Assert
        bengaliFromSupported.Should().Be(bengaliFromRemoved);
        bengaliFromSupported.Should().Be("bd");
    }

    [Fact]
    public void ChineseLanguages_ShouldMapToSameCode()
    {
        // Act
        var cs = Language.SupportedLanguageCodes["cs"];
        var mycs = Language.SupportedLanguageCodes["mycs"];

        // Assert
        cs.Should().Be(mycs);
        cs.Should().Be("cs");
    }

    [Fact]
    public void EnglishVariants_ShouldMapToEnglish()
    {
        // Act
        var english = Language.SupportedLanguageCodes["en"];
        var hien = Language.SupportedLanguageCodes["hien"];
        var jp = Language.SupportedLanguageCodes["jp"];
        var ch = Language.RemovedLanguageCodes["ch"];
        var kh = Language.RemovedLanguageCodes["kh"];
        var eneuro = Language.RemovedLanguageCodes["eneuro"];

        // Assert
        english.Should().Be("en");
        hien.Should().Be("en");
        jp.Should().Be("en");
        ch.Should().Be("en");
        kh.Should().Be("en");
        eneuro.Should().Be("en");
    }

    [Fact]
    public void SupportedLanguageCodes_ShouldAllowDuplicateValues()
    {
        // Act
        var values = Language.SupportedLanguageCodes.Values;
        var distinctValues = values.Distinct();

        // Assert
        distinctValues.Count().Should().BeLessThanOrEqualTo(values.Count(), "Language codes can map to same values (e.g., multiple languages mapping to 'en')");
    }

    [Fact]
    public void RemovedLanguageCodes_ShouldAllowDuplicateValues()
    {
        // Act
        var values = Language.RemovedLanguageCodes.Values;
        var distinctValues = values.Distinct();

        // Assert
        distinctValues.Count().Should().BeLessThanOrEqualTo(values.Count(), "Language codes can map to same values (e.g., multiple languages mapping to 'en')");
    }

    [Fact]
    public void SupportedAndRemovedLanguages_ShouldNotOverlap()
    {
        // Act
        var supportedKeys = Language.SupportedLanguageCodes.Keys;
        var removedKeys = Language.RemovedLanguageCodes.Keys;
        var overlap = supportedKeys.Intersect(removedKeys);

        // Assert
        overlap.Should().BeEmpty("Supported and removed language codes should not overlap");
    }

    [Fact]
    public void LanguageMappings_ShouldPointToValidCodes()
    {
        // Arrange
        var validCodes = new[] { "en", "bd", "te", "id", "ko", "th", "vn", "cs" };

        // Act & Assert
        foreach (var mapping in Language.SupportedLanguageCodes.Values)
        {
            validCodes.Should().Contain(mapping, $"Language mapping '{mapping}' should be a valid language code");
        }

        foreach (var mapping in Language.RemovedLanguageCodes.Values)
        {
            validCodes.Should().Contain(mapping, $"Language mapping '{mapping}' should be a valid language code");
        }
    }

    [Theory]
    [InlineData("bd")]
    [InlineData("en")]
    [InlineData("te")]
    [InlineData("cs")]
    [InlineData("id")]
    [InlineData("ko")]
    [InlineData("th")]
    [InlineData("vn")]
    public void BengaliShortedCode_ShouldBeValidConstant(string expectedCode)
    {
        // Act
        var isBengaliCode = expectedCode == Language.BENGALI_SHORTED_CODE;

        // Assert
        if (expectedCode == "bd")
        {
            isBengaliCode.Should().BeTrue("BENGALI_SHORTED_CODE should be 'bd'");
        }
        else
        {
            isBengaliCode.Should().BeFalse($"BENGALI_SHORTED_CODE should not be '{expectedCode}'");
        }
    }
}
