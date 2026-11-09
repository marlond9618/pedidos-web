using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

/// <summary>
/// HU-01 · Listar pedidos con filtros. Un test por criterio de aceptación que se verifica en el dominio.
/// Los criterios de errores HTTP (CA14–CA16, CA24–CA28) están en PedidosWeb.Api.Tests.
/// </summary>
public class ListadoPedidosServicioTests
{
    private readonly ListadoPedidosServicio _servicio = new(new PedidoRepositorioEnMemoria());

    private PaginaPedidos ListarValido(ConsultaPedidos consulta)
    {
        var resultado = _servicio.Listar(consulta);
        Assert.True(resultado.EsValido, resultado.Error);
        return resultado.Pagina!;
    }

    private static int[] Ids(PaginaPedidos pagina) => pagina.Items.Select(p => p.Id).ToArray();

    [Fact] // CA1
    public void Listar_SinFiltrosNiPaginacion_DevuelvePrimeraPaginaDe20De45()
    {
        var pagina = ListarValido(new ConsultaPedidos());

        Assert.Equal(45, pagina.Total);
        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(20, pagina.TamanoPagina);
        Assert.Equal(20, pagina.Items.Count);
        Assert.Equal(40, pagina.Items[0].Id);
        Assert.Equal(new DateOnly(2026, 9, 2), pagina.Items[0].Fecha);
        Assert.Equal(43, pagina.Items[^1].Id);
        Assert.Equal(new DateOnly(2026, 9, 28), pagina.Items[^1].Fecha);
    }

