using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;

namespace Usaditos2026.Server.Data
{
    public static class SeedData
    {
        public static async Task InicializarAsync(AppDbContext context)
        {
            if (await context.Categorias.AnyAsync())
                return;

            var remeras = new Categoria
            {
                Nombre = "Remeras",
                Descripcion = "Remeras y musculosas para niños"
            };
            var pantalones = new Categoria
            {
                Nombre = "Pantalones",
                Descripcion = "Pantalones, shorts y joggers para niños"
            };
            var abrigos = new Categoria
            {
                Nombre = "Abrigos",
                Descripcion = "Camperas, buzos y abrigos para niños"
            };

            context.Categorias.AddRange(remeras, pantalones, abrigos);
            await context.SaveChangesAsync();

            var productos = new List<Producto>
            {
                new Producto
                {
                    Nombre = "Remera Básica Blanca",
                    Descripcion = "Remera de algodón peinado con cuello redondo",
                    Precio = 4500m,
                    StockDisponible = 10,
                    Activo = true,
                    CategoriaId = remeras.Id
                },
                new Producto
                {
                    Nombre = "Remera Estampada Dinosaurio",
                    Descripcion = "Remera de algodón con estampado de dinosaurios",
                    Precio = 5200m,
                    StockDisponible = 1,
                    Activo = true,
                    CategoriaId = remeras.Id
                },
                new Producto
                {
                    Nombre = "Musculosa Rayas",
                    Descripcion = "Musculosa de algodón con rayas horizontales",
                    Precio = 3900m,
                    StockDisponible = 0,
                    Activo = true,
                    CategoriaId = remeras.Id
                },
                new Producto
                {
                    Nombre = "Pantalón Jogger Gris",
                    Descripcion = "Jogger de frisa con puños elásticos",
                    Precio = 7800m,
                    StockDisponible = 5,
                    Activo = true,
                    CategoriaId = pantalones.Id
                },
                new Producto
                {
                    Nombre = "Short Deportivo Azul",
                    Descripcion = "Short de poliéster con cintura elástica",
                    Precio = 4200m,
                    StockDisponible = 8,
                    Activo = true,
                    CategoriaId = pantalones.Id
                },
                new Producto
                {
                    Nombre = "Jean Clásico",
                    Descripcion = "Jean de denim con bolsillos y cierre",
                    Precio = 9500m,
                    StockDisponible = 1,
                    Activo = true,
                    CategoriaId = pantalones.Id
                },
                new Producto
                {
                    Nombre = "Buzo Canguro Rojo",
                    Descripcion = "Buzo de frisa con capucha y bolsillo canguro",
                    Precio = 8500m,
                    StockDisponible = 6,
                    Activo = true,
                    CategoriaId = abrigos.Id
                },
                new Producto
                {
                    Nombre = "Campera Rompeviento",
                    Descripcion = "Campera liviana rompeviento con capucha",
                    Precio = 11000m,
                    StockDisponible = 5,
                    Activo = true,
                    CategoriaId = abrigos.Id
                },
                new Producto
                {
                    Nombre = "Campera de Polar",
                    Descripcion = "Campera de polar abrigada con cierre",
                    Precio = 12500m,
                    StockDisponible = 2,
                    Activo = true,
                    CategoriaId = abrigos.Id
                }
            };

            context.Productos.AddRange(productos);

            var cliente = new Cliente
            {
                Nombre = "Cliente de Prueba",
                Email = "cliente@usaditos.com",
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            };

            var hasher = new PasswordHasher<Cliente>();
            cliente.PasswordHash = hasher.HashPassword(cliente, "Usaditos123!");

            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
        }
    }
}
