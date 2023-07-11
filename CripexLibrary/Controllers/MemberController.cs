using System.Security.Claims;
using CripexLibrary.Data;
using CripexLibrary.Models;
using CripexLibrary.Models.Enums;
using CripexLibrary.Models.ViewModels;
using CripexLibrary.Services.EmailService;
using CripexLibrary.Services.FileUploadService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CripexLibrary.Controllers
{
	[Authorize(Roles = "MEMBER, ADMIN, LIBRARIAN")]
	public class MemberController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<MemberController> _logger;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IHttpContextAccessor _httpcontextAccessor;
		private readonly IEmailService _emailService;
		private readonly IFileUploadService _fileUploadService;
		public const int PAGESIZE = 5;

		public MemberController(ApplicationDbContext context, 
								ILogger<MemberController> logger, 
								UserManager<ApplicationUser> userManager,
								IHttpContextAccessor httpcontextAccessor,
								IEmailService emailService,
								IFileUploadService fileUploadService)
		{
			_context = context;
			_logger = logger;
			_userManager = userManager;
			_httpcontextAccessor = httpcontextAccessor;
			_emailService = emailService;
			_fileUploadService = fileUploadService;
		}

		public IActionResult Index(int pageNumber = 1)
		{

			if (_httpcontextAccessor.HttpContext is null)
			{
				_logger.LogError("Error: User is null!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = _context.ApplicationUsers.FirstOrDefault(u => u.Id == Guid.Parse(userId));

			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			//get all book borrows by the user
			var bookBorrows = user?.BookBorrows.ToList();
			ViewBag.bookBorrowed = bookBorrows;

			//count total number of books borrowed by  the user
			var totalBooksBorrrowed = bookBorrows?.Count();
			ViewBag.totalBooksBorrrow = totalBooksBorrrowed;

			//count total number of books returned by the user
			var unReturnedBooks = bookBorrows?.Where(u => u.ReturnDate == null).Count();
			ViewBag.unReturnedBooks = unReturnedBooks;

			var booksReturnedCount = bookBorrows?.Where(u => u.ReturnDate != null).Count();
			ViewBag.booksReturnedCount = booksReturnedCount;

			var totalItems = bookBorrows.Count();

			var paging = new Pagination("Index", "Member", totalItems, pageNumber, PAGESIZE);

			var data = bookBorrows.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

			return View(new LibraryViewModel<BookBorrow> { 
				Items = data,
				Pagination = paging
			});
		}

		[HttpGet]
		public async Task<IActionResult> ReturnBook()
		{
			var Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
			Guid userId = Guid.Parse(Id);
			var unreturnedBooks = await _context.Borrow
													.Include(bb => bb.Book)
													.Include(bb => bb.BookCopy)
													.Where(bb => bb.UserId == userId &&
													bb.ReturnDate == null).ToListAsync();

			return View(unreturnedBooks);
		}

		public async Task<IActionResult> ReturnBorrowedBook(Guid id)
		{
			var bookBorrow = await _context.Borrow.FindAsync(id);

			if (bookBorrow == null)
			{
				_logger.LogError("Error: Book is null!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			string bookTitle = bookBorrow.Book.BookTitle;

			bookBorrow.ReturnDate = DateTime.Now;

			var bookCopy = bookBorrow.BookCopy;
			bookCopy.Status = Status.Available;

			_context.Update(bookBorrow);
			await _context.SaveChangesAsync();

			_context.Update(bookCopy);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Wishlist(int pageNumber = 1)
		{
			var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var bookWishlist = await _context.Wishlist.Where(w => w.UserId == Guid.Parse(UserId)).ToListAsync();
			var totalItems = bookWishlist.Count;
			var pageSize = 5;


			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			var paging = new Pagination("Wishlist", "Member", totalItems, pageNumber, pageSize);

			var data = bookWishlist.Skip((pageNumber - 1) * pageSize).Take(paging.PageSize).ToList();

			return View(new LibraryViewModel<Wishlist> {
				Items = data,
				Pagination = paging
			});
		}

		public IActionResult RemoveFromWishList(Guid? Id)
		{
			if (Id == null || Id == Guid.Empty)
			{
				return BadRequest();
			}

			var itemToRemove = _context.Wishlist.Find(Id);

			if (itemToRemove != null)
			{
				_context.Remove(itemToRemove);
				_context.SaveChanges();
				return RedirectToAction(nameof(Wishlist));
			}
			return RedirectToAction(nameof(Wishlist));
		}

		public async Task<IActionResult> EditAccount(Guid? Id)
		{
			if (Id == null || _context.ApplicationUsers == null)
			{
				_logger.LogError("Error: User not found!");

				return RedirectToAction("Error", "Home", new { errorMessage = "User Not Found" });
			}
			var member = await _context.ApplicationUsers.FindAsync(Id);

			return View(member);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
#nullable enable
		public async Task<IActionResult> EditAccount(Guid Id, [Bind("Name, Gender, DOB, Address, ProfilePhoto, PhoneNumber")] UserInputViewModel userModel, IFormFile? profilePic)
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
						_logger.LogError("Error: User not found!");

						return RedirectToAction("Error", "Home", new { errorMessage = "User Not Found" });
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(user);

		}
		private bool UserExists(Guid id)
		{
			return (_context.ApplicationUsers?.Any(e => e.Id == id)).GetValueOrDefault();
		}

		
	 }
}

