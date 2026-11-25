using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Shared.Constantes;

namespace Usaditos2026.BD.Datos
{
    public class AppDbContext : DbContext
    {
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ItemCarrito> ItemsCarrito { get; set; }
        public DbSet<ItemPedido> ItemsPedido { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Producto> Productos { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.Property(c => c.Rol)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue(RolConstantes.Cliente);
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Precio).HasPrecision(18, 2);
            });

            modelBuilder.Entity<ItemCarrito>()
                .Property(i => i.PrecioUnitario).HasPrecision(18, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total).HasPrecision(18, 2);

            modelBuilder.Entity<ItemPedido>(entity =>
            {
                // El nombre y el precio se copian al confirmar: son un snapshot, no un puntero al producto.
                entity.Property(i => i.NombreProducto).IsRequired().HasMaxLength(200);
                entity.Property(i => i.PrecioUnitario).HasPrecision(18, 2);

                entity.HasOne(i => i.Pedido)
                    .WithMany(p => p.Items)
                    .HasForeignKey(i => i.PedidoId);

                // Restrict evita borrar un producto que aparece en el historial de pedidos.
                entity.HasOne(i => i.Producto)
                    .WithMany(p => p.ItemsPedido)
                    .HasForeignKey(i => i.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
