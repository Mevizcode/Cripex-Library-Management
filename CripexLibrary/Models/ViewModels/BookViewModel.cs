using System.ComponentModel.DataAnnotations.Schema;

namespace CripexLibrary.Models.ViewModels
{
    [NotMapped]
    public class BookViewModel
    {
        public string BookTitle { get; set; }
        public string BookDescription { get; set; }
        public string Isbn { get; set; }
        public string BookImg { get; set; }
        public int? BookCopies { get; set; }
        public Guid[] CategoryId { get; set; }
        public Guid AuthorId { get; set; }
        public Guid PublisherId { get; set; }
    }
}
