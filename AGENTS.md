# AGENTS.md

## Proyecto
Pedidos Web: portal de pedidos de la distribuidora, modernización del sistema legado Pedidos Legado (Delphi + DBISAM).
API REST en ASP.NET Core (.NET 10, Minimal API) con 45 pedidos de ejemplo ficticios en memoria; no hay base de datos.

## Comandos
- Compilar: `dotnet build`
- Testear: `dotnet test`
- Ejecutar la API: `dotnet run --project src/PedidosWeb.Api` (http://localhost:5080; salud en `GET /salud`)

## Estilo de código
- Nombres en español: clases, métodos, variables, tests y comentarios (`Pedido`, `ObtenerPorId`, `FormatearFolio`).
- Dinero: `decimal` (CLP, sin decimales). Nunca `double` ni `float`.
- Fechas de pedido: `DateOnly` (hora local de Chile, sin hora).
- `PedidosWeb.Core`: dominio (modelos como `sealed record`, `IPedidoRepositorio`, reglas). No referencia ASP.NET.
- `PedidosWeb.Api`: endpoints Minimal API y registro de dependencias en `Program.cs`.
- Namespaces de archivo (`namespace X;`) y `Nullable` habilitado.

## Tests
- xUnit 2.9 en `tests/PedidosWeb.Core.Tests/`, con carpetas espejo de `src/PedidosWeb.Core/` (`Pedidos/`, `Folios/`).
- Nombre: `Metodo_Escenario_ResultadoEsperado` (ej.: `ObtenerPorId_IdInexistente_DevuelveNull`).
- Se ejecutan con `dotnet test` antes de cada commit.
- Tests HTTP de la API en `tests/PedidosWeb.Api.Tests/` (xUnit + `WebApplicationFactory<Program>`); se crea con HU-01.
  La lógica de dominio se prueba en `PedidosWeb.Core.Tests`.

## Seguridad
- Nunca secretos, claves ni cadenas de conexión en el código ni en el repo (`.env` y `*.pem` están en `.gitignore`).

## Reglas de trabajo
- Toda funcionalidad parte de una spec aprobada en `specs/<historia>/spec.md`.
- Un test xUnit por criterio de aceptación.
- No hacer commit sin tests en verde.
- Sin preguntar puedes: leer, compilar, testear y ejecutar la API; editar `specs/` y `docs/`;
  editar `src/` y `tests/` para una historia con spec aprobada.
- Pide confirmación antes de hacer commit o push.
- Pide confirmación antes de cambiar `Pedido.cs`, `IPedidoRepositorio` o los datos de ejemplo.
- Si la implementación choca con una spec aprobada, detente y pregunta; no cambies la spec por tu cuenta.
- Pide confirmación antes de agregar o actualizar paquetes NuGet: propón el paquete y la versión, y espera el OK.
