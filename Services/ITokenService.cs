using System.Security.Claims;

namespace Login.Services
{
    public interface ITokenService
    {
        string CreateToken(string userId, string email, string? role, string MaPhongBan);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
