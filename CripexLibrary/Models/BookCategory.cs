﻿namespace CripexLibrary.Models
{
    public class BookCategory
    {   
       public Guid BookId { get; set; }
       public virtual Book Book { get; set; } = null!;
       public Guid CategoryId { get; set; }
       public virtual Category Category { get; set; } = null!;
    }
}
