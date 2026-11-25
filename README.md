# Usaditos2026

## 🛍️ Estructura Full Stack con Blazor y C#

Guía y documentación del e-commerce desarrollado para Usaditos Hernando (cliente: Carolina) usando arquitectura en capas con Blazor, Entity Framework Core y SQL Server.

**Materia:** Modelado y Arquitectura de Software   
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
> *Las reglas de negocio (stock, entrega, pago y estados) viven siempre en la capa de servicios y no en la interfaz.*

---

## 🎯 Caso de Uso Implementado

El proyecto implementa de punta a punta los casos de uso **Agregar y eliminar producto del carrito** (Cliente), **Confirmar pedido (checkout) e historial de pedidos** (Cliente) y **Administrar catálogo y pedidos** (Admin). Los dos últimos son solo backend (API REST); no tienen pantallas Blazor.

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

### Confirmar pedido e historial

1. El cliente confirma el carrito activo desde `POST /api/pedidos`, enviando teléfono de contacto, tipo de entrega (`Retiro` o `Envio`), dirección (obligatoria solo si es envío), nota opcional y método de pago (`Efectivo`, `Transferencia` o `MercadoPago`).
2. El sistema revalida stock y producto activo de cada ítem, crea el pedido en estado `Confirmado` con `EstadoPago = Pendiente`, guarda el snapshot de nombre y precio, descuenta el stock y vacía el carrito, todo en una sola transacción.
3. El cliente consulta `GET /api/pedidos` (resumen, más nuevos primero) y `GET /api/pedidos/{id}` (detalle con ítems y datos de entrega/pago).
4. Si el pedido sigue `Confirmado` y no está pagado, puede cancelarlo con `POST /api/pedidos/{id}/cancelar`: vuelve a `Cancelado` y el stock se devuelve en la misma transacción.
5. Un pedido pagado, listo para retirar, entregado o cancelado no puede ser cancelado por el cliente (409). Un pedido de otro cliente da 403.

### Entrega, pago y administración de pedidos

- `GET /api/tienda` es público y devuelve dirección, horario y datos de transferencia configurados en la sección `Tienda` de `appsettings.json`.
- El administrador lista y filtra pedidos con `GET /api/admin/pedidos` (`estado`, `estadoPago`, `metodoPago`, `tipoEntrega`, `texto` para nombre/email del cliente, `fechaDesde`, `fechaHasta`, `page`, `pageSize`) y ve el detalle con `GET /api/admin/pedidos/{id}`.
- El detalle admin incluye `rowVersion` en base64. Cambiar de estado (`POST /{id}/estado`) y registrar pago (`POST /{id}/pago`) requieren ese valor; si está viejo, responde `409` con "El pedido fue modificado, recargá".
- Los estados son `Confirmado → ListoParaRetirar → Entregado`, con `Cancelado` como salida desde `Confirmado` o `ListoParaRetirar`. No se puede entregar sin `EstadoPago = Pagado`.
- `MarcarPago` permite `Pendiente`, `Pagado` y `Rechazado`; guarda `FechaPago` cuando es `Pagado` y la limpia en los otros casos. No se puede modificar el pago de un pedido `Entregado` o `Cancelado`.
- La cancelación del cliente y la del admin comparten la misma lógica de devolución de stock; el admin puede cancelar también un pedido `ListoParaRetirar`.

---

## 🧠 Decisiones de Diseño

