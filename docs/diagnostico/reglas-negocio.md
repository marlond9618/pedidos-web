# Reglas de negocio de Pedidos Legado

Rutas relativas a `legado/src/`. «Verificada: Sí» = abrí el archivo en esa línea y leí el código. «No» = la reporta el subagente `analista-legado` y no la he abierto yo. No me fío de los comentarios del código: lo que importa es lo que ejecuta.

| # | Regla | Archivo:línea | Condición | Efecto | Verificada |
|---|---|---|---|---|---|
| 1 | Descuento por volumen (motor de totales) | `UCalculoTotales.pas:77-83` | Base > 1.000.000 → 8 %; base > 500.000 → 5 %; si no, 0 %. Comparación estricta (`>`). | Porcentaje de descuento. **Duplicada con valores distintos** en la regla 2. El comentario de `UCalculoTotales.pas:4` dice «5 % sobre 400.000», lo contradice el código. | Sí |
| 2 | Descuento por volumen (al grabar el pedido) | `UFrmPedidos.pas:296-301` | Base >= 1.000.000 → 8 %; base >= 500.000 → 5 %; si no, 0 %. Comparación con `>=`. | Con base exacta de 500.000 o 1.000.000 el total grabado difiere del calculado en `UCalculoTotales`. | Sí |
| 3 | Recargo VIP y tope de descuento | `UFrmPedidos.pas:302-303`, `UFrmPedidos.pas:306-307`; `UCalculoTotales.pas:88-91` | Cliente VIP: +3 %. Si el porcentaje total supera 10 %, se limita a 10 %. | Porcentaje final de descuento. Duplicada en dos unidades. | Sí (`UFrmPedidos`); No (`UCalculoTotales`) |
| 4 | Descuento de Navidad | `UFrmPedidos.pas:304-305` | `gDescNavidad` y mes de la fecha = 12 → +2 %. Solo existe en `UFrmPedidos`. | Sumaría 2 %. `gDescNavidad` solo se inicializa en `False` (`UGlobales.pas:33`); por verificar que nada más lo active (parámetros, `.dfm`). Posible código muerto. | Sí (línea 304); «por verificar» que esté inactiva |
| 5 | Exclusión de la categoría OFE del descuento | `UCalculoTotales.pas:70`; `UFrmPedidos.pas:287` | Las líneas de categoría `OFE` no entran en la base de descuento. Una unidad normaliza (`UpperCase(Trim)`), la otra compara en SQL sin normalizar. | Distinta base de descuento según la categoría venga con espacios o minúsculas. Duplicada con criterio distinto. | No |
| 6 | Flete por región | `UCalculoTotales.pas:97-100`; `UFrmPedidos.pas:312-315` | Región 11 o 12: flete base × 1,15; otra región: flete base. | Flete del pedido. Valores de `FLETE_BASE` por región: por verificar en `REGIONES.csv`. | Sí (`UCalculoTotales`); No (`UFrmPedidos`) |
| 7 | IVA sobre neto más flete, tasa configurable | `UCalculoTotales.pas:105`; `UFrmPedidos.pas:318`; `UFrmPrincipal.pas:62-64`; `UGlobales.pas:31` | IVA = (neto + flete) × `gTasaIVA`. Por defecto 0,19; el parámetro `TASA_IVA` lo sobrescribe. | El IVA también grava el flete. En `UFrmInformes.pas:92` el informe usa 0,19 fijo (`vTotal / (1 + 0.19)`) e ignora la tasa configurada. | Parcial (`UFrmInformes.pas:92` sí; el resto no) |
| 8 | Redondeo al peso | `UCalculoTotales.pas:50`; `UUtilidades.pas:30` | `Trunc(valor + 0.49)`, duplicado como `Calc3` y `RedondeaPesos`. | 0,5 se redondea hacia abajo (10,5 → 10), contrario a lo que dicen los comentarios («redondeo al peso»). | Sí (`UUtilidades.pas:30`); No (`UCalculoTotales.pas:50`) |
| 9 | Pedido mínimo | `UFrmPedidos.pas:231-235` | Subtotal < 30.000 y región <> 13. | Se rechaza el pedido. Santiago (13) queda exento. | Sí |
| 10 | Máximo de líneas por pedido | `UFrmPedidos.pas:214-217` | Más de 40 líneas. | Se rechaza al grabar (no al agregar). | Sí |
| 11 | Bloqueo por deuda vencida | `UFrmPedidos.pas:111-117` | `SALDO_VENCIDO > 0` y `DaysBetween(Date, FECHA_VENCIMIENTO) > 60`. | Cliente bloqueado. `DaysBetween` no tiene signo, así que una fecha futura a más de 60 días también bloquearía; una fecha nula también. Por verificar con los datos. | Sí (línea 111-112); «por verificar» el efecto con fechas futuras y nulas |
| 12 | Stock: tope de faltante según tipo de cliente | `UStock.pas:40-50` | Cliente VIP: se permite hasta -20; cliente normal: no puede quedar bajo 0. | Si se supera, no se descuenta. El pedido ya está grabado (riesgo RS-06). | Sí |
| 13 | Corte horario de la fecha del pedido | `UUtilidades.pas:133-138` | Hora >= 18 → la fecha pasa al siguiente día hábil (lunes a viernes). Sin feriados. | Fecha del pedido. Un pedido antes de las 18:00 en fin de semana conserva su fecha (por verificar). | Sí (línea 133); lo demás por verificar |
| 14 | Informes solo con nivel >= 2 | `UFrmPrincipal.pas:95-99`; `UFrmPrincipal.pas:49-50` | `gNivelUsuario < 2` rechaza. Solo los usuarios `ADMIN` y `JEFEVENTAS` reciben nivel 9; ninguna ruta asigna nivel 2. | Solo esos dos nombres ven informes, sin contraseña. | Sí |
| 15 | Alta de cliente VIP restringida | `UFrmClientes.pas:143` | Tipo `V` y `gNivelUsuario < 2`. | Se rechaza. Otras validaciones de cliente (tipo N/V, región 1-16, razón social, no borrar con pedidos): `UFrmClientes.pas:107-153`. | Sí (línea 143); No (el resto) |
| 16 | Validación del dígito verificador del RUT (dos versiones) | `UUtilidades.pas:73-76`; `UFrmClientes.pas:193-196` | `UUtilidades`: resto 11 → `0`, 10 → `K`. `UFrmClientes`: resto 11 → `K`, 10 → `0`. | Un mismo RUT se acepta en un sitio y se rechaza en el otro. La forma estándar es la de `UUtilidades`. | Sí |
| 17 | Folio del pedido | `UFrmPedidos.pas:249-251` | `SELECT MAX(FOLIO)`; el nuevo folio es el máximo + 1. Estado inicial `C` (`:262`). | Folio correlativo sin bloqueo (ver riesgo RS-05). | Sí (línea 249); No (estado inicial) |
| 18 | Reglas de producto | `UFrmProductos.pas:87-102` | Precio > 0; categoría de 3 letras en mayúsculas; `STOCK_MINIMO` por defecto 10. | Validación al guardar. | No |
| 19 | Aviso de primer pedido | `UFrmPedidos.pas:338-342` | El cliente queda con exactamente 1 pedido. | Mensaje «avisar a cobranza»; no bloquea. | No |

## Comentarios que contradicen el código

- `UCalculoTotales.pas:4`: «5 % sobre 400.000»; el código usa 500.000 y 1.000.000.
- `UGlobales.pas:13`: niveles 1/2/9; en el código solo se asigna el 9 (`UFrmPrincipal.pas:49-50`).
- `UCalculoTotales.pas:5`: «NO TOCAR sin hablar con contabilidad»; la misma lógica está duplicada en `UFrmPedidos.pas:296-319` con valores distintos.
- `UFrmProductos.pas:43` («solo admin puede borrar») solo oculta el botón (`:44-45`) (por verificar).
