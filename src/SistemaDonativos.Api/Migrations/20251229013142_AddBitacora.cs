using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apiDonativos.Migrations
{
    /// <inheritdoc />
    public partial class AddBitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Bitacoras",
            //    columns: table => new
            //    {
            //        id_bitacora = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        id_usuario = table.Column<int>(type: "int", nullable: false),
            //        modulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        submodulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        accion = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Bitacoras", x => x.id_bitacora);
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bitacoras");
        }
    }
}