- **Tope por producto:** No hay límite fijo de unidades. El único tope es `StockDisponible`: la cantidad total en el carrito (o la nueva cantidad al modificar) no puede superarlo. Si se supera: `StockInsuficienteException` → HTTP 409. `ItemCarritoDto` expone el stock para que el botón "+" se desactive al alcanzarlo, pero la validación ocurre siempre en el servidor.
- **Persistencia:** El carrito no se conserva al cerrar sesión: se vacía al cerrar y también al iniciar sesión.
- **Catálogo público:** `/catalogo` se puede ver sin sesión. Al intentar agregar un producto sin sesión se va a `/login?returnUrl=/catalogo`; el login valida el `returnUrl` recibido en el POST y solo acepta rutas locales (`RetornoSeguro`), para evitar open redirects. `/carrito`, el checkout y los endpoints `/api/carrito` y `/api/pedidos` siguen requiriendo sesión; `/api/productos` es público.
- **Gestión de Stock:** Agregar al carrito no descuenta stock: solo lo valida. El descuento ocurre al confirmar el pedido; cancelar un pedido confirmado lo devuelve.
- **Estados del pedido:** `Pendiente` se usa solo para el pago; los estados del pedido son `Confirmado`, `ListoParaRetirar`, `Entregado` y `Cancelado` en `PedidoConstantes`. El checkout crea pedidos directamente en `Confirmado` con pago `Pendiente`.
- **Entrega y pago:** `TipoEntrega` (`Retiro`/`Envio`), `MetodoPago` (`Efectivo`/`Transferencia`/`MercadoPago`) y `EstadoPago` (`Pendiente`/`Pagado`/`Rechazado`) son enums C# con conversión a string en EF (`nvarchar(20)`); la API acepta y devuelve solo strings (`JsonStringEnumConverter` sin números). La dirección es obligatoria en envío y se descarta en retiro; el teléfono se valida en servidor.
- **Snapshot del pedido:** `ItemPedido` guarda copia del nombre y del **precio vigente del producto al confirmar** (`IsRequired`, `nvarchar(200)`), no una referencia al precio del carrito. Así, cambios posteriores en el catálogo no alteran pedidos viejos. El precio del carrito sigue siendo el vigente al agregar.
- **Transacción:** Confirmar y cancelar usan una sola transacción sobre el `AppDbContext` scoped; si falla algo, se hace rollback (pedido, stock y carrito quedan como estaban).
- **Concurrencia:** `Producto` y `Pedido` tienen `RowVersion` (`[Timestamp]`) como token de concurrencia. Si dos ventas compiten por el último stock, la segunda obtiene `DbUpdateConcurrencyException`, rollback y un 409 con mensaje claro. Lo mismo al cancelar simultáneamente o al usar un `rowVersion` viejo desde el panel admin.
- **Cancelación:** El cliente solo cancela pedidos `Confirmado` con pago `Pendiente`. El admin puede cancelar también `ListoParaRetirar`. En ambos casos el stock se devuelve en la misma transacción y un estado inválido responde 409.
- **Integridad del historial:** La FK `ItemPedido → Producto` es `DeleteBehavior.Restrict` (`ON DELETE NO ACTION`), para no perder pedidos antiguos si se elimina un producto.
- **Seguridad:** El identificador del cliente sale siempre de la sesión autenticada (nunca de lo que envía el navegador), así un cliente no puede ver ni modificar el carrito o los pedidos de otro. El `Rol` tampoco viaja en el registro: siempre se crea como `Cliente`; el claim `ClaimTypes.Role` se arma al iniciar sesión y la política `SoloAdmin` protege `/api/admin/*`.
- **Admin inicial:** `SeedData.CrearAdminInicialAsync` es un paso separado e idempotente (no depende de que existan categorías). Solo crea el admin si no hay otro con rol `Admin` y si `AdminInicial:Email`/`AdminInicial:Password` están configurados. Si el email ya pertenece a un cliente, omite la creación y deja un `LogWarning`; si no hay configuración, no crea nada y no falla.
- **ABM de administrador:** Solo API REST (`/api/admin/categorias` y `/api/admin/productos`). La baja de productos es lógica (`Activo = false`), nunca física, porque los pedidos los referencian. `ProductoAdminDto` expone `RowVersion` como base64; editar, desactivar y activar envían ese valor en la request y el servicio lo copia a `OriginalValues["RowVersion"]`, por lo que un valor viejo produce 409 ("El producto fue modificado, recargá").
- **Administración de pedidos:** Solo API REST (`/api/admin/pedidos`). Listado paginado y filtrable, detalle con `rowVersion`, transiciones de estado y registro de pago. No se agregaron páginas Blazor.
- **Imágenes de producto:** Una imagen principal opcional por producto (`Producto.ImagenUrl`, `nvarchar(300)`). Solo API REST: `POST /api/admin/productos/{id}/imagen` (multipart/form-data) y `DELETE /api/admin/productos/{id}/imagen`. El nombre del archivo **siempre lo genera el servidor** (`GUID` + extensión detectada); el `fileName` del cliente nunca se usa. Se aceptan JPEG, PNG y WebP verificando los magic bytes, con `RequestSizeLimit` y validación real en el servicio. SkiaSharp **4.157.0** (MIT) valida dimensiones antes de decodificar (lado ≤ 8000 px, ≤ 40 MP), aplica `SKCodec.EncodedOrigin` antes de re-codificar, redimensiona a 1200 px y elimina metadatos/GPS. Los archivos se guardan en `App_Data/uploads/productos` (fuera de `wwwroot`, ignorado por Git) y se sirven desde `/uploads/productos` con caché inmutable y `nosniff`. Si falla la base después de guardar, se borra el archivo nuevo; si sale bien, se borra el anterior (un fallo de borrado solo deja un `LogWarning`). El `rowVersion` viejo responde `409`; formato/dimensiones/tamaño inválidos responden `400`. En Linux se requiere `SkiaSharp.NativeAssets.Linux.NoDependencies` **4.157.0**; se documenta pero no se agrega al proyecto porque el despliegue objetivo es Windows.
- **Filtro de fechas del admin:** `fechaDesde`/`fechaHasta` se interpretan en `Tienda:ZonaHoraria` (default `America/Argentina/Cordoba`, con fallback `Argentina Standard Time` y luego UTC). `FechaHelper` aplica `DateTime.SpecifyKind(..., DateTimeKind.Unspecified)` antes de convertir, para que un valor con `Z` u offset no corra el día. `fechaHasta` se convierte al inicio del día siguiente y se compara con `<` en UTC (no `.Date.AddDays(1)` en el repositorio).
- **Fuera de alcance:** Pasarela de pago real / cobro online, pantallas Blazor del panel de administración, filtros por categoría, registro con verificación de email y recuperación de contraseña. El sistema solo registra el estado del pago que confirma el admin.

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
| `SkiaSharp` 4.157.0 | Decodificar, aplicar orientación EXIF, redimensionar y re-codificar imágenes (licencia MIT) |

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

