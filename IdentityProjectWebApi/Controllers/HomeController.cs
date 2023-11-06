using IdentityProjectWebApi.DataAccess.Identity;
using IdentityProjectWebApi.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityProject.Controllers
{
	public class HomeController : Controller
	{
		private readonly SignInManager<AppUser> _signInManager;
		private readonly UserManager<AppUser> _userManager;

		public HomeController(SignInManager<AppUser> signInManager,UserManager<AppUser> userManager)
		{
			_signInManager= signInManager;
			_userManager= userManager;
		}

		

		public IActionResult Index()
		{
			return View();
		}
		public IActionResult Login()
		{
			return View(new UserViewModel());
		}
		[HttpPost]
		public async Task<IActionResult> Login(UserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ModelState.AddModelError("", "Validasyon Hatası");
				return View();
			}
			var user = await _userManager.FindByEmailAsync(model.EMail);
			if (user == null)
			{
				ModelState.AddModelError("", "E-mail e ait kullanıcı bulunamadı.");
				return View(model);
			}
			if (user.LockoutEnabled == false)
			{
				ModelState.AddModelError("", "Hesabınız Aktif Değil");
				return View();
			}

			var result = await _signInManager.PasswordSignInAsync(user, model.Sifre, model.RememberMe, false);
			if (result.Succeeded)
			{
				return RedirectToAction("Index", "Account");

			}
			else
			{
				ModelState.AddModelError(string.Empty, "Şifre Hatalı");
				return View();
			}


		}
		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}
