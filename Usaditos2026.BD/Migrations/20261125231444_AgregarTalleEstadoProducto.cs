using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Usaditos2026.BD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTalleEstadoProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Productos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "MuyBueno");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAlta",
                table: "Productos",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Productos",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Talle",
                table: "Productos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Talle único");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaAlta",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Talle",
                table: "Productos");
        }
    }
}
