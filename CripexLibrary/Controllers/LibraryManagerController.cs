using CripexLibrary.Data;
using CripexLibrary.Models;
using CripexLibrary.Models.Enums;
using CripexLibrary.Models.ViewModels;
using CripexLibrary.Services.EmailService;
using CripexLibrary.Services.FileUploadService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace CripexLibrary.Controllers
{
	[Authorize(Roles = "LIBRARIAN, ADMIN")]
	public class LibraryManagerController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<LibraryManagerController> _logger;
		private readonly IFileUploadService _fileUploadService;
		private readonly IEmailService _emailServices;
		const int PAGESIZE = 5;
		
		public LibraryManagerController(ApplicationDbContext context, ILogger<LibraryManagerController> logger, IFileUploadService fileUploadService, IEmailService emailService)
		{
			_context = context;
			_logger = logger;
			_fileUploadService = fileUploadService;
			_emailServices = emailService;
		}

		[HttpGet]
		public IActionResult Index(int pageNumber = 1)
		{

			var books = _context.Book.ToList();
		   
			ViewBag.bookCount = books.Count();

			var borrowCount = _context.Borrow.Count();
			ViewBag.borrowCount = borrowCount;

			var booksReturn = _context.Borrow.Where(bb => bb.ReturnDate != null).Count();
			ViewBag.booksReturn = booksReturn;

			var booksOverDue = _context.Borrow.Where(bb => bb.DueDate < DateTime.UtcNow && bb.ReturnDate > bb.DueDate || bb.ReturnDate == null).Count();
			ViewBag.booksOverDue = booksOverDue;

			


			if (pageNumber < 1)
				pageNumber = 1;

			int totalItems = books.Count();

			var paging = new Pagination("Index", "LibraryManager", totalItems, pageNumber, PAGESIZE);

			var data = books.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();


			return View(new LibraryViewModel<Book> { 
				Items = data,
				Pagination = paging,
			});
		}

		public ActionResult Search(string query, int pageNumber = 1)
		{

			if (User.IsInRole("LIBRARIAN"))
			{
				var books = _context.Book.Where(b => b.BookTitle.Contains(query) || b.Author.AuthorName.Contains(query) || b.Publisher.PublisherName.Contains(query)).ToList();

				if (pageNumber < 1)
				{
					pageNumber = 1;
				}

				var totalItems = books.Count;

				var paging = new Pagination("Search", "LibraryManager", totalItems, pageNumber, PAGESIZE);

				var data = books.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

				return View("_BookSearchResults", new LibraryViewModel<Book> {
					Items = data,
					Pagination = paging

				});    
			}

			return RedirectToAction(nameof(Index));
		}


		public async Task<IActionResult> BooksBorrowed(int pageNumber = 1)
		{
			var borrowedBooks = await _context.Borrow.Where(b => b.BorrowDate != null).OrderByDescending(bb => bb.BorrowDate).ToListAsync();
			var totalItem = borrowedBooks.Count;
			
			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			int pageSize = 10;

			var paging = new Pagination("BooksBorrowed", "LibraryManager", totalItem, pageNumber, pageSize);

			var data = borrowedBooks.Skip((pageNumber - 1) * pageSize).Take(paging.PageSize).ToList();
			
			return View(new LibraryViewModel<BookBorrow>
			{
				Items = data,
				Pagination = paging
			});
		}

		public async Task<IActionResult> BookToReturn(int pageNumber = 1)
		{
			var bookToReturn = await _context.Borrow.Where(b => b.DueDate < DateTime.Now && b.ReturnDate == null).OrderByDescending(b => b.DueDate).ToListAsync();
			var totalItem = bookToReturn.Count;

			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			var paging = new Pagination("BookToReturn", "LibraryManager", totalItem, pageNumber, PAGESIZE);

			var data = bookToReturn.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

			return View(new LibraryViewModel<BookBorrow>
			{
				Items = data,
				Pagination = paging
			});
		}

		public async Task<IActionResult> BookOverdue(int pageNumber = 1)
		{
			var booksOverdue = await _context.Borrow.Where(b => b.DueDate < DateTime.Now && b.ReturnDate == null || b.ReturnDate > b.DueDate).OrderByDescending(b => b.ReturnDate == null).ToListAsync();
			var totalItem = booksOverdue.Count;

			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			var paging = new Pagination("BookOverdue", "LibraryManager", totalItem, pageNumber, PAGESIZE);

			var data = booksOverdue.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

			

			return View(new LibraryViewModel<BookBorrow>
			{
				Items = data,
				Pagination = paging
			});
		}

		[HttpPost]
		public async Task<IActionResult> SendOverDueEmail()
		{
			var overdueBorrows = await _context.Borrow.Include(b => b.User).Include(b => b.Book).Where(b => b.DueDate < DateTime.Now && b.ReturnDate == null).ToListAsync();

			foreach (var borrow in overdueBorrows)
			{
				await _emailServices.SendOverdueEmail(borrow.User.Email, borrow.User.UserName, borrow.Book.BookTitle);
			}
			_logger.LogInformation("Overdue Email sent Successful!");
			return RedirectToAction(nameof(BookOverdue));
		}

		public async Task<IActionResult> BooksReturned(int pageNumber = 1)
		{
			var bookReturned = await _context.Borrow.Where(b => b.ReturnDate != null).OrderByDescending(b => b.ReturnDate).ToListAsync();
			var totalItem = bookReturned.Count;

			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			var paging = new Pagination("BooksReturned", "LibraryManager", totalItem, pageNumber, PAGESIZE);

			var data = bookReturned.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

			return View(new LibraryViewModel<BookBorrow>
			{
				Items = data,
				Pagination = paging
			});
		}

		public async Task<IActionResult> BooksNotAvailable(int pageNumber = 1)
		{
			var bookNotAvailable = await _context.BookCopy.Where(bc => bc.CopyNumber < 1).ToListAsync();
			var totalItem = bookNotAvailable.Count;

			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			var paging = new Pagination("BooksNotAvailable", "LibraryManager", totalItem, pageNumber, PAGESIZE);

			var data = bookNotAvailable.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

			return View(new LibraryViewModel<BookCopy>
			{
				Items = data,
				Pagination = paging
			});
		}



		[HttpGet]
		public IActionResult AddBook()
		{
			var categories = _context.Category.ToList();
			ViewBag.Categories = categories;

			var author = _context.Author.ToList();
			ViewBag.Author = author;

			var publisher = _context.BookPublishers.ToList();
			ViewBag.Publisher = publisher;

			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddBook([Bind("BookTitle, BookDescription, Isbn, BookCopies, CategoryId, AuthorId, PublisherId")] BookViewModel bookModel, IFormFile bookImg)
		{
			if (ModelState.IsValid)
			{
				string fileName = "";
				if (bookImg != null && bookImg.Length > 0)
				{
					fileName = await _fileUploadService.UploadFileAsync(bookImg, false);

				}

				// Add book to database
				var book = new Book
				{
					BookTitle = bookModel.BookTitle,
					BookDescription = bookModel.BookDescription,
					Isbn = bookModel.Isbn,
					BookImg = fileName,
					Status = Status.Available,
					AuthorId = bookModel.AuthorId,
					PublisherId = bookModel.PublisherId
				};
				_context.Book.Add(book);

				await _context.SaveChangesAsync();
				_logger.LogInformation("New book Added!");

				if (bookModel.BookCopies == 0)
				{
					bookModel.BookCopies = 3;
				}

				// Add  copies when a new book is added
				for (var i = 1; i <= bookModel.BookCopies; i++)
				{
					var copy = new BookCopy
					{
						CopyNumber = i,
						BookId = book.Id
					};
					_context.BookCopy.Add(copy);
				}
				await _context.SaveChangesAsync();
				_logger.LogInformation("New book copies Added!");

				// Add book categories to database
				foreach (var categoryId in bookModel.CategoryId)
				{
					var bookCategory = new BookCategory
					{
						BookId = book.Id,
						CategoryId = categoryId
					};
					_context.BookCategories.Add(bookCategory);
				}
				await _context.SaveChangesAsync();

			}
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> BookDetail(Guid? Id)
		{
			if (Id == null)
			{
				_logger.LogError("Error: Book not found!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			var book = await _context.Book.FindAsync(Id);

			return View(book);
		}

		[HttpGet]
		public async Task<IActionResult> EditBook(Guid? Id)
		{
			var book = await _context.Book.FindAsync(Id);

			if (Id == null || _context.Book == null)
			{
				_logger.LogError("Error: Book not found!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			var author = _context.Author.ToList();
			ViewBag.Authors = author;

			var publisher = _context.BookPublishers.ToList();
			ViewBag.Publishers = publisher;

			return View(book);
		}
		

		[HttpPost]
		[ValidateAntiForgeryToken]
		#nullable enable
		public async Task<IActionResult> EditBook(Guid Id, [Bind("Id, BookTitle, BookDescription, Isbn, BookImg")] Book bookModel, IFormFile? bookImg, Guid authorId, Guid publisherId) //, AuthorId, PublisherId
		{
			if (Id != bookModel.Id)
			{
				_logger.LogError("Error: Book Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}
			if (ModelState.IsValid)
			{
				try
				{
					var exisitingBook = await _context.Book.FindAsync(Id);


					if (exisitingBook?.BookImg == null)
					{
						if (bookImg != null && bookImg.Length > 0)
						{
							bookModel.BookImg = await _fileUploadService.UploadFileAsync(bookImg, false);
						}
						else
						{
							ModelState.AddModelError("bookImg", "Invalid file! Please choose a valid file to continue!");
							_logger.LogError("Error: Invalid file! Please choose a valid file to continue!");

							return RedirectToAction("Error", "Home", new { errorMessage = "Invalid file! Please choose a valid file to continue!" });
						}
					}
					else
					{
						bookModel.BookImg = exisitingBook.BookImg;
					}



					if (exisitingBook != null)
					{
						exisitingBook.BookTitle = bookModel.BookTitle;
						exisitingBook.BookDescription = bookModel.BookDescription;
						exisitingBook.Isbn = bookModel.Isbn;
						exisitingBook.BookImg = bookModel.BookImg;
						exisitingBook.AuthorId = authorId;
						exisitingBook.PublisherId = publisherId;


						_context.Book.Update(exisitingBook);
					}

					await _context.SaveChangesAsync();

					_logger.LogInformation("A book was updated successfully!");

					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!BookExists(Id))
					{
						_logger.LogError("Error: Book does not exist!");

						return RedirectToAction("Error", "Home", new { errorMessage = "Book does not exist!" });
					}
				}
			}
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> DeleteBook(Guid? Id)
		{
			var book = await _context.Book.FindAsync(Id);

			if ((book == null) || !BookExists(book.Id))
			{
				_logger.LogError("Error: Book Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			_context.Book.Remove(book);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Book deleted!");

			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult AddAuthor()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddAuthor([Bind("AuthorName, Biography")] Author author)
		{

			if (ModelState.IsValid)
			{
				_context.Author.Add(author);
				await _context.SaveChangesAsync();
				_logger.LogInformation("Author added!");

			}
			return RedirectToAction(nameof(ListAuthor));
		}

		public IActionResult ListAuthor(int pageNumber = 1)
		{
			var authors = _context.Author.ToList();
			var totalItems = authors.Count();
			if (pageNumber < 1) {
				pageNumber = 1;
			}

			var paging = new Pagination("ListAuthor", "LibraryManager", totalItems, pageNumber, PAGESIZE);

			var data = authors.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

			return View(new LibraryViewModel<Author> { 
				Items = data,
				Pagination = paging
			});
		}

		[HttpGet]
		public async Task<IActionResult> EditAuthor(Guid? Id)
		{
			if (Id == null || _context.Author == null)
			{
				_logger.LogError("Error: Author Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			var author = await _context.Author.FindAsync(Id);

			return View(author);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditAuthor(Guid Id, [Bind("Id, AuthorName, Biography")] Author author)
		{
			if (Id != author.Id)
			{
				_logger.LogError("Error: Author Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			if (ModelState.IsValid)
			{
				try
				{
					var existingAuthor = await _context.Author.FindAsync(Id);

					if (existingAuthor != null)
					{
						existingAuthor.AuthorName = author.AuthorName;
						existingAuthor.Biography = author.Biography;


						_context.Update(existingAuthor);

					}
					await _context.SaveChangesAsync();

					_logger.LogInformation("Author details updated ");

					return RedirectToAction("ListAuthor");
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!AuthorExists(author.Id))
					{
						_logger.LogError("Error: Book Id not match!");

						return RedirectToAction("Error", "Home", new { errorMessage = "Author not Found" });
					}
				}
			}

			return View("ListAuthor");
		}

		[HttpGet]
		public async Task<IActionResult> AuthorDetail(Guid? Id)
		{
			if (Id == null)
			{
				_logger.LogError("Error: Author Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });

			}
			var authorDetails = await _context.Author.FindAsync(Id);

			return View(authorDetails);
		}


		public async Task<IActionResult> DeleteAuthor(Guid? Id)
		{
			var author = await _context.Author.FindAsync(Id);

			if ((author == null) || !AuthorExists(author.Id))
			{
				_logger.LogError("Error: Author Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			_context.Author.Remove(author);
			await _context.SaveChangesAsync();

			return RedirectToAction("ListAuthor");
		}

		[HttpGet]
		public IActionResult AddPublisher()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddPublisher([Bind("PublisherName")] BookPublisher bookPublisher)
		{
			if (ModelState.IsValid)
			{
				_context.BookPublishers.Add(bookPublisher);
				await _context.SaveChangesAsync();
				_logger.LogInformation("Publisher added!");
			}
			return RedirectToAction(nameof(Index));
		}

		public IActionResult ListPublisher(int pageNumber = 1)
		{
			var publishers = _context.BookPublishers.ToList();
			var totalItems = publishers.Count();

			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			var paging = new Pagination("ListPublisher", "LibraryManager", totalItems, pageNumber, PAGESIZE);

			var data = publishers.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();
			return View(new LibraryViewModel<BookPublisher> { 
				Items = data,
				Pagination = paging
			});
		}

		[HttpGet]
		public async Task<IActionResult> PublisherDetails(Guid? Id)
		{
			if (Id == null)
			{
				_logger.LogError("Error: Publisher Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			var publisher = await _context.BookPublishers.FindAsync(Id);

			return View(publisher);
		}

		[HttpGet]
		public async Task<IActionResult> EditPublisher(Guid? Id)
		{
			if (Id == null || _context.BookPublishers == null)
			{
				_logger.LogError("Error: Publisher Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			var publisher = await _context.BookPublishers.FindAsync(Id);

			return View(publisher);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPublisher(Guid Id, [Bind("")] BookPublisher publisher)
		{
			if (Id != publisher.Id)
			{
				_logger.LogError("Error: Publisher Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}
			if (ModelState.IsValid)
			{
				try
				{
					var existingPublisher = await _context.BookPublishers.FindAsync(Id);
					if (existingPublisher != null)
					{
						existingPublisher.PublisherName = publisher.PublisherName;

						_context.Update(existingPublisher);
					}
					await _context.SaveChangesAsync();

					return RedirectToAction("ListPublisher");
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!PublisherExists(publisher.Id))
					{
						_logger.LogError("Error: Publisher does not exist!");

						return RedirectToAction("Error", "Home", new { errorMessage = "Publisher Not Found" });
					}
				}
			}
			return View("ListPublisher");
		}

		public async Task<IActionResult> DeletePublisher(Guid? Id)
		{
			var publisher = await _context.BookPublishers.FindAsync(Id);

			if ((publisher == null) || !PublisherExists(publisher.Id))
			{
				_logger.LogError("Error: Publisher Id not match!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			_context.BookPublishers.Remove(publisher);
			await _context.SaveChangesAsync();

			return RedirectToAction("ListPublisher");
		}

		[HttpGet]
		public IActionResult AddCategory()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddCategory([Bind("CategoryName")] Category category)
		{
			if (ModelState.IsValid)
			{
				_context.Category.Add(category);
				await _context.SaveChangesAsync();
				_logger.LogInformation("Category added!");
			}
			return RedirectToAction(nameof(ListCategory));
		}

		public IActionResult ListCategory(int pageNumber = 1)
		{
			var categories = _context.Category.ToList();
			var totalItems = categories.Count();

			if (pageNumber < 1)
			{
				pageNumber = 1;
			}

			var paging = new Pagination("ListCategory", "LibraryManager", totalItems, pageNumber, PAGESIZE);

			var data = categories.Skip((pageNumber - 1) * PAGESIZE).Take(paging.PageSize).ToList();

			return View(new LibraryViewModel<Category>{
				Items = data,
				Pagination = paging
			});
		}

		[HttpGet]
		public async Task<IActionResult> CategoryDetails(Guid? Id)
		{
			if (Id == null)
			{
				_logger.LogError("Error: Category not found!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			var category = await _context.Category.FindAsync(Id);

			return View(category);
		}

		[HttpGet]
		public async Task<IActionResult> EditCategory(Guid? Id)
		{
			if (Id == null || _context.Category == null)
			{
				_logger.LogError("Error: Category not found!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}

			var categories = await _context.Category.FindAsync(Id);

			return View(categories);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditCategory(Guid? Id, [Bind("")] Category category)
		{
			if (Id != category.Id)
			{
				_logger.LogError("Error: Category not found!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Page Not Found" });
			}
			if (ModelState.IsValid)
			{
				try
				{
					var existingCategory = await _context.Category.FindAsync(Id);
					if (existingCategory != null)
					{
						existingCategory.CategoryName = category.CategoryName;

						_context.Category.Update(existingCategory);
					}
					await _context.SaveChangesAsync();

					return RedirectToAction("ListCategory");
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!CategoryExists(category.Id))
					{
						_logger.LogError("Error: Category not found!");

						return RedirectToAction("Error", "Home", new { errorMessage = "Category Not Found" });
					}
				}
			}
			return View("ListCategory");
		}

		public async Task<IActionResult> DeleteCategory(Guid? Id)
		{
			var category = await _context.Category.FindAsync(Id);

			if ((category == null) || !CategoryExists(category.Id))
			{
				_logger.LogError("Error: Category not found!");

				return RedirectToAction("Error", "Home", new { errorMessage = "Category Not Found" });
			}

			_context.Category.Remove(category);
			await _context.SaveChangesAsync();

			return RedirectToAction("ListCategory");
		}


		private bool BookExists(Guid Id)
		{
			return (_context.Book?.Any(e => e.Id == Id)).GetValueOrDefault();
		}

		private bool AuthorExists(Guid Id)
		{
			return (_context.Author?.Any(e => e.Id == Id)).GetValueOrDefault();
		}

		private bool PublisherExists(Guid Id)
		{
			return (_context.BookPublishers?.Any(e => e.Id == Id)).GetValueOrDefault();
		}

		private bool CategoryExists(Guid Id)
		{
			return (_context.Category?.Any(e => e.Id == Id)).GetValueOrDefault();
		}
	}
}
