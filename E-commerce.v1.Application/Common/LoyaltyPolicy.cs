using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Common;

public static class LoyaltyPolicy
{
    private const decimal PointUnitAmount = 100_000m;

    public static int CalculateEarnedPoints(decimal orderGrandTotal)
    {
        if (orderGrandTotal <= 0)
            return 0;

        return (int)Math.Floor(orderGrandTotal / PointUnitAmount);
    }

    public static LoyaltyRank ResolveRank(int loyaltyPoints)
    {
        if (loyaltyPoints >= 1_000)
            return LoyaltyRank.Diamond;
        if (loyaltyPoints >= 300)
            return LoyaltyRank.Gold;
        return LoyaltyRank.Silver;
    }

    public static decimal GetRankDiscountPercent(LoyaltyRank rank) => rank switch
    {
        LoyaltyRank.Diamond => 0.10m,
        LoyaltyRank.Gold => 0.05m,
        _ => 0m
    };
}
