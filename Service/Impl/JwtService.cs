using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessObject;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Service.Impl;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Generate access token with role claim
    public string GenerateToken(User user, List<string> roles, List<string> resources)
    {
        var email = user.Email;
        //var password = user.Password;


        var claims = new List<Claim>
        {
            new("Email", email),
            new("userid", user.UserId.ToString()),

            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("roles", string.Join(",", roles)),
            new("resources", string.Join(",", resources))
            //new Claim("Password", password),
            //new Claim(ClaimTypes.Role, role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Generate refresh token
    public string GenerateRefreshToken(User user, Dictionary<string, object> extraClaims)
    {
        var email = user.Email;
        //var password = user.Password;
        //var role = user.Role;

        var claims = new List<Claim>
        {
            new("Email", email),
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            //new Claim("Password", password),
            //new Claim(ClaimTypes.Role, role.ToString())
        };

        foreach (var claim in extraClaims) claims.Add(new Claim(claim.Key, claim.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Extract username from token
    public string ExtractUserName(string token)
    {
        var claims = ExtractAllClaims(token);
        return claims?.FindFirst(ClaimTypes.Name)?.Value;
    }

    // Extract roles from token
    public string ExtractRoles(string token)
    {
        var claims = ExtractAllClaims(token);
        return claims?.FindFirst("roles")?.Value;
    }

    // Check if the token is valid for the user
    public bool IsTokenValid(string token, ClaimsPrincipal userPrincipal)
    {
        var username = ExtractUserName(token);
        return username == userPrincipal.Identity.Name && !IsTokenExpired(token);
    }

    // Check if the token is expired
    private bool IsTokenExpired(string token)
    {
        var claims = ExtractAllClaims(token);
        var expiration = claims?.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        if (expiration == null) return true;

        var expirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiration));
        return expirationDate.UtcDateTime < DateTime.UtcNow;
    }

    // Extract all claims from token
    private ClaimsPrincipal ExtractAllClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}