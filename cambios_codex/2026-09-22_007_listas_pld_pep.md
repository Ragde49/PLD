# Listas PLD / PEP — base técnica no bloqueada

**Estado:** EN_PROGRESO  
**Fecha:** 2026-09-22

## Objetivo
Implementar la administración y consulta manual de listas PLD/PEP con trazabilidad, sin inventar reglas institucionales de similitud o bloqueo.

## Reglas confirmadas
- Las listas se reciben/cargan manualmente.
- Personas bloqueadas: nombre, RFC y CURP opcional.
- Se requiere búsqueda en listas bloqueadas/PEP.
- No existe proveedor externo contratado.

## Alcance
- versiones históricas por lista;
- una versión vigente seleccionada explícitamente;
- carga XLSX/CSV;
- búsqueda exacta normalizada por nombre/RFC/CURP;
- auditoría de consultas y coincidencias;
- integración de consulta desde identidad del cliente;
- seguridad página-handler.

## Fuera de alcance por cliente
- similitud/fonética;
- porcentaje de coincidencia;
- bloqueo automático;
- criterio para descartar/confirmar homónimos;
- integración con proveedor externo.

## SQL
PLD/SQL/20260922_004_listas_pld.sql
