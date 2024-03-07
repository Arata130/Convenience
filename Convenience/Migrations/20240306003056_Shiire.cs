using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Convenience.Migrations
{
    /// <inheritdoc />
    public partial class Shiire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shiire_jisseki",
                columns: table => new
                {
                    chumon_code = table.Column<string>(type: "character varying(20)", nullable: false),
                    shiire_date = table.Column<DateOnly>(type: "date", nullable: false),
                    seq_by_shiiredate = table.Column<long>(type: "bigint", nullable: false),
                    shiire_saki_code = table.Column<string>(type: "character varying(10)", nullable: false),
                    shiire_prd_code = table.Column<string>(type: "character varying(10)", nullable: false),
                    shiire_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    shohin_code = table.Column<string>(type: "character varying(10)", nullable: false),
                    nonyu_su = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shiire_jisseki", x => new { x.chumon_code, x.shiire_date, x.seq_by_shiiredate, x.shiire_saki_code, x.shiire_prd_code });
                    table.ForeignKey(
                        name: "FK_shiire_jisseki_chumon_jisseki_meisai_chumon_code_shiire_sak~",
                        columns: x => new { x.chumon_code, x.shiire_saki_code, x.shiire_prd_code, x.shohin_code },
                        principalTable: "chumon_jisseki_meisai",
                        principalColumns: new[] { "chumon_code", "shiire_saki_code", "shiire_prd_code", "shohin_code" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "soko_zaiko",
                columns: table => new
                {
                    shiire_saki_code = table.Column<string>(type: "character varying(10)", nullable: false),
                    soko_zaiko = table.Column<string>(type: "character varying(10)", nullable: false),
                    shohin_code = table.Column<string>(type: "character varying(10)", nullable: false),
                    soko_zaiko_case_su = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    soko_zaiko_su = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    last_shiire_date = table.Column<DateOnly>(type: "date", nullable: false),
                    last_delivery_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_soko_zaiko", x => new { x.shiire_saki_code, x.soko_zaiko, x.shohin_code });
                    table.ForeignKey(
                        name: "FK_soko_zaiko_shiire_master_shiire_saki_code_soko_zaiko_shohin~",
                        columns: x => new { x.shiire_saki_code, x.soko_zaiko, x.shohin_code },
                        principalTable: "shiire_master",
                        principalColumns: new[] { "shiire_saki_code", "shiire_prd_code", "shohin_code" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shiire_jisseki_chumon_code_shiire_saki_code_shiire_prd_code~",
                table: "shiire_jisseki",
                columns: new[] { "chumon_code", "shiire_saki_code", "shiire_prd_code", "shohin_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shiire_jisseki");

            migrationBuilder.DropTable(
                name: "soko_zaiko");
        }
    }
}
