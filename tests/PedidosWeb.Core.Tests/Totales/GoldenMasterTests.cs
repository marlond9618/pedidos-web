using System.Globalization;
using PedidosWeb.Core.Totales;

namespace PedidosWeb.Core.Tests.Totales;

/// <summary>Un test por cada caso de migracion/golden-master.csv (el contrato de la migración).</summary>
public class GoldenMasterTests
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private static string RutaGoldenMaster()
    {
        // Sube desde el directorio de salida hasta encontrar migracion/golden-master.csv en la raíz del repo.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidato = Path.Combine(dir.FullName, "migracion", "golden-master.csv");
            if (File.Exists(candidato)) return candidato;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("No se encontró migracion/golden-master.csv subiendo desde " + AppContext.BaseDirectory);
    }

    public static IEnumerable<object[]> Casos() =>
        File.ReadLines(RutaGoldenMaster())
            .Skip(1)
            .Where(fila => !string.IsNullOrWhiteSpace(fila))
            .Select(fila => new object[] { int.Parse(fila.Split(',')[1], Inv), fila });

    [Theory]
    [MemberData(nameof(Casos))]
    public void Totales_IgualAlGoldenMaster_CoincideDescuentoFleteIvaTotal(int folio, string fila)
    {
        // Columnas: caso,folio,tipo_cliente,region,flete_base,tasa_iva,lineas,subtotal,
        //           base_descuento,porc_descuento,descuento,flete,neto,iva,total
        var c = fila.Split(',');
        Assert.Equal(folio, int.Parse(c[1], Inv)); // el folio del caso corresponde a su fila
        var lineas = c[6].Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Split('x'))
            .Select(p => new LineaCalculo(int.Parse(p[0], Inv), decimal.Parse(p[1], Inv), p[2]))
            .ToList();

        var r = CalculoTotales.Calcular(lineas, c[2] == "V", int.Parse(c[3], Inv),
            decimal.Parse(c[4], Inv), decimal.Parse(c[5], Inv));

        Assert.Equal(decimal.Parse(c[10], Inv), r.Descuento); // descuento
        Assert.Equal(decimal.Parse(c[11], Inv), r.Flete);     // flete
        Assert.Equal(decimal.Parse(c[13], Inv), r.Iva);       // IVA
        Assert.Equal(decimal.Parse(c[14], Inv), r.Total);     // total
    }
}