## 🖼️ Configuración de imágenes y zona horaria

En `Usaditos2026.Server/Usaditos2026.Server/appsettings.json`:

```json
{
  "Tienda": {
    "ZonaHoraria": "America/Argentina/Cordoba"
  },
  "Imagenes": {
    "Ruta": "App_Data/uploads/productos",
    "TamanoMaximoMb": 8
  }
}
```

- `Imagenes:Ruta` puede ser relativa a la carpeta del proyecto Server o absoluta; se crea al arrancar.
- `TamanoMaximoMb` se valida en el servicio; el límite del request es de 9 MB para dejar margen al multipart.
- El límite de request usa la constante `ImagenesConstantes.TamanoMaximoRequestBytes`. Si se cambia `TamanoMaximoMb` a un valor mayor que el default, hay que ajustar también esa constante.
- En Linux, instalar `SkiaSharp.NativeAssets.Linux.NoDependencies` versión `4.157.0` junto con SkiaSharp; no hace falta en Windows.

---

## 🔄 Migraciones

La aplicación las aplica sola al arrancar en *Development*. Para manejarlas a mano, desde la raíz del repo:

```bash
# Aplicar las migraciones a la base de datos
dotnet ef database update --project Usaditos2026.BD --startup-project Usaditos2026.Server/Usaditos2026.Server

# Empezar de cero con una base limpia
dotnet ef database drop --project Usaditos2026.BD --startup-project Usaditos2026.Server/Usaditos2026.Server --force

# Crear una migración nueva (ejemplo: AgregarPedidos)
dotnet ef migrations add AgregarPedidos --project Usaditos2026.BD --startup-project Usaditos2026.Server/Usaditos2026.Server

# Revisar el SQL que genera una migración
dotnet ef migrations script 20260616193125_InitialCreate --project Usaditos2026.BD --startup-project Usaditos2026.Server/Usaditos2026.Server
```

