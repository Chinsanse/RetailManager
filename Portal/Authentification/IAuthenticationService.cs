using Portal.Models;

namespace Portal.Authentification
{
    public interface IAuthenticationService
    {
        Task<AuthenticatedUserModel> Login(AuthenticationUserModel userForAuthentication);
        Task LogOut();
    }
}