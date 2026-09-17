# SubastaYa

Trabajo Práctico - Proyecto de Software

SubastaYa es una plataforma web de subastas que permite publicar productos, consultar subastas, realizar pujas en tiempo real, administrar una billetera virtual y procesar automáticamente el cierre y la liquidación de las subastas.

## Tecnologías utilizadas

### Backend
- C#
- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- SQL Server
- Swagger / OpenAPI
- SignalR

### Frontend
- HTML5
- CSS3
- JavaScript Vanilla

### Base de datos
- SQL Server
- Entity Framework Core Code First
- Migraciones de Entity Framework Core

### Herramientas
- Visual Studio
- Git
- GitHub
- PowerShell

## Estructura del proyecto

```text
SubastaYa/
├── SubastaYa.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/
│   ├── Helpers/
│   ├── Hubs/
│   ├── Middleware/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Workers/
│   ├── Migrations/
│   ├── tests/
│   │   └── stress-test.ps1
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   ├── index.html
│   │   ├── login.html
│   │   ├── publish.html
│   │   ├── auction.html
│   │   ├── activities.html
│   │   ├── register.html
│   │   └── profile.html
│   ├── appsettings.json
│   ├── Program.cs
│   └── SubastaYa.Api.csproj
└── README.md
```

## Requisitos previos

Para ejecutar el proyecto es necesario tener instalado:

- .NET SDK 10
- SQL Server o SQL Server LocalDB
- PowerShell
- Git

## Clonar el repositorio

```powershell
git clone URL_DEL_REPOSITORIO
cd SubastaYa
```

## Configuración de la base de datos

La aplicación utiliza SQL Server y Entity Framework Core.

La cadena de conexión se encuentra configurada en:

```text
SubastaYa.Api/appsettings.json
```

La base de datos utilizada por el proyecto es:

```text
SubastaYaDb
```

## Crear la base de datos

El proyecto utiliza Entity Framework Core Code First y migraciones.

Desde la raíz del proyecto ejecutar:

```powershell
dotnet ef database update --project .\SubastaYa.Api
```

Este comando crea la base de datos y aplica todas las migraciones existentes.

Si `dotnet ef` no está instalado:

```powershell
dotnet tool install --global dotnet-ef
```

Luego volver a ejecutar:

```powershell
dotnet ef database update --project .\SubastaYa.Api
```

## Datos iniciales

Cuando la base de datos se encuentra vacía, el sistema ejecuta automáticamente el inicializador de datos.

Se crean usuarios, billeteras, categorías, subastas, pujas, movimientos y registros de auditoría necesarios para probar las funcionalidades principales.

### Usuarios de prueba

| Email               | Contraseña  |         Uso          |
|---------------------|-------------|----------------------|
| vendedor@test.com   |     123     | Vendedor             |
| comprador1@test.com |     123     | Comprador            |
| comprador2@test.com |     123     | Comprador            |
| sinfondos@test.com  |     123     | Comprador sin fondos |

## Categorías

El sistema incluye las siguientes categorías:

- Tecnología
- Coleccionables
- Indumentaria
- Vehículos
- Hogar y Muebles
- Electrodomésticos
- Herramientas
- Deportes y Fitness
- Mascotas
- Juegos y Juguetes
- Belleza y Cuidado Personal

Las categorías son cargadas automáticamente por el sistema al inicializar la base de datos.

## Ejecución del proyecto

Desde la raíz del proyecto:

```powershell
dotnet build
```

Si la compilación finaliza correctamente:

```powershell
dotnet run --project .\SubastaYa.Api
```

La aplicación quedará disponible mediante HTTPS en:

```text
https://localhost:7090
```

También puede encontrarse disponible mediante HTTP según la configuración de ejecución:

```text
http://localhost:5146
```

## Swagger

La documentación interactiva de la API se encuentra disponible en:

```text
https://localhost:7090/swagger
```

Swagger permite consultar los endpoints disponibles y realizar pruebas directamente sobre la API.

## Funcionalidades

### Catálogo de subastas

El catálogo permite consultar las subastas disponibles y aplicar distintos filtros.

Se pueden filtrar las subastas por:

- Estado.
- Categoría.
- Precio mínimo.
- Precio máximo.

También se pueden ordenar según:

- Tiempo restante.
- Mayor puja.

Cada publicación muestra información relevante como:

- Imagen.
- Título.
- Categoría.
- Precio o puja actual.
- Cantidad de pujas.
- Estado.
- Tiempo restante.

### Estados de las subastas

Las subastas pueden encontrarse en los siguientes estados:

```text
PROGRAMADA
ACTIVA
FINALIZADA
DESIERTA
```

**PROGRAMADA:** la subasta todavía no comenzó.

**ACTIVA:** la subasta se encuentra disponible para recibir pujas.

