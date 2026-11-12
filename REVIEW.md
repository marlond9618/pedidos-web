# Instrucciones de revisión · pedidos-web

Reglas de revisión del portal de pedidos. Se leen **desde `main`**: un PR no puede
cambiar estas reglas para aflojar su propia revisión. Derivan de `AGENTS.md`.

## Qué es Importante aquí
Lo que cambia un monto cobrado al cliente (el `Total` del pedido en CLP) o se aleja
del legado o del golden master sin una decisión escrita, y lo que rompe una regla de
dominio de `AGENTS.md`. Estilo, nombres y preferencias de diseño son Detalle como máximo.

## Tope de detalles
Máximo 3 detalles por revisión. Los hallazgos Importantes no tienen tope.

## No reportar
- Lo que ya valida el CI: compilación y tests en verde.
- `legado/**` y `migracion/golden-master.csv` (son referencia, no se tocan).
- Archivos generados (`bin/`, `obj/`).

## Revisar siempre
1. **Dinero en `decimal`** (CLP, sin decimales). Nunca `double` ni `float`.
2. **Fechas de pedido en `DateOnly`** (hora local de Chile, sin hora).
3. **Cada criterio de aceptación nuevo tiene un test xUnit** con un valor esperado
   concreto y nombre `Metodo_Escenario_ResultadoEsperado`.
4. **`PedidosWeb.Core` no referencia ASP.NET**: el dominio queda independiente de la API.
5. **Ningún secreto, clave ni cadena de conexión** en el código ni en el repo.
6. **Toda funcionalidad nace de una spec aprobada** en `specs/<historia>/spec.md`.

## Evidencia
Cada hallazgo cita `archivo:línea` y explica por qué en una frase.
