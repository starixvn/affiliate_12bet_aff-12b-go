using Fanex.Affiliate.Link.Criterias;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Criterias;

public class BaseCriteriaTests
{
    [Fact]
    public void BaseCriteria_ShouldInitializeWithSpKey()
    {
        // Arrange
        var spKey = "test_sp_key";

        // Act
        var criteria = new BaseCriteria(spKey);

        // Assert
        criteria.Should().NotBeNull();
        criteria.GetSettingKey().Should().Be(spKey);
    }

    [Theory]
    [InlineData("valid_key", true)]
    [InlineData("another_key", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ShouldReturnCorrectResult_BasedOnSpKey(string spKey, bool expectedResult)
    {
        // Arrange
        var criteria = new BaseCriteria(spKey);

        // Act
        var result = criteria.IsValid();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void GetSettingKey_ShouldReturnProvidedSpKey()
    {
        // Arrange
        var expectedSpKey = "my_stored_procedure_key";
        var criteria = new BaseCriteria(expectedSpKey);

        // Act
        var result = criteria.GetSettingKey();

        // Assert
        result.Should().Be(expectedSpKey);
    }

    [Fact]
    public void BaseCriteria_ShouldInheritFromCriteriaBase()
    {
        // Arrange
        var criteria = new BaseCriteria("test");

        // Act & Assert
        criteria.Should().BeAssignableTo<Fanex.Data.Repository.CriteriaBase>();
    }

    [Theory]
    [InlineData("sp_get_data")]
    [InlineData("SP_ANOTHER_PROC")]
    [InlineData("GetUserInfo")]
    [InlineData("validate_link_proc")]
    public void BaseCriteria_ShouldWorkWithVariousSpKeyFormats(string spKey)
    {
        // Arrange & Act
        var criteria = new BaseCriteria(spKey);

        // Assert
        criteria.GetSettingKey().Should().Be(spKey);
        criteria.IsValid().Should().BeTrue();
    }

    [Fact]
    public void BaseCriteria_ShouldHandleEmptyString()
    {
        // Arrange
        var criteria = new BaseCriteria("");

        // Act & Assert
        criteria.GetSettingKey().Should().Be("");
        criteria.IsValid().Should().BeFalse();
    }

    [Fact]
    public void BaseCriteria_ShouldHandleNull()
    {
        // Arrange
        var criteria = new BaseCriteria(null);

        // Act & Assert
        criteria.GetSettingKey().Should().BeNull();
        criteria.IsValid().Should().BeFalse();
    }

    [Fact]
    public void BaseCriteria_ShouldHandleWhitespace()
    {
        // Arrange
        var criteria = new BaseCriteria("   ");

        // Act & Assert
        criteria.GetSettingKey().Should().Be("   ");
        criteria.IsValid().Should().BeTrue("whitespace is considered non-empty");
    }

    [Fact]
    public void BaseCriteria_Multiple_ShouldBeIndependent()
    {
        // Arrange
        var criteria1 = new BaseCriteria("key1");
        var criteria2 = new BaseCriteria("key2");

        // Act & Assert
        criteria1.GetSettingKey().Should().Be("key1");
        criteria2.GetSettingKey().Should().Be("key2");
        criteria1.IsValid().Should().BeTrue();
        criteria2.IsValid().Should().BeTrue();
    }

    [Fact]
    public void BaseCriteria_Constructor_ShouldRequireSpKeyParameter()
    {
        // This test ensures the constructor with spKey parameter exists
        // Act & Assert
        var creating = () => new BaseCriteria("test");
        creating.Should().NotThrow();
    }
}

