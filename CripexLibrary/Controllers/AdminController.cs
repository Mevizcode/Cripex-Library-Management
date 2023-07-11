using System.Globalization;
using CripexLibrary.Data;
using CripexLibrary.Models;
using CripexLibrary.Models.ViewModels;
using CripexLibrary.Services.FileUploadService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Reflection.Metadata.BlobBuilder;

namespace CripexLibrary.Controllers
{
	[Authorize(Roles = "ADMIN")]
	public class AdminController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<ApplicationRole> _roleManger;
		private readonly ApplicationDbContext _context;
		private readonly IFileUploadService _fileUploadService;
		private readonly ILogger<AdminController> _logger;
		public const int PAGESIZE = 5;
		public AdminController(UserManager<ApplicationUser> userManager,
							   RoleManager<ApplicationRole> roleManger,
							   ApplicationDbContext context,
							   IFileUploadService fileUploadService,
							   ILogger<AdminController> logger)
		{
			_userManager = userManager;
			_roleManger = roleManger;
			_context = context;
			_fileUploadService = fileUploadService;
			_logger = logger;
		}


		[HttpGet]
		public IActionResult Index(int pageNumber = 1)
		{
			if (pageNumber < 1) {
				pageNumber = 1;
			}

			var countMembers = _userManager.Users.Count();
			ViewBag.MemberCount = countMembers;
			var countRoles = _roleManger.Roles.Count();
			ViewBag.RoleCount = countRoles;
			var countAssignedRoles = _context.UserRoles.Count();
			ViewBag.AssignedRoles = countAssignedRoles;

			var members = _userManager.Users.ToList();
			var totalItems = members.Count();

			var paging = new Pagination("Index", "Admin", totalItems, pageNumber, PAGESIZE);

			var data = members.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

			//Group users by roles
			var roles = members.SelectMany(u => u.UserRoles)
							   .Select(ur => ur.Role.Name)
							   .Distinct()
							   .ToList();

			var roleCounts = roles.Select(r => new
			{
				Role = r,
				Count = members.Count(u => u.UserRoles.Any(ur => ur.Role.Name == r))
			}).ToList();

			//calculate the percentage of members in each group
			var totalusers = members.Count;
			var roleData = roleCounts.Select(rc => new
			{
				Role = rc.Role,
				Percentage = (double)rc.Count / totalusers * 100
			}).ToList();

			ViewBag.Rolelabels = roleData.Select(rd => rd.Role).ToList();
			ViewBag.RolePercentages = roleData.Select(rd => rd.Percentage).ToList();

			var userRegistrationData = _context.Users.GroupBy(u => new
			{
				Year = u.DateJoined.Year,
				Month = u.DateJoined.Month
			})
				.Select(g => new
				{
					Month = g.Key.Month,
					Year = g.Key.Year,
					Count = g.Count()
				})
				.OrderBy(g => g.Year)
				.ThenBy(g => g.Month)
				.ToList();

			var labels = new List<string>();
			var datas = new List<int>();

			foreach (var item in userRegistrationData)
			{
				labels.Add($"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month)} {item.Year}");
				datas.Add(item.Count);
			}

			ViewBag.UserRegistrationLabels = labels;
			ViewBag.UserRegistrationData = datas;

			return View(new LibraryViewModel<ApplicationUser> {
				Items = data,
				Pagination = paging
			});
		}

