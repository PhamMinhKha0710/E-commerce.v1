
namespace E_commerce.v1.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}

public enum PaymentMethod
{
    Cod = 0,
    BankTransfer = 1,
    EWallet = 2
}

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1
}

public enum LoyaltyRank
{
    Silver = 0,
    Gold = 1,
    Diamond = 2
}