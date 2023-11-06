using System.ComponentModel.DataAnnotations;

namespace IdentityProjectWebApi.Model
{
    public class UserViewModel
    {
        [Required(ErrorMessage ="Boş Geçilemez")]
        public string EMail { get; set; }
        [Required]
        public string Sifre { get; set; }
        public bool RememberMe { get; set; }
    }
}
