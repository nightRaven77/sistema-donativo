using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apiDonativos.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Donadores",
            //    columns: table => new
            //    {
            //        id_donador = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        tipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RFC = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        email = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        colonia = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ciudad = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        estado = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        codigo_postal = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        notas = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Donadores", x => x.id_donador);
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Donadores");
        }
    }
}
