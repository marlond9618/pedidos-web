using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PedidosWeb.Api.Tests.Pedidos;

/// <summary>HU-02 · Detalle de un pedido: criterios que se verifican por HTTP.</summary>
public class DetallePedidoEndpointTests(WebApplicationFactory<Program> fabrica)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _cliente = fabrica.CreateClient();

    private async Task<(HttpStatusCode Codigo, JsonElement Cuerpo)> Get(int id)
    {
        var respuesta = await _cliente.GetAsync($"/api/pedidos/{id}/detalle");
        var texto = await respuesta.Content.ReadAsStringAsync();
        var cuerpo = texto.Length == 0 ? default : JsonDocument.Parse(texto).RootElement.Clone();
        return (respuesta.StatusCode, cuerpo);
    }

    [Fact] // CA-01 y forma JSON del ejemplo de la spec.
    public async Task DetallePedido_Pedido8_DevuelveLosDatosDelPedido()
    {
        var (codigo, d) = await Get(8);

        Assert.Equal(HttpStatusCode.OK, codigo);
        Assert.Equal(8, d.GetProperty("id").GetInt32());
        Assert.Equal("PED-STGO-000008", d.GetProperty("folio").GetString());
        Assert.Equal("2026-10-20", d.GetProperty("fecha").GetString());
        Assert.Equal("Entregado", d.GetProperty("estado").GetString());
        Assert.Equal("STGO", d.GetProperty("sucursal").GetString());
        Assert.Equal(
            new[] { "id", "folio", "fecha", "estado", "sucursal", "cliente", "lineas", "total" },
            d.EnumerateObject().Select(p => p.Name));
    }

    [Fact] // CA-02
    public async Task DetallePedido_Pedido8_DevuelveLaFichaDelCliente()
    {
        var (_, d) = await Get(8);
        var c = d.GetProperty("cliente");

        Assert.Equal("C-001", c.GetProperty("id").GetString());
        Assert.Equal("Ferretería El Clavo", c.GetProperty("nombre").GetString());
        Assert.Equal("76.123.456-0", c.GetProperty("rut").GetString());
        Assert.Equal("RM", c.GetProperty("region").GetString());
        Assert.True(c.GetProperty("activo").GetBoolean());
    }

    [Fact] // CA-03
    public async Task DetallePedido_Pedido8_DevuelveTresLineasConSubtotal()
    {
        var (_, d) = await Get(8);
        var lineas = d.GetProperty("lineas").EnumerateArray().ToList();

        Assert.Equal(3, lineas.Count);
        Assert.Equal(new[] { 1, 2, 3 }, lineas.Select(l => l.GetProperty("numero").GetInt32()));
        Assert.Equal("Harina para pan 25 kg", lineas[0].GetProperty("producto").GetString());
        Assert.Equal(3, lineas[0].GetProperty("cantidad").GetInt32());
        Assert.Equal(21500, lineas[0].GetProperty("precioUnitario").GetDecimal());
        Assert.Equal(64500, lineas[0].GetProperty("subtotal").GetDecimal());
    }

    [Fact] // CA-04
    public async Task DetallePedido_Pedido8_TotalEs120100()
    {
        var (_, d) = await Get(8);

        Assert.Equal(120100, d.GetProperty("total").GetDecimal());
    }

    [Fact] // CA-06
    public async Task DetallePedido_Pedido6ClienteInactivo_DevuelveActivoFalso()
    {
        var (codigo, d) = await Get(6);

        Assert.Equal(HttpStatusCode.OK, codigo);
        Assert.False(d.GetProperty("cliente").GetProperty("activo").GetBoolean());
    }

    [Fact] // CA-07
    public async Task DetallePedido_IdInexistente_Responde404SinCuerpo()
    {
        var respuesta = await _cliente.GetAsync("/api/pedidos/999/detalle");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("", await respuesta.Content.ReadAsStringAsync());
    }
}
