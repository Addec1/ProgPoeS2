using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CloudIce3.Functions;

public class JwtValidator
{
    private readonly TokenValidationParameters _p;

    public JwtValidator(IConfiguration cfg)
    {
        _p = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = cfg["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = cfg["JWT:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["JWT:Key"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }

    public ClaimsPrincipal? Validate(string? authHeader)
    {
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        var token = authHeader["Bearer ".Length..].Trim();
        var handler = new JwtSecurityTokenHandler();
        try { return handler.ValidateToken(token, _p, out _); }
        catch { return null; }
    }
}
