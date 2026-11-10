using PedidosWeb.Core.Clientes;

namespace PedidosWeb.Core.Pedidos;

/// <summary>Una línea del detalle de un pedido, con su subtotal.</summary>
/// <param name="Numero">Número de la línea dentro del pedido, desde 1.</param>
/// <param name="Producto">Descripción del producto.</param>
/// <param name="Cantidad">Unidades pedidas.</param>
/// <param name="PrecioUnitario">Precio de una unidad en CLP, sin IVA.</param>
/// <param name="Subtotal"><c>Cantidad × PrecioUnitario</c>, en CLP.</param>
public sealed record LineaDetalle(
    int Numero,
    string Producto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);

/// <summary>Un pedido con su cliente, sus líneas y su total (HU-02).</summary>
/// <param name="Pedido">El pedido.</param>
/// <param name="Cliente">El cliente del pedido, o null si no está en el catálogo.</param>
/// <param name="Lineas">Las líneas, ordenadas por número.</param>
/// <param name="Total">Suma de los subtotales de las líneas, en CLP, sin IVA.</param>
public sealed record DetallePedido(
    Pedido Pedido,
    Cliente? Cliente,
    IReadOnlyList<LineaDetalle> Lineas,
    decimal Total);
