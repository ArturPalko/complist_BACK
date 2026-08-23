using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace complist_BACK.Migrations
{
    /// <inheritdoc />
    public partial class MailSectionsManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mails_Sections_SectionId",
                table: "Mails");

            migrationBuilder.DropIndex(
                name: "IX_Mails_SectionId",
                table: "Mails");

            migrationBuilder.CreateTable(
                name: "MailSection",
                columns: table => new
                {
                    MailsId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    SectionsId = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_MailSection",
                        x => new
                        {
                            x.MailsId,
                            x.SectionsId
                        });

                    table.ForeignKey(
                        name: "FK_MailSection_Mails_MailsId",
                        column: x => x.MailsId,
                        principalTable: "Mails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_MailSection_Sections_SectionsId",
                        column: x => x.SectionsId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MailSection_SectionsId",
                table: "MailSection",
                column: "SectionsId");

            // Переносимо старі зв'язки Mail -> Section
            migrationBuilder.Sql("""
        INSERT INTO MailSection (MailsId, SectionsId)
        SELECT Id, SectionId
        FROM Mails
        WHERE SectionId IS NOT NULL;
    """);

            // Тільки тепер видаляємо старий SectionId
            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Mails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "Mails",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
        UPDATE Mails
        SET SectionId = (
            SELECT TOP 1 ms.SectionsId
            FROM MailSection ms
            WHERE ms.MailsId = Mails.Id
        );
    """);

            migrationBuilder.DropTable(
                name: "MailSection");

            migrationBuilder.CreateIndex(
                name: "IX_Mails_SectionId",
                table: "Mails",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mails_Sections_SectionId",
                table: "Mails",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");
        }
    }
}
