using IdentityProject.Model;
using IdentityProjectWebApi.DataAccess.Identity;
using IdentityProjectWebApi.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityProjectWebApi.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private readonly SignInManager<AppUser> _signInManager;
		private readonly UserManager<AppUser> _userManager;

		public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
		}
		//home controller oluştur index ve logini account dan çıkar ve account a Authorize yap
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
			if (user.LockoutEnabled==false)
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

		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
			return RedirectToAction("Index","Home");
		}
		public IActionResult Register()
		{
			return View(new AdminUserCreateViewModel());
		}
		[HttpPost]
		public async Task<IActionResult> Register(AdminUserCreateViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ModelState.AddModelError("", "Validasyon hatası");
				return View();

			}
			var emailresult = await _userManager.FindByEmailAsync(model.Email);
			var kullaniciadi = await _userManager.FindByNameAsync(model.Username);
			if (emailresult != null)
			{
				ModelState.AddModelError("", "Böyle bir e-mail mevcut.");
				return View();

			}
			if (kullaniciadi != null)
			{
				ModelState.AddModelError("", "Böyle bir kullanıcı adı mevcut.");
				return View();


			}
			AppUser user = new AppUser
			{
				Email = model.Email,
				PhoneNumber = model.PhoneNumber,
				UserName = model.Username,

			};
			var result = await _userManager.CreateAsync(user, model.Password);
			if (result.Succeeded)
			{
				ViewBag.Message = "Kayıt Başarılı";
				return RedirectToAction("Login");

			}
			else
			{
				result.Errors.ToList().ForEach(error => ModelState.AddModelError(error.Code, error.Description));
				return View();

			}


		}
		public IActionResult Users()
		{
			
			var models = new UserLists { Users = _userManager.Users.ToList() };
			return View(models);
		}

		[HttpGet]
		public async Task<IActionResult> Delete(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user != null)
			{
				var result = await _userManager.DeleteAsync(user);
				if (result.Succeeded)
				{
					return RedirectToAction("Users");
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(error.Code, error.Description);
					}
					return RedirectToAction("Users");
				}
			}
			else
			{
				ModelState.AddModelError("", "Kullanıcı Bulunamadı");
			}


			return RedirectToAction("Users");

		}
		public async Task<IActionResult> Detail(string id)
		{
			var model = await _userManager.FindByIdAsync(id);
			var modelgüncel=new AdminUserUpdatedViewModel() { Email= model.Email ,Id=model.Id,PhoneNumber=model.PhoneNumber,Username=model.UserName,Inaktif=model.LockoutEnabled};
			return View(modelgüncel);
		}
		[HttpPost]
		public async Task<IActionResult> Detail(AdminUserUpdatedViewModel model)
		{
			var result = await _userManager.FindByIdAsync(model.Id);
			if (!ModelState.IsValid)
			{
				ModelState.AddModelError("", "Validasyon Hatası");
				return View();
			}
			if (model.Password!=null&&model.RePassword!=null)
			{
				//buraya password yenileme yapılıcak
			}
			if (model.Inaktif==false)
			{
				//Hesap İnaktif İşaretlendiyse Kapat
				var inaktif = await _userManager.SetLockoutEnabledAsync(result, false);
				if (!inaktif.Succeeded)
				{
					inaktif.Errors.ToList().ForEach(error => ModelState.AddModelError(error.Code, error.Description));
					return View();
				}
			}
			else
			{
				//Hesap Kapalıysa Aktif Et İşaretlenmiştir (Eğer True ise Devam Et(Mevcut true dur))
				if (!await _userManager.GetLockoutEnabledAsync(result))
				{
					var actived = await _userManager.SetLockoutEnabledAsync(result, true);
					if (!actived.Succeeded)
					{
						actived.Errors.ToList().ForEach(error => ModelState.AddModelError(error.Code, error.Description));
						return View();
					}

				}
				
			}
			result.Email= model.Email;
			result.PhoneNumber= model.PhoneNumber;
			result.UserName = model.Username;

			var resultUpdate=_userManager.UpdateAsync(result);
			if (!resultUpdate.Result.Succeeded)
			{
				resultUpdate.Result.Errors.ToList().ForEach(x => ModelState.AddModelError(x.Code, x.Description));
				return View();
			}
			
			return RedirectToAction("Users");
		}

	}
}
