using System.ComponentModel.DataAnnotations;

namespace CripexLibrary.Models
{
    public class Wishlist
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        public Guid UserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; }
        public virtual List<Book> Books { get; }

        public virtual List<BookWishlist> BookWishlist { get; }
    }
}
