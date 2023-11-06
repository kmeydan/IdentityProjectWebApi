using Microsoft.AspNetCore.Identity;

namespace IdentityProjectWebApi.CustomValidation
{
    public class ErrorDescriber:IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError { Code = "DuplicateEmail", Description = $"{email} mail adresi mevcut. Farklı bir mail adresi giriniz." };
        }
		public override IdentityError PasswordTooShort(int length)
		{
			return new IdentityError { Code = "PasswordTooShort", Description = $"Parolanız en az 6 karakter içermelidir." };
		}
		public override IdentityError PasswordRequiresDigit()
		{
			return new IdentityError { Code = "PasswordRequiresDigit", Description = $"Parolanız en az 1 adet sayı içermelidir" };
		}
		public override IdentityError PasswordRequiresUpper()
		{
			return new IdentityError { Code = "PasswordRequiresUpper", Description = $"Parolanız en az 1 adet Büyük Harf içermelidir" };
		}
		public override IdentityError PasswordRequiresNonAlphanumeric()
		{
			return new IdentityError { Code = "PasswordRequiresNonAlphanumeric", Description = $"Parolanız en az 1 adet AlphaNumeric harf içermelidir" };
		}
	}
}
