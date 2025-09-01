using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace complist_BACK.Migrations
{
    /// <inheritdoc />
    public partial class SectionTableFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Sections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Department",
                table: "Sections",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
