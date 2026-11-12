namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// HU-03 · Filtra pedidos por rango de fechas de emisión.
/// Ver specs/HU-03-pedidos-por-fecha/spec.md.
/// </summary>
public static class FiltroPedidos
{
    /// <summary>
    /// Devuelve los pedidos cuya <see cref="Pedido.Fecha"/> está dentro del rango
    /// <paramref name="desde"/>..<paramref name="hasta"/>, inclusivo en ambos extremos (CA-01, CA-02, CA-03).
    /// </summary>
    /// <param name="pedidos">Pedidos a filtrar.</param>
    /// <param name="desde">Límite inferior del rango (inclusive).</param>
    /// <param name="hasta">Límite superior del rango (inclusive).</param>
    /// <exception cref="ArgumentException">Si <paramref name="desde"/> es posterior a <paramref name="hasta"/> (CA-04).</exception>
    public static IReadOnlyList<Pedido> EntreFechas(IEnumerable<Pedido> pedidos, DateOnly desde, DateOnly hasta)
    {
        if (desde > hasta)
            throw new ArgumentException(
                $"La fecha 'desde' ({desde:yyyy-MM-dd}) no puede ser posterior a 'hasta' ({hasta:yyyy-MM-dd}).",
                nameof(desde));

        return pedidos
            .Where(p => p.Fecha >= desde && p.Fecha <= hasta)
            .ToList();
    }
}
