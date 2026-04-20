using MediatR;

namespace E_commerce.v1.Application.Features.Payment.Commands.ProcessPayosWebhook;

public sealed record ProcessPayosWebhookCommand(string RawBody) : IRequest<Unit>;

