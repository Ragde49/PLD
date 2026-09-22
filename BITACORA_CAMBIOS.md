# Bitácora de cambios — PLD

Registro funcional y técnico de cambios relevantes del repositorio.

> La mecánica obligatoria para mantener esta bitácora está definida en `AGENTS.md`. Las entradas más recientes se agregan al inicio.

---

## 2026-09-22 — Estandarización de trabajo para ChatGPT/Codex

**Autor/agente:** ChatGPT / Bill  
**Resumen:** Se estableció un único archivo maestro de instrucciones para agentes y desarrolladores. Se creó la bitácora oficial y la carpeta de especificaciones de cambios para Codex.

**Archivos principales:**
- `AGENTS.md` — nueva fuente única de instrucciones permanentes.
- `AGENTE.md` — eliminado para evitar reglas duplicadas.
- `README.md` — advertencia visible de lectura obligatoria de `AGENTS.md`.
- `PLD/PLD.vbproj` — vínculo actualizado de `AGENTE.md` a `AGENTS.md`.
- `BITACORA_CAMBIOS.md` — creada.
- `cambios_codex/0000_PLANTILLA_CAMBIO.md` — creada.

**Base de datos / SQL:** Sin cambios.

**Validación:** Revisión documental de referencias y estructura. No se requiere compilación por tratarse de documentación y vínculo de contenido sin cambio de lógica compilable.

**Commit/PR:** Ver commit Git que contiene esta entrada.

**Observaciones:** A partir de este cambio, toda tarea debe comenzar leyendo `AGENTS.md` y toda tarea finalizada debe actualizar esta bitácora.

---

## 2026-09-22 — README inicial integral del proyecto

**Autor/agente:** ChatGPT / Bill  
**Resumen:** Se analizó la estructura del proyecto y se creó documentación general de arquitectura, módulos PLD/crédito, seguridad, SQL, configuración, SMTP, puesta en marcha y consideraciones operativas.

**Archivos principales:**
- `README.md`

**Base de datos / SQL:** Sin cambios.

**Validación:** Contraste del README contra el código, estructura del repositorio, proyecto VB.NET y scripts SQL disponibles.

**Commit:** `93640bdb6ef526f3f20c8d412fe86646e110b254`

**Observaciones:** Se documentó que `Web.config` es local/no versionado y que el handler de prueba SMTP requiere revisar su registro de seguridad.
