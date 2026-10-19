using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apiDonativos.Migrations
{
    /// <inheritdoc />
    public partial class FixDonativoArchivosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_donativos_archivos_donativos_detalle_id_detalle",
                table: "donativos_archivos");

            migrationBuilder.DropIndex(
                name: "IX_donativos_archivos_id_detalle",
                table: "donativos_archivos");

            migrationBuilder.DropColumn(
                name: "id_detalle",
                table: "donativos_archivos");

            migrationBuilder.RenameColumn(
                name: "fecha_actualizacion",
                table: "donativos_archivos",
                newName: "fecha_actualiza");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "fecha_actualiza",
                table: "donativos_archivos",
                newName: "fecha_actualizacion");

            migrationBuilder.AddColumn<int>(
                name: "id_detalle",
                table: "donativos_archivos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_donativos_archivos_id_detalle",
                table: "donativos_archivos",
                column: "id_detalle");

            migrationBuilder.AddForeignKey(
                name: "FK_donativos_archivos_donativos_detalle_id_detalle",
                table: "donativos_archivos",
                column: "id_detalle",
                principalTable: "donativos_detalle",
                principalColumn: "id_donativo_detalle",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
