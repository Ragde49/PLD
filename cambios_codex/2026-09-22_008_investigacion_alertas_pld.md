# Investigación y resolución trazable de alertas PLD

**Estado:** EN_PROGRESO  
**Fecha:** 2026-09-22

## Objetivo
Ampliar el flujo de alertas existente para documentar revisión, análisis y resultado justificada/no justificada, preservando historial y comentarios de Cumplimiento.

## Base existente
- bandeja de alertas;
- asignación;
- cambio de estatus;
- resolución actual;
- bitácora histórica;
- categoría, motivo y origen del evento.

## Alcance
- historial independiente de investigación;
- resultado EN_ANALISIS / JUSTIFICADA / NO_JUSTIFICADA;
- comentario obligatorio;
- snapshot de categoría/origen para contexto;
- usuario y fecha;
- integración con bitácora existente;
- consulta y captura desde detalle de alerta.

## Regla de seguridad funcional
Registrar un resultado de investigación **no cambia automáticamente** el estatus operativo de la alerta. No existe todavía una regla confirmada que equipare JUSTIFICADA/NO_JUSTIFICADA con Confirmada/Descartada/Cerrada.

## SQL
PLD/SQL/20260922_005_alertas_investigacion.sql