La migración `AgregarPedidos` crea `Pedidos` e `ItemsPedido`, agrega `RowVersion` a `Productos` y reduce `Productos.Nombre` a `nvarchar(200)` (no hay nombres tan largos en los datos de prueba). `AgregarRolCliente` agrega `Clientes.Rol` como `nvarchar(20)` no nulo con valor por defecto `Cliente`. `AgregarEntregaYPagoPedido` agrega los campos de entrega/pago en `Pedidos` (`nvarchar(20)` para los enums, con defaults `Retiro`, `Efectivo` y `Pendiente`), sin backfill manual. `AgregarImagenProducto` agrega `Productos.ImagenUrl` como `nvarchar(300)` nullable, sin cambios en otras tablas.

---

## 🧪 Datos de Prueba

**Cliente de prueba**

| Email | Contraseña |
| :--- | :--- |
| `cliente@usaditos.com` | `Usaditos123!` |

También se puede crear una cuenta nueva desde `/registro`.

**Admin de prueba (solo Development)**

| Email | Contraseña |
| :--- | :--- |
| `admin@usaditos.com` | `Admin123!` |

Se crea automáticamente desde `appsettings.Development.json` (`AdminInicial`). No existe en `appsettings.json` y la creación es idempotente: si el email ya es un cliente, se omite con un `LogWarning`.

**Catálogo de prueba:** 3 categorías (Remeras, Pantalones, Abrigos) y 9 productos con stock variado, pensados para probar cada escenario:

| Producto | Stock | Sirve para probar |
| :--- | :---: | :--- |
| Musculosa Rayas | 0 | Aviso de producto sin stock (`3a`) |
| Remera Estampada Dinosaurio | 1 | Stock insuficiente |
| Jean Clásico | 1 | Stock insuficiente |
| Varios productos | 5 o más | Llegar al tope por stock (sin límite fijo de unidades) |

### Pruebas manuales sugeridas

1. Entrar a `/catalogo` sin sesión: se ve el catálogo normalmente (sin redirección).
2. Sin sesión, tocar "Agregar al carrito": va a `/login?returnUrl=/catalogo`, muestra el aviso y, después de ingresar, vuelve al catálogo.
3. Iniciar sesión y agregar productos: el aviso muestra las unidades y el total del carrito.
4. Intentar agregar la **Musculosa Rayas**: aviso de producto sin stock.
5. Agregar un producto con stock 5 hasta llegar a 5 unidades: el botón "+" se deshabilita; el sexto intento desde la API responde 409 por stock (sin tope fijo, solo stock disponible).
6. En `/carrito`: modificar cantidades, eliminar un producto y verificar que el total se recalcula.
7. Vaciar el carrito y verificar el mensaje de carrito vacío.
8. Cerrar sesión, volver a ingresar y comprobar que el carrito está vacío.
9. Repetir en un ancho de pantalla de celular (~375 px).

---

## 🔌 API REST

Se puede probar desde Swagger (`/swagger`). `/api/productos` es público; los endpoints del carrito y de pedidos requieren haber iniciado sesión: primero ejecutar el login. Los endpoints `/api/admin/*` requieren además el rol `Admin`: con un `Cliente` responden `403` y sin sesión `401`.

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
| `GET` | `/api/tienda` | Datos públicos de dirección, horario y transferencia |
| `POST` | `/api/pedidos` | Confirma el carrito con datos de entrega/pago (201 con `PedidoDto`) |
| `GET` | `/api/pedidos` | Mis pedidos (resumen, más nuevos primero) |
| `GET` | `/api/pedidos/{id}` | Detalle de un pedido propio |
| `POST` | `/api/pedidos/{id}/cancelar` | Cancela un pedido confirmado y no pagado, y devuelve el stock |

**Gestión de pedidos (solo Admin)**

| Método | Ruta | Descripción |
| :---: | :--- | :--- |
| `GET` | `/api/admin/pedidos` | Lista pedidos con filtros y paginación |
| `GET` | `/api/admin/pedidos/{id}` | Detalle de un pedido (incluye `rowVersion`) |
| `POST` | `/api/admin/pedidos/{id}/estado` | Pasa a `ListoParaRetirar`, `Entregado` o `Cancelado` (requiere `rowVersion`) |
| `POST` | `/api/admin/pedidos/{id}/pago` | Registra `Pendiente`, `Pagado` o `Rechazado` (requiere `rowVersion`) |

