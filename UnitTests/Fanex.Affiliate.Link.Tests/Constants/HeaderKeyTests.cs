using Fanex.Affiliate.Link.Constants;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Constants;

public class HeaderKeyTests
{
    [Fact]
    public void HeaderKey_ShouldHaveCorrectValues()
    {
        // Assert
        HeaderKey.X_ACTUAL_IP.Should().Be("X-Actual-Ip");
        HeaderKey.X_FORWARDED_FOR.Should().Be("X-Forwarded-For");
    }

    [Fact]
    public void HeaderKey_ShouldNotBeNullOrEmpty()
    {
        // Assert
        HeaderKey.X_ACTUAL_IP.Should().NotBeNullOrEmpty();
        HeaderKey.X_FORWARDED_FOR.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HeaderKey_ShouldBeUnique()
    {
        // Assert
        var keys = new[]
        {
            HeaderKey.X_ACTUAL_IP,
            HeaderKey.X_FORWARDED_FOR
        };

        keys.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void HeaderKey_ShouldFollowNamingConvention()
    {
        // Assert
        HeaderKey.X_ACTUAL_IP.Should().StartWith("X-");
        HeaderKey.X_FORWARDED_FOR.Should().StartWith("X-");
    }
}
