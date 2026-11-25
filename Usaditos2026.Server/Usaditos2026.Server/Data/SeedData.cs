using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Usaditos2026.BD.Datos;
using Usaditos2026.BD.Datos.Entity;
using Usaditos2026.Shared.Constantes;

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
                    Talle = "4",
                    Estado = EstadoPrenda.NuevoConEtiqueta,
                    Observaciones = "Con etiqueta original, sin uso.",
                    FechaAlta = DateTime.UtcNow.AddDays(-1),
                    CategoriaId = remeras.Id
                },
                new Producto
                {
                    Nombre = "Remera Estampada Dinosaurio",
                    Descripcion = "Remera de algodón con estampado de dinosaurios",
                    Precio = 5200m,
                    StockDisponible = 1,
                    Activo = true,
                    Talle = "6",
                    Estado = EstadoPrenda.ComoNueva,
                    Observaciones = "Estampado intacto, sin marcas.",
                    FechaAlta = DateTime.UtcNow.AddDays(-2),
                    CategoriaId = remeras.Id
                },
                new Producto
                {
                    Nombre = "Musculosa Rayas",
                    Descripcion = "Musculosa de algodón con rayas horizontales",
                    Precio = 3900m,
                    StockDisponible = 0,
                    Activo = true,
                    Talle = "8",
                    Estado = EstadoPrenda.Bueno,
                    Observaciones = "Pequeña mancha en la manga.",
                    FechaAlta = DateTime.UtcNow.AddDays(-3),
                    CategoriaId = remeras.Id
                },
                new Producto
                {
                    Nombre = "Pantalón Jogger Gris",
                    Descripcion = "Jogger de frisa con puños elásticos",
                    Precio = 7800m,
                    StockDisponible = 5,
                    Activo = true,
                    Talle = "10",
                    Estado = EstadoPrenda.MuyBueno,
                    Observaciones = "Puños elásticos en buen estado.",
                    FechaAlta = DateTime.UtcNow.AddDays(-4),
                    CategoriaId = pantalones.Id
                },
                new Producto
                {
                    Nombre = "Short Deportivo Azul",
                    Descripcion = "Short de poliéster con cintura elástica",
                    Precio = 4200m,
                    StockDisponible = 8,
                    Activo = true,
                    Talle = "12",
                    Estado = EstadoPrenda.Bueno,
                    Observaciones = "Desgaste leve en las costuras.",
                    FechaAlta = DateTime.UtcNow.AddDays(-5),
                    CategoriaId = pantalones.Id
                },
                new Producto
                {
                    Nombre = "Jean Clásico",
                    Descripcion = "Jean de denim con bolsillos y cierre",
                    Precio = 9500m,
                    StockDisponible = 1,
                    Activo = true,
                    Talle = "14",
                    Estado = EstadoPrenda.ComoNueva,
                    Observaciones = "Sin signos de uso.",
                    FechaAlta = DateTime.UtcNow.AddDays(-6),
                    CategoriaId = pantalones.Id
                },
                new Producto
                {
                    Nombre = "Buzo Canguro Rojo",
                    Descripcion = "Buzo de frisa con capucha y bolsillo canguro",
                    Precio = 8500m,
                    StockDisponible = 6,
                    Activo = true,
                    Talle = "16",
                    Estado = EstadoPrenda.MuyBueno,
                    Observaciones = "Bolsillo canguro impecable.",
                    FechaAlta = DateTime.UtcNow.AddDays(-7),
                    CategoriaId = abrigos.Id
                },
                new Producto
                {
                    Nombre = "Campera Rompeviento",
                    Descripcion = "Campera liviana rompeviento con capucha",
                    Precio = 11000m,
                    StockDisponible = 5,
                    Activo = true,
                    Talle = TalleConstantes.TalleUnico,
                    Estado = EstadoPrenda.MuyBueno,
                    Observaciones = "Cierre y capucha funcionan correctamente.",
                    FechaAlta = DateTime.UtcNow.AddDays(-8),
                    CategoriaId = abrigos.Id
                },
                new Producto
                {
                    Nombre = "Campera de Polar",
                    Descripcion = "Campera de polar abrigada con cierre",
                    Precio = 12500m,
                    StockDisponible = 2,
                    Activo = true,
                    Talle = TalleConstantes.TalleUnico,
                    Estado = EstadoPrenda.Bueno,
                    Observaciones = "Polar con leve pelusa por uso.",
                    FechaAlta = DateTime.UtcNow.AddDays(-9),
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

        public static async Task CrearAdminInicialAsync(
            AppDbContext context,
            IConfiguration configuration,
            ILogger logger)
        {
            var email = configuration["AdminInicial:Email"];
            var password = configuration["AdminInicial:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            if (await context.Clientes.AnyAsync(c => c.Rol == RolConstantes.Admin))
                return;

            var emailNormalizado = email.Trim().ToLowerInvariant();
            var clienteExistente = await context.Clientes
                .FirstOrDefaultAsync(c => c.Email == emailNormalizado);

            if (clienteExistente is not null)
            {
                logger.LogWarning(
                    "No se creó el admin inicial: ya existe un cliente con el email {Email} y rol {Rol}. Revisá AdminInicial:Email.",
                    emailNormalizado,
                    clienteExistente.Rol);
                return;
            }

            var admin = new Cliente
            {
                Nombre = "Administrador",
                Email = emailNormalizado,
                Rol = RolConstantes.Admin,
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            };

            admin.PasswordHash = new PasswordHasher<Cliente>().HashPassword(admin, password);

            context.Clientes.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
