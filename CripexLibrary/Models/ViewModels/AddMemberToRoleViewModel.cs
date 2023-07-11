using System.ComponentModel.DataAnnotations.Schema;

namespace CripexLibrary.Models.ViewModels
{
    [NotMapped]
    public class AddMemberToRoleViewModel
    {
        public List<string> Users { get; set; }
        public List<string> Roles { get; set; }
    }
}