**ABM de catálogo (solo Admin)**

| Método | Ruta | Descripción |
| :---: | :--- | :--- |
| `GET` | `/api/admin/categorias` | Lista todas las categorías |
| `POST` | `/api/admin/categorias` | Crea una categoría |
| `PUT` | `/api/admin/categorias/{id}` | Modifica una categoría |
| `DELETE` | `/api/admin/categorias/{id}` | Elimina una categoría (409 si tiene productos) |
| `GET` | `/api/admin/productos` | Lista todos los productos, activos o no |
| `GET` | `/api/admin/productos/{id}` | Detalle de un producto (incluye `rowVersion`) |
| `POST` | `/api/admin/productos` | Crea un producto |
| `PUT` | `/api/admin/productos/{id}` | Modifica un producto (requiere `rowVersion`) |
| `DELETE` | `/api/admin/productos/{id}` | Desactiva un producto (baja lógica, requiere `rowVersion` en el body) |
| `POST` | `/api/admin/productos/{id}/activar` | Reactiva un producto (requiere `rowVersion` en el body) |
| `POST` | `/api/admin/productos/{id}/imagen` | Sube/reemplaza la imagen principal (multipart: `archivo` y `rowVersion` en base64) |
| `DELETE` | `/api/admin/productos/{id}/imagen` | Quita la imagen principal (requiere `rowVersion` en el body) |

**Códigos de error**

| Código | Significado |
| :---: | :--- |
| `400` | Datos inválidos (cantidad menor a uno, enum inválido, teléfono/dirección faltante, `rowVersion` vacío o imagen con formato/tamaño/dimensiones inválidos) |
| `401` | No hay sesión iniciada |
| `403` | El ítem o el pedido no pertenece al cliente; o el usuario no es Admin |
| `404` | El producto, el ítem, el pedido o la categoría no existe |
| `409` | Sin stock, carrito vacío, estado de pedido/pago inválido, nombre de categoría duplicado, categoría con productos, producto o pedido modificado por otro usuario (también al subir/quitar imagen con un `rowVersion` viejo), o conflicto de concurrencia del checkout |

### Flujo completo en Swagger

1. `POST /api/auth/login` con `{"email":"cliente@usaditos.com","password":"Usaditos123!"}`. La cookie queda guardada en Swagger UI.
2. `GET /api/productos` para elegir un `id` con stock (por ejemplo, `Remera Básica Blanca`).
3. `POST /api/carrito/items` con `{"productoId":1,"cantidad":2}` (se puede repetir con otro producto).
4. `GET /api/carrito` para ver el detalle y el total.
5. `GET /api/tienda` para ver los datos configurados (público).
6. `POST /api/pedidos` con `{"telefonoContacto":"3511234567","tipoEntrega":"Retiro","metodoPago":"Efectivo","notaCliente":"Dejar en mostrador"}`: responde `201` con el pedido confirmado, `estadoPago: "Pendiente"` y `fechaPago: null`; el carrito vuelve a quedar vacío y el stock se descuenta.
7. `GET /api/pedidos` y `GET /api/pedidos/{id}` para ver el resumen y el detalle con nombres/precios snapshot y datos de entrega/pago.
8. `POST /api/pedidos/{id}/cancelar`: el pedido pasa a `Cancelado` y el stock se devuelve. Repetirlo da `409` porque el pago sigue pendiente pero el estado ya no es `Confirmado`.
9. `POST /api/pedidos` con el carrito vacío o con `tipoEntrega: "Envio"` sin dirección da `409`/`400` respectivamente; `GET /api/pedidos/{id}` de otro cliente da `403`; sin sesión da `401`.

### Flujo admin en Swagger

