using PedidosWeb.Core.Clientes;
using PedidosWeb.Core.Lineas;

namespace PedidosWeb.Core.Pedidos;

/// <summary>Arma el detalle de un pedido: su cliente, sus líneas y su total (HU-02).</summary>
public sealed class DetallePedidoServicio
{
    private readonly IPedidoRepositorio _pedidos;
    private readonly IClienteRepositorio _clientes;
    private readonly ILineaPedidoRepositorio _lineas;

    public DetallePedidoServicio(
        IPedidoRepositorio pedidos,
        IClienteRepositorio clientes,
        ILineaPedidoRepositorio lineas)
    {
        _pedidos = pedidos;
        _clientes = clientes;
        _lineas = lineas;
    }

    /// <summary>El detalle del pedido, o null si el pedido no existe.</summary>
    public DetallePedido? Obtener(int id)
    {
        var pedido = _pedidos.ObtenerPorId(id);
        if (pedido is null)
            return null;

        var lineas = _lineas.ObtenerPorPedido(id)
            .Select(l => new LineaDetalle(l.Numero, l.Producto, l.Cantidad, l.PrecioUnitario, l.Cantidad * l.PrecioUnitario))
            .ToList();

        return new DetallePedido(
            pedido,
            _clientes.ObtenerPorId(pedido.ClienteId),
            lineas,
            lineas.Sum(l => l.Subtotal));
    }
}
