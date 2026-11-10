---
name: documentar-modulo
description: "Escribe la documentación de un módulo o servicio de PedidosWeb.Core en docs/modulos/<Modulo>.md a partir de su código y sus tests, sin modificar el código. Úsala cuando pidan documentar, explicar por escrito o dejar la ficha de una clase, servicio o módulo (por ejemplo ClientesService), aunque no nombren la Skill."
---

# Documentar un módulo

## Pasos
1. Ubica el módulo: lee su código en `src/PedidosWeb.Core/<Carpeta>/` y sus tests en `tests/PedidosWeb.Core.Tests/<Carpeta>/`.
2. Busca quién lo usa (`Grep` del nombre en `src/` y `tests/`, ignorando `bin/` y `obj/`) para saber si es código vivo o heredado.
3. Escribe `docs/modulos/<Modulo>.md` en español, con las secciones de abajo. Si el archivo ya existe, actualízalo en vez de duplicarlo.
4. Comprueba cada ejemplo contra los tests o ejecutando el código: los valores del documento deben ser los reales, no inventados.
5. Termina diciendo qué archivo escribiste y qué dudas o hallazgos (nombres poco claros, código sin uso) dejaste anotados.

## Qué debe tener docs/modulos/<Modulo>.md
- **Propósito**: qué resuelve el módulo, en una o dos frases, y su estado (vigente, heredado, reemplazado).
- **API pública**: tabla con cada miembro público (constructores y métodos): firma, qué devuelve y casos límite (ej.: id inexistente).
- **Reglas de negocio**: lo que el código decide por sí mismo (normalizaciones, tablas de equivalencia como región → zona).
- **Ejemplo de uso**: código C# mínimo y su resultado real, tomado de los tests.
- **Diagrama Mermaid**: un `flowchart` o `classDiagram` con el flujo o las dependencias.
- **Tests**: qué archivo los cubre y qué casos prueban.
- **Notas**: deuda técnica o cosas a tener en cuenta, sin proponer cambios de código.

## Reglas
- No modifiques `src/` ni `tests/`: esta Skill solo escribe en `docs/modulos/`.
- Documenta lo que el código hace hoy, aunque parezca raro; si algo parece un error, anótalo en Notas sin corregirlo.
- Usa los nombres reales del código (en español, como en AGENTS.md) y no copies el código completo: explícalo.
- Sin secretos ni datos reales: los ejemplos usan solo los datos ficticios del proyecto.
- Agrega el módulo a la tabla de `docs/README.md` si todavía no figura.
