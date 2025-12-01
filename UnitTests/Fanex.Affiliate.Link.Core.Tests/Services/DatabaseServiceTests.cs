using Fanex.Affiliate.Link.Core.Models;
using Fanex.Affiliate.Link.Core.Services;
using Fanex.Data.Repository;
using Moq;
using Xunit;

namespace Fanex.Affiliate.Link.Core.Tests.Services;

public class DatabaseServiceTests
{
    private readonly Mock<IDynamicRepository> _mockDynamicRepository;
    private readonly DatabaseService _databaseService;

    public DatabaseServiceTests()
    {
        _mockDynamicRepository = new Mock<IDynamicRepository>();
        _databaseService = new DatabaseService(_mockDynamicRepository.Object);
    }

    [Fact]
    public async Task InsertClickInfo_ShouldHandleNullClickTrackingEvent()
    {
        // Arrange
        ClickTrackingEvent? clickTrackingEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _databaseService.InsertClickInfo(clickTrackingEvent!));
    }

    [Fact]
    public async Task InsertClickInfo_ShouldCompleteSuccessfully()
    {
        // Arrange
        var clickTrackingEvent = new ClickTrackingEvent
        {
            AffCode = "AFF001",
            AffDomain = "example.com",
            PageCode = "PAGE001",
            LanguageCode = "en",
            ChannelCode = "CHANNEL001",
            IP = "192.168.1.1",
            Browser = "Chrome",
            UrlReferrer = "https://google.com",
            DomainReferrer = "google.com",
            OS = "Windows",
            Device = "Desktop",
            BrowserVersion = "120.0.0.0",
            BrowserResolution = "1920x1080",
            BrowserLanguage = "en-US",
            UserAgent = "Mozilla/5.0...",
            Country = "US",
            DeviceType = 1,
            RedirectLink = "https://target.com",
            ClickId = 12345,
            UpdatedTime = DateTime.UtcNow,
            AffMediaLink = "https://affiliate.com/link"
        };

        // Act
        await _databaseService.InsertClickInfo(clickTrackingEvent);

        // Assert - Test passes if no exception is thrown
        Assert.True(true);
    }
}