**FINALIZADA:** la subasta terminó y tuvo al menos una puja válida.

**DESIERTA:** la subasta terminó sin recibir ninguna puja.

### Publicación de subastas

Los usuarios pueden publicar nuevas subastas indicando:

- Título.
- Descripción.
- Imagen.
- Categoría.
- Precio base.
- Incremento mínimo.
- Fecha y hora de inicio.
- Fecha y hora de finalización.

Antes de crear la subasta se validan los datos ingresados.

Las fechas se manejan utilizando UTC en el backend.

### Sistema de pujas

Los compradores pueden realizar pujas sobre subastas activas.

Antes de registrar una puja se validan:

- Existencia de la subasta.
- Estado de la subasta.
- Fecha y hora actual.
- Que el usuario no sea el vendedor.
- Incremento mínimo requerido.
- Existencia de la billetera.
- Saldo disponible suficiente.

El sistema utiliza transacciones para mantener la consistencia entre las pujas y los saldos de las billeteras.

### Billetera

Cada usuario posee una billetera virtual.

La billetera mantiene:

```text
Saldo total
Saldo retenido
Saldo disponible
```

El saldo disponible representa el dinero que puede utilizarse para nuevas operaciones.

Cuando un usuario realiza una puja válida, el monto correspondiente queda retenido.

Cuando una puja deja de ser la ganadora, el monto retenido correspondiente se libera.

### Movimientos de dinero

Las operaciones de dinero se registran mediante un Ledger de transacciones.

Entre los movimientos registrados se encuentran:

- Depósitos.
- Retenciones.
- Liberaciones.
- Pagos.
- Cobros.

Las operaciones también quedan registradas mediante el sistema de auditoría.

### Anti-sniping

El sistema implementa una protección contra el cierre de una subasta mientras se están realizando pujas.

Si se registra una puja válida durante los últimos 60 segundos antes de la finalización:

```text
La subasta se extiende automáticamente 2 minutos.
```

La extensión se registra en la auditoría.

La respuesta de la API informa si la extensión fue aplicada mediante:

```text
antiSnipingApplied
```

### Actualización en tiempo real

La sala de subastas utiliza SignalR para actualizar información en tiempo real.

Las actualizaciones permiten mostrar:

- Nuevas pujas.
- Cambios en la puja máxima.
- Cambios en el tiempo restante.
- Extensiones por anti-sniping.
- Cambios de estado de la subasta.

### Cierre automático de subastas

El sistema posee Background Workers encargados de procesar automáticamente las subastas.

Cuando una subasta llega a su fecha de finalización y tiene un ganador, el sistema:

1. Marca la subasta como `FINALIZADA`.
2. Identifica la puja ganadora.
3. Utiliza el saldo retenido del comprador.
4. Acredita el importe correspondiente al vendedor.
5. Registra los movimientos de dinero.
6. Crea la operación de venta.
7. Registra el evento en la auditoría.

Si no existen pujas, la subasta pasa a:

```text
DESIERTA
```

### Concurrencia optimista

Las subastas utilizan un campo de versión para implementar control de concurrencia optimista.

El campo:

```text
Version
```

permite detectar cuando dos operaciones intentan modificar simultáneamente la misma subasta.

Cuando dos pujas concurrentes intentan modificar la misma versión:

```text
Una operación -> 201 Created
Otra operación -> 409 Conflict
```

El conflicto es controlado mediante `DbUpdateConcurrencyException`.

La API devuelve:

```text
HTTP 409 Conflict
```

junto con un mensaje indicando que la subasta fue modificada por otra puja.

### Auditoría

Las operaciones importantes del sistema generan registros de auditoría.

Entre ellas:

- Creación de subastas.
- Registro de pujas.
- Pujas rechazadas.
- Extensiones anti-sniping.
- Cambios de estado.
- Cierre de subastas.
- Operaciones sobre billeteras.
- Operaciones relacionadas con pagos y cobros.

### Actividades del usuario

La aplicación dispone de una sección de actividades donde cada usuario puede consultar únicamente sus propias operaciones.

Se pueden consultar:

- **Mis pujas:** pujas realizadas por el usuario.
- **Mis publicaciones:** subastas publicadas por el usuario.
- **Movimientos:** movimientos realizados sobre su billetera.

## API versionada

Los endpoints de la aplicación utilizan la versión:

```text
/api/v1/
```

Las respuestas HTTP incluyen el header:

```text
X-Api-version: 1.0
```

## Principales endpoints

### Subastas

```text
GET  /api/v1/auctions
GET  /api/v1/auctions/{id}
POST /api/v1/auctions
```

### Pujas

```text
POST /api/v1/auctions/{auctionId}/bids
```

### Categorías

```text
GET /api/v1/categories
```

### Billetera

