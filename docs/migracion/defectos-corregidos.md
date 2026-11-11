# Defectos corregidos · migración del cálculo de totales

## D1 · Regla 8 / RS-11 (R05) · Redondeo al peso (CORREGIDO)

- **Qué hace el legado:** `Calc3(valor) = Trunc(valor + 0.49)` redondea el 0,50 hacia **abajo**
  (10,50 → 10), al revés de lo que dicen sus comentarios «redondeo al peso»
  (`legado/src/UCalculoTotales.pas:48-51`; duplicado como `RedondeaPesos` en `legado/src/UUtilidades.pas:30`).
  Se aplica a descuento, flete e IVA.
- **Corrección:** `RedondearAlPeso` usa redondeo al peso correcto, medio hacia arriba
  (`Math.Round(valor, 0, MidpointRounding.AwayFromZero)`) en
  `src/PedidosWeb.Core/Totales/CalculoTotales.cs`. Es la **única** desviación respecto del legado.
- **Por qué:** el sesgo hacía que el cliente pagara 1–2 pesos distintos (de más o de menos) en
  pedidos donde el descuento, el flete o el IVA caían exactamente en x,50. Un «redondeo al peso»
  debe tratar el 0,50 de forma estándar, no sistemáticamente a la baja.
- **Casos del golden master que cambian:** 13 de 60. Folios **1002, 1006, 1008, 1018, 1021, 1023,
  1025, 1027, 1036, 1037, 1042, 1046, 1054**.

  | Folio | Componente que sube (x,50 → +1) | Efecto en `total` |
  |---|---|---|
  | 1002 | IVA | +1 |
  | 1006 | descuento (baja el neto) | −1 |
  | 1008 | IVA | +1 |
  | 1018 | flete | +1 |
  | 1021 | flete e IVA | +2 |
  | 1023 | flete | +1 |
  | 1025 | descuento (baja neto e IVA) | −2 |
  | 1027 | flete | +1 |
  | 1036 | flete | +1 |
  | 1037 | flete | +1 |
  | 1042 | flete | +1 |
  | 1046 | flete | +1 |
  | 1054 | flete | +1 |

- **Suma de la columna total:** 58.202.135 → 58.202.144 (**+9** en 60 pedidos).
- **Commit:** `fix: corrige R05 (redondeo al peso) y actualiza el golden master` — aparte del
  commit «igual al legado» (la migración fiel quedó en `f0517af`).
- **Convivencia:** mientras el legado (Delphi) y el portal coexistan, los totales diferirán hasta
  en 2 pesos por pedido en esos 13 casos. Contabilidad debe aceptar la diferencia o portar la misma
  corrección a `UUtilidades.pas:30` / `UCalculoTotales.pas:50` para que ambos sistemas coincidan.

## H1 · Umbrales de descuento duplicados y comentario erróneo (HALLAZGO, NO CORREGIDO)

- **Qué encontraste:** la regla de descuento por volumen está **duplicada con valores distintos**.
  El motor de cálculo usa comparación **estricta** `>` (`legado/src/UCalculoTotales.pas:77-82`:
  base > 1.000.000 → 8 %; base > 500.000 → 5 %), pero al grabar el pedido se usa `>=`
  (`legado/src/UFrmPedidos.pas:296-301`). Además, el comentario de cabecera
  (`legado/src/UCalculoTotales.pas:4`) dice «5 % sobre 400.000», umbral que **no existe** en el código.
- **Efecto:** con base **exacta** de 500.000 o 1.000.000, el total calculado (`>`) difiere del
  grabado (`>=`). En el golden master se ve el borde: folio **1008** (base 500.000) obtiene 0 % con `>`
  —con `>=` tendría 5 %—, y folio **1042** (base 1.000.000) obtiene 5 % —con `>=` tendría 8 %—.
  Son ~2 casos de 60, pero afectan el descuento completo del pedido.
- **Quién decide:** Comercial / Contabilidad (qué umbral y qué operador son los correctos, según el
  acta comercial real). Mientras tanto, el C# **replica la versión de `UCalculoTotales.pas`** (`>`,
  umbrales 500.000 y 1.000.000), que es el motor que generó el golden master. No se toca hasta que el
  negocio confirme la regla única.
