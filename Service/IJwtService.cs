using BusinessObject;

namespace Service;

public interface IJwtService
{
    string GenerateToken(User user, List<string> roles, List<string> resources);
    string GenerateRefreshToken(User user, Dictionary<string, object> extraClaims);
}