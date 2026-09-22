# Bitácora de cambios — PLD

Registro funcional y técnico de cambios relevantes del repositorio.

> La mecánica obligatoria para mantener esta bitácora está definida en `AGENTS.md`. Las entradas más recientes se agregan al inicio.

---

## 2026-09-22 — Cierre Fase 1 de crédito revolvente

**Autor/agente:** ChatGPT / Bill  
**Resumen:** Se cerró la integración técnica de Fase 1 del crédito revolvente. La captura identifica productos revolventes, bloquea la amortización fija CONDUSEF en frontend y backend, exige solicitud FINALIZADA para configurar línea/disponer y conecta la navegación Consulta PF → Revolvente → Pagos sin alterar el flujo de crédito simple.

**Archivos principales:**
- `PLD/Handlers/catalogos_handler.ashx.vb`
- `PLD/Handlers/solicitud_credito_handler.ashx.vb`
- `PLD/Secure/captura_solicitud_credito.aspx`
- `PLD/Handlers/handler_credito_revolvente.ashx.vb`
- `PLD/Secure/credito_revolvente.aspx`
- `PLD/Secure/pagos_credito.aspx`
- `PLD/SQL/20260922_001_credito_revolvente.sql`
- `README.md`
- `cambios_codex/2026-09-22_001_diseno_credito_revolvente.md`
- `cambios_codex/2026-09-22_002_plan_revolvente_hasta_rc_qa.md`

**Base de datos / SQL:** No se agregaron nuevos objetos respecto de la migración ya documentada. Se endureció la reejecución del registro página-handler para evitar inconsistencias si existieran relaciones inactivas previas. La migración aún debe aplicarse en un ambiente de pruebas.

**Estructura de BD actualizada:** No aplica; no cambió la estructura ya documentada.

**README actualizado:** Sí.

**Validación:** Revisión estática de contratos frontend/backend, diferenciación SIMPLE/REVOLVENTE, bloqueo server-side de amortización fija, requisito FINALIZADA, navegación y reejecución razonable del SQL. No se ejecutó compilación ni SQL Server desde el conector GitHub.

**Pendientes:** Fase 2 — endurecimiento operativo y regresión. Fase 3 continúa bloqueada por reglas financieras no confirmadas.

---

## 2026-09-22 — Roadmap de crédito revolvente hasta RC para QA

**Autor/agente:** ChatGPT / Bill  
**Resumen:** Se ordenó el trabajo restante del crédito revolvente en fases hasta llegar a una versión candidata para QA, separando lo ya implementado, lo que depende de reglas de negocio aún no confirmadas y lo que no debe bloquear la primera entrega operativa.

**Archivos principales:**
- `cambios_codex/2026-09-22_002_plan_revolvente_hasta_rc_qa.md`
- `BITACORA_CAMBIOS.md`

**Base de datos / SQL:** Sin cambios.

**Estructura de BD actualizada:** No aplica.

**README actualizado:** No aplica; no cambió el funcionamiento del sistema.

**Validación:** Revisión documental contra la implementación actual del revolvente, el diseño previo y las reglas maestras de `AGENTS.md`.

**Observaciones:** La primera RC puede excluir estado de cuenta final, TXT regulatorios y PLD avanzado de disposiciones. Las reglas financieras automáticas siguen condicionadas a definición aprobada.

---

## 2026-09-22 — Implementación de crédito revolvente (Fases 1 y 2)

**Autor/agente:** ChatGPT / Bill  
**Resumen:** Se implementó la estructura y operación básica de crédito revolvente conservando el flujo de crédito simple. Se agregó configuración de tipo revolvente, límite/vigencia, disposiciones múltiples, cálculo derivado de utilizado/disponible, historial operativo e integración con Consulta PF y pagos.

