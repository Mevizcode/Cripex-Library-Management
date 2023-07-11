using System.Diagnostics;
using System.Security.Claims;
using CripexLibrary.Data;
using CripexLibrary.Models;
using CripexLibrary.Models.Enums;
using CripexLibrary.Models.ViewModels;
using CripexLibrary.Services.EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CripexLibrary.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly IEmailService _emailServices;
		public const int PAGESIZE = 8;



		public HomeController(ILogger<HomeController> logger,
							  ApplicationDbContext context,
							  IEmailService emailServices)
		{
			_logger = logger;
			_context = context;
			_emailServices = emailServices;
		}

		public async Task<IActionResult> Index()
		{
			var featuredBooks = await _context
										.Book
										.Take(9)
										.Where(b => b.Status == 0 && b.BookTitle != "Sapiens: A Brief History of Humankinds")
										.ToListAsync();

			var bestBorrowedBooks = await _context
											.Borrow
											.GroupBy(bb => bb.BookId)
											.Select(group => new
											{
												Book = _context.Book.FirstOrDefault(b => b.Id == group.Key),
												BorrowCount = group.Count()
											})
											.OrderByDescending(result => result.BorrowCount)
											.Take(4)
											.ToListAsync();

			ViewBag.FeaturedBooks = featuredBooks;

			ViewBag.MostBorrowedBooks = bestBorrowedBooks;

			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public IActionResult About()
		{
			return View();
		}

		public IActionResult Contact()
		{

			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error(string errorMessage)
		{
			var errorViewModel = new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
				ErrorMessage = errorMessage,
				ErrorCode = 400
			};
			return View(errorViewModel);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Book(string sortOrder, int pageNumber = 1)
		{
			ViewData["BookTitleSort"] = String.IsNullOrEmpty(sortOrder) ? "bookTitle_desc" : "";
			ViewData["AuthorNameSort"] = sortOrder == "Author" ? "author_desc" : "Author";



			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			var books = await _context
									.Book
									.Include(b => b.BookCategories)
										.ThenInclude(bc => bc.Category)
									.ToListAsync();

			switch (sortOrder)
			{
				case "bookTitle_desc":
					books = books.OrderByDescending(b => b.BookTitle).ToList();
					break;

				case "Author":
					books = books.OrderByDescending(b => b.Author.AuthorName).ToList();
					break;

				case "author_desc":
					books = books.OrderByDescending(b => b.Author.AuthorName).ToList();
					break;

				default:
					books = books.OrderBy(b => b.BookTitle).ToList();

					break;
			}

			var totalItems = books.Count;

			var paging = new Pagination(
										"Book",
										"Home",
										totalItems,
										pageNumber,
										PAGESIZE);

			var data = books
							.Skip((pageNumber - 1) * PAGESIZE)
							.Take(paging.PageSize)
							.ToList();

			return View(new LibraryViewModel<Book>
			{
				Items = data,
				Pagination = paging
			});
		}

		public async Task<IActionResult> Search(string query, int pageNumber = 1)
		{

			var books = await _context
								.Book
								.Where(b => b.BookTitle.Contains(query) 
								|| b.Author.AuthorName.Contains(query) 
								|| b.Publisher.PublisherName.Contains(query)).ToListAsync();
			if (pageNumber < 1)
			{
				pageNumber = 1;
			}
			var totalItems = books.Count;
			var paging = new Pagination("Search", "LibraryManager", totalItems, pageNumber, PAGESIZE);

			var data = books.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

			return View(new LibraryViewModel<Book>
			{
				Items = data,
				Pagination = paging

			});

		}

		[AllowAnonymous]
		public async Task<IActionResult> BookDetail(Guid? Id, string authorName)
		{
			if (Id == null || authorName == null)
			{
				_logger.LogError("Error: Book Id and AuthorName is null!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			var booksByAuthor = await _context
											.Book
											.Where(b => b.Author.AuthorName == authorName && Id != b.Id)
											.ToListAsync();

			ViewBag.MoreBooksByAuthor = booksByAuthor;



			return View(await _context
									.Book
									.Include(b => b.BookCategories)
										.ThenInclude(bc => bc.Category)
									.FirstOrDefaultAsync(b => b.Id == Id));
		}

		[Authorize]
		public async Task<IActionResult> Borrow(Guid? Id)
		{
			BookBorrow borrowBook = new BookBorrow();
			if (Id == null || Id == Guid.Empty)
			{

				_logger.LogError("Error: Book Id is null");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			var bookToBorrow = await _context
											.Book
											.FindAsync(Id);

			var borrowerId = User
								.FindFirstValue(
								ClaimTypes
								.NameIdentifier);

			if (borrowerId == null)
			{

				return RedirectToAction("Error", "Home", new { errorMessage = "Please Login to continue!" });
			}
			var borrower = await _context
										.ApplicationUsers
										.FindAsync(
										Guid.Parse(borrowerId));

			var availableBookCopy = await _context
												.BookCopy
												.Where(bc => bc.Status == 0)
												.FirstOrDefaultAsync();

			var existingBorrow = await _context
												.Borrow
												.Where(bb => bb.UserId == borrower.Id
												&& bb.BookId == bookToBorrow.Id
												&& bb.ReturnDate == null)
												.FirstOrDefaultAsync();
			var borrowedBooks = _context
										.Borrow
										.Where(b => b.UserId == Guid.Parse(borrowerId)
										&& (b.ReturnDate == null))
										.ToList(); //b.DueDate < DateTime.UtcNow ||
			ViewBag.borrowedBooks = borrowedBooks;

			if (borrowedBooks.Count > 2)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = "You cannot borrow another book until you return all overdue books!" });
			}

			try
			{
				if (existingBorrow != null)
				{
					var existingBorrowError = "You cannot borrow another copy of " +
											  "this book until the first one is returned.";

					_logger.LogError(existingBorrowError);

					throw new Exception(existingBorrowError);
				}
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
			}
			try
			{
				var overdueBorrow = await _context
												.Borrow
												.Where(bb => bb.UserId == borrower.Id
												&& (bb.ReturnDate > bb.DueDate.AddDays(10)))
												.ToListAsync();

				if (overdueBorrow.Count > 0)
				{
					var overdueBorrowError = $"You have {overdueBorrow.Count} " +
											 $"overdue book(s) that must be returned " +
											 $"before you can borrow another book.";

					_logger.LogError("Error: " + overdueBorrowError);

					throw new Exception(overdueBorrowError);
				}
			}
			catch (Exception ex)
			{
				return RedirectToAction("Error", "Home", new { errorMessage = ex.Message });
			}



			if (availableBookCopy != null)
			{
				borrowBook = new BookBorrow
				{
					UserId = borrower.Id,
					BookId = bookToBorrow.Id,
					BookCopyId = availableBookCopy.Id,
					BorrowDate = DateTime.UtcNow.ToLocalTime(),
				};
				_context.Borrow.Add(borrowBook);
				await _context.SaveChangesAsync();

				var bookCopyBoworred = await _context
													.BookCopy
													.FindAsync(borrowBook
													.BookCopyId);

				if (bookCopyBoworred != null)
				{
					bookCopyBoworred.Status = Status.Borrowed;
				}

				_context.BookCopy.Update(bookCopyBoworred);

				await _context.SaveChangesAsync();
			}

			return RedirectToAction(
				"BorrowSuccess",
				new
				{
					id = borrowBook.Id,
					borrowBook
				});
		}


		[Authorize]
		public async Task<IActionResult> BorrowSuccess(Guid id, BookBorrow bookBorrow)
		{
			var borrowedBook = await _context.Borrow.FindAsync(id);

			var bookTitle = borrowedBook.Book.BookTitle;

			var email = borrowedBook.User.Email;

			var quantity = 1;

			await _emailServices.SendConfirmationEmail(email, quantity, bookTitle);

			_logger.LogInformation("Borrow success email sent!");


			return View(borrowedBook);
		}

		[Authorize]
		public async Task<IActionResult> AddToWishList(Guid? Id)
		{
			if (Id == null || Id == Guid.Empty)
			{
				return NotFound();
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var book = await _context.Book.FindAsync(Id);
			// Check if the user already has a wishlist
			var existingWishlist = _context.Wishlist.SingleOrDefault(w => w.UserId == Guid.Parse(userId));

			//var bookExist = _context.BookWishlist.Where(b => b.BookId == Id).ToList();

			//if (bookExist.Count > 1)
			//{
			//	_logger.LogError("Error: Book Areadly Added to wishlist!");

			//	return RedirectToAction("Error", "Home", new { errorMessage = "Book Areadly Added to wishlist" });
			//}

			if (existingWishlist == null)
			{
				var wishlist = new Wishlist
				{
					UserId = Guid.Parse(userId)
				};
				_context.Add(wishlist);
				await _context.SaveChangesAsync();
			}
			else
			{

				var userWishlist = await _context.Wishlist.FirstOrDefaultAsync(w => w.UserId == Guid.Parse(userId));
				if (userWishlist != null) {
					var newWishlist = new Wishlist
					{
						UserId = userWishlist.UserId
					};
					_context.Wishlist.Add(newWishlist);
				}
				await _context.SaveChangesAsync();
				//var bookWishlist = new BookWishlist
				//{
				//	BookId = (Guid)Id,
				//	WishlistId = userWishlist.Id

				//};
				//_context.Add(bookWishlist);
			}

			
			//await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "Book added to wishlist successfully!";

			return RedirectToAction(nameof(Book));
		}
	}
}
