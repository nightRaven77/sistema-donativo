using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apiDonativos.Migrations
{
    /// <inheritdoc />
    public partial class AddDonativoArchivosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "donativos_archivos",
                columns: table => new
                {
                    id_archivo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_detalle = table.Column<int>(type: "int", nullable: false),
                    id_donativo = table.Column<int>(type: "int", nullable: false),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nombre_original = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fecha_actualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_donativos_archivos", x => x.id_archivo);
                    table.ForeignKey(
                        name: "FK_donativos_archivos_donativos_detalle_id_detalle",
                        column: x => x.id_detalle,
                        principalTable: "donativos_detalle",
                        principalColumn: "id_donativo_detalle",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_donativos_archivos_donativos_id_donativo",
                        column: x => x.id_donativo,
                        principalTable: "donativos",
                        principalColumn: "id_donativo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_donativos_archivos_id_detalle",
                table: "donativos_archivos",
                column: "id_detalle");

            migrationBuilder.CreateIndex(
                name: "IX_donativos_archivos_id_donativo",
                table: "donativos_archivos",
                column: "id_donativo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "donativos_archivos");
        }
    }
}
