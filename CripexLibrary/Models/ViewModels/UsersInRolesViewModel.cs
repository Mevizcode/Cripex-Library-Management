using System.ComponentModel.DataAnnotations.Schema;

namespace CripexLibrary.Models.ViewModels
{
    [NotMapped]
    public class UsersInRolesViewModel
    {
        public string RoleName { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
           
}
