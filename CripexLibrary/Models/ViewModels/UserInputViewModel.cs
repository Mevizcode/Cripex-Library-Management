using System.ComponentModel.DataAnnotations.Schema;

namespace CripexLibrary.Models.ViewModels
{
	[NotMapped]
#nullable enable
	public class UserInputViewModel
	{
		public string? Name { get; set; }
		public string? Gender { get; set; }
		public DateTime? DOB { get; set; }
		public string? Address { get; set; }
		public string? ProfilePhoto { get; set; }
		public string? PhoneNumber { get; set; }
	}
}