		public ActionResult Search(string query)
		{

			if (User.IsInRole("ADMIN"))
			{
				var users = _context
								.Users
								.Where(u => u.Name.Contains(query) 
								|| u.UserName.StartsWith(query) 
								|| u.Email.Contains(query))
								.ToList();

				//if (pageNumber < 1)
				//{
				//	pageNumber = 1;
				//}

				var totalItems = users.Count;
				ViewBag.TotalItems = totalItems;

				//var paging = new Pagination("Search", "Admin", totalItems, pageNumber, PAGESIZE);

				//var data = users.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

				return View("_UserSearchResults", users);
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public IActionResult AddMember()
		{

			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddNewMember([Bind("Email,Password,ConfirmPassword")] InputViewModel model)
		{
			if (ModelState.IsValid)
			{
				var defaultImg = "user_pp_placeholder.png";

				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					DateJoined = DateTime.UtcNow.ToLocalTime(),
					ProfilePhoto = defaultImg
				};

				var result = await _userManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{
					await _userManager.FindByEmailAsync(model.Email);
					await _userManager.AddToRoleAsync(user, "MEMBER");
				}
			}

			return RedirectToAction(nameof(Index)); ;
		}
		[HttpGet]
		public async Task<IActionResult> Details(Guid? Id)
		{
			if (Id == null) 
			{ 
				return NotFound();
			}
			
			var member = await _context.ApplicationUsers.FindAsync(Id);
			if (member == null)
			{
				return NotFound();
			}
			return View(member);
		}

		public async Task<IActionResult> EditMember(Guid? Id)
		{
			if (Id == null || _context.ApplicationUsers == null)
			{
				return NotFound("User not found!");
			}
			var member = await _context.ApplicationUsers.FindAsync(Id);
			
			return View(member);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		#nullable enable
		public async Task<IActionResult> EditMember(Guid Id, [Bind("Name, Gender, DOB, Address, ProfilePhoto, PhoneNumber")] UserInputViewModel userModel, IFormFile? profilePic)
		{
			var user = await _context.Users.FindAsync(Id);
			if (ModelState.IsValid)
			{
				
				try
				{
					string fileName = "";
					if ((user?.ProfilePhoto == "user_pp_placeholder.png") && profilePic != null && profilePic.Length > 0)
					{
						fileName = await _fileUploadService.UploadFileAsync(profilePic, true);
					}
					else
					{
						fileName = "user_pp_placeholder.png";
					}

					if (UserExists(Id) && (user != null))
					{
						user.Name = userModel.Name;
						user.Gender = userModel.Gender;
						user.DOB = userModel.DOB;
						user.Address = userModel.Address;
						user.ProfilePhoto = fileName;
						user.PhoneNumber = userModel.PhoneNumber;

						_context.Entry(user).State = EntityState.Modified;
					}
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!UserExists(Id))
					{
						return NotFound();
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(user);

		}

		public  IActionResult DeleteMember(Guid? Id)
		{
			var member = _context.ApplicationUsers.Find(Id);
			if ((member == null) || !UserExists(member.Id))
			{
				return BadRequest("User does not exist!");
			}

			_context.Entry(member).State = EntityState.Deleted;
			_context.SaveChanges();
			
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Roles()
		{
			return View(_context.Roles.ToList());
		}

		[HttpGet]
		public IActionResult AddRole()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> AddRole([Bind("Description, Name")] RoleViewModel role)
		{
			if (ModelState.IsValid)
			{
				var newRole = new ApplicationRole
				{
					Name = role.Name.ToUpper(),
					Description = role.Description,
					NormalizedName = role.Name.ToUpper()
					
				};
				_context.ApplicationRoles.Add(newRole);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Roles));
		}

		[HttpGet]
		public IActionResult EditRole(Guid? Id)
		{
			if (Id == null || _context.ApplicationRoles == null)
			{
				return NotFound();
			}

			var roles = _context.ApplicationRoles.Find(Id);

			return View(roles);
		}

		[HttpPost]
		public async Task<IActionResult> EditRole(Guid Id, [Bind("Description, Name")] RoleViewModel role)
		{
			var roles = _context.ApplicationRoles.Find(Id);
			if (ModelState.IsValid)
			{
				try
				{
					if (roles is not null)
					{
						roles.Name = role.Name;
						roles.Description = role.Description;

						_context.ApplicationRoles.Update(roles);
					}
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					return NotFound();
				}
			}
			return RedirectToAction(nameof(Roles));
		}

		public IActionResult DeleteRole(Guid? Id)
		{
			var role = _context.ApplicationRoles.Find(Id);
			if ((role == null) || !RoleExists(role.Id))
			{
				return BadRequest("Role does not exist!");
			}

			_context.Entry(role).State = EntityState.Deleted;
			_context.SaveChanges();

			return RedirectToAction(nameof(Roles));
		}


		[HttpGet]
		public async Task<IActionResult> AddMemberToRole()
		{
			var users = await _userManager.Users.Select(u => u.UserName).ToListAsync();
			var roles = await _roleManger.Roles.Select(r => r.Name).ToListAsync();

			var viewModel = new AddMemberToRoleViewModel
			{
				Users = users,
				Roles = roles
			};

			return View(viewModel);
		}

		public async Task<IActionResult> AddMemberToRole(string userName, string roleName)
		{
			var user = await _userManager.FindByNameAsync(userName);
			if (user == null)
			{
				return NotFound();
			}

			var role = await _roleManger.FindByNameAsync(roleName);
			if (role == null)
			{
				return NotFound();
			}

			var result = await _userManager.AddToRoleAsync(user, roleName);
			if (!result.Succeeded)
			{
				return BadRequest();
			}

			return RedirectToAction(nameof(UsersInRole));
		}

		public async Task<IActionResult> UsersInRole()
		{

			var userRoles = await _context.UserRoles.Include(ur => ur.User).Include(ur => ur.Role).ToListAsync();

			var usersAndRoles = new List<string>();

			foreach (var userRole in userRoles)
			{
				ViewBag.UserId = userRole.UserId;
				ViewBag.RoleName = userRole.Role.Name;

				var userRoleString = $"{userRole.User.UserName} - {userRole.Role.Name}";
				usersAndRoles.Add(userRoleString);
			}
			ViewBag.usersAndRoles = usersAndRoles;

			return View(usersAndRoles);
		}

		//public async Task<IActionResult> RemoveMemberFromRole(string userId, string roleName)
		//{
		//	var user = await _userManager.FindByIdAsync(userId);

		//	if (user == null)
		//	{
		//		return NotFound();
		//	}

		//	var result = await _userManager.RemoveFromRoleAsync(user, roleName);

		//	if (result.Succeeded)
		//	{
		//		return RedirectToAction(nameof(UsersInRole));
		//	}
		//	else
		//	{
		//		foreach (var error in result.Errors)
		//		{
		//			ModelState.AddModelError("", error.Description);
		//		}

		//		return View();
		//	}
		//}

		private bool UserExists(Guid id)
		{
			return (_context.ApplicationUsers?.Any(e => e.Id == id)).GetValueOrDefault();
		}

		private bool RoleExists(Guid id)
		{
			return (_context.ApplicationRoles?.Any(e => e.Id == id)).GetValueOrDefault();
		}

	}
}
