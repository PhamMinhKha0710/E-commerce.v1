
namespace E_commerce.v1.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    /// <summary>In transit with carrier (e.g. Ahamove).</summary>
    Delivering = 4
}

public enum PaymentMethod
{
    Cod = 0,
    BankTransfer = 1,
    EWallet = 2
}

public enum PaymentProvider
{
    PayOS = 0
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Expired = 3,
    Cancelled = 4
}

public enum StockReservationStatus
{
    Reserved = 0,
    Converted = 1,
    Released = 2
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