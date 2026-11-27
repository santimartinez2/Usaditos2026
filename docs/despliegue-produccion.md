# Guía de Despliegue, Migraciones y Respaldos en Producción

Documentación técnica y operativa para el despliegue seguro, la ejecución de migraciones de base de datos y la estrategia de respaldos del e-commerce **Usaditos Hernando** (.NET 10 / Blazor Web App).

---

## 1. Consideraciones Iniciales de Arquitectura

El sistema está construido sobre una arquitectura limpia en capas (`Shared` → `BD` → `Repositorio` → `Servicios` → `Server`), con renderizado interactivo en servidor (`InteractiveServer`) y APIs REST documentadas para administración y catálogo.

En entornos de producción:
- **`Database.MigrateAsync()` no se ejecuta de forma automática al iniciar la aplicación.** Esto previene bloqueos de concurrencia en despliegues con múltiples réplicas o instancias, evitando fallos catastróficos si una migración pesada se ejecuta sin supervisión.
- **Los datos de prueba (categorías y catálogo de prendas de muestra) corren exclusivamente en `Development`.** En producción, la base de datos arranca limpia y las prendas se gestionan desde el panel de administración.
- **La inicialización del usuario Administrador es segura e idempotente:** requiere variables de entorno explícitas; si no se proveen, la aplicación continúa su arranque registrando una advertencia en los logs sin crear credenciales por defecto.

---

## 2. Variables de Entorno para Producción

En producción, la configuración se suministra mediante variables de entorno del sistema operativo, secretos del orquestador (Docker Swarm, Kubernetes) o del servicio de hosting (Azure App Service, AWS ECS, Linux Systemd, IIS).

Las variables utilizan la convención de doble guión bajo (`__`) para representar jerarquías en la configuración de .NET:

### 2.1. Entorno de Ejecución
| Variable | Valor | Descripción |
| :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Activa el manejo de errores seguro para usuarios finales, HSTS, y deshabilita Swagger y seeds de prueba. |

### 2.2. Base de Datos (SQL Server)
| Variable | Ejemplo de Valor | Descripción |
| :--- | :--- | :--- |
| `ConnectionStrings__DefaultConnection` *(o `ConnectionStrings__ConnSqlServer`)* | `Server=db.tudominio.com;Database=UsaditosProd;User Id=sa_usaditos;Password=TuClaveSegura!;TrustServerCertificate=False;Encrypt=True;` | Cadena de conexión a la instancia de SQL Server de producción. |

> **Nota:** La aplicación soporta indistintamente `ConnectionStrings__DefaultConnection` o `ConnectionStrings__ConnSqlServer`.

### 2.3. Semilla Segura del Usuario Administrador
| Variable | Ejemplo de Valor | Descripción |
| :--- | :--- | :--- |
| `AdminSeed__Email` | `admin@usaditoshernando.com.ar` | Correo electrónico del usuario administrador inicial. |
| `AdminSeed__Password` | `ClaveCompleja$2026!` | Contraseña inicial (se procesa inmediatamente con `PasswordHasher<Cliente>` con PBKDF2/SHA-256 y nunca se almacena en texto plano). |

> Si el usuario con dicho email ya existe en la base de datos, el sistema no duplica ni sobreescribe sus datos. Si estas variables no se configuran en producción, el sistema emite una advertencia en el log (`LogWarning`) y continúa funcionando normalmente.

### 2.4. Datos Institucionales y de Cobro de la Tienda (`Tienda`)
Permite personalizar la información de contacto y de transferencias sin modificar el código ni el archivo `appsettings.json`:

| Variable | Ejemplo de Valor | Descripción |
| :--- | :--- | :--- |
| `Tienda__Direccion` | `Av. San Martín 150, Hernando, Córdoba` | Dirección física visible para retiros en el local. |
| `Tienda__Horario` | `Lunes a viernes 9:00 a 18:00 · Sábados 10:00 a 13:00` | Horario de atención al público. |
| `Tienda__Alias` | `USADITOS.HERNANDO` | Alias de CBU/CVU bancario o billetera virtual para transferencias. |
| `Tienda__Titular` | `Carolina Usaditos` | Nombre del titular de la cuenta bancaria. |
| `Tienda__WhatsApp` | `+54 9 353 4123456` | Teléfono de WhatsApp para coordinación y envío de comprobantes. |
| `Tienda__Telefono` | `+54 9 353 4123456` | Teléfono fijo o de contacto alternativo. |
| `Tienda__Email` | `contacto@usaditoshernando.com.ar` | Correo de atención al cliente. |
| `Tienda__ZonaHoraria` | `America/Argentina/Cordoba` | Identificador de zona horaria IANA para el filtrado de pedidos y fechas locales. |