1. Hacer logout (`POST /api/auth/logout`) si había un cliente logueado y ejecutar `POST /api/auth/login` con `{"email":"admin@usaditos.com","password":"Admin123!"}`.
2. `GET /api/admin/categorias` → `POST /api/admin/categorias` con `{"nombre":"Accesorios","descripcion":"Gorros y bufandas"}`. Repetir el mismo nombre da `409`.
3. `POST /api/admin/productos` con `{"nombre":"Gorro de lana","descripcion":"Invierno","precio":12.50,"stockDisponible":8,"categoriaId":4}` (usar un `id` de categoría existente).
4. `GET /api/admin/productos` lista también los inactivos; `GET /api/productos` (público) no muestra los desactivados.
5. `GET /api/admin/productos/{id}` devuelve `rowVersion` en base64. Para modificar, enviar `PUT /api/admin/productos/{id}` con todos los campos y ese `rowVersion`, por ejemplo `{"nombre":"Gorro de lana","descripcion":"Invierno","precio":14.00,"stockDisponible":8,"categoriaId":4,"rowVersion":"AAAAAAAACJ8="}` (copiar el valor real de la respuesta).
6. Concurrencia: guardar el `rowVersion` original, hacer un `PUT` con precio `14.00` y después otro `PUT` con el `rowVersion` guardado y precio `16.00` → el segundo responde `409` con "El producto fue modificado, recargá".
7. `DELETE /api/admin/productos/{id}` con body `{"rowVersion":"<base64 actual>"}` → `204`. El producto desaparece de `/api/productos` pero sigue en `/api/admin/productos` con `activo: false`.
8. `POST /api/admin/productos/{id}/activar` con el `rowVersion` devuelto por el listado admin → vuelve a aparecer en el catálogo público.
9. `DELETE /api/admin/categorias/{id}` de una categoría con productos da `409`; `DELETE` de una categoría vacía da `204`.
10. Hacer logout y loguearse como `cliente@usaditos.com`: cualquier `/api/admin/*` responde `403`. Sin sesión responde `401`.

### Flujo de imágenes en Swagger (Admin)

1. Loguearse como `admin@usaditos.com` y hacer `GET /api/admin/productos/{id}` para copiar el `rowVersion` actual.
2. En `POST /api/admin/productos/{id}/imagen`, completar `rowVersion` con ese base64, elegir un archivo `.jpg`, `.png` o `.webp` en `archivo` y ejecutar. La respuesta devuelve `imagenUrl` con la forma `/uploads/productos/<guid>.<ext>`.
3. Abrir esa URL en el navegador: sirve la imagen redimensionada, sin metadatos EXIF/GPS y con `Cache-Control` inmutable.
4. Repetir el `POST` con el `rowVersion` viejo: responde `409`. Subir un `.txt`, un `.svg` o un archivo mayor a 8 MB responde `400`.
5. `DELETE /api/admin/productos/{id}/imagen` con `{"rowVersion":"<base64 actual>"}` deja `imagenUrl` en `null` y borra el archivo. Repetir con el valor viejo da `409`.
6. El mismo producto se ve en `GET /api/productos` y `GET /api/admin/productos` con el `imagenUrl` correspondiente. Las imágenes quedan fuera de Git en `App_Data/uploads/productos`.

### Flujo de pedidos en Swagger (Admin)

