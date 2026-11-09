# HU-01 · Listar pedidos con filtros

Estado: APROBADA por el PO (2026-11-09)

## Historia de usuario
Como vendedor, quiero listar pedidos filtrando por cliente, estado y rango de fechas.

## Objetivo
Que el vendedor encuentre rápidamente los pedidos que le interesan sin revisar uno por uno: la API devuelve
los pedidos del repositorio que cumplen todos los filtros indicados, en un orden estable y predecible,
reutilizando el modelo `Pedido` y `IPedidoRepositorio.ObtenerTodos()` existentes.

## Reglas de negocio
- RN1. Todos los filtros son opcionales. Sin filtros se listan todos los pedidos. El vendedor ve todos los pedidos, no solo los suyos.
- RN2. Los filtros se combinan con Y (AND): un pedido aparece solo si cumple todos los indicados.
- RN3. Cliente: un solo parámetro `cliente`, que se interpreta según su forma:
  - Si tiene la forma de código `C-NNN` (ej.: `C-001`), filtra por `ClienteId` exacto.
  - Si no, filtra por texto parcial de `ClienteNombre` sin distinguir mayúsculas (`el clavo` encuentra `Ferretería El Clavo`).
  - Un código que no existe (ej.: `C-999`) no es error: devuelve 200 con `items` vacío y `total = 0`.
  - La forma de código no distingue mayúsculas: `c-001` equivale a `C-001`.
  - La búsqueda por nombre solo ignora mayúsculas. Las tildes y la Ñ deben coincidir: `ferreteria` no encuentra `Ferretería` y `dona` no encuentra `Doña`.
- RN4. Rango de fechas: `desde` y `hasta` sobre `Fecha` (`DateOnly`), ambos inclusivos. Se puede indicar uno solo.
  - Si `desde` > `hasta`, es un error de validación: `desde` no puede ser posterior a `hasta`. `desde` = `hasta` es válido (CA9).
  - Formato aceptado: solo `yyyy-MM-dd` (ISO 8601, ej.: `2026-10-01`). Cualquier otro formato o una fecha inexistente es error 400.
- RN5. Estado: filtra por `Estado`. Lista cerrada de estados válidos: `Pendiente`, `Confirmado`, `Despachado`, `Entregado`, `Anulado`.
  - No distingue mayúsculas: `pendiente`, `PENDIENTE` y `Pendiente` son equivalentes.
  - Se acepta un solo estado por consulta.
  - Cualquier otro texto es un error (ver Errores esperados).
- RN6. Sin filtro de estado se incluyen los pedidos `Anulado`.
- RN7. Orden: `Fecha` ascendente; si empatan, `Id` ascendente.
- RN8. Paginación: parámetros `pagina` (empieza en 1) y `tamanoPagina`. Por defecto `pagina = 1` y `tamanoPagina = 20`; máximo `tamanoPagina = 100`.
  - La paginación se aplica después de filtrar y ordenar.
  - La respuesta trae `items`, `total` (pedidos que cumplen los filtros, sin paginar), `pagina` y `tamanoPagina`.
  - `pagina < 1`, `tamanoPagina < 1` o `tamanoPagina > 100` es error 400 (no se ajusta al límite).
  - Un valor que no es un entero (`pagina=abc`, `tamanoPagina=2.5`) se trata igual que uno fuera de rango: mismo 400, mismo mensaje y mismo lugar en el orden de validación.
  - Pedir una página posterior a la última es error 400. Excepción: si no hay resultados (`total = 0`), `pagina = 1` es válida y responde 200 con `items` vacío, para que CA13 y CA21 se cumplan.

## Criterios de aceptación
Con los 45 pedidos de `PedidoRepositorioEnMemoria`. Salvo que se indique otra cosa, "devuelve N pedidos" se refiere a `items` con `total = N`, y todos caben en la página 1 de 20.

