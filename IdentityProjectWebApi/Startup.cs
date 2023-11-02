using IdentityProjectWebApi.DataAccess.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityProjectWebApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));
			services.AddDefaultIdentity<AppUser>()
				.AddEntityFrameworkStores<AppIdentityDbContext>();
		//	services.AddIdentity<AppUser, AppRole>(options =>
		//	{
		//		// Password settings.
		//		options.Password.RequireDigit = true;
		//		options.Password.RequireLowercase = true;
		//		options.Password.RequireNonAlphanumeric = true;
		//		options.Password.RequireUppercase = true;
		//		options.Password.RequiredLength = 6;
		//		options.Password.RequiredUniqueChars = 1;

		//		// Lockout settings.
		//		options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
		//		options.Lockout.MaxFailedAccessAttempts = 5;
		//		options.Lockout.AllowedForNewUsers = true;

		//		// User settings.
		//		options.User.AllowedUserNameCharacters =
		//		"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
		//		options.User.RequireUniqueEmail = false;
		//	});
		//	services.ConfigureApplicationCookie(options =>
		//	{
		//		// Cookie settings
		//		options.Cookie.HttpOnly = true;
		//		options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

		//		options.LoginPath = "/Identity/Account/Login";
		//		options.AccessDeniedPath = "/Identity/Account/AccessDenied";
		//		options.SlidingExpiration = true;
		//	});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
