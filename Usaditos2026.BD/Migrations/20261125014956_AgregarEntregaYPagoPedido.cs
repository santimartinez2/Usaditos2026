using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Usaditos2026.BD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEntregaYPagoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionEnvio",
                table: "Pedidos",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoPago",
                table: "Pedidos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPago",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                table: "Pedidos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Efectivo");

            migrationBuilder.AddColumn<string>(
                name: "NotaCliente",
                table: "Pedidos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoContacto",
                table: "Pedidos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoEntrega",
                table: "Pedidos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Retiro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DireccionEnvio",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "FechaPago",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "NotaCliente",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "TelefonoContacto",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "TipoEntrega",
                table: "Pedidos");
        }
    }
}
