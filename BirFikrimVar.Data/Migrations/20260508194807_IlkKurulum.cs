using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BirFikrimVar.Data.Migrations
{
    /// <inheritdoc />
    public partial class IlkKurulum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    Soyad = table.Column<string>(type: "text", nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roller",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fikirler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Baslik = table.Column<string>(type: "text", nullable: false),
                    Ozet = table.Column<string>(type: "text", nullable: false),
                    Kapsam = table.Column<string>(type: "text", nullable: false),
                    YenilikciYan = table.Column<string>(type: "text", nullable: false),
                    GucluYonler = table.Column<string>(type: "text", nullable: false),
                    ZayifYonler = table.Column<string>(type: "text", nullable: false),
                    Firsatlar = table.Column<string>(type: "text", nullable: false),
                    Tehditler = table.Column<string>(type: "text", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BasvuruSahibiId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fikirler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fikirler_Kullanicilar_BasvuruSahibiId",
                        column: x => x.BasvuruSahibiId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_Roller_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciRolleri",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciRolleri", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_KullaniciRolleri_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KullaniciRolleri_Roller_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Degerlendirmeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FikirId = table.Column<int>(type: "integer", nullable: false),
                    DegerlendiriciId = table.Column<string>(type: "text", nullable: false),
                    Tur = table.Column<int>(type: "integer", nullable: false),
                    StratejikUyumPuani = table.Column<int>(type: "integer", nullable: false),
                    YenilikcilikPuani = table.Column<int>(type: "integer", nullable: false),
                    UygulanabilirlikPuani = table.Column<int>(type: "integer", nullable: false),
                    BeklenenFaydaPuani = table.Column<int>(type: "integer", nullable: false),
                    RiskVeSurdurulebilirlikPuani = table.Column<int>(type: "integer", nullable: false),
                    KaynakIhtiyaciPuani = table.Column<int>(type: "integer", nullable: false),
                    EkNot = table.Column<string>(type: "text", nullable: false),
                    DegerlendirmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Degerlendirmeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Degerlendirmeler_Fikirler_FikirId",
                        column: x => x.FikirId,
                        principalTable: "Fikirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Degerlendirmeler_Kullanicilar_DegerlendiriciId",
                        column: x => x.DegerlendiriciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DosyaEkleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FikirId = table.Column<int>(type: "integer", nullable: false),
                    OrijinalAd = table.Column<string>(type: "text", nullable: false),
                    GuvenliAd = table.Column<string>(type: "text", nullable: false),
                    DosyaYolu = table.Column<string>(type: "text", nullable: false),
                    Uzanti = table.Column<string>(type: "text", nullable: false),
                    MimeTuru = table.Column<string>(type: "text", nullable: false),
                    BoyutBayt = table.Column<long>(type: "bigint", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "FikirDurumGecmisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FikirId = table.Column<int>(type: "integer", nullable: false),
                    EskiDurum = table.Column<int>(type: "integer", nullable: false),
                    YeniDurum = table.Column<int>(type: "integer", nullable: false),
                    IslemYapanKullaniciId = table.Column<string>(type: "text", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FikirDurumGecmisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FikirDurumGecmisleri_Fikirler_FikirId",
                        column: x => x.FikirId,
                        principalTable: "Fikirler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FikirDurumGecmisleri_Kullanicilar_IslemYapanKullaniciId",
                        column: x => x.IslemYapanKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Degerlendirmeler_DegerlendiriciId",
                table: "Degerlendirmeler",
                column: "DegerlendiriciId");

            migrationBuilder.CreateIndex(
                name: "IX_Degerlendirmeler_FikirId",
                table: "Degerlendirmeler",
                column: "FikirId");

            migrationBuilder.CreateIndex(
                name: "IX_DosyaEkleri_FikirId",
                table: "DosyaEkleri",
                column: "FikirId");

            migrationBuilder.CreateIndex(
                name: "IX_FikirDurumGecmisleri_FikirId",
                table: "FikirDurumGecmisleri",
                column: "FikirId");

            migrationBuilder.CreateIndex(
                name: "IX_FikirDurumGecmisleri_IslemYapanKullaniciId",
                table: "FikirDurumGecmisleri",
                column: "IslemYapanKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Fikirler_BasvuruSahibiId",
                table: "Fikirler",
                column: "BasvuruSahibiId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Kullanicilar",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Kullanicilar",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciRolleri_RoleId",
                table: "KullaniciRolleri",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roller",
                column: "NormalizedName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Degerlendirmeler");

            migrationBuilder.DropTable(
                name: "DosyaEkleri");

            migrationBuilder.DropTable(
                name: "FikirDurumGecmisleri");

            migrationBuilder.DropTable(
                name: "KullaniciRolleri");

            migrationBuilder.DropTable(
                name: "Fikirler");

            migrationBuilder.DropTable(
                name: "Roller");

            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}
