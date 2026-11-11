namespace PedidosWeb.Core.Totales;

/// <summary>Una línea del pedido, como TLineaCalculo en legado/src/UCalculoTotales.pas.</summary>
public record LineaCalculo(int Cantidad, decimal PrecioUnitario, string Categoria);

/// <summary>Resultado del cálculo, como TResultadoTotales en el legado.</summary>
public record ResultadoTotales(decimal Subtotal, decimal BaseDescuento, decimal PorcDescuento,
    decimal Descuento, decimal Flete, decimal Neto, decimal Iva, decimal Total);

/// <summary>
/// Migración fiel de legado/src/UCalculoTotales.pas (Delphi). Replica el cálculo EXACTO,
/// defectos incluidos, para igualar migracion/golden-master.csv. Dinero en decimal (CLP).
/// Diferencia con el legado: allá todo era Double; aquí es decimal, aritmética exacta.
/// </summary>
public static class CalculoTotales
{
    /// <summary>Redondeo al peso del legado: Trunc(valor + 0.49). Ojo: NO es redondeo al medio;
    /// un valor terminado en .50 baja (Trunc(x.99)), solo sube desde .51. (UCalculoTotales.pas:48-51)</summary>
    private static decimal Calc3(decimal valor) => Math.Truncate(valor + 0.49m);

    /// <summary>Suma Cantidad*PrecioUnitario de TODAS las líneas, incluidas 'OFE'. Sin redondeo. (:53-60)</summary>
    private static decimal CalcularSubtotal(IReadOnlyList<LineaCalculo> lineas)
    {
        decimal resultado = 0m;
        foreach (var l in lineas)
            resultado += l.Cantidad * l.PrecioUnitario;
        return resultado;
    }

    /// <summary>Suma solo las líneas cuya categoría (UpperCase+Trim) no sea 'OFE'. Sin redondeo. (:63-73)</summary>
    private static decimal CalcularBaseDescuento(IReadOnlyList<LineaCalculo> lineas)
    {
        decimal resultado = 0m;
        foreach (var l in lineas)
        {
            if ((l.Categoria ?? string.Empty).Trim().ToUpperInvariant() != "OFE")
                resultado += l.Cantidad * l.PrecioUnitario;
        }
        return resultado;
    }

    /// <summary>Tramos por volumen con comparación estricta (>): &gt;1.000.000 =&gt; 0.08; &gt;500.000 =&gt; 0.05; resto 0. (:75-83)</summary>
    private static decimal PorcentajeVolumen(decimal baseDescuento)
    {
        if (baseDescuento > 1000000m)
            return 0.08m;
        if (baseDescuento > 500000m)
            return 0.05m;
        return 0m;
    }

    /// <summary>Volumen + 3 puntos si es VIP, con tope en 0.10 (recorta solo si &gt; 0.10). (:85-92)</summary>
    private static decimal PorcentajeDescuento(decimal baseDescuento, bool esVip)
    {
        var resultado = PorcentajeVolumen(baseDescuento);
        if (esVip)
            resultado += 0.03m;
        if (resultado > 0.10m)
            resultado = 0.10m;
        return resultado;
    }

    /// <summary>Flete por región: 11 o 12 recargan 15%; otras, el base. Ambos con Calc3. (:95-101)</summary>
    private static decimal CalcularFlete(decimal fleteBase, int region)
    {
        if (region == 11 || region == 12)
            return Calc3(fleteBase * 1.15m);
        return Calc3(fleteBase);
    }

    /// <summary>IVA sobre (Neto + Flete), con Calc3. (:103-106)</summary>
    private static decimal CalcularIva(decimal neto, decimal flete, decimal tasa)
        => Calc3((neto + flete) * tasa);

    /// <summary>
    /// Calcula los totales del pedido igual que CalcularTotales en UCalculoTotales.pas (:108-122).
    /// El orden importa: Descuento, Flete e IVA se truncan como componentes ANTES de combinarse;
    /// Subtotal, Neto y Total NO se truncan.
    /// </summary>
    public static ResultadoTotales Calcular(IReadOnlyList<LineaCalculo> lineas, bool esVip,
        int region, decimal fleteBase, decimal tasaIva)
    {
        var subtotal = CalcularSubtotal(lineas);                       // :111
        var baseDescuento = CalcularBaseDescuento(lineas);            // :112
        var porcDescuento = PorcentajeDescuento(baseDescuento, esVip); // :113
        var descuento = Calc3(baseDescuento * porcDescuento);         // :114 (redondea aquí)
        var neto = subtotal - descuento;                              // :115 (sin redondeo)
        var flete = lineas.Count == 0                                 // :116-119
            ? 0m
            : CalcularFlete(fleteBase, region);
        var iva = CalcularIva(neto, flete, tasaIva);                  // :120 (redondea)
        var total = neto + flete + iva;                              // :121 (sin redondeo)

        return new ResultadoTotales(subtotal, baseDescuento, porcDescuento,
            descuento, flete, neto, iva, total);
    }
}
