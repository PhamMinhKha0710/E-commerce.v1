
namespace E_commerce.v1.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2
}

public enum PaymentMethod
{
    Cod = 0,
    BankTransfer = 1,
    EWallet = 2
}