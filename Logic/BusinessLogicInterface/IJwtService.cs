using Database.Entities;

namespace Logic.BusinessLogicInterface
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user, string roleName);
    }
}