- [x] CA1. Sin filtros ni paginación: `total = 45`, `pagina = 1`, `tamanoPagina = 20`, `items` trae 20 pedidos; el primero es el Id 40 (2026-09-02) y el último el Id 43 (2026-09-28).
- [x] CA2. `cliente = C-001` (código) devuelve 5 pedidos, en este orden: 40, 32, 24, 16, 8.
- [x] CA3. `cliente = el clavo` (nombre parcial, en minúsculas) devuelve los mismos 5 pedidos de CA2.
- [x] CA4. `estado = Pendiente` devuelve 5 pedidos, en este orden: 36, 9, 45, 18, 27.
- [x] CA5. `estado = Anulado` devuelve 5 pedidos: 38, 11, 20, 29, 2.
- [x] CA6. Sin filtro de estado, el resultado incluye los 5 `Anulado` (2, 11, 20, 29, 38).
- [x] CA7. `desde = 2026-10-01`, `hasta = 2026-10-31`: `total = 23` (todo octubre); la página 1 trae 20 pedidos, del 41 (2026-10-01) al 23 (2026-10-28).
- [x] CA8. Rango inclusivo: `desde = 2026-09-30`, `hasta = 2026-10-01` devuelve 2 pedidos: 1 y 41.
- [x] CA9. `desde = hasta = 2026-09-02` devuelve solo el pedido 40.
- [x] CA10. Solo `desde = 2026-10-25` devuelve 7 pedidos: 25, 4, 44, 23, 2, 42, 21.
- [x] CA11. Combinados: `cliente = C-001`, `estado = Entregado` devuelve 40, 24, 8.
- [x] CA12. Combinados: `cliente = C-001`, `estado = Entregado`, `desde = 2026-10-01` devuelve solo el 8.
- [x] CA13. Filtros válidos sin coincidencias (`cliente = C-002`, `estado = Entregado`, `desde = 2026-10-01`) devuelven `items` vacío y `total = 0`, no error.
- [x] CA14. `estado = Perdido` responde 400 con `{ "error": "El estado 'Perdido' no es válido. Valores permitidos: Pendiente, Confirmado, Despachado, Entregado, Anulado." }`.
- [x] CA15. `desde = 2026-10-31`, `hasta = 2026-10-01` responde 400 con `{ "error": "La fecha 'desde' no puede ser posterior a 'hasta'." }`.
- [x] CA16. `desde` con un valor que no es `yyyy-MM-dd` o no es una fecha real (`31-10-2026`, `2026/10/01`, `2026-13-01`) responde 400 con `{ "error": "La fecha 'desde' debe tener el formato yyyy-MM-dd." }`; `hasta = 31-10-2026` responde el mensaje equivalente para `'hasta'`.
- [x] CA17. `estado = pendiente` (minúsculas) devuelve los mismos 5 pedidos de CA4.
- [x] CA18. Sin filtros, `pagina = 3`: `total = 45`, `items` trae 5 pedidos: 44, 23, 2, 42, 21.
- [x] CA19. Sin filtros, `tamanoPagina = 100`: `items` trae los 45 pedidos, del 40 al 21.
- [x] CA20. `cliente = DOÑA` (nombre parcial, en mayúsculas) devuelve los 6 pedidos de C-002: 13, 5, 45, 37, 29, 21.
- [x] CA21. `cliente = C-999` responde 200 con `items` vacío y `total = 0`.
- [x] CA22. `cliente = c-001` (código en minúscula) devuelve los mismos 5 pedidos de CA2.
- [x] CA23. `cliente = ferretería` (con tilde) devuelve los 5 pedidos de C-001; `cliente = ferreteria` (sin tilde) responde 200 con `items` vacío y `total = 0`.
- [x] CA24. `pagina = 0` responde 400 con `{ "error": "'pagina' debe ser mayor o igual a 1." }`; `tamanoPagina = 0` y `tamanoPagina = 101` responden, cada uno, 400 con `{ "error": "'tamanoPagina' debe estar entre 1 y 100." }`. `tamanoPagina = 100` es válido (CA19).
- [x] CA25. Sin filtros, `pagina = 4` (hay 3 páginas de 20) responde 400 con `{ "error": "La página 4 no existe. Hay 3 página(s)." }`. `pagina = 3` es válida (CA18).
- [x] CA26. `cliente = C-999`, `pagina = 2` responde 400 con `{ "error": "La página 2 no existe. Hay 1 página(s)." }`.
- [x] CA27. `estado = Perdido`, `pagina = 0` responde 400 solo con el mensaje de estado de CA14 (estado se valida antes que pagina).
- [x] CA28. `pagina = abc` responde 400 con `{ "error": "'pagina' debe ser mayor o igual a 1." }`; `tamanoPagina = 2.5` responde 400 con `{ "error": "'tamanoPagina' debe estar entre 1 y 100." }`.

