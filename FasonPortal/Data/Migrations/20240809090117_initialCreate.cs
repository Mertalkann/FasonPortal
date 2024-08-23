using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FasonPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FabrikaId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Atolyeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atolyeler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fabrikalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fabrikalar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IsTipleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsTipleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AtolyeIsler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtolyeId = table.Column<int>(type: "int", nullable: false),
                    IsTipiId = table.Column<int>(type: "int", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtolyeIsler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtolyeIsler_Atolyeler_AtolyeId",
                        column: x => x.AtolyeId,
                        principalTable: "Atolyeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtolyeIsler_IsTipleri_IsTipiId",
                        column: x => x.IsTipiId,
                        principalTable: "IsTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IsEmirleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsTipiId = table.Column<int>(type: "int", nullable: false),
                    AtolyeId = table.Column<int>(type: "int", nullable: false),
                    Adet = table.Column<int>(type: "int", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FabrikaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsEmirleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IsEmirleri_Atolyeler_AtolyeId",
                        column: x => x.AtolyeId,
                        principalTable: "Atolyeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IsEmirleri_Fabrikalar_FabrikaId",
                        column: x => x.FabrikaId,
                        principalTable: "Fabrikalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IsEmirleri_IsTipleri_IsTipiId",
                        column: x => x.IsTipiId,
                        principalTable: "IsTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FabrikaId",
                table: "AspNetUsers",
                column: "FabrikaId");

            migrationBuilder.CreateIndex(
                name: "IX_AtolyeIsler_AtolyeId",
                table: "AtolyeIsler",
                column: "AtolyeId");

            migrationBuilder.CreateIndex(
                name: "IX_AtolyeIsler_IsTipiId",
                table: "AtolyeIsler",
                column: "IsTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_IsEmirleri_AtolyeId",
                table: "IsEmirleri",
                column: "AtolyeId");

            migrationBuilder.CreateIndex(
                name: "IX_IsEmirleri_FabrikaId",
                table: "IsEmirleri",
                column: "FabrikaId");

            migrationBuilder.CreateIndex(
                name: "IX_IsEmirleri_IsTipiId",
                table: "IsEmirleri",
                column: "IsTipiId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Fabrikalar_FabrikaId",
                table: "AspNetUsers",
                column: "FabrikaId",
                principalTable: "Fabrikalar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Fabrikalar_FabrikaId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "AtolyeIsler");

            migrationBuilder.DropTable(
                name: "IsEmirleri");

            migrationBuilder.DropTable(
                name: "Atolyeler");

            migrationBuilder.DropTable(
                name: "Fabrikalar");

            migrationBuilder.DropTable(
                name: "IsTipleri");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FabrikaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FabrikaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");
        }
    }
}
