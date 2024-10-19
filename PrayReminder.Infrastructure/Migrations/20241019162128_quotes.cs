using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PrayReminder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class quotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Quotes",
                columns: new[] { "Id", "Author", "Body" },
                values: new object[,]
                {
                    { new Guid("56d82b24-c0d1-4d40-8e08-a2dfe90294ef"), null, "namozga shoshiling ish qochib ketmaydi" },
                    { new Guid("64090c66-858e-4ade-a596-03a36a6985d1"), null, "yashang ishni tashang, namoz vaqti bo'ldi" },
                    { new Guid("bb9c3395-177a-40e8-bae1-f1455b0719c3"), null, "“Albatta, namoz mo‘minlarga vaqtida farz qilingandir” (Niso surasi, 103-oyat)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quotes");
        }
    }
}
