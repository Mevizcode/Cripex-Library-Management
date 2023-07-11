using Microsoft.AspNetCore.Identity;

namespace CripexLibrary.Models
{
#nullable enable
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
        public ApplicationRole() : base() 
        { 
            Id = Guid.NewGuid();
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            Id = Guid.NewGuid();
        }

    }
}
