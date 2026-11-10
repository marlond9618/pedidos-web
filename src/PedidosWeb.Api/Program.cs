using PedidosWeb.Core.Clientes;
using PedidosWeb.Core.Lineas;
using PedidosWeb.Core.Pedidos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IPedidoRepositorio, PedidoRepositorioEnMemoria>();
builder.Services.AddSingleton<ListadoPedidosServicio>();
builder.Services.AddSingleton<IClienteRepositorio, ClienteRepositorioEnMemoria>();
builder.Services.AddSingleton<ILineaPedidoRepositorio, LineaPedidoRepositorioEnMemoria>();
builder.Services.AddSingleton<DetallePedidoServicio>();

var app = builder.Build();

// Para comprobar que la API está arriba.
app.MapGet("/salud", () => Results.Ok(new { estado = "ok" }));

// Detalle de un pedido por id.
app.MapGet("/api/pedidos/{id:int}", (int id, IPedidoRepositorio repositorio) =>
    repositorio.ObtenerPorId(id) is { } pedido
        ? Results.Ok(pedido)
        : Results.NotFound());

// HU-02 · Detalle de un pedido con su cliente, sus líneas y su total (specs/HU-02-detalle-pedido/spec.md).
app.MapGet("/api/pedidos/{id:int}/detalle", (int id, DetallePedidoServicio servicio) =>
    servicio.Obtener(id) is { } d
        ? Results.Ok(new
        {
            id = d.Pedido.Id,
            folio = d.Pedido.Folio,
            fecha = d.Pedido.Fecha,
            estado = d.Pedido.Estado,
            sucursal = d.Pedido.Sucursal,
            cliente = d.Cliente is null ? null : new
            {
                id = d.Cliente.Id,
                nombre = d.Cliente.Nombre,
                rut = d.Cliente.Rut,
                region = d.Cliente.Region,
                activo = d.Cliente.Activo,
            },
            lineas = d.Lineas,
            total = d.Total,
        })
        : Results.NotFound());

// HU-01 · Listar pedidos con filtros (specs/HU-01-listar-pedidos/spec.md).
// Los parámetros se reciben como texto para que la validación (y sus mensajes exactos) la haga el dominio;
// si fueran int o DateOnly, Minimal API respondería su propio 400.
app.MapGet("/api/pedidos", (
    string? cliente,
    string? estado,
    string? desde,
    string? hasta,
    string? pagina,
    string? tamanoPagina,
    ListadoPedidosServicio servicio) =>
{
    var resultado = servicio.Listar(new ConsultaPedidos(cliente, estado, desde, hasta, pagina, tamanoPagina));
    return resultado.EsValido
        ? Results.Ok(resultado.Pagina)
        : Results.BadRequest(new { error = resultado.Error });
});

app.Run();

// Permite crear tests de integración con WebApplicationFactory<Program> más adelante.
public partial class Program { }