1. Loguearse como `admin@usaditos.com` y crear un pedido con el cliente (o usar uno existente de `GET /api/admin/pedidos`).
2. `GET /api/admin/pedidos` con `?estado=Confirmado&page=1&pageSize=10`; probar también `texto=cliente@usaditos.com`, `metodoPago=Efectivo` y `fechaHasta` (incluye el día completo).
3. `GET /api/admin/pedidos/{id}` y copiar el `rowVersion` devuelto.
4. `POST /api/admin/pedidos/{id}/pago` con `{"estadoPago":"Pagado","rowVersion":"<base64 actual>"}`; responde el pedido con `fechaPago`.
5. Repetir el `POST /api/admin/pedidos/{id}/pago` con el `rowVersion` viejo: responde `409` con "El pedido fue modificado, recargá".
6. Con el `rowVersion` actual, `POST /api/admin/pedidos/{id}/estado` con `{"nuevoEstado":"ListoParaRetirar","rowVersion":"<base64 actual>"}` y después `{"nuevoEstado":"Entregado","rowVersion":"<base64 actual>"}`.
7. Probar entregar un pedido impago da `409`; marcar pago de uno `Entregado` o `Cancelado` también da `409`.
8. `POST /api/admin/pedidos/{id}/estado` con `{"nuevoEstado":"Cancelado","rowVersion":"<base64 actual>"}` devuelve el stock. Un cliente con pedido pagado ya no puede cancelarlo por `/api/pedidos/{id}/cancelar`.

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
- [x] Catálogo público con precio y stock (visible sin sesión)
- [x] Visitante que intenta agregar al carrito vuelve al catálogo después de login
- [x] Datos de prueba cargados automáticamente
- [x] Agregar producto al carrito verificando stock en el servidor
- [x] Aviso de producto sin stock (`3a`)
- [x] Sin límite fijo: el único tope es el stock disponible
- [x] Aviso al agregar con las unidades y el total actualizados del carrito
- [x] Pantalla del carrito con el detalle de productos y el total
- [x] Eliminar producto y recalcular el total
- [x] Modificar cantidad desde el carrito (`5a`)
- [x] Mensaje de carrito vacío con acceso al catálogo (`6a`)
- [x] Vaciar carrito (`7a`)
- [x] El carrito se vacía al cerrar e iniciar sesión
- [x] El cliente solo puede acceder a su propio carrito
- [x] Diseño responsive para celulares
- [x] Confirmar pedido en una transacción con revalidación de stock y producto activo
- [x] Snapshot de nombre y precio en `ItemPedido` (no se ve afectado por cambios del catálogo)
- [x] Descuento de stock al confirmar y carrito vuelto a vacío
- [x] Historial de pedidos (resumen y detalle) con control de propiedad
- [x] Cancelación de pedidos confirmados con devolución de stock en transacción
- [x] Concurrencia optimista con `RowVersion` en `Producto` y `Pedido` (409)
- [x] Roles `Cliente`/`Admin` con política `SoloAdmin` (401 sin sesión, 403 para clientes)
- [x] Admin inicial idempotente desde `appsettings.Development.json` con aviso si el email ya es cliente
- [x] Migración `AgregarRolCliente` con valor por defecto `Cliente`
- [x] ABM de categorías por API (alta, modificación y baja con validación de productos asociados)
- [x] ABM de productos por API (alta, modificación, baja lógica y reactivación)
- [x] Concurrencia optimista en el ABM mediante `rowVersion` enviado por el cliente (409)
- [x] Checkout con teléfono, tipo de entrega, dirección condicional, nota y método de pago
- [x] Enums de entrega/pago como strings en EF y en la API (sin aceptar números)
- [x] `GET /api/tienda` público con dirección, horario y transferencia desde configuración
- [x] Listado admin de pedidos paginado, filtrable y con detalle
- [x] Transiciones admin `ListoParaRetirar`/`Entregado` con validación de pago
- [x] Registro admin de pago con `rowVersion` y 409 por valor viejo
- [x] Cancelación compartida cliente/admin con devolución de stock en transacción
- [x] Migración `AgregarEntregaYPagoPedido` con defaults y longitudes revisadas
- [x] Imagen principal por producto con `POST/DELETE /api/admin/productos/{id}/imagen` (solo API REST, sin páginas Blazor)
- [x] SkiaSharp 4.157.0 fijado: magic bytes, límite de dimensiones, orientación EXIF antes de re-codificar, resize a 1200 px y sin metadatos/GPS
- [x] Archivos fuera de `wwwroot`, servidos por `/uploads/productos` con caché inmutable y `nosniff`, ignorados por Git
- [x] Concurrencia de imagen mediante `rowVersion` (409 con valor viejo) y borrado del archivo viejo recién después de guardar en la base
- [x] Filtro de fechas del admin interpretado en `Tienda:ZonaHoraria` con `SpecifyKind(..., Unspecified)` y comparación UTC exclusiva
- [ ] Contador de unidades del menú actualizado en vivo (hoy se actualiza al navegar entre páginas)
- [ ] Pantallas Blazor para confirmar pedido y ver el historial (fuera de alcance: solo backend)
