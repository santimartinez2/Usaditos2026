# Usaditos2026

## 🛍️ Estructura Full Stack con Blazor y C#

Guía y documentación del e-commerce desarrollado para Usaditos Hernando (cliente: Carolina) usando arquitectura en capas con Blazor, Entity Framework Core y SQL Server.

**Materia:** Modelado y Arquitectura de Software – Evaluación 2  
**Alumno:** Santino Martinez  
**Profesor:** Sergio Algorry  

---

## 📁 Estructura de la Solución

```
Usaditos2026/
├── Usaditos2026.Server/              # Proyecto principal Blazor (host + API + páginas)
│   └── Usaditos2026.Server.Client/   # Proyecto Blazor (layout y componentes compartidos)
├── Usaditos2026.Shared/              # Biblioteca de clases compartidas (DTOs, constantes, excepciones)
├── Usaditos2026.Servicios/           # Biblioteca de clases (lógica de negocio)
├── Usaditos2026.BD/                  # Biblioteca de clases (contexto de base de datos)
│   └── Datos/
│       └── Entity/                   # Entidades de la base de datos
└── Usaditos2026.Repositorio/         # Biblioteca de clases (acceso a datos)
```

---

## 🔗 Diagrama de Dependencias

```
Usaditos2026.Shared
├── ← Usaditos2026.BD
│   └── ← Usaditos2026.Repositorio
├── ← Usaditos2026.Servicios
│   ├── ← Usaditos2026.Server.Client
│   │   └── ← Usaditos2026.Server
│   └── ← Usaditos2026.Server
└── (referenciado también por Server.Client y Server)
```

### Tabla de Referencia de Dependencias

| Proyecto | Depende de |
| :--- | :--- |
| **Usaditos2026.BD** | `Usaditos2026.Shared` |
| **Usaditos2026.Repositorio** | `Usaditos2026.BD`, `Usaditos2026.Shared` |
| **Usaditos2026.Servicios** | `Usaditos2026.BD`, `Usaditos2026.Repositorio`, `Usaditos2026.Shared` |
| **Usaditos2026.Server.Client** | `Usaditos2026.Servicios`, `Usaditos2026.Shared` |
| **Usaditos2026.Server** | `Server.Client`, `Servicios`, `Repositorio`, `BD`, `Shared` |

> **Flujo de una operación:** Página / Controller → Servicio → Repositorio → Base de datos.  
> *Las reglas de negocio (stock, límite de unidades) viven siempre en la capa de servicios y no en la interfaz.*

---

## 🎯 Caso de Uso Implementado

El proyecto implementa de punta a punta un único caso de uso: **Agregar y eliminar producto del carrito** (actor: Cliente).

### Escenario Principal

1. El cliente inicia sesión y navega el catálogo.
2. Selecciona un producto y lo agrega al carrito.
3. El sistema verifica que haya stock disponible.
4. El sistema agrega el producto y actualiza el total.
5. El cliente repite el proceso con otros productos.
6. Accede al carrito y ve el detalle de lo seleccionado.
7. Elimina un producto.
8. El sistema recalcula el total.

### Extensiones

| Código | Situación | Comportamiento |
| :---: | :--- | :--- |
| **3a** | El producto no tiene stock disponible | El sistema avisa al cliente que no está disponible |
| **5a** | El cliente quiere cambiar una cantidad | Se modifica desde el carrito y se recalcula el total |
| **6a** | El carrito está vacío | Se muestra un mensaje y un acceso para volver al catálogo |
| **7a** | El cliente quiere vaciar el carrito | Se eliminan todos los productos y se muestra el carrito vacío |

### Requisitos Especiales

- El carrito se actualiza en tiempo real, sin recargar la página.
- El stock se verifica siempre en el servidor, antes de agregar.
- Funciona desde dispositivos móviles.

---

## 🧠 Decisiones de Diseño

- **Límite de unidades:** Límite de 3 unidades por producto en el carrito. El tope real de cada producto es el menor entre 3 y su stock disponible.
- **Persistencia:** El carrito no se conserva al cerrar sesión: se vacía al cerrar y también al iniciar sesión.
- **Gestión de Stock:** Agregar al carrito no descuenta stock: solo lo valida. El descuento corresponde a la confirmación del pedido, que queda fuera de esta entrega.
- **Seguridad:** El identificador del cliente sale siempre de la sesión autenticada (nunca de lo que envía el navegador), así un cliente no puede ver ni modificar el carrito de otro.
- **Fuera de alcance:** Pagos, confirmación de pedido, panel de administración, historial de pedidos y filtros por categoría.

---

## 📦 Paquetes NuGet

### En `Usaditos2026.BD`
| Paquete | Descripción |
| :--- | :--- |
| `Microsoft.EntityFrameworkCore.SqlServer` | Proveedor de SQL Server para EF Core |
| `Microsoft.EntityFrameworkCore.Tools` | Herramientas para migraciones (`add-migration`, `update-database`) |

### En `Usaditos2026.Servicios`
| Paquete | Descripción |
| :--- | :--- |
| `Microsoft.Extensions.Identity.Core` | `PasswordHasher` para guardar contraseñas hasheadas |

### En `Usaditos2026.Server.Client`
| Paquete | Descripción |
| :--- | :--- |
| `Microsoft.AspNetCore.Components.Authorization` | `AuthorizeView` para mostrar contenido según la sesión |

### En `Usaditos2026.Server`
| Paquete | Descripción |
| :--- | :--- |
| `Microsoft.EntityFrameworkCore.SqlServer` | Necesario para el registro del contexto |
| `Microsoft.EntityFrameworkCore.Tools` | Herramientas de EF Core |
| `Swashbuckle.AspNetCore` | Swagger para probar la API |

---

