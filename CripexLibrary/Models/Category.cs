namespace CripexLibrary.Models
{
    public class Category
    {
        public Guid Id { get; set; } = new Guid();
        public string CategoryName { get; set; } = null!;
        public virtual List<Book> Books { get; } = new();
        public virtual List<BookCategory> BookCategories { get; }

    }
}
