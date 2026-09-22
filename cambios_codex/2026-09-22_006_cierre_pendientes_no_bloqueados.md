# Cierre de pendientes desarrollables sin insumos del cliente

**Estado:** COMPLETADO  
**Fecha:** 2026-09-22  
**Solicitante:** Edgar / proyecto PLD  
**Responsable:** ChatGPT / Bill + Codex

## Objetivo

Avanzar y cerrar todos los pendientes que pueden desarrollarse con reglas ya confirmadas, dejando bloqueado únicamente lo que dependa de respuestas, layouts, muestras, umbrales o decisiones del cliente.

## Paquetes incluidos

### A. Correcciones operativas
- corregir entidad/estado de nacimiento;
- marcar y validar campos obligatorios según backend existente;
- mejorar selección e identificación de cliente/crédito/producto en pagos.

### B. Listas PLD / PEP
- revisar y reutilizar estructuras existentes;
- implementar base técnica para carga manual, histórico y consulta cuando pueda hacerse sin inventar reglas de coincidencia;
- no definir umbrales de similitud ni bloqueo automático sin criterio del cliente.

### C. Investigación de alertas
- ampliar el flujo existente de alertas para investigación, comentarios y resolución trazable;
- reutilizar bitácora y estatus existentes;
- no inventar criterios regulatorios ni de dictaminación.

### D. Documentación y preparación
- actualizar SQL incremental, estructura BD, README, proyecto y bitácora cuando aplique;
- no ejecutar compilación ni pruebas de ambiente hasta que termine el alcance desarrollable, por instrucción del solicitante.

## Bloqueos externos que deben quedar como únicos pendientes

- reglas financieras de Fase 3;
- umbrales PLD de perfil transaccional y reglas avanzadas de disposiciones;
- formato/muestra de estado de cuenta;
- layouts/instructivos TXT regulatorios;
- criterios institucionales de coincidencia/acción para listas cuando no estén definidos;
- integración con proveedor externo de listas;
- ejecución final de compilación, migraciones y QA al terminar todo.

## Criterio de terminado

La tarea se marca COMPLETADO cuando no quede desarrollo identificable que pueda implementarse responsablemente con la información ya confirmada en el repositorio y requerimientos recibidos.


---

## Resultado de cierre

Se agotó el alcance identificable que podía implementarse sin nuevas decisiones del cliente.

### Completado

- corrección `entidad_nacimiento_id` → `estado_nacimiento_id`;
- obligatorios de identidad alineados con backend/BD;
- CURP obligatoria conforme al esquema real;
- persistencia real del checkbox PEP en `cliente_persona_fisica.puesto_politico`;
- UX de selección de pagos con producto y tipo de crédito;
- corrección de clasificación `NO EFECTIVO`;
- listas manuales PLD/PEP versionadas con XLSX/CSV;
- consulta exacta y auditoría de listas;
- integración de listas con Identidad;
- investigación de alertas con comentario, conclusión e historial;
- preflight integral adicional;
- README, estructura de BD y proyecto sincronizados.

### Únicos pendientes funcionales

Quedan solamente asuntos que requieren definición/insumo del cliente:

1. Fase 3 financiera del revolvente.
2. Umbrales PLD del perfil transaccional.
3. Definición de perfil esperado de disposiciones y reglas avanzadas.
4. Estado de cuenta: muestra y formato.
5. TXT regulatorios: layouts/instructivos/ejemplos vigentes.
6. Listas/PEP: similitud, homónimos, bloqueo/autorización y proveedor externo.
7. Relación institucional entre resultado de investigación y estatus de alerta, si desean automatizarla.

### Validación diferida por instrucción del solicitante

Compilación, aplicación de migraciones y QA integral se ejecutarán al final, una vez incorporadas las respuestas del cliente.
