using Microsoft.AspNetCore.Identity;

namespace IdentityProjectWebApi.CustomValidation
{
    public class ErrorDescriber:IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError { Code = "DuplicateEmail", Description = $"{email} mail adresi mevcut. Farklı bir mail adresi giriniz." };
        }
    }
}
