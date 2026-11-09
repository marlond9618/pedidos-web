namespace PedidosWeb.Core.Folios;

/// <summary>
/// Genera el folio de un pedido con el mismo formato del sistema legado.
/// Las reglas completas y los ejemplos están en el README, sección «La función del taller».
/// </summary>
public static class FolioFormatter
{
    /// <summary>
    /// Devuelve el folio con el formato PED-{SUCURSAL}-{NUMERO}, por ejemplo "PED-STGO-000123".
    /// </summary>
    /// <param name="numero">Número correlativo del pedido, entre 1 y 999999.</param>
    /// <param name="sucursal">Código de la sucursal: de 2 a 4 letras de la A a la Z.</param>
    public static string FormatearFolio(int numero, string sucursal)
    {
        // R1 (y R5: se valida primero el número)
        if (numero < 1 || numero > 999999)
            throw new ArgumentOutOfRangeException(nameof(numero), numero,
                "El número de pedido debe estar entre 1 y 999999.");

        // R3: null, vacía o solo espacios
        if (string.IsNullOrWhiteSpace(sucursal))
            throw new ArgumentException("La sucursal es obligatoria.", nameof(sucursal));

        // R2: sin espacios en los extremos y en mayúsculas (invariant: evita sorpresas de cultura)
        var codigo = sucursal.Trim().ToUpperInvariant();

        // R3: de 2 a 4 letras ASCII A–Z (rechaza Ñ, tildes, números y espacios internos)
        if (codigo.Length is < 2 or > 4 || !codigo.All(c => c is >= 'A' and <= 'Z'))
            throw new ArgumentException(
                "La sucursal debe tener de 2 a 4 letras de la A a la Z (sin Ñ, tildes, números ni espacios).",
                nameof(sucursal));

        // R4
        return $"PED-{codigo}-{numero:D6}";
    }
}