Nota: en los datos de ejemplo no hay dos pedidos con la misma fecha, así que el desempate por `Id` (RN7) no se puede probar con ellos.

Dónde se prueban: la lógica de filtros, orden y paginación va en `tests/PedidosWeb.Core.Tests`. Los criterios que se verifican por HTTP (códigos, cuerpo `{ "error" }`, mensajes) van en un proyecto nuevo, `tests/PedidosWeb.Api.Tests` (xUnit + `WebApplicationFactory<Program>`).

## Ejemplos de entrada y salida
Ruta: `GET /api/pedidos?cliente=&estado=&desde=&hasta=&pagina=&tamanoPagina=` (todos los parámetros son opcionales).

Entrada: `GET /api/pedidos?cliente=C-001&estado=Entregado&desde=2026-10-01`

Salida (200). Cada elemento de `items` tiene la forma actual de `Pedido`:

```json
{
  "items": [
    { "id": 8, "folio": "PED-STGO-000008", "clienteId": "C-001", "clienteNombre": "Ferretería El Clavo",
      "fecha": "2026-10-20", "estado": "Entregado", "total": 120100, "sucursal": "STGO" }
  ],
  "total": 1,
  "pagina": 1,
  "tamanoPagina": 20
}
```

Entrada: `GET /api/pedidos?cliente=C-002&estado=Entregado&desde=2026-10-01`
Salida (200): `{ "items": [], "total": 0, "pagina": 1, "tamanoPagina": 20 }`.

## Errores esperados
Formato: HTTP 400 con cuerpo JSON `{ "error": "<mensaje en español>" }`.

Los textos son exactos y los tests los comparan literalmente. `{valor}`, `{pagina}` y `{paginas}` se reemplazan por el dato real.

| Caso | Mensaje exacto | CA |
|---|---|---|
| Estado no válido | `El estado '{valor}' no es válido. Valores permitidos: Pendiente, Confirmado, Despachado, Entregado, Anulado.` | CA14 |
| `desde` mal escrita | `La fecha 'desde' debe tener el formato yyyy-MM-dd.` | CA16 |
| `hasta` mal escrita | `La fecha 'hasta' debe tener el formato yyyy-MM-dd.` | CA16 |
| `desde` > `hasta` | `La fecha 'desde' no puede ser posterior a 'hasta'.` | CA15 |
| `pagina` < 1 | `'pagina' debe ser mayor o igual a 1.` | CA24 |
| `tamanoPagina` < 1 o > 100 | `'tamanoPagina' debe estar entre 1 y 100.` | CA24 |
| Página posterior a la última | `La página {pagina} no existe. Hay {paginas} página(s).` | CA25, CA26 |

`{valor}` es el texto tal como lo envió el usuario. `{paginas}` es la cantidad de páginas, con un mínimo de 1, porque con `total = 0` la página 1 existe (RN8).

- Si hay varios errores a la vez, se informa solo el primero según este orden de validación: `estado` → `desde` (formato) → `hasta` (formato) → `desde` > `hasta` → `pagina` < 1 → `tamanoPagina` fuera de rango → página posterior a la última (CA27).
- Un código de cliente inexistente (`C-999`) **no** es error: 200 con lista vacía (CA21).

## Fuera de alcance
- Crear, editar, anular o cambiar el estado de pedidos.
- Detalle de pedido (ya existe `GET /api/pedidos/{id}`).
- Filtros por sucursal, folio o total; totales o sumatorias del listado.
- Persistencia real (PostgreSQL llega en la Unidad 5) y cambios a los datos de ejemplo.
- Autenticación y permisos por vendedor: cualquier vendedor ve todos los pedidos (RN1). `Pedido` no tiene un campo de vendedor.

## Preguntas abiertas
Ninguna.
