using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PedidosWeb.Api.Tests.Pedidos;

/// <summary>
/// HU-01 · Listar pedidos con filtros: criterios que se verifican por HTTP (códigos y cuerpo { "error" }).
/// Los mensajes se comparan literalmente, como pide la spec.
/// </summary>
public class ListarPedidosEndpointTests(WebApplicationFactory<Program> fabrica)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _cliente = fabrica.CreateClient();

    private async Task<(HttpStatusCode Codigo, JsonElement Cuerpo)> Get(string query)
    {
        var respuesta = await _cliente.GetAsync($"/api/pedidos{query}");
        var texto = await respuesta.Content.ReadAsStringAsync();
        return (respuesta.StatusCode, JsonDocument.Parse(texto).RootElement.Clone());
    }

    private async Task AfirmarError(string query, string mensajeEsperado)
    {
        var (codigo, cuerpo) = await Get(query);

        Assert.Equal(HttpStatusCode.BadRequest, codigo);
        Assert.Equal(mensajeEsperado, cuerpo.GetProperty("error").GetString());
        Assert.Single(cuerpo.EnumerateObject());
    }

    [Fact] // Ejemplo de entrada y salida de la spec.
    public async Task ListarPedidos_EjemploDeLaSpec_DevuelveLaFormaJsonEsperada()
    {
        var (codigo, cuerpo) = await Get("?cliente=C-001&estado=Entregado&desde=2026-10-01");

        Assert.Equal(HttpStatusCode.OK, codigo);
        Assert.Equal(1, cuerpo.GetProperty("total").GetInt32());
        Assert.Equal(1, cuerpo.GetProperty("pagina").GetInt32());
        Assert.Equal(20, cuerpo.GetProperty("tamanoPagina").GetInt32());
        var item = Assert.Single(cuerpo.GetProperty("items").EnumerateArray());
        Assert.Equal(8, item.GetProperty("id").GetInt32());
        Assert.Equal("PED-STGO-000008", item.GetProperty("folio").GetString());
        Assert.Equal("C-001", item.GetProperty("clienteId").GetString());
        Assert.Equal("Ferretería El Clavo", item.GetProperty("clienteNombre").GetString());
        Assert.Equal("2026-10-20", item.GetProperty("fecha").GetString());
        Assert.Equal("Entregado", item.GetProperty("estado").GetString());
        Assert.Equal(120100m, item.GetProperty("total").GetDecimal());
        Assert.Equal("STGO", item.GetProperty("sucursal").GetString());
    }

    [Fact] // CA14
    public async Task ListarPedidos_EstadoInvalido_Responde400ConMensaje()
    {
        await AfirmarError("?estado=Perdido",
            "El estado 'Perdido' no es válido. Valores permitidos: Pendiente, Confirmado, Despachado, Entregado, Anulado.");
    }

    [Fact] // CA15
    public async Task ListarPedidos_DesdePosteriorAHasta_Responde400ConMensaje()
    {
        await AfirmarError("?desde=2026-10-31&hasta=2026-10-01",
            "La fecha 'desde' no puede ser posterior a 'hasta'.");
    }

    [Theory] // CA16
    [InlineData("?desde=31-10-2026", "La fecha 'desde' debe tener el formato yyyy-MM-dd.")]
    [InlineData("?desde=2026/10/01", "La fecha 'desde' debe tener el formato yyyy-MM-dd.")]
    [InlineData("?desde=2026-13-01", "La fecha 'desde' debe tener el formato yyyy-MM-dd.")]
    [InlineData("?hasta=31-10-2026", "La fecha 'hasta' debe tener el formato yyyy-MM-dd.")]
    public async Task ListarPedidos_FechaConFormatoInvalido_Responde400ConMensaje(string query, string mensaje)
    {
        await AfirmarError(query, mensaje);
    }

    [Theory] // CA24
    [InlineData("?pagina=0", "'pagina' debe ser mayor o igual a 1.")]
    [InlineData("?tamanoPagina=0", "'tamanoPagina' debe estar entre 1 y 100.")]
    [InlineData("?tamanoPagina=101", "'tamanoPagina' debe estar entre 1 y 100.")]
    public async Task ListarPedidos_PaginacionFueraDeRango_Responde400ConMensaje(string query, string mensaje)
    {
        await AfirmarError(query, mensaje);
    }

    [Fact] // CA25
    public async Task ListarPedidos_PaginaPosteriorALaUltima_Responde400ConMensaje()
    {
        await AfirmarError("?pagina=4", "La página 4 no existe. Hay 3 página(s).");
    }

    [Fact] // CA26
    public async Task ListarPedidos_SinResultadosYPagina2_Responde400ConUnaPagina()
    {
        await AfirmarError("?cliente=C-999&pagina=2", "La página 2 no existe. Hay 1 página(s).");
    }

    [Fact] // CA27
    public async Task ListarPedidos_EstadoInvalidoYPagina0_InformaSoloElEstado()
    {
        await AfirmarError("?estado=Perdido&pagina=0",
            "El estado 'Perdido' no es válido. Valores permitidos: Pendiente, Confirmado, Despachado, Entregado, Anulado.");
    }

    [Theory] // CA28
    [InlineData("?pagina=abc", "'pagina' debe ser mayor o igual a 1.")]
    [InlineData("?tamanoPagina=2.5", "'tamanoPagina' debe estar entre 1 y 100.")]
    public async Task ListarPedidos_PaginacionNoEntera_Responde400ComoFueraDeRango(string query, string mensaje)
    {
        await AfirmarError(query, mensaje);
    }
}
