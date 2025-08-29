using Fanex.Affiliate.Link.Constants;
using FluentAssertions;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Constants;

public class CacheKeyTests
{
    [Fact]
    public void CacheKey_ShouldHaveCorrectValues()
    {
        // Assert
        CacheKey.AFF_INFO_BY_AFF_ID_CACHE_KEY.Should().Be("Aff.12B.Go.AffInfo.AffId");
        CacheKey.AFF_INFO_BY_AFF_CODE_CACHE_KEY.Should().Be("Aff.12B.Go.AffInfo.AffCode");
        CacheKey.COMM_GROUP_CACHE_KEY.Should().Be("Aff.12B.Go.CommGroup");
    }

    [Fact]
    public void CacheKey_ShouldNotBeNullOrEmpty()
    {
        // Assert
        CacheKey.AFF_INFO_BY_AFF_ID_CACHE_KEY.Should().NotBeNullOrEmpty();
        CacheKey.AFF_INFO_BY_AFF_CODE_CACHE_KEY.Should().NotBeNullOrEmpty();
        CacheKey.COMM_GROUP_CACHE_KEY.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CacheKey_ShouldBeUnique()
    {
        // Assert
        var keys = new[]
        {
            CacheKey.AFF_INFO_BY_AFF_ID_CACHE_KEY,
            CacheKey.AFF_INFO_BY_AFF_CODE_CACHE_KEY,
            CacheKey.COMM_GROUP_CACHE_KEY
        };

        keys.Should().OnlyHaveUniqueItems();
    }
}
