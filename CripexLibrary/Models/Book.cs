using System.ComponentModel.DataAnnotations;
using CripexLibrary.Models.Enums;

namespace CripexLibrary.Models
{
#nullable enable
    public class Book
	{
		public Guid Id { get; set; } = new Guid();
		[Display(Name = "Book Title")]
		public string BookTitle { get; set; } = null!;
		[DataType(DataType.MultilineText)]
		[Display(Name = "Book Description")]
		public string BookDescription { get; set; } = null!;
		[DataType(DataType.Text), StringLength(13)]
		public string Isbn { get; set; } = null!;
		[Display(Name = "Book Cover Url")]
		public string? BookImg { get; set; }
		public Status Status { get; set; } = Status.Available;
		public Guid AuthorId { get; set; }
		public virtual Author? Author { get; }
		public Guid PublisherId { get; set; } 
		//public Guid WishlistsId { get; set; }
		//public virtual Wishlist? Wishlists { get; }
		public virtual BookPublisher? Publisher { get; }
		public virtual List<Wishlist> Wishlist { get; }
		public virtual List<Category> Category { get; }
		public virtual List<BookCategory> BookCategories { get; }
		public virtual List<BookCopy> Copies { get; }
		public virtual List<BookBorrow> BookBorrows { get; }
		public virtual List<BookWishlist> BookWishlist { get; }
	}
}
