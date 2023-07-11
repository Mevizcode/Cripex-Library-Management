using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CripexLibrary.Migrations
{
    /// <inheritdoc />
    public partial class editpublishermodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Book_BookPublisher_PublisherId",
                table: "Book");

            migrationBuilder.DropForeignKey(
                name: "FK_BookBorrow_ApplicationUsers_UserId",
                table: "BookBorrow");

            migrationBuilder.DropForeignKey(
                name: "FK_BookBorrow_BookCopy_BookCopyId",
                table: "BookBorrow");

            migrationBuilder.DropForeignKey(
                name: "FK_BookBorrow_Book_BookId",
                table: "BookBorrow");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCategory_Book_BookId",
                table: "BookCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCategory_Category_CategoryId",
                table: "BookCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookPublisher",
                table: "BookPublisher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookCategory",
                table: "BookCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookBorrow",
                table: "BookBorrow");

            migrationBuilder.RenameTable(
                name: "BookPublisher",
                newName: "BookPublishers");

            migrationBuilder.RenameTable(
                name: "BookCategory",
                newName: "BookCategories");

            migrationBuilder.RenameTable(
                name: "BookBorrow",
                newName: "Borrow");

            migrationBuilder.RenameIndex(
                name: "IX_BookCategory_CategoryId",
                table: "BookCategories",
                newName: "IX_BookCategories_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_BookBorrow_UserId",
                table: "Borrow",
                newName: "IX_Borrow_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_BookBorrow_BookId",
                table: "Borrow",
                newName: "IX_Borrow_BookId");

            migrationBuilder.RenameIndex(
                name: "IX_BookBorrow_BookCopyId",
                table: "Borrow",
                newName: "IX_Borrow_BookCopyId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookPublishers",
                table: "BookPublishers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookCategories",
                table: "BookCategories",
                columns: new[] { "BookId", "CategoryId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Borrow",
                table: "Borrow",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Book_BookPublishers_PublisherId",
                table: "Book",
                column: "PublisherId",
                principalTable: "BookPublishers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookCategories_Book_BookId",
                table: "BookCategories",
                column: "BookId",
                principalTable: "Book",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookCategories_Category_CategoryId",
                table: "BookCategories",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Borrow_ApplicationUsers_UserId",
                table: "Borrow",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Borrow_BookCopy_BookCopyId",
                table: "Borrow",
                column: "BookCopyId",
                principalTable: "BookCopy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Borrow_Book_BookId",
                table: "Borrow",
                column: "BookId",
                principalTable: "Book",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Book_BookPublishers_PublisherId",
                table: "Book");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCategories_Book_BookId",
                table: "BookCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCategories_Category_CategoryId",
                table: "BookCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrow_ApplicationUsers_UserId",
                table: "Borrow");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrow_BookCopy_BookCopyId",
                table: "Borrow");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrow_Book_BookId",
                table: "Borrow");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Borrow",
                table: "Borrow");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookPublishers",
                table: "BookPublishers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookCategories",
                table: "BookCategories");

            migrationBuilder.RenameTable(
                name: "Borrow",
                newName: "BookBorrow");

            migrationBuilder.RenameTable(
                name: "BookPublishers",
                newName: "BookPublisher");

            migrationBuilder.RenameTable(
                name: "BookCategories",
                newName: "BookCategory");

            migrationBuilder.RenameIndex(
                name: "IX_Borrow_UserId",
                table: "BookBorrow",
                newName: "IX_BookBorrow_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Borrow_BookId",
                table: "BookBorrow",
                newName: "IX_BookBorrow_BookId");

            migrationBuilder.RenameIndex(
                name: "IX_Borrow_BookCopyId",
                table: "BookBorrow",
                newName: "IX_BookBorrow_BookCopyId");

            migrationBuilder.RenameIndex(
                name: "IX_BookCategories_CategoryId",
                table: "BookCategory",
                newName: "IX_BookCategory_CategoryId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookBorrow",
                table: "BookBorrow",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookPublisher",
                table: "BookPublisher",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookCategory",
                table: "BookCategory",
                columns: new[] { "BookId", "CategoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Book_BookPublisher_PublisherId",
                table: "Book",
                column: "PublisherId",
                principalTable: "BookPublisher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookBorrow_ApplicationUsers_UserId",
                table: "BookBorrow",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookBorrow_BookCopy_BookCopyId",
                table: "BookBorrow",
                column: "BookCopyId",
                principalTable: "BookCopy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookBorrow_Book_BookId",
                table: "BookBorrow",
                column: "BookId",
                principalTable: "Book",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCategory_Book_BookId",
                table: "BookCategory",
                column: "BookId",
                principalTable: "Book",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookCategory_Category_CategoryId",
                table: "BookCategory",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
