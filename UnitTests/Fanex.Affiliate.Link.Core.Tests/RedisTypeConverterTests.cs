using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace Fanex.Affiliate.Link.Core.Tests;

public class RedisTypeConverterTests
{
    [Fact]
    public void ToRedisValue_ShouldReturnNull_WhenValueIsNull()
    {
        // Arrange
        object? value = null;

        // Act
        var result = RedisTypeConverter.ToRedisValue(value);

        // Assert
        result.Should().Be(RedisValue.Null);
    }

    [Fact]
    public void ToRedisValue_ShouldReturnByteArray_WhenValueIsString()
    {
        // Arrange
        var stringValue = "test string";

        // Act
        var result = RedisTypeConverter.ToRedisValue(stringValue);

        // Assert
        result.Should().BeOfType<RedisValue>();
        result.HasValue.Should().BeTrue();
        result.IsNull.Should().BeFalse();
        // The result should be a byte array (serialized string)
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void ToRedisValue_ShouldSerializeObject_WhenValueIsNotByteArray()
    {
        // Arrange
        var testObject = new TestModel { Id = 1, Name = "Test" };

        // Act
        var result = RedisTypeConverter.ToRedisValue(testObject);

        // Assert
        result.Should().BeOfType<RedisValue>();
        result.HasValue.Should().BeTrue();
        result.IsNull.Should().BeFalse();
    }

    [Fact]
    public void ToObject_ShouldReturnDefault_WhenValueIsNull()
    {
        // Arrange
        var redisValue = RedisValue.Null;

        // Act
        var result = RedisTypeConverter.ToObject<object>(redisValue);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToObject_ShouldReturnDefault_WhenBinaryDataIsNull()
    {
        // Arrange
        var redisValue = RedisValue.Null;

        // Act
        var result = RedisTypeConverter.ToObject<TestModel>(redisValue);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToObject_ShouldReturnDefault_WhenDeserializationFails()
    {
        // Arrange
        var invalidData = "invalid data";
        var redisValue = new RedisValue(invalidData);

        // Act
        var result = RedisTypeConverter.ToObject<TestModel>(redisValue);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToRedisValueAndToObject_ShouldRoundTripCorrectly()
    {
        // Arrange
        var originalObject = new TestModel { Id = 999, Name = "Round Trip Test" };

        // Act
        var redisValue = RedisTypeConverter.ToRedisValue(originalObject);
        var result = RedisTypeConverter.ToObject<TestModel>(redisValue);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(originalObject.Id);
        result.Name.Should().Be(originalObject.Name);
    }

    // Test models - made public for MessagePack serialization
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
