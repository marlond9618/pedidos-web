# Riesgos de Pedidos Legado

Rutas relativas a `legado/src/`. Evidencia leída por mí en el código: marcada con ✔. Sin ✔: la reporta el subagente `analista-legado` y está sin abrir; lo dudoso va como «por verificar».

| ID | Riesgo | Evidencia (archivo:línea) | Categoría | Prioridad |
|---|---|---|---|---|
| RS-01 | Clave de la base escrita en el código y registrada en la sesión. | `UDMPrincipal.pas:41` ✔ (`CLAVE_BD = 'Distrib#2008'`), `:59` ✔ | Seguridad | Alta |
| RS-02 | Sin autenticación: el usuario se teclea en un `InputBox` y `ADMIN`/`JEFEVENTAS` reciben nivel 9 sin contraseña. | `UFrmPrincipal.pas:46-50` ✔ | Seguridad | Alta |
| RS-03 | SQL por concatenación con texto libre. `EscapaComillas` (`UUtilidades.pas:164`) existe pero no se usa en estos puntos. | `UFrmInformes.pas:69` ✔; `UFrmClientes.pas:76`, `UFrmPedidos.pas:102`, `:152`, `:261-262`, `UDMPrincipal.pas:85` | Seguridad | Alta |
| RS-04 | Grabación del pedido sin transacción: cabecera con total 0, líneas, `UPDATE` del total y stock se ejecutan por separado. Un fallo intermedio deja un pedido incompleto. | `UFrmPedidos.pas:255` ✔ (`// TODO transaccion`), `:263-324` | Datos e integridad | Alta |
| RS-05 | Folio e ID de cliente con `MAX()+1`, sin bloqueo: dos puestos pueden obtener el mismo valor. Por verificar si el esquema lo impide (las claves primarias lo harían fallar en vez de duplicar). | `UFrmPedidos.pas:249-251` ✔; `UFrmClientes.pas:160-163` | Datos e integridad | Alta |
| RS-06 | El stock se valida después de grabar el pedido: si falta, solo hay un mensaje y el pedido queda igual. | `UFrmPedidos.pas:324` ✔; `UStock.pas:40-50` ✔, `:69-72` | Datos e integridad | Alta |
| RS-07 | Error tragado al registrar movimientos de stock (`except` vacío). Además `MOV_STOCK` no existe en `esquema-dbisam.sql`, así que el registro podría fallar siempre sin aviso (por verificar contra la base real). | `UStock.pas:84-90` ✔ | Datos e integridad | Media |
| RS-08 | Totales con reglas duplicadas y distintas (`>` frente a `>=`, Navidad solo en un lado): el total grabado puede diferir del mostrado. La comparación entre ambos solo avisa con `gModoPrueba`, que parece no activarse (por verificar). | `UCalculoTotales.pas:77-83` ✔, `UFrmPedidos.pas:296-307` ✔, `:327-332` | Mantenibilidad | Alta |
| RS-09 | Dos validadores de RUT incompatibles en el dígito verificador (11 y 10 invertidos). | `UUtilidades.pas:73-76` ✔, `UFrmClientes.pas:193-196` ✔ | Datos e integridad | Alta |
| RS-10 | Descuento de stock leído y luego escrito como valor absoluto: con concurrencia, un descuento pisa a otro. | `UStock.pas:38-53` | Datos e integridad | Media |
| RS-11 | Dinero con `Double` y redondeo propio (`Trunc(x + 0.49)`, redondea 0,5 hacia abajo), duplicado. | `UUtilidades.pas:30` ✔, `UCalculoTotales.pas:50` | Datos e integridad | Media |
| RS-12 | IVA fijo en el informe: si `TASA_IVA` cambia, neto e IVA del informe salen mal. | `UFrmInformes.pas:92` ✔ | Datos e integridad | Media |
| RS-13 | Conversión decimal dependiente de la configuración regional (`FloatToStr`/`StrToFloat` en SQL y tasa de IVA). Por verificar en el entorno de ejecución. | `UFrmPedidos.pas:271`, `:320`; `UFrmPrincipal.pas:64` | Datos e integridad | Media |
| RS-14 | Año de 2 dígitos con pivote en 50 (`yy` < 50 → 20yy) y sin manejo de errores para texto vacío o inválido. | `UUtilidades.pas:103` ✔, `:96-112` | Fechas | Media |
| RS-15 | Corte a las 18 h con el reloj del PC cliente y sin feriados: un pedido puede fecharse en un feriado. | `UUtilidades.pas:133` ✔, `:122-125` | Fechas | Media |
| RS-16 | `DaysBetween` sin signo en el bloqueo por deuda: una fecha de vencimiento futura o nula puede bloquear a un cliente sin deuda vencida real. Por verificar con los datos. | `UFrmPedidos.pas:111-112` ✔ | Fechas | Media |
| RS-17 | Estado global mutable (`gUsuario`, `gNivelUsuario`, `gTasaIVA`…) modificado desde varios formularios. | `UGlobales.pas:11-21`; `UFrmPrincipal.pas:49-50` ✔ | Mantenibilidad | Media |
| RS-18 | Ruta de datos fija en el código (`C:\PEDIDOS\DATOS`). | `UGlobales.pas:36`; `UDMPrincipal.dfm:12` | Seguridad | Baja |
| RS-19 | Código muerto probable: `gFleteBase`, `gModoPrueba`, `gDescNavidad`, `gImpresora`, y funciones de `UUtilidades` (`LimpiaRut`, `FormateaRut`, `SoloNumeros`, `Rellena`, `NombreMes`). Por verificar con una búsqueda de referencias. | `UGlobales.pas:15`, `:21`, `:33`; `UUtilidades.pas:9-20` | Mantenibilidad | Baja |
| RS-20 | Nombres poco descriptivos: `Calc3` (redondeo) y `Proc2` (pinta los totales). | `UCalculoTotales.pas:29`; `UFrmPedidos.pas:367` | Mantenibilidad | Baja |
| RS-21 | El CSV de informes no escapa `;` y se escribe en la carpeta de la base (`gRutaDatos`). | `UFrmInformes.pas:111`, `:118-122` | Seguridad | Baja |

## Por verificar

- Valores reales de `TASA_IVA` y `FLETE_BASE` (`PARAMETROS.csv`, `REGIONES.csv`; no abiertos).
- Si existe `MOV_STOCK` en la base real (el esquema del repo no la define).
- Si algo activa `gDescNavidad` o `gModoPrueba` fuera de los `.pas`.
- Si algún proceso cambia `ESTADO` a un valor distinto de `C` (el informe «solo confirmados» filtra `ESTADO = 'C'`, `UFrmInformes.pas:71`).
