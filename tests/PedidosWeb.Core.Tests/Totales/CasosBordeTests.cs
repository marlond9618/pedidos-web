using PedidosWeb.Core.Totales;

namespace PedidosWeb.Core.Tests.Totales;

/// <summary>
/// Casos de borde que el golden master (migracion/golden-master.csv) no cubre.
/// Complementan el contrato: cada uno ejercita una rama o defecto distinto de
/// CalculoTotales.Calcular. No usan el CSV; llaman directo al cálculo.
/// </summary>
public class CasosBordeTests
{
    // Caso 1: el guard lineas.Count == 0 => flete 0 (UCalculoTotales.pas:116-117).
    // El golden master siempre trae entre 1 y 5 líneas, nunca un pedido vacío.
    [Fact]
    public void Calcular_PedidoVacio_FleteYTotalesEnCero()
    {
        var r = CalculoTotales.Calcular(Array.Empty<LineaCalculo>(),
            esVip: false, region: 11, fleteBase: 10000m, tasaIva: 0.19m);

        Assert.Equal(0m, r.Subtotal);
        Assert.Equal(0m, r.BaseDescuento);
        Assert.Equal(0m, r.PorcDescuento);
        Assert.Equal(0m, r.Descuento);
        Assert.Equal(0m, r.Flete); // flete 0 pese a región 11 (recargo) y flete base 10.000
        Assert.Equal(0m, r.Neto);
        Assert.Equal(0m, r.Iva);
        Assert.Equal(0m, r.Total);
    }

    // Caso 2: Subtotal, Neto y Total NO se redondean; solo descuento, flete e IVA.
    // Con un precio no entero, el total arrastra centavos. Todos los precios del CSV son enteros.
    [Fact]
    public void Calcular_PrecioConDecimales_SubtotalNetoYTotalArrastranCentavos()
    {
        var lineas = new[] { new LineaCalculo(3, 1500.50m, "CON") };

        var r = CalculoTotales.Calcular(lineas,
            esVip: false, region: 5, fleteBase: 4900m, tasaIva: 0.19m);

        Assert.Equal(4501.50m, r.Subtotal);      // 3 × 1500,50, sin redondear
        Assert.Equal(4501.50m, r.BaseDescuento);
        Assert.Equal(0m, r.PorcDescuento);
        Assert.Equal(0m, r.Descuento);
        Assert.Equal(4900m, r.Flete);
        Assert.Equal(4501.50m, r.Neto);
        Assert.Equal(1786m, r.Iva);              // round(9401,50 × 0,19) = round(1786,285)
        Assert.Equal(11187.50m, r.Total);        // total con centavos pese a "CLP sin decimales"
    }

    // Caso 3: las líneas OFE se excluyen de la base pero suman al subtotal/neto/total.
    // Un pedido 100% OFE paga 0 de descuento aunque el volumen sea alto. No hay pedido todo-OFE en el CSV.
    [Fact]
    public void Calcular_PedidoTodoOferta_DescuentoCeroPeseAlVolumen()
    {
        var lineas = new[] { new LineaCalculo(50, 50000m, "OFE") };

        var r = CalculoTotales.Calcular(lineas,
            esVip: false, region: 5, fleteBase: 4900m, tasaIva: 0.19m);

        Assert.Equal(2500000m, r.Subtotal);
        Assert.Equal(0m, r.BaseDescuento);       // todo OFE => base 0
        Assert.Equal(0m, r.PorcDescuento);
        Assert.Equal(0m, r.Descuento);           // 0 descuento pese a 2,5 millones
        Assert.Equal(4900m, r.Flete);
        Assert.Equal(2500000m, r.Neto);
        Assert.Equal(475931m, r.Iva);
        Assert.Equal(2980831m, r.Total);
    }

    // Caso 4: comparación estricta > en el umbral, con VIP. Base = 1.000.000 NO es > 1.000.000,
    // así que es tramo 5% + 3% VIP = 8% (no 10/11%). Es el punto donde el motor (>) diverge del
    // grabado (>=), el hallazgo H1. El único caso con base 1.000.000 en el CSV (folio 1042) es no-VIP.
    [Fact]
    public void Calcular_VipConBaseExacta1Millon_PorcentajeOchoPorCientoPorComparacionEstricta()
    {
        var lineas = new[] { new LineaCalculo(40, 25000m, "ABA") };

        var r = CalculoTotales.Calcular(lineas,
            esVip: true, region: 5, fleteBase: 4900m, tasaIva: 0.19m);

        Assert.Equal(1000000m, r.Subtotal);
        Assert.Equal(1000000m, r.BaseDescuento);
        Assert.Equal(0.08m, r.PorcDescuento);    // 5% (volumen, con >) + 3% (VIP); no se topa en 10%
        Assert.Equal(80000m, r.Descuento);
        Assert.Equal(920000m, r.Neto);
        Assert.Equal(4900m, r.Flete);
        Assert.Equal(175731m, r.Iva);
        Assert.Equal(1100631m, r.Total);
    }

    // Caso 5: la exclusión es por coincidencia EXACTA de 'OFE' (tras Trim + mayúsculas).
    // 'OFERTA' no es 'OFE', así que entra a la base y genera descuento. Blinda contra un
    // "contiene OFE" ingenuo. El CSV solo trae 'OFE' exacto.
    [Fact]
    public void Calcular_CategoriaMalEscritaOferta_NoSeExcluyeDeLaBase()
    {
        var lineas = new[] { new LineaCalculo(20, 30000m, "OFERTA") };

        var r = CalculoTotales.Calcular(lineas,
            esVip: false, region: 5, fleteBase: 4900m, tasaIva: 0.19m);

        Assert.Equal(600000m, r.Subtotal);
        Assert.Equal(600000m, r.BaseDescuento);  // 'OFERTA' NO se excluye
        Assert.Equal(0.05m, r.PorcDescuento);
        Assert.Equal(30000m, r.Descuento);
        Assert.Equal(570000m, r.Neto);
        Assert.Equal(4900m, r.Flete);
        Assert.Equal(109231m, r.Iva);
        Assert.Equal(684131m, r.Total);
    }
}
