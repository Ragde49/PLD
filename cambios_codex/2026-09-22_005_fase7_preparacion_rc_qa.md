# Fase 7 — Preparación de versión candidata para QA

**Estado:** COMPLETADO  
**Fecha:** 2026-09-22  
**Solicitante:** Edgar / proyecto PLD  
**Responsable:** ChatGPT / Bill + Codex

## Objetivo

Dejar el repositorio preparado de forma reproducible para instalar, compilar y validar la RC operativa de crédito revolvente en un ambiente de pruebas.

## Resultado

Se completó en repositorio:

- orden de instalación SQL;
- preflight SQL post-migración;
- checklist de instalación y QA;
- casos de humo del flujo revolvente;
- casos de regresión del crédito simple;
- casos de seguridad;
- validaciones de trazabilidad;
- lista explícita de exclusiones;
- revisión de referencias del proyecto;
- búsqueda de patrones de secretos versionados;
- documentación sincronizada.

## Artefactos

- PLD/SQL/20260922_003_preflight_rc_revolvente.sql
- PLD/Docs md/RC_QA_CREDITO_REVOLVENTE.md

## Orden SQL

Ambiente existente:

1. 20260922_001_credito_revolvente.sql
2. 20260922_002_perfil_transaccional_cliente.sql
3. 20260922_003_preflight_rc_revolvente.sql

Si la seguridad base no existe, ejecutar antes:

1. 20260513_seguridad.sql
2. 20260513_seguridad_pagina_handler.sql

## Gate externo pendiente

La preparación de Fase 7 está cerrada, pero no se declara todavía RC-QA liberada porque desde el conector GitHub no es posible:

- compilar PLD.sln en Visual Studio con .NET Framework 4.5.2;
- ejecutar migraciones contra la instancia SQL Server de pruebas;
- ejecutar pruebas funcionales con usuarios y roles reales.

La decisión GO solo puede emitirse cuando esos gates externos terminen en PASS.

## Exclusiones

Se mantienen fuera de esta RC operativa mientras no existan insumos aprobados:

- reglas financieras automáticas de Fase 3;
- estado de cuenta final de Fase 5;
- TXT regulatorios de Fase 6;
- umbrales nuevos del perfil PLD;
- perfil esperado de disposiciones;
- integración externa Quién es Quién.

## Criterio de cierre

**CUMPLIDO para preparación técnica del repositorio.**  
**GO QA: PENDIENTE de gates externos.**
