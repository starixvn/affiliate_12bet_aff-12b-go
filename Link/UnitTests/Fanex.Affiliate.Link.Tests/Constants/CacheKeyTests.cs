using Fanex.Affiliate.Link.Constants;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Constants;

public class CacheKeyTests
{
    [Fact]
    public void CacheKey_ShouldHaveCorrectCommGroupCacheKey()
    {
        // Act & Assert
        CacheKey.COMM_GROUP_CACHE_KEY.Should().Be("Aff.12B.Go.CommGroup");
    }

    [Fact]
    public void CacheKey_ShouldHaveCorrectAffInfoByAffCodeCacheKey()
    {
        // Act & Assert
        CacheKey.AFF_INFO_BY_AFF_CODE_CACHE_KEY.Should().Be("Aff.12B.Go.AffInfo.AffCode");
    }

    [Fact]
    public void CacheKey_ShouldHaveCorrectAffInfoByAffIdCacheKey()
    {
        // Act & Assert
        CacheKey.AFF_INFO_BY_AFF_ID_CACHE_KEY.Should().Be("Aff.12B.Go.AffInfo.AffId");
    }

    [Fact]
    public void CacheKey_ShouldHaveCorrectMediaLinkCacheKey()
    {
        // Act & Assert
        CacheKey.MEDIA_LINK_CACHE_KEY.Should().Be("Aff.12B.Go.MediaLink");
    }

    [Fact]
    public void CacheKey_ShouldHaveCorrectSupportedMarketCacheKey()
    {
        // Act & Assert
        CacheKey.SUPPORTED_MARKET_CACHE_KEY.Should().Be("Aff.12B.Go.Supported.Makets");
    }

    [Fact]
    public void CacheKey_ShouldHaveCorrectMarketInfoCacheKey()
    {
        // Act & Assert
        CacheKey.MARKET_INFO_CACHE_KEY.Should().Be("Aff.12B.Go.Maket.Info");
    }

    [Fact]
    public void CacheKey_ShouldHaveCorrectMasterDataCacheKey()
    {
        // Act & Assert
        CacheKey.MASTER_DATA.Should().Be("Aff.12B.Go.MasterData");
    }

    [Fact]
    public void CacheKey_ShouldHaveCorrectClickIdCacheKey()
    {
        // Act & Assert
        CacheKey.CLICKID.Should().Be("Aff.12B.Go.ClickId");
    }

    [Fact]
    public void CacheKey_ShouldHaveCorrectClickQueuesCacheKey()
    {
        // Act & Assert
        CacheKey.CLICKQUEUES.Should().Be("Aff.12B.Go.ClickQueues");
    }

    [Fact]
    public void CacheKey_ShouldHaveAllRequiredKeys()
    {
        // Act & Assert
        CacheKey.COMM_GROUP_CACHE_KEY.Should().NotBeNullOrEmpty();
        CacheKey.AFF_INFO_BY_AFF_CODE_CACHE_KEY.Should().NotBeNullOrEmpty();
        CacheKey.AFF_INFO_BY_AFF_ID_CACHE_KEY.Should().NotBeNullOrEmpty();
        CacheKey.MEDIA_LINK_CACHE_KEY.Should().NotBeNullOrEmpty();
        CacheKey.SUPPORTED_MARKET_CACHE_KEY.Should().NotBeNullOrEmpty();
        CacheKey.MARKET_INFO_CACHE_KEY.Should().NotBeNullOrEmpty();
        CacheKey.MASTER_DATA.Should().NotBeNullOrEmpty();
        CacheKey.CLICKID.Should().NotBeNullOrEmpty();
        CacheKey.CLICKQUEUES.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CacheKey_ShouldHaveUniqueValues()
    {
        // Arrange
        var cacheKeys = new[]
        {
            CacheKey.COMM_GROUP_CACHE_KEY,
            CacheKey.AFF_INFO_BY_AFF_CODE_CACHE_KEY,
            CacheKey.AFF_INFO_BY_AFF_ID_CACHE_KEY,
            CacheKey.MEDIA_LINK_CACHE_KEY,
            CacheKey.SUPPORTED_MARKET_CACHE_KEY,
            CacheKey.MARKET_INFO_CACHE_KEY,
            CacheKey.MASTER_DATA,
            CacheKey.CLICKID,
            CacheKey.CLICKQUEUES
        };

        // Act
        var uniqueKeys = cacheKeys.Distinct().ToArray();

        // Assert
        uniqueKeys.Should().HaveCount(cacheKeys.Length);
    }

    [Fact]
    public void CacheKey_ShouldFollowNamingConvention()
    {
        // Arrange
        var cacheKeys = new[]
        {
            CacheKey.COMM_GROUP_CACHE_KEY,
            CacheKey.AFF_INFO_BY_AFF_CODE_CACHE_KEY,
            CacheKey.AFF_INFO_BY_AFF_ID_CACHE_KEY,
            CacheKey.MEDIA_LINK_CACHE_KEY,
            CacheKey.SUPPORTED_MARKET_CACHE_KEY,
            CacheKey.MARKET_INFO_CACHE_KEY,
            CacheKey.MASTER_DATA,
            CacheKey.CLICKID,
            CacheKey.CLICKQUEUES
        };

        // Act & Assert
        foreach (var key in cacheKeys)
        {
            key.Should().StartWith("Aff.12B.Go.");
        }
    }
}