### 2.5. Almacenamiento de Imágenes (`Imagenes`)
| Variable | Ejemplo de Valor | Descripción |
| :--- | :--- | :--- |
| `Imagenes__Ruta` | `App_Data/uploads/productos` *(o ruta absoluta ej. `/var/data/usaditos/uploads`)* | Carpeta donde se almacenan las fotos redimensionadas de los productos. |
| `Imagenes__TamanoMaximoMb` | `8` | Límite máximo en MB permitido por foto al subirla. |

---

## 3. Ejemplo de Configuración en Producción

### Archivo `.env` (solo para orquestadores locales, NUNCA commitear a Git)
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=sql.usaditos.internal;Database=Usaditos2026Prod;User Id=usr_usaditos;Password=PasswordRobusto2026!;TrustServerCertificate=False;Encrypt=True;
AdminSeed__Email=carolina@usaditoshernando.com.ar
AdminSeed__Password=ClaveSeguraSuperAdmin2026!
Tienda__Direccion=Av. San Martín 150, Hernando, Córdoba
Tienda__Horario=Lunes a viernes 9:00 a 18:00 · Sábados 10:00 a 13:00
Tienda__Alias=USADITOS.HERNANDO
Tienda__Titular=Carolina Usaditos
Tienda__WhatsApp=+54 9 353 4123456
Tienda__Email=contacto@usaditoshernando.com.ar
Tienda__ZonaHoraria=America/Argentina/Cordoba
Imagenes__Ruta=App_Data/uploads/productos
Imagenes__TamanoMaximoMb=8
```

### Configuración en Linux (Systemd Service)
```ini
[Unit]
Description=Usaditos Hernando Web App
After=network.target

[Service]
WorkingDirectory=/var/www/usaditos
ExecStart=/usr/bin/dotnet /var/www/usaditos/Usaditos2026.Server.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=usaditos-app
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ConnectionStrings__DefaultConnection=Server=localhost;Database=Usaditos2026Prod;User Id=appuser;Password=TuClave!;TrustServerCertificate=True;
Environment=AdminSeed__Email=admin@usaditoshernando.com.ar
Environment=AdminSeed__Password=TuClaveAdmin2026!
Environment=Tienda__Alias=USADITOS.HERNANDO
Environment=Tienda__Titular=Carolina Usaditos
Environment=Tienda__Direccion=Av. San Martín 150, Hernando, Córdoba
Environment=Tienda__WhatsApp=+54 9 353 4123456

[Install]
WantedBy=multi-user.target
```

---

## 4. Aplicación de Migraciones de EF Core

Antes de iniciar la aplicación en producción por primera vez o al actualizar versiones que incluyan cambios de esquema, las migraciones deben aplicarse manualmente.

### Método A: Generación de Script SQL Idempotente (Recomendado)
Es la práctica recomendada para producción, ya que permite que un DBA o auditor revise las sentencias antes de aplicarlas:

```bash
# Desde la raíz de la solución
dotnet ef migrations script --idempotent \
  --project Usaditos2026.BD \
  --startup-project Usaditos2026.Server/Usaditos2026.Server \
  --output ./migraciones-produccion.sql
```

El archivo `migraciones-produccion.sql` resultante contiene bloques condicionales `IF NOT EXISTS` que aplican únicamente las migraciones faltantes sin dañar datos existentes. Luego se ejecuta contra la base de SQL Server usando `sqlcmd` o SQL Server Management Studio (SSMS):

```bash
sqlcmd -S sql.usaditos.internal -d Usaditos2026Prod -U sa_usaditos -P "TuClaveSegura!" -i ./migraciones-produccion.sql
```

### Método B: Actualización Directa vía CLI
Si el servidor de despliegue tiene acceso directo de red a la base de datos y cuenta con el SDK de .NET:

```bash
dotnet ef database update \
  --project Usaditos2026.BD \
  --startup-project Usaditos2026.Server/Usaditos2026.Server \
  --connection "Server=sql.usaditos.internal;Database=Usaditos2026Prod;User Id=sa_usaditos;Password=TuClaveSegura!;TrustServerCertificate=False;Encrypt=True;"
