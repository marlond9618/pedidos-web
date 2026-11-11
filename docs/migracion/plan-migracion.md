# Plan de migración · Pedidos Legado → Pedidos Web

Estrategia del diagnóstico (U4): **modernizar por etapas** (strangler fig), empezando por el módulo
de cálculo, que no depende de formularios ni de la base (`docs/diagnostico/matriz-decision.md`).

## 1. Etapas

1. **Cálculo de totales en C#** (`src/PedidosWeb.Core/Totales/`) — **hecho** (U5). Congelado con el
   golden master (60 casos) y migrado fiel al legado, con una corrección registrada (R05, ver
   `defectos-corregidos.md`).
2. **Pedidos y stock** (`UFrmPedidos`, `UStock`): número de pedido sin `MAX+1`, grabado transaccional
   y validación de stock antes de grabar. Resolver antes la regla de descuento duplicada (H1).
3. **Maestros** (clientes, productos): refactor dentro de Delphi (SQL parametrizado, RUT único), sin migrar.
4. **Informes** y **acceso a datos/seguridad**: modernizar; sacar la clave del código y el login sin contraseña.

## 2. Convivencia

- El portal (.NET) y el legado (Delphi) calculan en paralelo sobre los mismos pedidos; se comparan con
  el golden master como contrato. Cualquier nuevo pedido se valida contra ambos antes de confiar en el portal.
- **Diferencia conocida:** por R05, 13 de 60 casos difieren hasta en 2 pesos. Contabilidad la acepta o
  porta el fix a `UUtilidades.pas:30` para que ambos coincidan.
- Mientras el negocio no fije la regla de descuento (H1), el C# replica la versión de `UCalculoTotales.pas` (`>`).

## 3. Riesgos

| Riesgo | Mitigación |
|---|---|
| Dinero en `Double` + redondeo propio en el legado (RS-11): resultados no reproducibles. | El portal usa `decimal` y el golden master como contrato; cada caso que cambia se registra. |
| Reglas duplicadas con valores distintos (H1: `>` vs `>=`, comentario que miente). | Decisión del negocio antes de la etapa 2; hasta entonces una sola fuente de verdad (el C#). |
| Clave en el código y login sin contraseña (RS-01/RS-02). | Abordar en la etapa de acceso a datos; nunca portar secretos al repo (`.env`/`*.pem` en `.gitignore`). |
| Pedido grabado sin transacción y `MAX+1` para el folio (módulo 2). | Grabado transaccional e identidad de BD al modernizar Pedidos; validar stock antes de confirmar. |

## 4. Criterio de corte

- **Etapa 1 (cálculo):** se apaga el cálculo del legado cuando el portal iguala el golden master
  (60/60) y Contabilidad firma la diferencia de R05. Vuelta atrás: reactivar el cálculo Delphi (el
  legado sigue intacto, solo lectura).
- **Etapas siguientes:** cada módulo se apaga en el legado solo tras correr en paralelo sin diferencias
  no explicadas durante un cierre contable completo. Si aparece una diferencia, se vuelve al legado de
  ese módulo y se reabre la spec.
