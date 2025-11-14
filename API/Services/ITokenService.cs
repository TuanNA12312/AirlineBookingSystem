using BusinessObject;

namespace API.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
