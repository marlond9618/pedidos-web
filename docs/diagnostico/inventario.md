# Inventario de Pedidos Legado

Líneas contadas con `wc -l` (no estimadas). Rutas relativas a `legado/`.

## Archivos de `src/`

| Archivo | Tipo | Responsabilidad | Líneas |
|---|---|---|---|
| `UGlobales.pas` | Globales | Variables globales de sesión y configuración (usuario, nivel, tasa IVA 0.19, ruta de datos) con valores por defecto. | 41 |
| `UUtilidades.pas` | Utilidades | RUT, fechas, formato de pesos, fecha del pedido (corte a las 18h) y escape de comillas. | 189 |
| `UCalculoTotales.pas` | Unidad de lógica | Calcula subtotal, descuento, flete, IVA y total, y maneja el arreglo de líneas. | 147 |
| `UStock.pas` | Unidad de lógica | Consulta y descuenta stock y registra movimientos. | 93 |
| `UDMPrincipal.pas` | Data module | Conecta a DBISAM, abre las tablas maestras, lee parámetros y ejecuta SQL genérico. | 100 |
| `UDMPrincipal.dfm` | Data module (DFM) | `TDBISAMDatabase`, 5 `TDBISAMTable` y 2 `TDBISAMQuery` (`qryGeneral`, `qryAux`). | 65 |
| `UFrmPrincipal.pas` | Formulario | Ventana principal con menú: pide usuario, carga parámetros y abre los demás formularios. | 104 |
| `UFrmPrincipal.dfm` | Formulario (DFM) | `TMainMenu` y `TStatusBar`; sin componentes de datos. | 62 |
| `UFrmClientes.pas` | Formulario | Mantenimiento de clientes: alta, baja, edición, validación de RUT y tipo VIP. | 215 |
| `UFrmClientes.dfm` | Formulario (DFM) | `tblCli` (CLIENTES), `dsCli`, grilla y 6 `TDBEdit`. | 213 |
| `UFrmProductos.pas` | Formulario | Mantenimiento de productos con filtro y validaciones. | 105 |
| `UFrmProductos.dfm` | Formulario (DFM) | `tblProd` (PRODUCTOS), `dsProd`, grilla y navegador. | 98 |
| `UFrmPedidos.pas` | Formulario | Toma de pedidos: valida, graba cabecera y líneas, recalcula total, descuenta stock. | 420 |
| `UFrmPedidos.dfm` | Formulario (DFM) | `TStringGrid`, edits, memo y botones; sin componentes de datos. | 226 |
| `UFrmInformes.pas` | Formulario | Informe de ventas por fechas y exportación a CSV. | 137 |
| `UFrmInformes.dfm` | Formulario (DFM) | `qryInforme`, `dsInforme` y grilla. | 160 |
| `PedidosLegado.dpr` | Programa | Registra las 10 unidades y crea `DMPrincipal` y `FrmPrincipal`. | 24 |

(`.pas` y `.dfm`: 10 y 6 archivos en `src/`, más el `.dpr`; `UGlobales`, `UUtilidades`, `UCalculoTotales` y `UStock` no tienen `.dfm`.)

## Tablas (`db/esquema-dbisam.sql`, 74 líneas)

| Tabla | Responsabilidad | Clave | Filas de ejemplo |
|---|---|---|---|
| `REGIONES` | Regiones con flete base. | `CODIGO` | 16 |
| `CLIENTES` | Clientes: RUT, tipo, región, saldo vencido y fecha de vencimiento. | `ID_CLIENTE` (índice por `RUT`) | 30 |
| `PRODUCTOS` | Catálogo con precio, stock y stock mínimo. | `ID_PRODUCTO` (índice por `CODIGO`) | 40 |
| `PEDIDOS` | Cabecera del pedido: folio, fecha, cliente, usuario, estado, total. | `FOLIO` (índices por cliente y fecha) | 60 |
| `PEDIDO_LINEAS` | Líneas del pedido con cantidad y precio unitario. | `FOLIO`+`LINEA` | 169 |
| `PARAMETROS` | Configuración clave/valor (p. ej. `TASA_IVA`). | `CLAVE` | 6 |

## Lo que me llamó la atención

- `legado/README.md` dice Delphi 5, Paradox/BDE y Crystal Reports, pero el código usa DBISAM (`dbisamtb`) y el esquema es DBISAM 4. El README está desactualizado.
- El código consulta `MOV_STOCK` (`UStock.pas:85`) y la tabla no existe en `esquema-dbisam.sql`. El schema está incompleto o el registro de movimientos falla en silencio (`except` vacío, `UStock.pas:89-90`).
- La clave de la base está en el código (`UDMPrincipal.pas:41`) y la ruta `C:\PEDIDOS\DATOS` está fija (`UGlobales.pas:36`, `UDMPrincipal.dfm:12`).
- Reglas duplicadas y distintas: descuento (`UCalculoTotales.pas:77-79` vs `UFrmPedidos.pas:296-298`) y dígito verificador del RUT (`UFrmClientes.pas:170-200` vs `UUtilidades.pas:52-80`).
- Los formularios mezclan reglas de negocio con la interfaz (deuda > 60 días, pedido mínimo, máximo de 40 líneas en `UFrmPedidos.pas`).
