# Usaditos Hernando — E-Commerce de Moda Infantil Circular

Plataforma integral de comercio electrónico desarrollada con **.NET 10**, **Blazor Web App**, **Entity Framework Core** y **SQL Server**, diseñada para la compra, venta en consignación y gestión de indumentaria infantil seleccionada en Hernando, Córdoba, Argentina.

El proyecto promueve la sustentabilidad y la economía circular de prendas infantiles, ofreciendo una experiencia fluida tanto para las familias compradoras como para la administración del negocio.

---

## 🚀 Características Principales

### 🛍️ Experiencia del Cliente (Tienda Online)
- **Catálogo Interactivo con Búsqueda y Filtros Facetados:** Filtrado dinámico por categoría, talle infantil canónico (`RN`, `0-3 meses`, `2`, `4`, `6`... hasta calzado), estado de la prenda (`Nuevo con etiqueta`, `Como nueva`, `Muy bueno`, `Bueno`), texto libre y disponibilidad en stock.
- **Carrito de Compras en Tiempo Real:** Validación estricta de stock disponible en el servidor, actualización reactiva sin recarga de página y control de tope según inventario real.
- **Proceso de Checkout Ágil:**
  - **Modalidades de Entrega:** Retiro gratuito en el local comercial o envío a coordinar a domicilio.
  - **Medios de Pago:** Efectivo al retirar/recibir o transferencia bancaria (con datos de CBU/CVU dinámicos y copiado de alias en un clic).
  - **Notas del Cliente:** Campo opcional para aclaraciones sobre quién retira o referencias del domicilio.
- **Snapshot Inmutable de Pedidos:** Al confirmar la compra, se resguarda una copia fotográfica del nombre y precio unitario vigente de cada prenda, asegurando la consistencia histórica del pedido ante futuras modificaciones del catálogo.
- **Seguimiento e Historial:** Consulta del estado en vivo (`Confirmado`, `Listo para retirar`, `Entregado`, `Cancelado`) con posibilidad de cancelación y restitución automática de stock mientras el pago esté pendiente.

### 🛠️ Panel de Control y Administración (Admin)
- **Control de Inventario y Catálogo (ABM):** Gestión integral de categorías y productos con baja lógica (`Activo = false`), preservando la integridad referencial con los pedidos históricos.
- **Concurrencia Optimista con Tokens `RowVersion`:** Protección contra escrituras simultáneas y sobreescrituras accidentales mediante tokens `[Timestamp]` (`byte[]`) en productos y pedidos (HTTP 409 con aviso de recarga).
- **Procesamiento de Imágenes con SkiaSharp:**
  - Validación segura por magic bytes (JPEG, PNG, WebP) rechazando extensiones engañosas.
  - Validación de dimensiones y resolución máxima (hasta 40 MP y 8000 px de lado) antes de decodificar en memoria.
  - Corrección automática de orientación EXIF antes del guardado.
  - Redimensionado a 1200 px de ancho máximo y purga de metadatos privados/GPS.
  - Almacenamiento fuera de `wwwroot` con entrega estática protegida (`Cache-Control: public, max-age=31536000, immutable` y `X-Content-Type-Options: nosniff`).
- **Gestión Avanzada de Pedidos:** Listado paginado y filtrable, transiciones de estado de entrega, registro de cobro y filtro de fechas interpretado en la zona horaria local (`America/Argentina/Cordoba`).

---

## 🏛️ Arquitectura de Software

La solución sigue una **arquitectura en capas desacoplada**, con separación estricta de responsabilidades y flujo unidireccional de llamadas:

```
[ Capa de Presentación ]
Usaditos2026.Server (Host ASP.NET Core, Controladores REST, Páginas Blazor InteractiveServer)
      │
      ├──> Usaditos2026.Server.Client (Componentes Razor, Layouts y Vistas)
      │
[ Capa de Lógica de Negocio ]
      └──> Usaditos2026.Servicios (Reglas de stock, transacciones, autenticación, imágenes, pedidos)
            │
[ Capa de Acceso a Datos ]
            └──> Usaditos2026.Repositorio (Consultas LINQ, AsNoTracking, persistencia y filtros)
                  │
[ Capa de Base de Datos ]
                  └──> Usaditos2026.BD (AppDbContext, Entidades, Mapeos y Migraciones EF Core)
                        │
[ Capa Transversal ]
                        └──> Usaditos2026.Shared (DTOs, Opciones, Constantes, Enums, Excepciones)
```

> **Regla de negocio fundamental:** La interfaz de usuario (Blazor / Controllers) nunca accede directamente a la base de datos ni contiene lógica transaccional. Las validaciones de stock, snapshots de precios y transiciones de estado residen exclusivamente en la capa de `Servicios`.

### Proyectos de la Solución