**Archivos principales:**
- `PLD/SQL/20260922_001_credito_revolvente.sql`
- `PLD/Handlers/handler_credito_revolvente.ashx(.vb)`
- `PLD/Secure/credito_revolvente.aspx(.vb/.designer.vb)`
- `PLD/Handlers/handler_pagos_credito.ashx.vb`
- `PLD/Handlers/catalogo_creditos_pld.ashx.vb`
- `PLD/Secure/catalogo_creditos.aspx`
- `PLD/Handlers/solicitudPFHandler.ashx.vb`
- `PLD/Secure/solicitud_pf.aspx`
- `PLD/PLD.vbproj`

**Base de datos / SQL:** Nueva migración incremental. No se ejecutó contra producción desde este entorno.

**Estructura de BD actualizada:** Sí.

**README actualizado:** Sí.

**Validación:** Revisión estática de contratos frontend/backend, SQL parametrizado, seguridad página-handler, concurrencia de disposiciones/pagos y referencias del proyecto. No fue posible ejecutar Visual Studio/MSBuild ni una instancia SQL Server desde el conector GitHub.

**Pendientes:** Intereses, pago mínimo, prelación, mora y estado de cuenta contractual siguen pendientes de definición funcional.

---

## 2026-09-22 — Diseño técnico-funcional de crédito revolvente

**Autor/agente:** ChatGPT / Bill  
**Resumen:** Se analizó el punto 10 del requerimiento funcional y se documentó una arquitectura para soportar crédito revolvente sin romper el crédito simple actual. La recomendación conserva `solicitud_credito` como cabecera operativa, conserva `pagos_credito`, agrega conceptualmente disposiciones y cambia el cálculo de saldo revolvente a disposiciones menos capital amortizado.

**Archivos principales:**
- `cambios_codex/2026-09-22_001_diseno_credito_revolvente.md` — análisis, arquitectura, reglas estructurales, impactos, alternativas, fases y decisiones pendientes.
- `BITACORA_CAMBIOS.md` — esta entrada.

**Base de datos / SQL:** Sin cambios. No se generaron migraciones; el documento es diseño pendiente de aprobación.

**Estructura de BD actualizada:** No aplica; no cambió el esquema real.

**README actualizado:** No aplica; no cambió el funcionamiento actual del sistema.

**Validación:** Revisión de solicitud, producto financiero, pagos, búsquedas/saldos, UI actual y estructura documentada. Se confirmó que no existe actualmente una entidad de disposiciones/línea ni un saldo revolvente calculado correctamente.

**Commit/PR:** Ver commit Git que contiene esta entrada.

**Observaciones:** No implementar cálculo de interés, pago mínimo, prelación de pagos ni estado de cuenta hasta confirmar las reglas funcionales correspondientes. El diseño estructural recomienda preservar el crédito simple y reutilizar los componentes existentes.

---

## 2026-09-22 — README y estructura de base de datos obligatoriamente sincronizados

**Autor/agente:** ChatGPT / Bill  
**Resumen:** Se reforzaron las instrucciones maestras para obligar a ChatGPT, Codex y otros agentes a mantener actualizado el README y la documentación de estructura de base de datos en cada cambio aplicable.

**Archivos principales:**
- `AGENTS.md` — reglas de cierre, mantenimiento de README y sincronización obligatoria de cambios de BD.
- `PLD/SQL/ESTRUCTURA_BASE_DATOS.md` — nueva referencia canónica de estructura conocida y reglas de mantenimiento.
- `README.md` — referencia a la documentación canónica de estructura de BD.
- `BITACORA_CAMBIOS.md` — esta entrada.

**Base de datos / SQL:** Sin cambios de esquema. No se ejecutó migración. Se creó documentación de estructura conocida basada únicamente en objetos confirmados por scripts/código existentes.

**Validación:** Revisión documental. No se requiere compilación porque no se modificó lógica ejecutable ni proyecto.

**Commit/PR:** Ver commit Git que contiene esta entrada.

**Observaciones:** Desde este cambio, una tarea que modifique base de datos no se considera terminada si no actualiza simultáneamente el SQL incremental y `PLD/SQL/ESTRUCTURA_BASE_DATOS.md`. El README también debe mantenerse al día cuando el cambio afecte la documentación general del sistema.

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
