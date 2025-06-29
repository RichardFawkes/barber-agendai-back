using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbeariaSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Tenants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Website",
                table: "Tenants");
        }
    }
}
