using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BirFikrimVar.Data.Migrations
{
    /// <inheritdoc />
    public partial class DegerlendirmeVeDosyaGuncellemesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DosyaEkleri");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Kullanicilar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenEndDate",
                table: "Kullanicilar",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FikirDosyalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FikirId = table.Column<int>(type: "integer", nullable: false),
                    OrijinalDosyaAdi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    KayitliDosyaYolu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Uzanti = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DosyaBoyutu = table.Column<long>(type: "bigint", nullable: false),
                    YuklenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FikirDosyalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FikirDosyalari_Fikirler_FikirId",
                        column: x => x.FikirId,
                        principalTable: "Fikirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KomisyonDegerlendirmeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FikirId = table.Column<int>(type: "integer", nullable: false),
                    DegerlendiriciId = table.Column<string>(type: "text", nullable: false),
                    Puan = table.Column<int>(type: "integer", nullable: false),
                    DegerlendirmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KomisyonDegerlendirmeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KomisyonDegerlendirmeleri_Fikirler_FikirId",
                        column: x => x.FikirId,
                        principalTable: "Fikirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnOnayDegerlendirmeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FikirId = table.Column<int>(type: "integer", nullable: false),
                    KategoriId = table.Column<int>(type: "integer", nullable: false),
                    DegerlendiriciId = table.Column<string>(type: "text", nullable: false),
                    Puan = table.Column<int>(type: "integer", nullable: false),
                    DegerlendirmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnOnayDegerlendirmeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnOnayDegerlendirmeleri_Fikirler_FikirId",
                        column: x => x.FikirId,
                        principalTable: "Fikirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnOnayDegerlendirmeleri_Kategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "Kategoriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FikirDosyalari_FikirId",
                table: "FikirDosyalari",
                column: "FikirId");

            migrationBuilder.CreateIndex(
                name: "IX_KomisyonDegerlendirmeleri_FikirId",
                table: "KomisyonDegerlendirmeleri",
                column: "FikirId");

            migrationBuilder.CreateIndex(
                name: "IX_OnOnayDegerlendirmeleri_FikirId",
                table: "OnOnayDegerlendirmeleri",
                column: "FikirId");

            migrationBuilder.CreateIndex(
                name: "IX_OnOnayDegerlendirmeleri_KategoriId",
                table: "OnOnayDegerlendirmeleri",
                column: "KategoriId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FikirDosyalari");

            migrationBuilder.DropTable(
                name: "KomisyonDegerlendirmeleri");

            migrationBuilder.DropTable(
                name: "OnOnayDegerlendirmeleri");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "RefreshTokenEndDate",
                table: "Kullanicilar");

            migrationBuilder.CreateTable(
                name: "DosyaEkleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FikirId = table.Column<int>(type: "integer", nullable: false),
                    BoyutBayt = table.Column<long>(type: "bigint", nullable: false),
                    DosyaYolu = table.Column<string>(type: "text", nullable: false),
                    GuvenliAd = table.Column<string>(type: "text", nullable: false),
                    MimeTuru = table.Column<string>(type: "text", nullable: false),
                    OrijinalAd = table.Column<string>(type: "text", nullable: false),
                    Uzanti = table.Column<string>(type: "text", nullable: false),
                    YuklenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DosyaEkleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DosyaEkleri_Fikirler_FikirId",
                        column: x => x.FikirId,
                        principalTable: "Fikirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DosyaEkleri_FikirId",
                table: "DosyaEkleri",
                column: "FikirId");
        }
    }
}
