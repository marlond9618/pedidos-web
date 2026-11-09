# HU-01 · Listar pedidos con filtros: plan

Spec: `spec.md` (APROBADA por el PO, 2026-11-09).

## Diseño
- **Core** (`src/PedidosWeb.Core/Pedidos/`)
  - `ConsultaPedidos`: los parámetros de la query string tal como llegan (texto).
  - `ListadoPedidosServicio.Listar(ConsultaPedidos)`: valida en el orden de la spec, filtra sobre
    `IPedidoRepositorio.ObtenerTodos()`, ordena (`Fecha`, luego `Id`) y pagina. Devuelve un `ResultadoListado`
    con una `PaginaPedidos` o con el mensaje de error exacto.
  - No cambian `Pedido`, `IPedidoRepositorio` ni los datos de ejemplo.
- **Api** (`Program.cs`): `GET /api/pedidos`. Todos los parámetros se reciben como `string?`. Si se declararan
  como `int` o `DateOnly`, Minimal API respondería su propio 400 y no saldrían los mensajes de la spec (CA16, CA28).
  Un error responde 400 con `{ "error": "..." }`; un resultado válido responde 200 con `{ items, total, pagina, tamanoPagina }`.
- **Comparaciones**: el nombre de cliente usa `StringComparison.OrdinalIgnoreCase`, que ignora mayúsculas
  (incluida Ñ/ñ) pero no las tildes (CA20, CA23). Las fechas usan `DateOnly.TryParseExact("yyyy-MM-dd")`. Los enteros
  usan `int.TryParse` con `NumberStyles.None`, que no acepta espacios, signos ni decimales.

## Tests (uno por CA)
- `tests/PedidosWeb.Core.Tests/Pedidos/ListadoPedidosServicioTests.cs`: CA1–CA13 y CA17–CA23, el desempate por Id (RN7)
  con un repositorio de prueba y el supuesto de los parámetros vacíos.
- `tests/PedidosWeb.Api.Tests/Pedidos/ListarPedidosEndpointTests.cs`: CA14–CA16, CA24–CA28 y la forma JSON del ejemplo.
- Paquete nuevo, aprobado: `Microsoft.AspNetCore.Mvc.Testing` 10.0.14.

## Supuestos pendientes de confirmar con el PO
La spec no los cubre. Se implementaron así y la spec no se modificó:
1. **Parámetro vacío** (`estado=`, `desde=`, `pagina=`, ...) = no indicado. No es error.
2. **Código de cliente**: `C-` o `c-` seguido de exactamente 3 dígitos (`^[Cc]-[0-9]{3}$`). `C-1` o `C-0001` se buscan como texto del nombre.
3. **Espacios**: no se recortan. `estado=" Pendiente"` es un estado inválido y `pagina=" 2"` no es un entero.
4. **Parámetro repetido**: ASP.NET une los valores con coma (`estado=Pendiente&estado=Anulado` → `"Pendiente,Anulado"`),
   así que se trata como un valor inválido: 400 de estado, de fecha o de paginación, según el parámetro. En `cliente`
   no existe un valor inválido: `"a,b"` se busca como texto y normalmente no encuentra nada.
