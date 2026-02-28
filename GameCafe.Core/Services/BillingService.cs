using GameCafe.Core.Models;

namespace GameCafe.Core.Services;

public interface IBillingService
{
    decimal CalculateSessionCost(Session session, BillingRate rate);
    decimal CalculateHourlyCost(decimal hourlyRate, int durationMinutes);
    decimal CalculatePerMinuteCost(decimal ratePerMinute, int durationMinutes);
}

public class BillingService : IBillingService
{
    public decimal CalculateSessionCost(Session session, BillingRate rate)
    {
        return rate.BillingType switch
        {
            BillingModel.Hourly => CalculateHourlyCost(rate.Rate, session.DurationMinutes),
            BillingModel.PerMinute => CalculatePerMinuteCost(rate.Rate, session.DurationMinutes),
            BillingModel.FlatRate => rate.Rate,
            _ => 0m
        };
    }

    public decimal CalculateHourlyCost(decimal hourlyRate, int durationMinutes)
    {
        if (durationMinutes == 0) return 0m;
        
        // Calculate number of hours, rounding up fractional hours
        var hoursRoundedUp = Math.Ceiling(durationMinutes / 60m);
        return hoursRoundedUp * hourlyRate;
    }

    public decimal CalculatePerMinuteCost(decimal ratePerMinute, int durationMinutes)
    {
        return durationMinutes * ratePerMinute;
    }
}
