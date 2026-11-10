using PedidosWeb.Core.Clientes;

namespace PedidosWeb.Core.Tests.Clientes;

public class ClienteRepositorioEnMemoriaTests
{
    private readonly ClienteRepositorioEnMemoria _repositorio = new();

    [Fact]
    public void ObtenerPorId_ClienteExistente_DevuelveSuFicha()
    {
        var cliente = _repositorio.ObtenerPorId("C-001");

        Assert.NotNull(cliente);
        Assert.Equal("Ferretería El Clavo", cliente.Nombre);
        Assert.Equal("76.123.456-0", cliente.Rut);
        Assert.Equal("RM", cliente.Region);
        Assert.True(cliente.Activo);
    }

    [Fact]
    public void ObtenerPorId_ClienteInactivo_DevuelveActivoFalso()
    {
        var cliente = _repositorio.ObtenerPorId("C-007");

        Assert.NotNull(cliente);
        Assert.False(cliente.Activo);
    }

    [Fact]
    public void ObtenerPorId_IdInexistente_DevuelveNull()
    {
        Assert.Null(_repositorio.ObtenerPorId("C-999"));
    }

    [Fact]
    public void ObtenerPorId_DatosDeEjemplo_TodosLosClientesDeLosPedidosExisten()
    {
        var idsDeClientes = new PedidosWeb.Core.Pedidos.PedidoRepositorioEnMemoria()
            .ObtenerTodos().Select(p => p.ClienteId).Distinct();

        Assert.All(idsDeClientes, id => Assert.NotNull(_repositorio.ObtenerPorId(id)));
    }
}
