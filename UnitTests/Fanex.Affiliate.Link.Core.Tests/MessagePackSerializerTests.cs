using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Core.Tests;

public class MessagePackSerializerTests
{
    private readonly MessagePackSerializer _serializer;

    public MessagePackSerializerTests()
    {
        _serializer = new MessagePackSerializer();
    }

    [Fact]
    public void Serialize_ShouldSerializeSimpleObject()
    {
        // Arrange
        var testObject = new TestModel { Id = 1, Name = "Test", IsActive = true };

        // Act
        var serializedData = _serializer.Serialize(testObject);

        // Assert
        serializedData.Should().NotBeNull();
        serializedData.Should().NotBeEmpty();
    }

    [Fact]
    public void Serialize_ShouldSerializeComplexObject()
    {
        // Arrange
        var testObject = new ComplexTestModel
        {
            Id = 1,
            Name = "Test",
            Properties = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 123 },
                { "key3", true }
            },
            Items = new List<string> { "item1", "item2", "item3" }
        };

        // Act
        var serializedData = _serializer.Serialize(testObject);

        // Assert
        serializedData.Should().NotBeNull();
        serializedData.Should().NotBeEmpty();
    }

    [Fact]
    public void Serialize_ShouldHandleNullObject()
    {
        // Arrange
        object? testObject = null;

        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var serializedData = _serializer.Serialize(testObject);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        serializedData.Should().NotBeNull();
        serializedData.Should().NotBeEmpty();
    }

    [Fact]
    public void Deserialize_ShouldDeserializeSimpleObject()
    {
        // Arrange
        var originalObject = new TestModel { Id = 1, Name = "Test", IsActive = true };
        var serializedData = _serializer.Serialize(originalObject);

        // Act
        var deserializedObject = _serializer.Deserialize<TestModel>(serializedData);

        // Assert
        deserializedObject.Should().NotBeNull();
        deserializedObject!.Id.Should().Be(originalObject.Id);
        deserializedObject.Name.Should().Be(originalObject.Name);
        deserializedObject.IsActive.Should().Be(originalObject.IsActive);
    }

    [Fact]
    public void Deserialize_ShouldDeserializeComplexObject()
    {
        // Arrange
        var originalObject = new ComplexTestModel
        {
            Id = 1,
            Name = "Test",
            Properties = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 123 },
                { "key3", true }
            },
            Items = new List<string> { "item1", "item2", "item3" }
        };
        var serializedData = _serializer.Serialize(originalObject);

        // Act
        var deserializedObject = _serializer.Deserialize<ComplexTestModel>(serializedData);

        // Assert
        deserializedObject.Should().NotBeNull();
        deserializedObject!.Id.Should().Be(originalObject.Id);
        deserializedObject.Name.Should().Be(originalObject.Name);
        deserializedObject.Properties.Should().BeEquivalentTo(originalObject.Properties);
        deserializedObject.Items.Should().BeEquivalentTo(originalObject.Items);
    }

    [Fact]
    public void Deserialize_ShouldReturnDefault_WhenDataIsInvalid()
    {
        // Arrange
        var invalidData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

        // Act
        var deserializedObject = _serializer.Deserialize<TestModel>(invalidData);

        // Assert
        deserializedObject.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ShouldReturnDefault_WhenDataIsEmpty()
    {
        // Arrange
        var emptyData = new byte[0];

        // Act
        var deserializedObject = _serializer.Deserialize<TestModel>(emptyData);

        // Assert
        deserializedObject.Should().BeNull();
    }

    [Fact]
    public void SerializeAndDeserialize_ShouldWorkWithPrimitiveTypes()
    {
        // Arrange
        var testCases = new object[]
        {
            "string value",
            42,
            3.14,
            true,
            DateTime.UtcNow,
            Guid.NewGuid()
        };

        foreach (var testCase in testCases)
        {
            // Act
            var serializedData = _serializer.Serialize(testCase);
            var deserializedObject = _serializer.Deserialize<object>(serializedData);

            // Assert
            serializedData.Should().NotBeNull();
            serializedData.Should().NotBeEmpty();
            deserializedObject.Should().NotBeNull();
        }
    }

    [Fact]
    public void SerializeAndDeserialize_ShouldWorkWithCollections()
    {
        // Arrange
        var testCases = new object[]
        {
            new List<int> { 1, 2, 3, 4, 5 },
            new string[] { "a", "b", "c" },
            new Dictionary<string, int> { { "a", 1 }, { "b", 2 } }
        };

        foreach (var testCase in testCases)
        {
            // Act
            var serializedData = _serializer.Serialize(testCase);
            var deserializedObject = _serializer.Deserialize<object>(serializedData);

            // Assert
            serializedData.Should().NotBeNull();
            serializedData.Should().NotBeEmpty();
            deserializedObject.Should().NotBeNull();
        }
    }

    [Fact]
    public void Deserialize_ShouldHandleNullData()
    {
        // Arrange
        byte[]? nullData = null;

        // Act
        var deserializedObject = _serializer.Deserialize<TestModel>(nullData!);

        // Assert
        deserializedObject.Should().BeNull();
    }

    // Test models - made public for MessagePack serialization
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ComplexTestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> Items { get; set; } = new();
    }
}
