using System.Data;
using CripexLibrary.Data;
using CripexLibrary.Models;
using CripexLibrary.Models.Enums;
using CripexLibrary.Services.EmailService;
using CripexLibrary.Services.FileUploadService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class Program
{
	private static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container. 
		// register a connection to db and ApplicationDbContext
		var connectionString = builder.Configuration.GetConnectionString("CripexLibraryConnection") ?? throw new InvalidOperationException("Connection string 'CripexLibraryConnection' not found.");
		builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString)
					   .UseLazyLoadingProxies());


		//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

		// service for custom Identity
		builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
			{
				options.SignIn.RequireConfirmedAccount = false;
				options.SignIn.RequireConfirmedPhoneNumber = false;
				options.SignIn.RequireConfirmedEmail = false;
				options.Lockout.AllowedForNewUsers = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequiredLength = 6;
				options.Password.RequireDigit = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireUppercase = false;
				options.Password.RequiredUniqueChars = 0;
			})
				.AddRoles<ApplicationRole>()
				.AddUserManager<UserManager<ApplicationUser>>()
				.AddRoleManager<RoleManager<ApplicationRole>>()
				.AddDefaultTokenProviders()
				.AddDefaultUI()
				.AddEntityFrameworkStores<ApplicationDbContext>();

		// add razor pages
		builder.Services.AddRazorPages();
		//builder.Services.AddMvc().AddRazorOptions(options => { options.ViewLocationFormats.Add("/ViewComponents/{0}.cshtml"); });
		builder.Services.AddControllersWithViews();
		builder.Services.AddScoped<IFileUploadService, LocalFileUploadService>();
		builder.Services.AddScoped<IEmailService, EmailServices>();
		builder.Services.AddAuthorization(options =>
		{
			options.AddPolicy("AdminOrLibrarian", policy =>
				policy.RequireRole("ADMIN", "LIBRARIAN"));
		});

		var app = builder.Build();

		using (var scope = app.Services.CreateScope())
		{
			var roleManger = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
			var userManger = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

			//create default Roles for application
			var roleName = new[] { RoleType.ADMIN.ToString(), RoleType.LIBRARIAN.ToString(), RoleType.MEMBER.ToString() };

			foreach (var role in roleName)
			{
				if (!await roleManger.RoleExistsAsync(role))
				{
					await roleManger.CreateAsync(new ApplicationRole(role));
				}
			}

			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			//create default admin user
			var powerUser = new ApplicationUser
			{
				UserName = configuration.GetSection("Admin")["UserName"],
				NormalizedUserName = configuration.GetSection("Admin")["NormalizedUname"],
				Email = configuration.GetSection("Admin")["Email"],
				EmailConfirmed = true,
				LockoutEnabled = false,
				DateJoined = DateTime.UtcNow.ToLocalTime(),
			};
			
			var pwd = configuration.GetSection("Admin")["Password"];
			var email = configuration.GetSection("Admin")["Email"];
			var _user = await userManger.FindByEmailAsync(email);

			if (_user == null)
			{
				var createPowerUser = await userManger.CreateAsync(powerUser, pwd);
				if (createPowerUser.Succeeded)
				{
					await userManger.AddToRoleAsync(powerUser, "ADMIN");
				}
			}

			//create default library manager
			var manager = new ApplicationUser
			{
				UserName = configuration.GetSection("Manager")["UserName"],
				Email = configuration.GetSection("Manager")["Email"],
				EmailConfirmed = true,
				LockoutEnabled = false,
				DateJoined = DateTime.UtcNow.ToLocalTime(),
			};

			var pwd1 = configuration.GetSection("Manager")["Password"];
			var email1 = configuration.GetSection("Manager")["Email"];
			var _manager = await userManger.FindByEmailAsync(email1);

			if (_manager == null) {
				var createLibraryManager = await userManger.CreateAsync(manager, pwd1);
				if (createLibraryManager.Succeeded)
				{
					await userManger.AddToRoleAsync(manager, "LIBRARIAN");
				}
			}

			//create default member
			var member = new ApplicationUser
			{
				UserName = configuration.GetSection("TestUser")["UserName"],
				Email = configuration.GetSection("TestUser")["Email"],
				EmailConfirmed = true,
				LockoutEnabled = false,
				DateJoined = DateTime.UtcNow.ToLocalTime(),
			};

			var pwd2 = configuration.GetSection("TestUser")["Password"];
			var email2 = configuration.GetSection("TestUser")["Email"];
			var _member = await userManger.FindByEmailAsync(email2);

			if (_member == null)
			{
				var createMember = await userManger.CreateAsync(member, pwd2);
				if (createMember.Succeeded)
				{
					await userManger.AddToRoleAsync(member, "MEMBER");
				}
			}
		}


		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Home/Error");
			app.UseHsts();
		}

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");
		app.MapRazorPages();

		app.Run();
	}
}