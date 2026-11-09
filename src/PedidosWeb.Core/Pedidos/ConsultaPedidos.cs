namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// Filtros y paginación del listado de pedidos (HU-01), tal como llegan en la query string.
/// Son textos crudos: la validación y la conversión las hace <see cref="ListadoPedidosServicio"/>.
/// Un valor null o vacío se trata como "no indicado".
/// </summary>
public sealed record ConsultaPedidos(
    string? Cliente = null,
    string? Estado = null,
    string? Desde = null,
    string? Hasta = null,
    string? Pagina = null,
    string? TamanoPagina = null);