```

---

## 5. Estrategia y Rutina de Respaldos (Backups)

El sistema cuenta con dos activos críticos que deben respaldarse regularmente:
1. La base de datos relacional (catálogo, clientes, pedidos, historiales y snapshots).
2. El almacenamiento físico de imágenes de prendas (`App_Data/uploads`).

### 5.1. Respaldo de Base de Datos SQL Server

Se recomienda una política de respaldo combinada:
- **Respaldo Completo (Full):** Diario fuera del horario comercial (ej. 02:00 AM).
- **Respaldo Diferencial:** Cada 6 horas.
- **Respaldo de Logs de Transacciones:** Cada 30 minutos (si la base está en modelo de recuperación *Full*).

#### Script T-SQL para Backup Completo con Compresión:
```sql
DECLARE @Fecha NVARCHAR(30) = CONVERT(NVARCHAR(20), GETDATE(), 112) + '_' + REPLACE(CONVERT(NVARCHAR(8), GETDATE(), 108), ':', '');
DECLARE @RutaArchivo NVARCHAR(250) = N'/var/opt/mssql/backup/Usaditos2026Prod_Full_' + @Fecha + N'.bak';

BACKUP DATABASE [Usaditos2026Prod]
TO DISK = @RutaArchivo
WITH NOFORMAT, NOINIT,
NAME = N'Usaditos2026Prod-Full Backup',
SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10, CHECKSUM;
GO
```

#### Verificación de Integridad de la Base:
```sql
DBCC CHECKDB ([Usaditos2026Prod]) WITH NO_INFOMSGS, ALL_ERRORMSGS;
GO
```

### 5.2. Respaldo de Archivos de Imágenes (`App_Data/uploads`)

Las prendas subidas por los administradores se guardan en la ruta configurada en `Imagenes:Ruta` (por defecto `App_Data/uploads/productos/`).

#### Características de los archivos:
- Las imágenes son **inmutables**: una vez guardadas, su nombre es un GUID único generado por el servidor y nunca se modifican en el lugar (solo se reemplazan o borran lógicamente).
- Se sirven con encabezados `Cache-Control: public, max-age=31536000, immutable` y `X-Content-Type-Options: nosniff`.

#### Script de Respaldo Incremental en Linux (Rsync / Cron):
```bash
#!/bin/bash
# Sincronización diaria hacia almacenamiento externo o bucket montado
ORIGEN="/var/www/usaditos/App_Data/uploads/"
DESTINO="/mnt/backups_usaditos/uploads_$(date +\%Y\%m\%d)/"

mkdir -p "$DESTINO"
rsync -avz --delete "$ORIGEN" "$DESTINO"
```

#### Script de Respaldo en Windows (Robocopy / PowerShell):
```powershell
$Origen = "C:\inetpub\Usaditos2026\App_Data\uploads"
$Destino = "\\servidor-backup\usaditos\uploads"
robocopy $Origen $Destino /MIR /R:3 /W:5 /LOG:"C:\logs\backup_imagenes.log"
```

---

## 6. Procedimiento de Recuperación ante Desastres (Disaster Recovery)

En caso de contingencia mayor (pérdida de hardware o servidor comprometido):

1. **Restaurar el backup de base de datos más reciente:**
   ```sql
   RESTORE DATABASE [Usaditos2026Prod]
   FROM DISK = N'/var/opt/mssql/backup/Usaditos2026Prod_Full_ULTIMO.bak'
   WITH RECOVERY, REPLACE;
   GO
   ```
2. **Restaurar la carpeta `App_Data/uploads`:** Copiar los archivos respaldados a la ruta configurada en `Imagenes:Ruta` del nuevo host.
3. **Verificar permisos del proceso web:** Asegurar que el usuario que ejecuta la aplicación (`www-data`, `IIS_IUSRS`, etc.) tenga permisos de lectura y escritura en `App_Data/uploads`.
4. **Verificar variables de entorno:** Configurar las variables `ConnectionStrings__*`, `AdminSeed__*` y `Tienda__*`.
5. **Iniciar la aplicación y verificar:**
   - La aplicación arranca sin errores de conexión.
   - Las fotos de los productos se visualizan correctamente.
   - El catálogo responde búsquedas y filtros en `/api/productos/buscar`.
   - Se puede acceder al checkout y panel de administración.