```text
GET  /api/v1/wallets/{userId}
POST /api/v1/wallets/{userId}/transactions
GET  /api/v1/wallets/{userId}/transactions
```

### Actividades

```text
GET /api/v1/users/{userId}/bids
GET /api/v1/users/{userId}/auctions
```

## Códigos HTTP

La API utiliza códigos HTTP de acuerdo con el resultado de las operaciones:

```text
200 OK
201 Created
400 Bad Request
404 Not Found
409 Conflict
500 Internal Server Error
```

Los conflictos de concurrencia utilizan:

```text
409 Conflict
```

Los mensajes de error mostrados por el sistema utilizan el formato:

```text
[CODE-ERROR] - mensaje del error
```

## Stress Test

El proyecto incluye una prueba de concurrencia mediante PowerShell.

Archivo:

```text
SubastaYa.Api/tests/stress-test.ps1
```

El script envía dos pujas idénticas de forma concurrente contra la misma subasta.

### Ejecución

Con la API ejecutándose:

```powershell
powershell -ExecutionPolicy Bypass -File .\SubastaYa.Api\tests\stress-test.ps1 `
    -AuctionId 1 `
    -BuyerId 3 `
    -Amount 50000 `
    -BaseUrl "https://localhost:7090"
```

### Resultado esperado

El resultado correcto es:

```text
201
409
```

Una petición debe crear correctamente la puja:

```text
201 Created
```

Mientras que la otra debe ser rechazada por concurrencia:

```text
409 Conflict
```

El script valida automáticamente que los códigos obtenidos sean exactamente:

```text
201,409
```

Si el resultado es diferente, el script informa el error.

### Prueba de concurrencia realizada

El stress test fue ejecutado sobre la subasta 1 utilizando:

```text
AuctionId: 1
BuyerId: 3
Amount: 50000
```

El resultado obtenido fue:

```text
StatusCode: 201
StatusCode: 409
```

Por lo tanto, la prueba de concurrencia optimista se encuentra funcionando correctamente.

## Frontend

El frontend se encuentra dentro de:

```text
SubastaYa.Api/wwwroot/
```

Las vistas principales incluyen:

- Catálogo.
- Login.
- Registro.
- Detalle de subasta.
- Publicación de subasta.
- Billetera.
- Actividades.

El frontend utiliza:

- HTML.
- CSS.
- JavaScript Vanilla.

No se utilizan frameworks frontend externos.

## Flujo básico de uso

### 1. Iniciar sesión

Utilizar uno de los usuarios de prueba.

Ejemplo:

```text
Email: comprador2@test.com
Contraseña: 123
```

### 2. Consultar el catálogo

Desde el catálogo se pueden visualizar las subastas y aplicar filtros.

### 3. Ingresar a una subasta

Desde el catálogo se puede acceder al detalle de una subasta activa.

### 4. Realizar una puja

Ingresar un monto que cumpla con el incremento mínimo y que pueda ser cubierto por el saldo disponible.

### 5. Consultar la billetera

La billetera muestra el saldo total, retenido y disponible.

### 6. Consultar actividades

La sección de actividades permite consultar las pujas, publicaciones y movimientos correspondientes al usuario actualmente autenticado.

## Compilación

Para verificar que el proyecto compile correctamente:

```powershell
dotnet build
```

El proyecto debe finalizar la compilación sin errores.

## Ejecución rápida

Una vez configurado SQL Server y aplicadas las migraciones:

```powershell
dotnet ef database update --project .\SubastaYa.Api
```

Luego:

```powershell
dotnet build
```

Y finalmente:

```powershell
dotnet run --project .\SubastaYa.Api
```

Abrir en el navegador:

```text
https://localhost:7090
```

Swagger:

```text
https://localhost:7090/swagger
```

## Reiniciar la base de datos

En caso de necesitar una instalación limpia, se puede eliminar la base de datos `SubastaYaDb` y volver a ejecutar las migraciones:

```powershell
dotnet ef database drop --project .\SubastaYa.Api --force
```

Luego:

```powershell
dotnet ef database update --project .\SubastaYa.Api
```

Al iniciar nuevamente la aplicación, el inicializador cargará los datos de prueba.

## Notas

- Las fechas se almacenan y procesan en UTC.
- Los montos monetarios utilizan `decimal`.
- Las operaciones de puja y retención de saldo se realizan de manera transaccional.
- La concurrencia se controla mediante Optimistic Locking.
- SignalR se utiliza para actualizaciones en tiempo real.
- Los Background Workers procesan activaciones y cierres automáticos.
- Las operaciones importantes quedan registradas en auditoría.
- El catálogo obtiene las categorías desde la API.

## Autores

Trabajo realizado para la materia:

**Proyecto de Software**

Proyecto:

**SubastaYa**

Por: Lucas Muñoz y Martin Piaggi