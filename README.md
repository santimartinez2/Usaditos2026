# Usaditos2026

## 🛍️ Estructura Full Stack con Blazor y C#

Guía y documentación del e-commerce desarrollado para Usaditos Hernando (cliente: Carolina) usando arquitectura en capas con Blazor, Entity Framework Core y SQL Server.

**Materia:** Modelado y Arquitectura de Software (Evaluación 2)  
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
│       └── ← Usaditos2026.Servicios
│           ├── ← Usaditos2026.Server.Client
│           │   └── ← Usaditos2026.Server
│           └── ← Usaditos2026.Server
└── (referenciado también por todos los demás proyectos)
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

- El carrito se actualiza sin recargar la página (el detalle y el total del carrito, y el aviso del catálogo con las unidades y el total).
- El stock se verifica siempre en el servidor, antes de agregar.
- Funciona desde dispositivos móviles (diseño responsive).

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

## 🚀 Cómo Ejecutarlo

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (viene incluido con Visual Studio) o una instancia de SQL Server configurada en `appsettings.json`

### 1. Clonar el repositorio

```bash
git clone https://github.com/santimartinez2/Usaditos2026.git
cd Usaditos2026
```

### 2. Ejecutar la aplicación

```bash
dotnet run --project Usaditos2026.Server/Usaditos2026.Server
```

También se puede abrir `Usaditos2026.slnx` en Visual Studio y ejecutar el proyecto `Usaditos2026.Server`.

### 3. Abrir en el navegador

- Aplicación: <http://localhost:5250>
- Swagger: <http://localhost:5250/swagger>

> Con el perfil **https** la dirección es `https://localhost:7200`.

Al iniciar en modo *Development* (el modo por defecto al ejecutar con `dotnet run` o con F5), la aplicación **crea la base de datos, aplica las migraciones y carga los datos de prueba automáticamente**. No hace falta ningún paso manual.

---

## ⚙️ Cadena de Conexión

Está en `Usaditos2026.Server/Usaditos2026.Server/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ConnSqlServer": "Server=(localdb)\\mssqllocaldb;Database=Usaditos2026;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

> Por defecto usa **SQL Server LocalDB**. Si se usa otra instancia (por ejemplo SQL Express), se puede reemplazar la cadena en ese archivo, o pasarla por variable de entorno antes de ejecutar, sin editar el código:
>
> ```powershell
> $env:ConnectionStrings__ConnSqlServer="Server=.\SQLEXPRESS;Database=Usaditos2026;Trusted_Connection=True;TrustServerCertificate=True;"
> ```

---

## 🔄 Migraciones

La aplicación las aplica sola al arrancar en *Development*. Para manejarlas a mano, desde la raíz del repo:

```bash
# Aplicar las migraciones a la base de datos
dotnet ef database update --project Usaditos2026.BD --startup-project Usaditos2026.Server/Usaditos2026.Server

