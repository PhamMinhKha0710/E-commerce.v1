using MediatR;

namespace E_commerce.v1.Application.Features.Shipping.Commands.ProcessAhamoveWebhook;

public record ProcessAhamoveWebhookCommand(System.Text.Json.JsonElement Payload) : IRequest<Unit>;
