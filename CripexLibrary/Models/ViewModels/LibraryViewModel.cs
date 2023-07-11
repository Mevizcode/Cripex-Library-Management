using System.ComponentModel.DataAnnotations.Schema;

namespace CripexLibrary.Models.ViewModels
{
    [NotMapped]
    internal class LibraryViewModel<T>
    {
        public List<T> Items { get; set; }
        public Pagination Pagination { get; set; }
    }
}