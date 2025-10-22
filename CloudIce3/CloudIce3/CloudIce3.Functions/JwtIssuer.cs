using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CloudIce3.Functions;

public class JwtIssuer
{
    private readonly string _issuer, _aud, _key;

    public JwtIssuer(IConfiguration cfg)
    {
        _issuer = cfg["JWT:Issuer"]!;
        _aud = cfg["JWT:Audience"]!;
        _key = cfg["JWT:Key"]!;
    }

    public string Issue(string username, string role, string email, TimeSpan? life = null)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _issuer, _aud, claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(life ?? TimeSpan.FromHours(6)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}