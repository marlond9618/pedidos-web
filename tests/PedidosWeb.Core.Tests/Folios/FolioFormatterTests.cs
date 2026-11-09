using PedidosWeb.Core.Folios;

namespace PedidosWeb.Core.Tests.Folios;

public class FolioFormatterTests
{
    // Casos base del README (tres primeras filas de la tabla de ejemplos).
    // Los demás casos (espacios, mayúsculas, rangos, sucursales inválidas) todavía no tienen test:
    // escribirlos es parte del taller.
    [Theory]
    [InlineData(123, "STGO", "PED-STGO-000123")]
    [InlineData(1, "VAL", "PED-VAL-000001")]
    [InlineData(999999, "CO", "PED-CO-999999")]
    public void FormatearFolio_CasosBase_DevuelveElFolioEsperado(int numero, string sucursal, string esperado)
    {
        var folio = FolioFormatter.FormatearFolio(numero, sucursal);

        Assert.Equal(esperado, folio);
    }

    [Theory]
    [InlineData(45, " stgo ", "PED-STGO-000045")]
    public void FormatearFolio_SucursalConEspaciosYMinusculas_SeNormaliza(int numero, string sucursal, string esperado)
    {
        Assert.Equal(esperado, FolioFormatter.FormatearFolio(numero, sucursal));
    }

    [Theory]
    [InlineData(0, "STGO")]
    [InlineData(1000000, "STGO")]
    [InlineData(-5, "VAL")]
    public void FormatearFolio_NumeroFueraDeRango_LanzaArgumentOutOfRange(int numero, string sucursal)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FolioFormatter.FormatearFolio(numero, sucursal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("S")]
    [InlineData("SANTI")]
    [InlineData("ÑUÑO")]
    [InlineData("ST1")]
    [InlineData("S GO")]
    public void FormatearFolio_SucursalInvalida_LanzaArgumentException(string? sucursal)
    {
        Assert.ThrowsAny<ArgumentException>(() => FolioFormatter.FormatearFolio(7, sucursal!));
    }

    [Fact]
    public void FormatearFolio_NumeroYSucursalInvalidos_ValidaPrimeroElNumero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FolioFormatter.FormatearFolio(0, "S"));
    }
}
