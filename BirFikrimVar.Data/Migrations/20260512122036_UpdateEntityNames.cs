using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirFikrimVar.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KullaniciId",
                table: "Degerlendirmeler",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnOnayDegerlendirmeleri_DegerlendiriciId",
                table: "OnOnayDegerlendirmeleri",
                column: "DegerlendiriciId");

            migrationBuilder.CreateIndex(
                name: "IX_KomisyonDegerlendirmeleri_DegerlendiriciId",
                table: "KomisyonDegerlendirmeleri",
                column: "DegerlendiriciId");

            migrationBuilder.CreateIndex(
                name: "IX_Degerlendirmeler_KullaniciId",
                table: "Degerlendirmeler",
                column: "KullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Degerlendirmeler_Kullanicilar_KullaniciId",
                table: "Degerlendirmeler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KomisyonDegerlendirmeleri_Kullanicilar_DegerlendiriciId",
                table: "KomisyonDegerlendirmeleri",
                column: "DegerlendiriciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OnOnayDegerlendirmeleri_Kullanicilar_DegerlendiriciId",
                table: "OnOnayDegerlendirmeleri",
                column: "DegerlendiriciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Degerlendirmeler_Kullanicilar_KullaniciId",
                table: "Degerlendirmeler");

            migrationBuilder.DropForeignKey(
                name: "FK_KomisyonDegerlendirmeleri_Kullanicilar_DegerlendiriciId",
                table: "KomisyonDegerlendirmeleri");

            migrationBuilder.DropForeignKey(
                name: "FK_OnOnayDegerlendirmeleri_Kullanicilar_DegerlendiriciId",
                table: "OnOnayDegerlendirmeleri");

            migrationBuilder.DropIndex(
                name: "IX_OnOnayDegerlendirmeleri_DegerlendiriciId",
                table: "OnOnayDegerlendirmeleri");

            migrationBuilder.DropIndex(
                name: "IX_KomisyonDegerlendirmeleri_DegerlendiriciId",
                table: "KomisyonDegerlendirmeleri");

            migrationBuilder.DropIndex(
                name: "IX_Degerlendirmeler_KullaniciId",
                table: "Degerlendirmeler");

            migrationBuilder.DropColumn(
                name: "KullaniciId",
                table: "Degerlendirmeler");
        }
    }
}
