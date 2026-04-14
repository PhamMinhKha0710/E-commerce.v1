namespace E_commerce.v1.Application.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string? Email { get; }
    string GetActor();
}
