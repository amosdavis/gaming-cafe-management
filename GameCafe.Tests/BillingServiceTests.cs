using GameCafe.Core.Services;
using GameCafe.Core.Models;

namespace GameCafe.Tests;

public class BillingServiceTests
{
    private readonly BillingService _service = new();

    [Fact]
    public void CalculateHourlyCost_30Minutes_ReturnsHourlyRate()
    {
        // Arrange
        const decimal hourlyRate = 5.00m;
        const int durationMinutes = 30;

        // Act
        var cost = _service.CalculateHourlyCost(hourlyRate, durationMinutes);

        // Assert
        Assert.Equal(5.00m, cost); // Rounds up to 1 hour
    }

    [Fact]
    public void CalculateHourlyCost_61Minutes_RoundsUpToTwoHours()
    {
        // Arrange
        const decimal hourlyRate = 5.00m;
        const int durationMinutes = 61;

        // Act
        var cost = _service.CalculateHourlyCost(hourlyRate, durationMinutes);

        // Assert
        Assert.Equal(10.00m, cost); // 1 hour 1 minute = 2 hours
    }

    [Fact]
    public void CalculateHourlyCost_60Minutes_ExactlyOneHour()
    {
        // Arrange
        const decimal hourlyRate = 5.00m;
        const int durationMinutes = 60;

        // Act
        var cost = _service.CalculateHourlyCost(hourlyRate, durationMinutes);

        // Assert
        Assert.Equal(5.00m, cost);
    }

    [Fact]
    public void CalculatePerMinuteCost_45Minutes_CalculatesExactly()
    {
        // Arrange
        const decimal ratePerMinute = 0.10m;
        const int durationMinutes = 45;

        // Act
        var cost = _service.CalculatePerMinuteCost(ratePerMinute, durationMinutes);

        // Assert
        Assert.Equal(4.50m, cost);
    }

    [Fact]
    public void CalculateSessionCost_WithHourlyRate_UsesHourlyCalculation()
    {
        // Arrange
        var rate = new BillingRate { BillingType = BillingModel.Hourly, Rate = 5.00m };
        var session = new Session { StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(45) };

        // Act
        var cost = _service.CalculateSessionCost(session, rate);

        // Assert
        Assert.Equal(5.00m, cost); // 45 minutes rounds up to 1 hour
    }

    [Fact]
    public void CalculateSessionCost_WithPerMinuteRate_UsesPerMinuteCalculation()
    {
        // Arrange
        var rate = new BillingRate { BillingType = BillingModel.PerMinute, Rate = 0.10m };
        var session = new Session { StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(30) };

        // Act
        var cost = _service.CalculateSessionCost(session, rate);

        // Assert
        Assert.Equal(3.00m, cost);
    }

    [Fact]
    public void CalculateSessionCost_WithFlatRate_ReturnsFixedCost()
    {
        // Arrange
        var rate = new BillingRate { BillingType = BillingModel.FlatRate, Rate = 2.99m };
        var session = new Session { StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(2) };

        // Act
        var cost = _service.CalculateSessionCost(session, rate);

        // Assert
        Assert.Equal(2.99m, cost);
    }

    [Fact]
    public void CalculateHourlyCost_ZeroDuration_ReturnsZero()
    {
        // Arrange
        const decimal hourlyRate = 5.00m;
        const int durationMinutes = 0;

        // Act
        var cost = _service.CalculateHourlyCost(hourlyRate, durationMinutes);

        // Assert
        Assert.Equal(0m, cost);
    }

    [Fact]
    public void CalculateHourlyCost_LargeAmount_CalculatesCorrectly()
    {
        // Arrange
        const decimal hourlyRate = 25.00m;
        const int durationMinutes = 150; // 2.5 hours

        // Act
        var cost = _service.CalculateHourlyCost(hourlyRate, durationMinutes);

        // Assert
        Assert.Equal(75.00m, cost); // Rounds up to 3 hours
    }
}
