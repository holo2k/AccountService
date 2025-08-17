using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.Migrations
{
    /// <inheritdoc />
    public partial class InboxCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_inbox_consumed",
                table: "inbox_consumed");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inbox_consumed",
                table: "inbox_consumed",
                columns: new[] { "MessageId", "Handler" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_inbox_consumed",
                table: "inbox_consumed");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inbox_consumed",
                table: "inbox_consumed",
                column: "MessageId");
        }
    }
}
