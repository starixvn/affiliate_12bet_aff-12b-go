using Fanex.Affiliate.Link.Helpers;
using FluentAssertions;
using System;
using Xunit;

namespace Fanex.Affiliate.Link.Tests.Helpers;

public class DateTimeHelperTests
{
    [Fact]
    public void GmtMinus4TimeSpan_ShouldReturnCorrectTimeSpan()
    {
        // Act
        var result = DateTimeHelper.GmtMinus4TimeSpan;

        // Assert
        result.Should().Be(TimeSpan.FromHours(-4));
    }

    [Fact]
    public void ServerTimeOffset_ShouldReturnCurrentDateTimeOffset_WhenDefaultProvider()
    {
        // Act
        var result = DateTimeHelper.ServerTimeOffset;

        // Assert
        result.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ServerTime_ShouldReturnCurrentDateTime_WhenDefaultProvider()
    {
        // Act
        var result = DateTimeHelper.ServerTime;

        // Assert
        result.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GmtMinus4Now_ShouldReturnCorrectTime_WhenDefaultProvider()
    {
        // Act
        var result = DateTimeHelper.GmtMinus4Now;

        // Assert
        var expected = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(-4)).DateTime;
        result.Should().BeCloseTo(expected, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UtcNow_ShouldReturnCurrentUtcDateTime_WhenDefaultProvider()
    {
        // Act
        var result = DateTimeHelper.UtcNow;

        // Assert
        result.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateDateTime_ShouldReturnCorrectDateTime_WhenAllParametersProvided()
    {
        // Arrange
        var year = 2023;
        var month = 12;
        var day = 25;
        var hour = 15;
        var minute = 30;
        var second = 45;

        // Act
        var result = DateTimeHelper.CreateDateTime(year, month, day, hour, minute, second);

        // Assert
        result.Year.Should().Be(year);
        result.Month.Should().Be(month);
        result.Day.Should().Be(day);
        result.Hour.Should().Be(hour);
        result.Minute.Should().Be(minute);
        result.Second.Should().Be(second);
        result.Kind.Should().Be(DateTimeKind.Unspecified);
    }

    [Fact]
    public void CreateDateTime_ShouldReturnCorrectDateTime_WhenOnlyDateParametersProvided()
    {
        // Arrange
        var year = 2023;
        var month = 12;
        var day = 25;

        // Act
        var result = DateTimeHelper.CreateDateTime(year, month, day);

        // Assert
        result.Year.Should().Be(year);
        result.Month.Should().Be(month);
        result.Day.Should().Be(day);
        result.Hour.Should().Be(0);
        result.Minute.Should().Be(0);
        result.Second.Should().Be(0);
        result.Kind.Should().Be(DateTimeKind.Unspecified);
    }

    [Fact]
    public void ServerTimeOffset_ShouldReturnCustomDateTimeOffset_WhenProviderIsSet()
    {
        // Arrange
        var customDateTimeOffset = new DateTimeOffset(2023, 12, 25, 15, 30, 45, TimeSpan.Zero);
        var originalProvider = DateTimeHelper.DateTimeOffsetProvider;

        try
        {
            DateTimeHelper.DateTimeOffsetProvider = () => customDateTimeOffset;

            // Act
            var result = DateTimeHelper.ServerTimeOffset;

            // Assert
            result.Should().Be(customDateTimeOffset);
        }
        finally
        {
            // Restore original provider
            DateTimeHelper.DateTimeOffsetProvider = originalProvider;
        }
    }

    [Fact]
    public void ServerTime_ShouldReturnCustomDateTime_WhenProviderIsSet()
    {
        // Arrange
        var customDateTimeOffset = new DateTimeOffset(2023, 12, 25, 15, 30, 45, TimeSpan.Zero);
        var originalProvider = DateTimeHelper.DateTimeOffsetProvider;

        try
        {
            DateTimeHelper.DateTimeOffsetProvider = () => customDateTimeOffset;

            // Act
            var result = DateTimeHelper.ServerTime;

            // Assert
            result.Should().Be(customDateTimeOffset.DateTime);
        }
        finally
        {
            // Restore original provider
            DateTimeHelper.DateTimeOffsetProvider = originalProvider;
        }
    }

    [Fact]
    public void GmtMinus4Now_ShouldReturnCorrectTime_WhenProviderIsSet()
    {
        // Arrange
        var customDateTimeOffset = new DateTimeOffset(2023, 12, 25, 15, 30, 45, TimeSpan.Zero);
        var originalProvider = DateTimeHelper.DateTimeOffsetProvider;

        try
        {
            DateTimeHelper.DateTimeOffsetProvider = () => customDateTimeOffset;

            // Act
            var result = DateTimeHelper.GmtMinus4Now;

            // Assert
            var expected = customDateTimeOffset.ToOffset(TimeSpan.FromHours(-4)).DateTime;
            result.Should().Be(expected);
        }
        finally
        {
            // Restore original provider
            DateTimeHelper.DateTimeOffsetProvider = originalProvider;
        }
    }

    [Fact]
    public void UtcNow_ShouldReturnCustomUtcDateTime_WhenProviderIsSet()
    {
        // Arrange
        var customDateTimeOffset = new DateTimeOffset(2023, 12, 25, 15, 30, 45, TimeSpan.Zero);
        var originalProvider = DateTimeHelper.DateTimeOffsetProvider;

        try
        {
            DateTimeHelper.DateTimeOffsetProvider = () => customDateTimeOffset;

            // Act
            var result = DateTimeHelper.UtcNow;

            // Assert
            result.Should().Be(customDateTimeOffset.UtcDateTime);
        }
        finally
        {
            // Restore original provider
            DateTimeHelper.DateTimeOffsetProvider = originalProvider;
        }
    }
}

