namespace PedidosWeb.Core.Pedidos;

/// <summary>Una página del listado de pedidos.</summary>
/// <param name="Items">Pedidos de la página, ya filtrados y ordenados.</param>
/// <param name="Total">Cantidad de pedidos que cumplen los filtros, sin paginar.</param>
/// <param name="Pagina">Número de página (empieza en 1).</param>
/// <param name="TamanoPagina">Cantidad máxima de pedidos por página.</param>
public sealed record PaginaPedidos(
    IReadOnlyList<Pedido> Items,
    int Total,
    int Pagina,
    int TamanoPagina);

/// <summary>Resultado del listado: una página o un mensaje de error de validación.</summary>
public sealed record ResultadoListado(PaginaPedidos? Pagina, string? Error)
{
    public bool EsValido => Error is null;

    public static ResultadoListado Exito(PaginaPedidos pagina) => new(pagina, null);

    public static ResultadoListado Fallo(string error) => new(null, error);
}
