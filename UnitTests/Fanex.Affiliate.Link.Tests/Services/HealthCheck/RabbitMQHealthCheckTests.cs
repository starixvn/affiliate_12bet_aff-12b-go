using Fanex.Affiliate.Link.Core.Services.Interfaces;
using Fanex.Affiliate.Link.Services.HealthCheck;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ONEbook.UM.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Services.HealthCheck
{
    public class RabbitMQHealthCheckTests
    {
        private readonly Mock<Fanex.Data.Repository.IDynamicRepository> _mockRepository;
        private readonly Mock<IRedisService> _mockRedisService;
        private readonly Mock<IRabbitBus> _mockRabbitBus;
        private readonly Mock<IMaintenanceService> _mockMaintenanceService;
        private readonly Mock<IHealthCheckConfigurationService> _mockConfigService;
        private readonly Mock<IHealthCheckHistoryService> _mockHistoryService;
        private readonly Mock<ILogger<HealthCheckService>> _mockLogger;
        private readonly IConfiguration _configuration;
        private readonly HealthCheckService _healthCheckService;

        public RabbitMQHealthCheckTests()
        {
            _mockRepository = new Mock<Fanex.Data.Repository.IDynamicRepository>();
            _mockRedisService = new Mock<IRedisService>();
            _mockRabbitBus = new Mock<IRabbitBus>();
            _mockMaintenanceService = new Mock<IMaintenanceService>();
            _mockConfigService = new Mock<IHealthCheckConfigurationService>();
            _mockHistoryService = new Mock<IHealthCheckHistoryService>();
            _mockLogger = new Mock<ILogger<HealthCheckService>>();
            _configuration = new ConfigurationBuilder().Build();

            _healthCheckService = new HealthCheckService(
                _mockRepository.Object,
                _mockRedisService.Object,
                _mockRabbitBus.Object,
                _mockMaintenanceService.Object,
                _mockConfigService.Object,
                _mockHistoryService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CheckRabbitMQHealthAsync_ShouldReturnUnhealthy_WhenNotConnected()
        {
            // Arrange
            _mockRabbitBus.Setup(x => x.IsConnected).Returns(false);

            // Act
            var result = await _healthCheckService.CheckRabbitMQHealthAsync();

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Unhealthy");
            result.Name.Should().Be("RabbitMQ");
            result.Description.Should().Be("Component is not functioning properly");
            result.ErrorMessage.Should().Be("RabbitMQ connection is not active");
            result.ResponseTime.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public async Task CheckRabbitMQHealthAsync_ShouldReturnHealthy_WhenConnected()
        {
            // Arrange
            var connectionInfo = new Dictionary<string, object>
            {
                ["IsConnected"] = true,
                ["ConnectionState"] = "Connected",
                ["ConnectionActive"] = true
            };

            _mockRabbitBus.Setup(x => x.IsConnected).Returns(true);
            _mockRabbitBus.Setup(x => x.GetConnectionInfoAsync()).ReturnsAsync(connectionInfo);

            // Act
            var result = await _healthCheckService.CheckRabbitMQHealthAsync();

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Healthy");
            result.Name.Should().Be("RabbitMQ");
            result.Description.Should().Contain("RabbitMQ connection is active and responsive");
            result.Description.Should().Contain("response time:");
            result.Data.Should().BeEquivalentTo(connectionInfo);
            result.ResponseTime.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public async Task CheckRabbitMQHealthAsync_ShouldReturnWarning_WhenTimeoutExceptionOccurs()
        {
            // Arrange
            _mockRabbitBus.Setup(x => x.IsConnected).Returns(true);
            _mockRabbitBus.Setup(x => x.GetConnectionInfoAsync()).ThrowsAsync(new TimeoutException());

            // Act
            var result = await _healthCheckService.CheckRabbitMQHealthAsync();

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Warning");
            result.Name.Should().Be("RabbitMQ");
            result.Description.Should().Be("RabbitMQ health check timeout");
            result.ResponseTime.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public async Task CheckRabbitMQHealthAsync_ShouldReturnWarning_WhenGeneralExceptionOccurs()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            _mockRabbitBus.Setup(x => x.IsConnected).Returns(true);
            _mockRabbitBus.Setup(x => x.GetConnectionInfoAsync()).ThrowsAsync(exception);

            // Act
            var result = await _healthCheckService.CheckRabbitMQHealthAsync();

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Warning");
            result.Name.Should().Be("RabbitMQ");
            result.Description.Should().Be("RabbitMQ connection issue: Test exception");
            result.ResponseTime.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public async Task CheckRabbitMQHealthAsync_ShouldLogDebug_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            _mockRabbitBus.Setup(x => x.IsConnected).Returns(true);
            _mockRabbitBus.Setup(x => x.GetConnectionInfoAsync()).ThrowsAsync(exception);

            // Act
            await _healthCheckService.CheckRabbitMQHealthAsync();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("RabbitMQ health check failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CheckRabbitMQHealthAsync_ShouldCallCorrectMethods()
        {
            // Arrange
            var connectionInfo = new Dictionary<string, object>
            {
                ["IsConnected"] = true,
                ["ConnectionState"] = "Connected"
            };

            _mockRabbitBus.Setup(x => x.IsConnected).Returns(true);
            _mockRabbitBus.Setup(x => x.GetConnectionInfoAsync()).ReturnsAsync(connectionInfo);

            // Act
            var result = await _healthCheckService.CheckRabbitMQHealthAsync();

            // Assert
            _mockRabbitBus.Verify(x => x.IsConnected, Times.Once);
            _mockRabbitBus.Verify(x => x.GetConnectionInfoAsync(), Times.Once);
            result.Status.Should().Be("Healthy");
        }

        [Fact]
        public async Task CheckRabbitMQHealthAsync_ShouldNotCallGetConnectionInfo_WhenNotConnected()
        {
            // Arrange
            _mockRabbitBus.Setup(x => x.IsConnected).Returns(false);

            // Act
            await _healthCheckService.CheckRabbitMQHealthAsync();

            // Assert
            _mockRabbitBus.Verify(x => x.IsConnected, Times.Once);
            _mockRabbitBus.Verify(x => x.GetConnectionInfoAsync(), Times.Never);
        }
    }
}