using System.Security.Claims;

namespace Client.Interfaces.Authorisation
{
    public interface ITokenServices
    {
        IEnumerable<Claim> ParseClaimsFromJwt(string jwt);
        Task StoreRoleToken(string roleToken);
        Task<string?> GetRoleToken();
        Task RemoveRoleToken();
    }
}