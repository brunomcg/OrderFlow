namespace OrderFlow.API.Endpoints.DTOs;
public sealed class CustomLoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}
