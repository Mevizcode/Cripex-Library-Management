using CripexLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CripexLibrary.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, 
        ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<Book> Book { get; set; }
        public DbSet<Author> Author { get; set; }
        public DbSet<BookPublisher> BookPublishers { get; set; }
        public DbSet<BookCopy> BookCopy { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookBorrow> Borrow { get; set; }
        public DbSet<Wishlist> Wishlist { get; set; }
        public DbSet<BookWishlist> BookWishlist { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
                {
                    //Each User can have many UserClaims
                    b.ToTable("ApplicationUsers");
                    b.HasMany(e => e.Claims)
                     .WithOne()
                     .HasForeignKey(uc => uc.UserId)
                     .IsRequired();

                    //Each User can have many UserLogins
                    b.HasMany(e => e.Logins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                    //Each User can have many UserToken 
                    b.HasMany(e => e.Tokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                    //Each User can have many entries in the UserRole join table
                    b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();

                    b.HasMany(e => e.BookBorrows)
                     .WithOne(e => e.User)
                     .HasForeignKey(u => u.UserId)
                     .IsRequired();
                });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.ToTable("Roles");
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            modelBuilder.Entity<ApplicationUserRole>(b =>
            {
                // change the name of the table
                b.ToTable("UserRoles");
            });

            modelBuilder.Entity<Book>(b =>
            { 
                // Each Book can have many copies
                b.HasMany(e => e.Copies)
                 .WithOne(e => e.Book)
                 .HasForeignKey(e => e.BookId)
                 .IsRequired();

                // Each book can be borrowed many times
                b.HasMany(e => e.BookBorrows)
                 .WithOne(e => e.Book)
                 .HasForeignKey(bb => bb.BookId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired();
            });

            //One publisher can have many books
            modelBuilder.Entity<BookPublisher>()
                .HasMany(e => e.Books)
                .WithOne(e => e.Publisher)
                .HasForeignKey(bp => bp.PublisherId)
                .IsRequired();

            //One author can have many books
            modelBuilder.Entity<Author>()
                .HasMany(e => e.Books)
                .WithOne(e => e.Author)
                .HasForeignKey(a => a.AuthorId)
                .IsRequired();

            //each book can belong to many categories
            //each category can have many books
            modelBuilder.Entity<Category>()
                .HasMany(e => e.Books)
                .WithMany(e => e.Category)
                .UsingEntity<BookCategory>();

            //one bookCopy can be borrowed many times
            //(In different occasions)
            modelBuilder.Entity<BookCopy>()
                .HasMany(e => e.Borrows)
                .WithOne(e => e.BookCopy)
                .HasForeignKey(c => c.BookCopyId)
                .IsRequired();

            //one book can be added to many wishlists
            //one wishlist contains many books
            //modelBuilder.Entity<Wishlist>()
            //    .HasMany(e => e.Books)
            //    .WithMany(e => e.Wishlist)
            //    .UsingEntity<BookWishlist>();

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.ApplicationUser)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId);

            //modelBuilder.Entity<BookWishlist>()
            //    .HasKey(bw => new { bw.BookId, bw.WishlistId });

            //modelBuilder.Entity<BookWishlist>()
            //    .HasOne(bw => bw.Book)
            //    .WithMany(b => b.BookWishlist)
            //    .HasForeignKey(bw => bw.BookId);

            //modelBuilder.Entity<BookWishlist>()
            //    .HasOne(bw => bw.Wishlist)
            //    .WithMany(w => w.BookWishlist)
            //    .HasForeignKey(bw => bw.WishlistId);


            //modelBuilder.Entity<Wishlist>()
            //    .HasMany(w => w.Books)
            //    .WithOne()
            //    .HasForeignKey(b => b.WishlistsId);

        }
	}
}
