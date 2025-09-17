namespace Login.Services
{
    public interface ITokenService
    {
        string CreateToken(string userId, string email, string? role, string MaPhongBan);
    }
}
