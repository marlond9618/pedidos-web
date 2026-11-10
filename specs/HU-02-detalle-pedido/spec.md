# HU-02 · Detalle de un pedido

Estado: APROBADA por el PO (2026-11-10)

## Historia de usuario
**Historia:** Como vendedor, quiero ver un pedido con su cliente, sus líneas y su total, para revisarlo completo sin abrir varias pantallas.

## Qué devuelve

La ruta `GET /api/pedidos/{id}/detalle` devuelve:

- Del pedido: `id`, `folio`, `fecha`, `estado`, `sucursal` (los mismos datos de `Pedido`, sin `clienteId`, `clienteNombre` ni `total`, que van en `cliente` y `total`).
- Del cliente: `id`, `nombre`, `rut`, `region`, `activo`. El RUT se devuelve tal como está guardado.
- De cada línea: `numero`, `producto`, `cantidad`, `precioUnitario` y `subtotal` (`cantidad × precioUnitario`), ordenadas por `numero`.
- Total: suma de los `subtotal` de las líneas, en CLP, sin IVA, `decimal` sin decimales.

La ruta existente `GET /api/pedidos/{id}` no cambia.

## Criterios de aceptación

- **CA-01** · Dado el pedido 8, cuando se pide su detalle, entonces responde 200 con folio `PED-STGO-000008`, fecha `2026-10-20`, estado `Entregado` y sucursal `STGO`.
- **CA-02** · Dado el pedido 8, cuando se pide su detalle, entonces `cliente` es C-001 «Ferretería El Clavo», RUT `76.123.456-0`, región `RM`, activo.
- **CA-03** · Dado el pedido 8, cuando se pide su detalle, entonces trae 3 líneas, numeradas 1, 2 y 3, con sus subtotales (la línea 1: Harina para pan 25 kg, 3 × 21.500 = 64.500).
- **CA-04** · Dado el pedido 8, cuando se pide su detalle, entonces `total = 120100`.
- **CA-05** · Dado cualquiera de los 45 pedidos de ejemplo, cuando se pide su detalle, entonces `total` es igual al `Total` guardado en el pedido.
- **CA-06** · Dado el pedido 6 (cliente C-007, inactivo), cuando se pide su detalle, entonces responde 200 con `cliente.activo = false`.

## Si el pedido no existe

`GET /api/pedidos/999/detalle` responde 404 sin cuerpo (igual que `GET /api/pedidos/999`).
- **CA-07** · Dado el id 999, cuando se pide su detalle, entonces responde 404.

Si el pedido existe pero su cliente no está en el catálogo de clientes, responde 200 con `cliente: null` (no ocurre con los datos de ejemplo; se prueba en Core con un repositorio de prueba).
- **CA-08** · Dado un pedido cuyo cliente no existe en el catálogo, cuando se pide su detalle, entonces `cliente` es `null` y el resto del detalle se devuelve igual.

## Ejemplo

`GET /api/pedidos/8/detalle` → 200:

```json
{
  "id": 8, "folio": "PED-STGO-000008", "fecha": "2026-10-20", "estado": "Entregado", "sucursal": "STGO",
  "cliente": { "id": "C-001", "nombre": "Ferretería El Clavo", "rut": "76.123.456-0", "region": "RM", "activo": true },
  "lineas": [
    { "numero": 1, "producto": "Harina para pan 25 kg", "cantidad": 3, "precioUnitario": 21500, "subtotal": 64500 },
    { "numero": 2, "producto": "Mantequilla 250 g", "cantidad": 12, "precioUnitario": 2950, "subtotal": 35400 },
    { "numero": 3, "producto": "Bebida cola 1,5 L", "cantidad": 10, "precioUnitario": 2020, "subtotal": 20200 }
  ],
  "total": 120100
}
```

## Dónde se prueban
Lógica (cliente, líneas, total, CA-08) en `tests/PedidosWeb.Core.Tests`; la respuesta HTTP (200, 404, forma JSON) en `tests/PedidosWeb.Api.Tests`.

## Fuera de alcance
- IVA, descuentos y otros cargos.
- Crear, editar o anular pedidos o líneas.
- Cambios a `Pedido`, `IPedidoRepositorio`, los datos de ejemplo o el `ClientesService` heredado.
- Paginación de líneas.

## Preguntas abiertas
Ninguna. Supuestos confirmados: total sin IVA como suma de líneas, ficha de cliente completa y `cliente: null` si no existe.
