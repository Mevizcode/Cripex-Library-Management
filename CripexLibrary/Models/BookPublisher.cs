using System.ComponentModel.DataAnnotations;

namespace CripexLibrary.Models
{
    public class BookPublisher
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        [Display(Name = "Publisher")]
        public string PublisherName { get; set; } = null!;
        public virtual List<Book> Books { get; }
    }
}
