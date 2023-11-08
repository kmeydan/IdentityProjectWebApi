using IdentityProjectWebApi.DataAccess.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace IdentityProject.Model
{
	public class UsersAddRoleViewModel
	{
		public List<SelectListItem> roleLists { get; set; }
		public UserDetail appUser { get; set; }
	}
	public class UserDetail
	{
		public string Id { get; set; }
		public string Role { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
	}
}
