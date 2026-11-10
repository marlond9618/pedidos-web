# Matriz de decisión de Pedidos Legado

Etapa 3 · Decidir. Pesos aceptados por el equipo; puntajes propuestos por Claude con base en `inventario.md`, `reglas-negocio.md` y `riesgos.md`.

## Pesos

| Criterio | Qué mide | Peso |
|---|---|---|
| Costo | Cuánto cuesta en horas y dinero | 20 % |
| Complejidad | Qué tan difícil es técnicamente | 15 % |
| Riesgo | Qué puede salir mal en el camino | 30 % |
| Impacto | Cuánto valor entrega al negocio | 25 % |
| Personas | Conocimiento disponible en el equipo | 10 % |
| **Total** |  | **100 %** |

Escala 1 a 5, donde 5 es lo mejor en ese criterio (barato, simple, poco riesgo, mucho valor, equipo preparado).
Estrategias: **Mantener** (dejarlo como está), **Refactorizar** (limpiarlo dentro de Delphi), **Modernizar** (portar a .NET conservando las reglas), **Reemplazar** (rediseñar desde cero).

## Puntajes por criterio

Orden: Costo, Complejidad, Riesgo, Impacto, Personas.

| Módulo | Mantener | Refactorizar | Modernizar | Reemplazar |
|---|---|---|---|---|
| 1. Cálculo y utilidades | 5, 5, 1, 1, 5 | 4, 4, 3, 3, 5 | 3, 4, 4, 5, 3 | 2, 3, 2, 4, 2 |
| 2. Pedidos y stock | 5, 5, 1, 1, 5 | 3, 3, 3, 3, 5 | 3, 2, 3, 5, 3 | 1, 1, 2, 5, 2 |
| 3. Maestros | 5, 5, 2, 2, 5 | 4, 4, 4, 2, 5 | 3, 3, 3, 3, 3 | 2, 3, 3, 3, 2 |
| 4. Informes | 5, 5, 2, 1, 5 | 4, 4, 3, 2, 5 | 3, 3, 4, 4, 3 | 3, 3, 4, 4, 2 |
| 5. Acceso a datos y seguridad | 5, 5, 1, 1, 5 | 3, 3, 2, 2, 5 | 3, 2, 4, 5, 3 | 2, 2, 4, 5, 3 |

## Matriz (puntaje ponderado)

| Módulo | Mantener | Refactorizar | Modernizar | Reemplazar | Recomendación |
|---|---|---|---|---|---|
| 1. Cálculo y utilidades (`UCalculoTotales`, `UUtilidades`, `UGlobales`) | 2,80 | 3,55 | **3,95** | 2,65 | Modernizar |
| 2. Pedidos y stock (`UFrmPedidos`, `UStock`) | 2,80 | 3,20 | **3,35** | 2,40 | Modernizar por etapas, después del módulo 1 |
| 3. Maestros (`UFrmClientes`, `UFrmProductos`) | 3,35 | **3,60** | 3,00 | 2,70 | Refactorizar |
| 4. Informes (`UFrmInformes`) | 3,10 | 3,30 | **3,55** | 3,45 | Modernizar |
| 5. Acceso a datos y seguridad (`UDMPrincipal`, `UFrmPrincipal`) | 2,80 | 2,65 | **3,65** | 3,45 | Modernizar |

Justificación:
1. Cálculo: no depende de formularios ni de la base (`dependencias.md`); tiene reglas duplicadas y distintas (`UCalculoTotales.pas:77` frente a `UFrmPedidos.pas:296`).
2. Pedidos y stock: diferencia mínima (3,35 frente a 3,20); mayor riesgo y complejidad (sin transacción, `MAX+1`, stock validado tras grabar).
3. Maestros: pequeños y de bajo valor; se corrigen el SQL concatenado (RS-03) y el RUT inconsistente (RS-09) sin migrar.
4. Informes: Modernizar gana por poco a Reemplazar. Por verificar si el informe real es CSV o Crystal Reports (el README dice Crystal; el código exporta CSV); si es Crystal, Reemplazar sube.
5. Acceso a datos: clave en el código y login sin contraseña (RS-01, RS-02); Refactorizar en Delphi no los resuelve.

Limitación: no hay horas ni presupuesto reales ni datos del equipo, así que Costo y Personas son juicio, no medición. Los puntajes de Riesgo en los módulos 2 y 3 son los más discutibles.

## Resumen ejecutivo

1. **Qué es y su estado:** sistema de pedidos de la distribuidora en Delphi + DBISAM (10 unidades, 6 tablas), con reglas de negocio mezcladas en los formularios y un README que ya no refleja el código.
2. **Hallazgo principal (con evidencia):** las mismas reglas están duplicadas con valores distintos: el descuento usa `>` en `UCalculoTotales.pas:77` y `>=` en `UFrmPedidos.pas:296`, y el RUT se valida de dos formas (`UUtilidades.pas:73`, `UFrmClientes.pas:193`).
3. **Riesgo más urgente:** la clave de la base está en el código (`UDMPrincipal.pas:41`), el login no pide contraseña (`UFrmPrincipal.pas:49`) y el pedido se graba sin transacción (`UFrmPedidos.pas:255`).
4. **Estrategia y por dónde empezar:** modernizar por etapas, empezando por las reglas de cálculo (módulo 1), que no dependen de formularios ni de la base; los maestros se refactorizan sin migrar.
5. **Próximo paso que se pide aprobar:** que el negocio fije qué versión de las reglas duplicadas es la correcta y escribir la spec del módulo de cálculo en `specs/`.