# Empezar de cero con una base limpia
dotnet ef database drop --project Usaditos2026.BD --startup-project Usaditos2026.Server/Usaditos2026.Server --force
```

---

## 🧪 Datos de Prueba

**Cliente de prueba**

| Email | Contraseña |
| :--- | :--- |
| `cliente@usaditos.com` | `Usaditos123!` |

También se puede crear una cuenta nueva desde `/registro`.

**Catálogo de prueba:** 3 categorías (Remeras, Pantalones, Abrigos) y 9 productos con stock variado, pensados para probar cada escenario:

| Producto | Stock | Sirve para probar |
| :--- | :---: | :--- |
| Musculosa Rayas | 0 | Aviso de producto sin stock (`3a`) |
| Remera Estampada Dinosaurio | 1 | Stock insuficiente |
| Jean Clásico | 1 | Stock insuficiente |
| Varios productos | 5 o más | Límite de 3 unidades por producto |

### Pruebas manuales sugeridas

1. Entrar a `/catalogo` sin sesión: redirige a `/login`.
2. Iniciar sesión y agregar productos: el aviso muestra las unidades y el total del carrito.
3. Intentar agregar la **Musculosa Rayas**: aviso de producto sin stock.
4. Agregar 4 veces un producto con stock suficiente: el cuarto intento avisa el límite de 3 unidades.
5. En `/carrito`: modificar cantidades, eliminar un producto y verificar que el total se recalcula.
6. Vaciar el carrito y verificar el mensaje de carrito vacío.
7. Cerrar sesión, volver a ingresar y comprobar que el carrito está vacío.
8. Repetir en un ancho de pantalla de celular (~375 px).

---

## 🔌 API REST

Se puede probar desde Swagger (`/swagger`). Los endpoints del carrito requieren haber iniciado sesión: primero ejecutar el login.

| Método | Ruta | Descripción |
| :---: | :--- | :--- |
| `POST` | `/api/auth/login` | Inicia sesión |
| `POST` | `/api/auth/logout` | Cierra sesión y vacía el carrito |
| `GET` | `/api/productos` | Lista los productos activos |
| `GET` | `/api/carrito` | Obtiene el carrito del cliente |
| `POST` | `/api/carrito/items` | Agrega un producto al carrito |
| `PUT` | `/api/carrito/items/{itemId}` | Modifica la cantidad de un ítem |
| `DELETE` | `/api/carrito/items/{itemId}` | Elimina un ítem del carrito |
| `DELETE` | `/api/carrito` | Vacía el carrito |

**Códigos de error**

| Código | Significado |
| :---: | :--- |
| `401` | No hay sesión iniciada |
| `403` | El ítem no pertenece al carrito del cliente |
| `404` | El producto o el ítem no existe |
| `409` | Sin stock suficiente o límite de unidades superado |

---

## 🛠️ Solución de Problemas

| Problema | Cómo comprobarlo y resolverlo |
| :--- | :--- |
| No compila / versión de .NET | Ejecutar `dotnet --list-sdks`: tiene que aparecer una versión `10.x`. |
| Error de conexión a la base | Ejecutar `sqllocaldb info`: tiene que listar `MSSQLLocalDB`. Si no existe, usar otra instancia con la variable de entorno de la sección *Cadena de Conexión*. |
| Falla la restauración de paquetes | Se necesita acceso a `nuget.org`. Verificar la conexión o el proxy de la red. |
| El puerto 5250 está ocupado | Hay otra copia de la aplicación en ejecución. Detenerla (`Ctrl+C` o detener la depuración) y volver a lanzar. |
| Advertencia de certificado con https | Usar el perfil http: `dotnet run --launch-profile http --project Usaditos2026.Server/Usaditos2026.Server` y abrir `http://localhost:5250`. |
| No abre `Usaditos2026.slnx` | El formato `.slnx` requiere una versión reciente de Visual Studio. Ejecutar desde la terminal con `dotnet run`. |
| Aparecen advertencias al compilar (NU1902/NU1903, CS8618) | Son avisos de auditoría de paquetes de la plantilla y de propiedades de las entidades. No afectan el funcionamiento. |

---

## ✅ Checklist del Caso de Uso

- [x] Registro e inicio de sesión de clientes (cookie, contraseña hasheada)
- [x] Catálogo de productos con precio y stock
- [x] Datos de prueba cargados automáticamente
- [x] Agregar producto al carrito verificando stock en el servidor
- [x] Aviso de producto sin stock (`3a`)
- [x] Límite de 3 unidades por producto
- [x] Aviso al agregar con las unidades y el total actualizados del carrito
- [x] Pantalla del carrito con el detalle de productos y el total
- [x] Eliminar producto y recalcular el total
- [x] Modificar cantidad desde el carrito (`5a`)
- [x] Mensaje de carrito vacío con acceso al catálogo (`6a`)
- [x] Vaciar carrito (`7a`)
- [x] El carrito se vacía al cerrar e iniciar sesión
- [x] El cliente solo puede acceder a su propio carrito
- [x] Diseño responsive para celulares
- [ ] Contador de unidades del menú actualizado en vivo (hoy se actualiza al navegar entre páginas)