    [Fact] // CA2
    public void Listar_ClienteCodigo_DevuelveSusPedidosOrdenados()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "C-001"));

        Assert.Equal(5, pagina.Total);
        Assert.Equal([40, 32, 24, 16, 8], Ids(pagina));
    }

    [Fact] // CA3
    public void Listar_ClienteNombreParcialMinusculas_DevuelveLosMismosQueElCodigo()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "el clavo"));

        Assert.Equal(5, pagina.Total);
        Assert.Equal([40, 32, 24, 16, 8], Ids(pagina));
    }

    [Fact] // CA4
    public void Listar_EstadoPendiente_Devuelve5PedidosOrdenados()
    {
        var pagina = ListarValido(new ConsultaPedidos(Estado: "Pendiente"));

        Assert.Equal(5, pagina.Total);
        Assert.Equal([36, 9, 45, 18, 27], Ids(pagina));
    }

    [Fact] // CA5
    public void Listar_EstadoAnulado_Devuelve5PedidosOrdenados()
    {
        var pagina = ListarValido(new ConsultaPedidos(Estado: "Anulado"));

        Assert.Equal(5, pagina.Total);
        Assert.Equal([38, 11, 20, 29, 2], Ids(pagina));
    }

    [Fact] // CA6
    public void Listar_SinFiltroDeEstado_IncluyeLosAnulados()
    {
        var pagina = ListarValido(new ConsultaPedidos(TamanoPagina: "100"));

        var ids = Ids(pagina);
        Assert.All(new[] { 2, 11, 20, 29, 38 }, id => Assert.Contains(id, ids));
    }

    [Fact] // CA7
    public void Listar_RangoOctubre_Devuelve23YPrimeraPaginaDe20()
    {
        var pagina = ListarValido(new ConsultaPedidos(Desde: "2026-10-01", Hasta: "2026-10-31"));

        Assert.Equal(23, pagina.Total);
        Assert.Equal(20, pagina.Items.Count);
        Assert.Equal(41, pagina.Items[0].Id);
        Assert.Equal(new DateOnly(2026, 10, 1), pagina.Items[0].Fecha);
        Assert.Equal(23, pagina.Items[^1].Id);
        Assert.Equal(new DateOnly(2026, 10, 28), pagina.Items[^1].Fecha);
    }

    [Fact] // CA8
    public void Listar_RangoDeDosDias_EsInclusivoEnAmbosExtremos()
    {
        var pagina = ListarValido(new ConsultaPedidos(Desde: "2026-09-30", Hasta: "2026-10-01"));

        Assert.Equal(2, pagina.Total);
        Assert.Equal([1, 41], Ids(pagina));
    }

    [Fact] // CA9
    public void Listar_DesdeIgualAHasta_DevuelveSoloLosDeEseDia()
    {
        var pagina = ListarValido(new ConsultaPedidos(Desde: "2026-09-02", Hasta: "2026-09-02"));

        Assert.Equal(1, pagina.Total);
        Assert.Equal([40], Ids(pagina));
    }

    [Fact] // CA10
    public void Listar_SoloDesde_DevuelveLosPedidosDesdeEsaFecha()
    {
        var pagina = ListarValido(new ConsultaPedidos(Desde: "2026-10-25"));

        Assert.Equal(7, pagina.Total);
        Assert.Equal([25, 4, 44, 23, 2, 42, 21], Ids(pagina));
    }

    [Fact] // CA11
    public void Listar_ClienteYEstado_CombinaConY()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "C-001", Estado: "Entregado"));

        Assert.Equal(3, pagina.Total);
        Assert.Equal([40, 24, 8], Ids(pagina));
    }

    [Fact] // CA12
    public void Listar_ClienteEstadoYDesde_CombinaLosTres()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "C-001", Estado: "Entregado", Desde: "2026-10-01"));

        Assert.Equal(1, pagina.Total);
        Assert.Equal([8], Ids(pagina));
    }

    [Fact] // CA13
    public void Listar_FiltrosValidosSinCoincidencias_DevuelveVacioSinError()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "C-002", Estado: "Entregado", Desde: "2026-10-01"));

        Assert.Equal(0, pagina.Total);
        Assert.Empty(pagina.Items);
        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(20, pagina.TamanoPagina);
    }

    [Fact] // CA17
    public void Listar_EstadoEnMinusculas_DevuelveLosMismosQueCA4()
    {
        var pagina = ListarValido(new ConsultaPedidos(Estado: "pendiente"));

        Assert.Equal(5, pagina.Total);
        Assert.Equal([36, 9, 45, 18, 27], Ids(pagina));
    }

    [Fact] // CA18
    public void Listar_Pagina3_DevuelveLosUltimos5()
    {
        var pagina = ListarValido(new ConsultaPedidos(Pagina: "3"));

        Assert.Equal(45, pagina.Total);
        Assert.Equal(3, pagina.Pagina);
        Assert.Equal([44, 23, 2, 42, 21], Ids(pagina));
    }

    [Fact] // CA19
    public void Listar_TamanoPagina100_DevuelveLos45()
    {
        var pagina = ListarValido(new ConsultaPedidos(TamanoPagina: "100"));

        Assert.Equal(45, pagina.Items.Count);
        Assert.Equal(100, pagina.TamanoPagina);
        Assert.Equal(40, pagina.Items[0].Id);
        Assert.Equal(21, pagina.Items[^1].Id);
    }

    [Fact] // CA20
    public void Listar_ClienteNombreEnMayusculasConEnie_DevuelveLosDeC002()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "DOÑA"));

        Assert.Equal(6, pagina.Total);
        Assert.Equal([13, 5, 45, 37, 29, 21], Ids(pagina));
    }

    [Fact] // CA21
    public void Listar_CodigoDeClienteInexistente_DevuelveVacioSinError()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "C-999"));

        Assert.Equal(0, pagina.Total);
        Assert.Empty(pagina.Items);
    }

    [Fact] // CA22
    public void Listar_CodigoDeClienteEnMinuscula_EquivaleAlCodigo()
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: "c-001"));

        Assert.Equal(5, pagina.Total);
        Assert.Equal([40, 32, 24, 16, 8], Ids(pagina));
    }

    [Theory] // CA23
    [InlineData("ferretería", 5)]
    [InlineData("ferreteria", 0)]
    public void Listar_ClienteNombreConYSinTilde_LasTildesDebenCoincidir(string cliente, int totalEsperado)
    {
        var pagina = ListarValido(new ConsultaPedidos(Cliente: cliente));

        Assert.Equal(totalEsperado, pagina.Total);
        if (totalEsperado > 0)
            Assert.Equal([40, 32, 24, 16, 8], Ids(pagina));
    }

    // RN7: no se puede probar con los datos de ejemplo (no hay fechas repetidas), así que se usa un repositorio propio.
    [Fact]
    public void Listar_PedidosConLaMismaFecha_DesempataPorIdAscendente()
    {
        var fecha = new DateOnly(2026, 10, 1);
        var repositorio = new RepositorioDePrueba(
            new Pedido(3, "PED-STGO-000003", "C-001", "Cliente", fecha, "Pendiente", 1000, "STGO"),
            new Pedido(1, "PED-STGO-000001", "C-001", "Cliente", fecha, "Pendiente", 1000, "STGO"),
            new Pedido(2, "PED-STGO-000002", "C-001", "Cliente", fecha, "Pendiente", 1000, "STGO"));
        var servicio = new ListadoPedidosServicio(repositorio);

        var resultado = servicio.Listar(new ConsultaPedidos());

        Assert.Equal([1, 2, 3], resultado.Pagina!.Items.Select(p => p.Id));
    }

    // Supuesto pendiente de confirmar con el PO: un parámetro vacío equivale a no indicarlo.
    [Fact]
    public void Listar_ParametrosVacios_SeTratanComoNoIndicados()
    {
        var pagina = ListarValido(new ConsultaPedidos("", "", "", "", "", ""));

        Assert.Equal(45, pagina.Total);
        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(20, pagina.TamanoPagina);
    }

    private sealed class RepositorioDePrueba(params Pedido[] pedidos) : IPedidoRepositorio
    {
        public IReadOnlyList<Pedido> ObtenerTodos() => pedidos;

        public Pedido? ObtenerPorId(int id) => pedidos.FirstOrDefault(p => p.Id == id);
    }
}
