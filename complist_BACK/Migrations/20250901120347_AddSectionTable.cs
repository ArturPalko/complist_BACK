using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace complist_BACK.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Positions_PositionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MailTypeIdP",
                table: "Mails");

            migrationBuilder.AlterColumn<int>(
                name: "PositionId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Mails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "Mails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_SectionId",
                table: "Users",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Mails_SectionId",
                table: "Mails",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DepartmentId",
                table: "Sections",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mails_Sections_SectionId",
                table: "Mails",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Positions_PositionId",
                table: "Users",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Sections_SectionId",
                table: "Users",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mails_Sections_SectionId",
                table: "Mails");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Positions_PositionId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Sections_SectionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Users_SectionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Mails_SectionId",
                table: "Mails");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Mails");

            migrationBuilder.AlterColumn<int>(
                name: "PositionId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Mails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MailTypeIdP",
                table: "Mails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Positions_PositionId",
                table: "Users",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
