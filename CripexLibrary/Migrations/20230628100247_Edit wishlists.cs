using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CripexLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Editwishlists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookWishlist_Book_BookId",
                table: "BookWishlist");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "BookWishlist",
                newName: "BooksId");

            migrationBuilder.AlterColumn<string>(
                name: "CategoryName",
                table: "Category",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PublisherName",
                table: "BookPublishers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_UserId",
                table: "Wishlist",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookWishlist_Book_BooksId",
                table: "BookWishlist",
                column: "BooksId",
                principalTable: "Book",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wishlist_ApplicationUsers_UserId",
                table: "Wishlist",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookWishlist_Book_BooksId",
                table: "BookWishlist");

            migrationBuilder.DropForeignKey(
                name: "FK_Wishlist_ApplicationUsers_UserId",
                table: "Wishlist");

            migrationBuilder.DropIndex(
                name: "IX_Wishlist_UserId",
                table: "Wishlist");

            migrationBuilder.RenameColumn(
                name: "BooksId",
                table: "BookWishlist",
                newName: "BookId");

            migrationBuilder.AlterColumn<string>(
                name: "CategoryName",
                table: "Category",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PublisherName",
                table: "BookPublishers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookWishlist_Book_BookId",
                table: "BookWishlist",
                column: "BookId",
                principalTable: "Book",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
