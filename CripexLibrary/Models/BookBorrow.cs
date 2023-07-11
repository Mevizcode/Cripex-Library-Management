using System.ComponentModel.DataAnnotations;

namespace CripexLibrary.Models
{
    public class BookBorrow
    {
        public Guid Id { get; set; } = new Guid();
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public Guid BookId { get; set; }
        public virtual Book Book { get; set; }
        public Guid BookCopyId { get; set; }
        public virtual BookCopy BookCopy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Borrowed")]
        public DateTime BorrowDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Returned")]
        public DateTime? ReturnDate { get; set; }
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(10);
    }
}
