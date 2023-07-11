using System.ComponentModel.DataAnnotations.Schema;

namespace CripexLibrary.Models.ViewModels
{
	[NotMapped]
#nullable enable
	public class RoleViewModel
	{
		public string Name { get; set; }

		public string? Description { get; set; }
	}
}
