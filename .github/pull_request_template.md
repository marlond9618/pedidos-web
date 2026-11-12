<!--
Plantilla de PR de pedidos-web. Un PR nace revisable: chico, con spec, con tests en verde.
Borra los comentarios de ayuda antes de abrir. Rellena cada sección; usa [COMPLETAR] si algo falta.
-->

## Qué cambia
<!-- En una o dos frases: qué comportamiento nuevo o distinto aporta este PR. -->

## Por qué
<!-- La historia y su spec aprobada. Enlaza specs/<historia>/spec.md. -->
Spec: `specs/<historia>/spec.md`

## Cómo probarlo
<!-- Comandos y resultado esperado. Mapea cada criterio de aceptación a su test. -->
```bash
dotnet build
dotnet test
```
- CA-01 → `Metodo_Escenario_ResultadoEsperado`
- …

## Checklist (ver REVIEW.md y AGENTS.md)
- [ ] Nace de una spec aprobada en `specs/<historia>/spec.md`.
- [ ] Un test xUnit por criterio de aceptación, con valor esperado concreto y nombre `Metodo_Escenario_ResultadoEsperado`.
- [ ] `dotnet build` y `dotnet test` en verde (sin tests no hay commit).
- [ ] Dinero en `decimal` (CLP, sin decimales); fechas de pedido en `DateOnly`.
- [ ] `PedidosWeb.Core` no referencia ASP.NET.
- [ ] Sin secretos, claves ni cadenas de conexión en el código ni en el repo.
- [ ] No cambia `Pedido.cs`, `IPedidoRepositorio` ni los datos de ejemplo (o lo justifico abajo y fue confirmado).
- [ ] No afecta el `Total` cobrado ni se aleja del legado / golden master sin decisión escrita.

## Fuera de alcance
<!-- Qué quedó deliberadamente fuera, para que el revisor no lo busque. -->

## Notas para el revisor
<!-- Dudas abiertas, decisiones discutibles, o dónde quieres una segunda mirada. [COMPLETAR] si no hay. -->
