using PedidosWeb.Core.Clientes;
using PedidosWeb.Core.Lineas;
using PedidosWeb.Core.Pedidos;

namespace PedidosWeb.Core.Tests.Pedidos;

public class DetallePedidoServicioTests
{
    private readonly PedidoRepositorioEnMemoria _pedidos = new();
    private readonly DetallePedidoServicio _servicio;

    public DetallePedidoServicioTests()
    {
        _servicio = new DetallePedidoServicio(
            _pedidos, new ClienteRepositorioEnMemoria(), new LineaPedidoRepositorioEnMemoria());
    }

    // CA-01
    [Fact]
    public void Obtener_Pedido8_DevuelveLosDatosDelPedido()
    {
        var pedido = _servicio.Obtener(8)!.Pedido;

        Assert.Equal("PED-STGO-000008", pedido.Folio);
        Assert.Equal(new DateOnly(2026, 10, 20), pedido.Fecha);
        Assert.Equal("Entregado", pedido.Estado);
        Assert.Equal("STGO", pedido.Sucursal);
    }

    // CA-02
    [Fact]
    public void Obtener_Pedido8_DevuelveLaFichaDelCliente()
    {
        var cliente = _servicio.Obtener(8)!.Cliente;

        Assert.NotNull(cliente);
        Assert.Equal("C-001", cliente.Id);
        Assert.Equal("Ferretería El Clavo", cliente.Nombre);
        Assert.Equal("76.123.456-0", cliente.Rut);
        Assert.Equal("RM", cliente.Region);
        Assert.True(cliente.Activo);
    }

    // CA-03
    [Fact]
    public void Obtener_Pedido8_DevuelveTresLineasConSubtotales()
    {
        var lineas = _servicio.Obtener(8)!.Lineas;

        Assert.Equal(new[] { 1, 2, 3 }, lineas.Select(l => l.Numero));
        Assert.Equal("Harina para pan 25 kg", lineas[0].Producto);
        Assert.Equal(3, lineas[0].Cantidad);
        Assert.Equal(21500m, lineas[0].PrecioUnitario);
        Assert.Equal(new[] { 64500m, 35400m, 20200m }, lineas.Select(l => l.Subtotal));
    }

    // CA-04
    [Fact]
    public void Obtener_Pedido8_TotalEs120100()
    {
        Assert.Equal(120100m, _servicio.Obtener(8)!.Total);
    }

    // CA-05
    [Fact]
    public void Obtener_LosDe45Pedidos_TotalIgualAlTotalGuardadoEnElPedido()
    {
        var distintos = _pedidos.ObtenerTodos()
            .Where(p => _servicio.Obtener(p.Id)!.Total != p.Total)
            .Select(p => p.Id)
            .ToList();

        Assert.Equal(45, _pedidos.ObtenerTodos().Count);
        Assert.Empty(distintos);
    }

    // CA-06
    [Fact]
    public void Obtener_Pedido6ClienteInactivo_DevuelveActivoFalso()
    {
        var cliente = _servicio.Obtener(6)!.Cliente;

        Assert.NotNull(cliente);
        Assert.Equal("C-007", cliente.Id);
        Assert.False(cliente.Activo);
    }

    // CA-07 (el 404 HTTP se prueba en Api.Tests)
    [Fact]
    public void Obtener_IdInexistente_DevuelveNull()
    {
        Assert.Null(_servicio.Obtener(999));
    }

    // CA-08
    [Fact]
    public void Obtener_ClienteFueraDelCatalogo_DevuelveClienteNullYElRestoDelDetalle()
    {
        var sinClientes = new ClienteRepositorioEnMemoria(new List<Cliente>());
        var servicio = new DetallePedidoServicio(_pedidos, sinClientes, new LineaPedidoRepositorioEnMemoria());

        var detalle = servicio.Obtener(8)!;

        Assert.Null(detalle.Cliente);
        Assert.Equal("PED-STGO-000008", detalle.Pedido.Folio);
        Assert.Equal(3, detalle.Lineas.Count);
        Assert.Equal(120100m, detalle.Total);
    }
}
