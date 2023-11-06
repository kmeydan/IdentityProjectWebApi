using IdentityProjectWebApi.DataAccess.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace IdentityProjectWebApi.CustomValidation
{
    public class IdentityPasswordValidation:IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            
            throw new System.NotImplementedException();
        }

    }
}
