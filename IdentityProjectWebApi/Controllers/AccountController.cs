﻿using IdentityProject.Model;
using IdentityProjectWebApi.DataAccess.Identity;
using IdentityProjectWebApi.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
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
		private readonly RoleManager<AppRole> _roleManager;
		public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
			return RedirectToAction("Index", "Home");
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
				var userRoleAddResult = await _userManager.AddToRoleAsync(user, "Kullanıcı");
				if (!userRoleAddResult.Succeeded)
				{
					ModelState.AddModelError("", "Role Eklerken Hata Oluştu");
					return View(model);
				}
				ViewBag.Message = "Kayıt Başarılı";
				return RedirectToAction("Users");

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
		[Authorize(Roles = "Admin")]
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
			var modelgüncel = new AdminUserUpdatedViewModel() { Email = model.Email, Id = model.Id, PhoneNumber = model.PhoneNumber, Username = model.UserName, Inaktif = model.LockoutEnabled };
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
			if (model.Password != null && model.RePassword != null)
			{
				//buraya password yenileme yapılıcak
				var removePassword = await _userManager.RemovePasswordAsync(result);
				var newPassword = await _userManager.AddPasswordAsync(result, model.Password);
				if (!newPassword.Succeeded)
				{
					newPassword.Errors.ToList().ForEach(error => ModelState.AddModelError(error.Code, error.Description));
					return View();
				}
			}
			//inaktif durumu kontrolü değişimi
			if (model.Inaktif == false)
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
			//inaktif
			result.Email = model.Email;
			result.PhoneNumber = model.PhoneNumber;
			result.UserName = model.Username;

			var resultUpdate = _userManager.UpdateAsync(result);
			if (!resultUpdate.Result.Succeeded)
			{
				resultUpdate.Result.Errors.ToList().ForEach(x => ModelState.AddModelError(x.Code, x.Description));
				return View();
			}

			return RedirectToAction("Users");
		}
		[Authorize(Roles = "Admin")]
		[HttpGet]
		public async Task<IActionResult> UserCreateRole(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			var userRoles = await _userManager.GetRolesAsync(user);

			if (user == null)
			{
				ModelState.AddModelError("", "Kullanıcı Bulunamadı.");
				return View();
			}
			var roles = _roleManager.Roles.ToList();
			var model = new UsersAddRoleViewModel()
			{
				appUser = new UserDetail
				{
					Email = user.Email,
					Id = user.Id,
					Name = user.UserName,
					Role = userRoles.FirstOrDefault(),
				},
				roleLists = roles.Select(x => new SelectListItem { Text = x.Name, Value = x.Id }).ToList()
			};
			return View(model);
		}
		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> UserCreateRole(UsersAddRoleViewModel model,string roleId)
		{

			if (model.appUser.Id == null && roleId== null)
			{
				ModelState.AddModelError("","Kullanıcı gönderilmedi.");
				return RedirectToAction("Users");
			}
			var role = await _roleManager.FindByIdAsync(roleId);
			var user = await _userManager.FindByIdAsync(model.appUser.Id);
			var userDefaultRole = await _userManager.GetRolesAsync(user);
			if (user == null && role == null)
			{
				ViewBag.Message = "Veriler Getirilemedi";
				return RedirectToAction("Users");
			}
			//default u boş ise kullanıcıya rol eklemek isterse 
			else if (userDefaultRole.FirstOrDefault()==null && role!=null)
			{
				var userRoleAdd = await _userManager.AddToRoleAsync(user, role.Name);
				if (!userRoleAdd.Succeeded)
				{
					userRoleAdd.Errors.ToList().ForEach(x => ModelState.AddModelError(x.Code, x.Description));
					return RedirectToAction("Users");
				}
			}
			//default değeri değiştirmek isterse
			else if (userDefaultRole.FirstOrDefault() != role.Name)
			{  //kullanıcı rolü silinir ve yenisi eklenir.
				var deletedUserRole = await _userManager.RemoveFromRoleAsync(user, userDefaultRole.FirstOrDefault());
				if (!deletedUserRole.Succeeded)
				{
					deletedUserRole.Errors.ToList().ForEach(x => ModelState.AddModelError(x.Code, x.Description));
					return RedirectToAction("Users");
				}
				var userRoleAdd = await _userManager.AddToRoleAsync(user, role.Name);
				if (!userRoleAdd.Succeeded)
				{
					userRoleAdd.Errors.ToList().ForEach(x => ModelState.AddModelError(x.Code, x.Description));
					return RedirectToAction("Users");
				}
			}
			return RedirectToAction("Users");
		}
		[Authorize(Roles ="Admin")]
		[HttpGet]
		public IActionResult RolAdd()
		{
			return View(new AppRole());
		}
		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> RolAdd(AppRole role)
		{
			var result = await _roleManager.CreateAsync(role);
			if (!result.Succeeded)
			{
				result.Errors.ToList().ForEach(x => ModelState.AddModelError(x.Code, x.Description));
				return View();
			}
			return RedirectToAction("Rols");
		}
		public IActionResult Rols()
		{
			var roles = new RoleList { Roles = _roleManager.Roles.ToList() };
			return View(roles);
		}



	}
}
