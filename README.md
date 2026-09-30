# ProductCatalog API

API REST para la gestión de un catálogo de productos y su stock, desarrollada en .NET 10.

**URL pública:** https://fmproductcatalogapi.onrender.com/swagger

## Tecnologías
- .NET 10 / ASP.NET Core
- Entity Framework Core 10 + PostgreSQL 18
- FluentValidation
- OpenAPI nativo + Swagger UI
- Docker / Docker Compose
- xUnit

## Arquitectura

Clean Architecture en cuatro capas:

```
src/
├── ProductCatalog.Domain          → Entidades e invariantes de negocio
├── ProductCatalog.Application     → Casos de uso, DTOs, validaciones, puertos
├── ProductCatalog.Infrastructure  → EF Core, PostgreSQL, repositorios
└── ProductCatalog.Api             → Controllers, manejo de errores, Swagger
tests/
└── ProductCatalog.UnitTests       → Pruebas unitarias del dominio
```

Las dependencias apuntan hacia el dominio: `Api → Application → Domain` e `Infrastructure → Application`. El dominio no depende de ningún framework.

## Ejecución local

### Opción 1: Docker (recomendada)
Requisitos: Docker Desktop. Los puertos 8080 y 5432 deben estar libres.

```bash
git clone https://github.com/jhony2070298/FMProductCatalog
cd FMProductCatalog
docker compose up -d --build
```

Una vez iniciados los contenedores, abrir en el navegador: http://localhost:8080/swagger

Las migraciones se aplican automáticamente al iniciar la API.

Para detener y eliminar los contenedores:
```bash
docker compose down -v
```

### Opción 2: .NET SDK + PostgreSQL en Docker
Requisitos: .NET 10 SDK y Docker Desktop. El puerto 5432 debe estar libre.

```bash
git clone https://github.com/jhony2070298/FMProductCatalog
cd FMProductCatalog
docker compose up -d postgres
dotnet run --project src/ProductCatalog.Api --launch-profile http
```

Una vez la consola muestre `Now listening on: http://localhost:5059`, abrir en el navegador: http://localhost:5059/swagger

Las migraciones se aplican automáticamente al iniciar la API.

Para detener: `Ctrl + C` en la consola de la API y luego:
```bash
docker compose down -v
```

## Pruebas
Requisitos: .NET 10 SDK.

```bash
dotnet test
```

Pruebas unitarias del dominio: invariantes del producto (nombre, precio, stock inicial) y reglas de ajuste de stock (no negativo, cantidad cero, overflow).

## Despliegue (Render)
1. Crear una base PostgreSQL en Render.
2. Crear un Web Service desde el repositorio con runtime Docker, en la misma región de la base. Se usa un plan pago para evitar la suspensión por inactividad.
3. Configurar las variables de entorno:
   - `ConnectionStrings__Default`: cadena de conexión en formato Npgsql usando el host interno.
   - `PORT`: `8080`
4. Configurar el Health Check Path: `/health`

Las migraciones se aplican automáticamente en cada despliegue al iniciar la API.

## Endpoints
| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| GET | /api/products?page=1&pageSize=20 | Listado paginado | 200, 400 |
| GET | /api/products/{id} | Consultar por id | 200, 404 |
| POST | /api/products | Crear producto | 201, 400 |
| PUT | /api/products/{id} | Actualizar nombre, descripción y precio | 200, 400, 404, 409 |
| PATCH | /api/products/{id}/stock | Sumar o restar stock (`quantity` con signo) | 200, 400, 404, 409, 422 |
| DELETE | /api/products/{id} | Eliminar | 204, 404, 409 |
| GET | /health | Estado de la API y la base de datos | 200, 503 |

### Ejemplo: ajuste de stock
```http
PATCH /api/products/{id}/stock
Content-Type: application/json

{ "quantity": -3 }
```

Una cantidad positiva suma y una negativa resta. Si el stock resultante fuera negativo, la API responde 422 (respuesta resumida):

```json
{
  "title": "Stock insuficiente",
  "status": 422,
  "detail": "Stock insuficiente para el producto ... Stock actual: 2, ajuste solicitado: -3.",
  "currentStock": 2,
  "requestedAdjustment": -3
}
```

## Decisiones técnicas

### Dominio
- **Modelo de dominio rico.** La entidad encapsula sus datos y reglas. La modificación del stock se separa de los demás atributos del producto: solo cambia a través del endpoint de stock, y el PUT no puede sobrescribirlo ni saltarse la regla de concurrencia.
- **Guid v7 como identificador.** Son ordenados por tiempo, así que no fragmentan el índice de la clave primaria como los Guid v4.

### Concurrencia
- **Control de disponibilidad de forma segura mediante concurrencia optimista.** Se usa `xmin`, una columna de sistema de PostgreSQL que cambia en cada modificación de la fila, como token de concurrencia: al guardar, EF Core verifica que el valor no haya cambiado desde la lectura. Si otra petición modificó el producto, se reintenta la operación completa (releer, aplicar la regla y guardar), para que la validación del stock siempre se haga sobre el valor actual.

### Persistencia
- **EF Core en lugar de Dapper + procedimientos almacenados.** En mi experiencia con SQL Server he usado Dapper y procedimientos almacenados en escenarios de lectura intensiva y BI. Para este caso transaccional, EF Core ofrece de forma nativa concurrencia optimista, unidad de trabajo y migraciones, y respeta un dominio encapsulado. Al estar la persistencia detrás de `IProductRepository`, cambiar a Dapper solo afectaría la capa de Infrastructure.
- **CHECK constraints como defensa en profundidad.** Aunque el dominio lo impide, la base de datos tampoco acepta stock negativo ni precio menor o igual a cero.
- **Migraciones al iniciar.** Es simple para una sola instancia; en producción con varias instancias se ejecutarían como un paso del pipeline.

### API
- **Manejo de errores con ProblemDetails.** Los errores se devuelven en un formato JSON estándar, y FluentValidation entrega errores detallados por campo.
  - 400: datos inválidos.
  - 404: el producto no existe.
  - 409: se agotaron los reintentos por concurrencia. Hubo contención; el cliente puede reintentar.
  - 422: el stock resultante sería negativo. La petición es válida pero la regla de negocio lo impide; reintentar no ayuda.
  - 500: error inesperado, con mensaje genérico y `traceId`, sin exponer detalles internos.

### Infraestructura
- **Docker.** La misma imagen se ejecuta en local y en producción, lo que elimina las diferencias entre entornos. Con un solo comando se levanta la API junto con la base de datos, sin instalar el SDK ni PostgreSQL, y la solución puede desplegarse en cualquier plataforma que soporte contenedores.

## Mejoras futuras
- Pruebas de integración con Testcontainers, incluyendo escenarios de concurrencia real.
- Autenticación y autorización.