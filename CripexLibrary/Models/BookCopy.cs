
using CripexLibrary.Models.Enums;

namespace CripexLibrary.Models
{
    public class BookCopy
    {
        public Guid Id { get; set; } = new Guid();
        public int CopyNumber { get; set; }
        public Status Status { get; set; } = Status.Available;
        public Guid BookId { get; set; }
        public virtual Book Book { get; set; }
        public virtual List<BookBorrow> Borrows { get; }
    }
}