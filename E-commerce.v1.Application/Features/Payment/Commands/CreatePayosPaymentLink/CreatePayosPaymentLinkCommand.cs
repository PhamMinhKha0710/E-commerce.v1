using MediatR;

namespace E_commerce.v1.Application.Features.Payment.Commands.CreatePayosPaymentLink;

public sealed record CreatePayosPaymentLinkCommand(
    Guid UserId,
    Guid OrderId,
    decimal? ClientTotalAmount,
    string? Description) : IRequest<CreatePayosPaymentLinkResponse>;

public sealed class CreatePayosPaymentLinkResponse
{
    public Guid OrderId { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public string? PaymentLinkId { get; set; }
    public long? OrderCode { get; set; }
}

