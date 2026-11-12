using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

/// <summary>
/// HU-03 · Filtrar pedidos por rango de fechas. Un test por criterio de aceptación.
/// Ver specs/HU-03-pedidos-por-fecha/spec.md.
/// </summary>
public class FiltroPedidosTests
{
    private static Pedido Con(int id, DateOnly fecha) =>
        new(id, $"PED-STGO-{id:000000}", "C-001", "Cliente", fecha, "Pendiente", 1000, "STGO");

    private static readonly DateOnly Desde = new(2026, 10, 1);
    private static readonly DateOnly Hasta = new(2026, 10, 31);

    [Fact] // CA-01
    public void EntreFechas_FechaIgualADesde_SeIncluye()
    {
        var pedido = Con(1, Desde);

        var resultado = FiltroPedidos.EntreFechas([pedido], Desde, Hasta);

        Assert.Equal([1], resultado.Select(p => p.Id));
    }

    [Fact] // CA-02
    public void EntreFechas_FechaIgualAHasta_SeIncluye()
    {
        var pedido = Con(2, Hasta);

        var resultado = FiltroPedidos.EntreFechas([pedido], Desde, Hasta);

        Assert.Equal([2], resultado.Select(p => p.Id));
    }

    [Fact] // CA-03
    public void EntreFechas_FechaFueraDelRango_NoSeIncluye()
    {
        var antes = Con(3, new DateOnly(2026, 9, 30));
        var despues = Con(4, new DateOnly(2026, 11, 1));
        var dentro = Con(5, new DateOnly(2026, 10, 15));

        var resultado = FiltroPedidos.EntreFechas([antes, despues, dentro], Desde, Hasta);

        Assert.Equal([5], resultado.Select(p => p.Id));
    }

    [Fact] // CA-04
    public void EntreFechas_DesdeMayorQueHasta_LanzaArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => FiltroPedidos.EntreFechas([], Hasta, Desde));

        Assert.Contains("desde", ex.Message);
        Assert.Contains("hasta", ex.Message);
    }
}
