using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Calendar.Infrastructure.Security;

public sealed class JwtAccessTokenService(IOptions<JwtOptions> options) : IAccessTokenService
{
    private const string BearerTokenType = "Bearer";
    private readonly JwtOptions _options = options.Value;

    public AccessToken CreateForAdmin(Admin admin)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, admin.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("account_type", "admin"),
            new Claim("business_id", admin.BusinessId.ToString()),
            new Claim("display_name", admin.DisplayName)
        };

        return CreateToken(claims);
    }

    public AccessToken CreateForCustomer(Customer customer)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, customer.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new Claim(ClaimTypes.Role, "Customer"),
            new Claim("account_type", "customer"),
            new Claim("first_name", customer.FirstName)
        };

        return CreateToken(claims);
    }

    private AccessToken CreateToken(IEnumerable<Claim> claims)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAtUtc = now.AddMinutes(_options.AccessTokenMinutes);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: signingCredentials);

        return new AccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            BearerTokenType,
            expiresAtUtc);
    }
}
