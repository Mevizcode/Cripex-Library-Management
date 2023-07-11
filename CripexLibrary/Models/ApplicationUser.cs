using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CripexLibrary.Models
{
#nullable enable
	public class ApplicationUser : IdentityUser<Guid>
	{
		[PersonalData]
		[DataType(DataType.Text)]
		[Display(Name = "Full Name")]
		public string? Name { get; set; }

		[PersonalData]
		[DataType(DataType.Text)]
		public string? Gender { get; set; }

		[PersonalData]
		[DataType(DataType.Date)]
		[Display(Name = "Date of Birth")]
		public DateTime? DOB { get; set; }

		[PersonalData]
		[DataType(DataType.Text)]
		public string? Address { get; set; }

		[PersonalData] 
		[DataType(DataType.Text)]
		[Display(Name = "Profile Photo")]
		public string? ProfilePhoto { get; set; }
		[Display(Name = "Date Joined")]
		public DateTime DateJoined { get; set; }

		public virtual ICollection<IdentityUserClaim<Guid>> Claims { get; set; }
		public virtual ICollection<IdentityUserLogin<Guid>> Logins { get; set; }
		public virtual ICollection<IdentityUserToken<Guid>> Tokens { get; set; }
		[Display(Name = "User Roles")]
		public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
		[Display(Name = "Book Borrows")]
		public virtual List<BookBorrow> BookBorrows { get; }

		public virtual List<Wishlist> Wishlists { get; }

	}
}
