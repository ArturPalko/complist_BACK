using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace complist_BACK.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressFieldToEntitySection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserTypeId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Sections",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Sections");

            migrationBuilder.AlterColumn<int>(
                name: "UserTypeId",
                table: "Users",
                type: "int",
                nullable: true,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