| Proyecto | Tipo | Responsabilidad |
| :--- | :--- | :--- |
| **`Usaditos2026.Shared`** | Class Library | DTOs de request/response, opciones fuertemente tipadas (`TiendaOpciones`, `ImagenesOpciones`), enums, constantes de roles/estados y excepciones de dominio. |
| **`Usaditos2026.BD`** | Class Library | Entidades del dominio (`Producto`, `Categoria`, `Cliente`, `Carrito`, `Pedido`, etc.), configuración de Entity Framework Core y migraciones. |
| **`Usaditos2026.Repositorio`** | Class Library | Repositorios de datos para catálogo, clientes, carritos, pedidos y vistas administrativas. |
| **`Usaditos2026.Servicios`** | Class Library | Reglas de negocio, transacciones atómicas, hashing de contraseñas (`PasswordHasher`), procesamiento gráfico de imágenes (`SkiaSharp`) y servicio de tienda. |
| **`Usaditos2026.Server`** | Web App (.NET 10) | Aplicación anfitriona, inyección de dependencias, endpoints API REST con Swagger, middleware de seguridad, archivos estáticos y Blazor InteractiveServer. |
| **`Usaditos2026.Server.Client`** | Razor Class Library | Layouts (`MainLayout`, `AdminLayout`), componentes visuales, páginas y navegación. |
| **`Usaditos2026.Tests`** | xUnit Test Project | Suite de pruebas unitarias y de integración para reglas de negocio, stock, concurrencia y servicios. |

---

## 💻 Stack Tecnológico

- **Framework:** .NET 10 (C# 13)
- **Frontend / UI:** Blazor Web App (`InteractiveServer`), Razor Components y Bootstrap 5.3
- **Persistencia & ORM:** Entity Framework Core 10 con SQL Server
- **Procesamiento de Imágenes:** SkiaSharp 4.157.0 (Licencia MIT)
- **Autenticación:** Cookie Authentication nativa con contraseñas hasheadas mediante `PasswordHasher<Cliente>` (PBKDF2/SHA-256) y políticas de autorización por roles (`SoloAdmin`).
- **Documentación de API:** OpenAPI / Swagger UI
- **Testing:** xUnit, EF Core InMemory Provider y suite automatizada de pruebas de negocio.

---

## ⚙️ Configuración del Sistema

La configuración se administra a través de `appsettings.json` y puede sobreescribirse mediante variables de entorno del sistema operativo:

```json
{
  "ConnectionStrings": {
    "ConnSqlServer": "Server=(localdb)\\mssqllocaldb;Database=Usaditos2026;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Tienda": {
    "Direccion": "Av. San Martín 150, Hernando, Córdoba",
    "Horario": "Lunes a viernes 9:00-18:00 · Sábados 10:00-13:00",
    "Alias": "USADITOSHERNANDO",
    "Titular": "Usaditos Hernando",
    "WhatsApp": "+54 9 353 4123456",
    "Telefono": "+54 9 353 4123456",
    "Email": "contacto@usaditoshernando.com.ar",
    "ZonaHoraria": "America/Argentina/Cordoba"
  },
  "Imagenes": {
    "Ruta": "App_Data/uploads/productos",
    "TamanoMaximoMb": 8
  }
}
```

---

## 🏃 Cómo Ejecutarlo en Desarrollo Local

### Requisitos Previos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server LocalDB (incluido con Visual Studio) o una instancia local de SQL Server / SQL Express.

### 1. Clonar el repositorio
```bash
git clone https://github.com/santimartinez2/Usaditos2026.git
cd Usaditos2026
```

### 2. Ejecutar la aplicación
```bash
dotnet run --project Usaditos2026.Server/Usaditos2026.Server
```

Al iniciar en entorno **Development**, el sistema:
1. Crea la base de datos y aplica todas las migraciones pendientes automáticamente.
2. Carga datos de prueba de categorías y prendas con distintos talles, estados y niveles de stock.
3. Inicializa el usuario administrador y cliente de prueba.

### 3. Acceder en el navegador
- **Tienda Web:** [http://localhost:5250](http://localhost:5250) *(o `https://localhost:7200`)*
- **Documentación Swagger / API:** [http://localhost:5250/swagger](http://localhost:5250/swagger)

### Credenciales de Prueba (Entorno Development)

| Rol | Correo Electrónico | Contraseña | Acceso |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin@usaditos.com` | `Admin123!` | Catálogo, ABM, Pedidos y Swagger `/api/admin/*` |
| **Cliente** | `cliente@usaditos.com` | `Usaditos123!` | Catálogo, Carrito, Checkout y Mis Pedidos |

---

## 🧪 Ejecución de Pruebas Unitarias

La solución cuenta con pruebas automatizadas que validan reglas críticas de negocio (stock insuficiente, control de concurrencia, snapshots de precios, cancelaciones y opciones de tienda):

```bash
dotnet test
```

---

## 🚢 Despliegue en Producción

Para consultar los requisitos de variables de entorno para producción, la aplicación manual de scripts de migración SQL idempotentes y las rutinas de respaldo de base de datos e imágenes, consultá la guía detallada:

👉 **[Guía de Despliegue, Migraciones y Respaldos en Producción](docs/despliegue-produccion.md)**

---

## 📄 Licencia y Créditos

Desarrollado para **Usaditos Hernando**. Código disponible bajo fines de portfolio profesional y demostración de arquitectura en la plataforma .NET.
