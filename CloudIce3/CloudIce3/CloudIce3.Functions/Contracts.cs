namespace CloudIce3.Functions;

public record RegisterRequest(string Username, string Email, string Password, string? DisplayName);
public record LoginRequest(string UsernameOrEmail, string Password);
public record AuthResponse(string Token, string Username, string Email, string DisplayName, string BlobUrl);
public record ProfileGetResponse(string Username, string Email, string DisplayName, string BlobUrl);
public record ProfileUpdateRequest(string Email, string DisplayName, string? BlobUrl);
