using System.ComponentModel.DataAnnotations.Schema;

namespace CripexLibrary.Models
{
    [NotMapped]
    public class BookWishlist
    {
        
        public Guid BookId { get; set; }
		public virtual Book Book { get; set; }
        public Guid WishlistId { get; set; }
        public virtual Wishlist Wishlist { get; set; }
    }
}
