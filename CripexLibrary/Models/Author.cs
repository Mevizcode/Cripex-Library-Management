using System.ComponentModel.DataAnnotations;

namespace CripexLibrary.Models
{
#nullable enable
	public class Author
	{
		public Guid Id { get; set; } = new Guid();
		[DataType(DataType.Text)]
		[Display(Name = "Author Name")]
		public string AuthorName { get; set; } = null!;
		[DataType(DataType.MultilineText)]
		public string? Biography { get; set; }
		public virtual List<Book> Books { get; } = new List<Book>();
	}
}
