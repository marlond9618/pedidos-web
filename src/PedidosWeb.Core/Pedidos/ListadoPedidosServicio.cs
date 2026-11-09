using System.Globalization;
using System.Text.RegularExpressions;

namespace PedidosWeb.Core.Pedidos;

/// <summary>
/// HU-01 · Listar pedidos con filtros por cliente, estado y rango de fechas, con orden y paginación.
/// Ver specs/HU-01-listar-pedidos/spec.md.
/// </summary>
public sealed partial class ListadoPedidosServicio(IPedidoRepositorio repositorio)
{
    public const int TamanoPaginaPorDefecto = 20;
    public const int TamanoPaginaMaximo = 100;

    /// <summary>Lista cerrada de estados válidos (RN5), en el orden del mensaje de error.</summary>
    public static readonly IReadOnlyList<string> EstadosValidos =
        ["Pendiente", "Confirmado", "Despachado", "Entregado", "Anulado"];

    // RN3: forma de código de cliente "C-NNN" (exactamente 3 dígitos; sin distinguir mayúsculas).
    [GeneratedRegex("^[Cc]-[0-9]{3}$")]
    private static partial Regex FormaCodigoCliente();

    public ResultadoListado Listar(ConsultaPedidos consulta)
    {
        // Orden de validación de la spec: estado → desde → hasta → desde > hasta → pagina → tamanoPagina → última página.
        string? estado = null;
        if (EstaIndicado(consulta.Estado))
        {
            estado = EstadosValidos.FirstOrDefault(e => string.Equals(e, consulta.Estado, StringComparison.OrdinalIgnoreCase));
            if (estado is null)
                return ResultadoListado.Fallo(
                    $"El estado '{consulta.Estado}' no es válido. Valores permitidos: {string.Join(", ", EstadosValidos)}.");
        }

        DateOnly? desde = null;
        if (EstaIndicado(consulta.Desde))
        {
            if (!IntentarLeerFecha(consulta.Desde!, out var fecha))
                return ResultadoListado.Fallo("La fecha 'desde' debe tener el formato yyyy-MM-dd.");
            desde = fecha;
        }

        DateOnly? hasta = null;
        if (EstaIndicado(consulta.Hasta))
        {
            if (!IntentarLeerFecha(consulta.Hasta!, out var fecha))
                return ResultadoListado.Fallo("La fecha 'hasta' debe tener el formato yyyy-MM-dd.");
            hasta = fecha;
        }

        if (desde > hasta)
            return ResultadoListado.Fallo("La fecha 'desde' no puede ser posterior a 'hasta'.");

        var pagina = 1;
        if (EstaIndicado(consulta.Pagina) && (!IntentarLeerEntero(consulta.Pagina!, out pagina) || pagina < 1))
            return ResultadoListado.Fallo("'pagina' debe ser mayor o igual a 1.");

        var tamanoPagina = TamanoPaginaPorDefecto;
        if (EstaIndicado(consulta.TamanoPagina)
            && (!IntentarLeerEntero(consulta.TamanoPagina!, out tamanoPagina) || tamanoPagina is < 1 or > TamanoPaginaMaximo))
            return ResultadoListado.Fallo($"'tamanoPagina' debe estar entre 1 y {TamanoPaginaMaximo}.");

        var filtrados = repositorio.ObtenerTodos()
            .Where(p => CumpleCliente(p, consulta.Cliente))
            .Where(p => estado is null || string.Equals(p.Estado, estado, StringComparison.OrdinalIgnoreCase))
            .Where(p => desde is null || p.Fecha >= desde)
            .Where(p => hasta is null || p.Fecha <= hasta)
            .OrderBy(p => p.Fecha)
            .ThenBy(p => p.Id)
            .ToList();

        var total = filtrados.Count;
        // Con total = 0 la página 1 existe (RN8), por eso el mínimo es 1.
        var paginas = Math.Max(1, (total + tamanoPagina - 1) / tamanoPagina);
        if (pagina > paginas)
            return ResultadoListado.Fallo($"La página {pagina} no existe. Hay {paginas} página(s).");

        var items = filtrados
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();

        return ResultadoListado.Exito(new PaginaPedidos(items, total, pagina, tamanoPagina));
    }

    // Supuesto pendiente de confirmar con el PO: un parámetro vacío equivale a no indicarlo.
    private static bool EstaIndicado(string? valor) => !string.IsNullOrEmpty(valor);

    private static bool CumpleCliente(Pedido pedido, string? cliente)
    {
        if (!EstaIndicado(cliente))
            return true;

        // RN3: si tiene forma de código, ClienteId exacto; si no, texto parcial del nombre.
        // OrdinalIgnoreCase ignora mayúsculas (incluida Ñ/ñ) pero no las tildes.
        return FormaCodigoCliente().IsMatch(cliente!)
            ? string.Equals(pedido.ClienteId, cliente, StringComparison.OrdinalIgnoreCase)
            : pedido.ClienteNombre.Contains(cliente!, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IntentarLeerFecha(string texto, out DateOnly fecha) =>
        DateOnly.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);

    // Sin espacios, signos ni decimales: cualquier otra cosa no es un entero válido (RN8, CA28).
    private static bool IntentarLeerEntero(string texto, out int numero) =>
        int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out numero);
}
