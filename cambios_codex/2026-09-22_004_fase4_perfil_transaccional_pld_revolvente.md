# Fase 4 — Perfil transaccional y PLD del crédito revolvente

**Estado:** EN_PROGRESO  
**Fecha:** 2026-09-22  
**Solicitante:** Edgar / proyecto PLD  
**Responsable:** ChatGPT / Bill + Codex

## Objetivo

Implementar la base técnica del perfil transaccional esperado del cliente y compararlo contra pagos reales, reutilizando el motor PLD existente y sin inventar umbrales de alerta.

## Reglas confirmadas

- El perfil transaccional se captura con los datos del cliente.
- Debe incluir:
  - número esperado de pagos por mes;
  - monto esperado de pagos por mes.
- Una alerta significa una condición que requiere revisión, no rechazo automático.
- Los umbrales PLD no deben inventarse.
- El perfil esperado de disposiciones todavía no está aprobado y queda fuera de esta implementación.

## Alcance técnico

1. Ampliar `cliente_persona_fisica` con los dos valores de perfil y auditoría.
2. Capturarlos/consultarlos desde la identidad del cliente.
3. Crear comparación mensual de pagos reales vs esperados.
4. Exponer:
   - pagos esperados;
   - monto esperado;
   - pagos reales;
   - monto real;
   - desviación absoluta;
   - desviación porcentual.
5. Integrar un nuevo origen configurable `PERFIL_TRANSACCIONAL` al motor de alertas existente.
6. No insertar reglas ni valores umbral automáticamente.
7. Mostrar resumen informativo del perfil en el módulo revolvente.

## Fuera de alcance

- perfil esperado de disposiciones;
- umbrales nuevos;
- reglas regulatorias nuevas;
- rechazo automático por desviación;
- cálculo financiero de Fase 3.

## SQL

`PLD/SQL/20260922_002_perfil_transaccional_cliente.sql`

## Criterios de aceptación

- perfil guardado y recuperado con cliente;
- valores negativos bloqueados;
- comparación real/esperado reproducible;
- motor de alertas puede evaluar reglas `PERFIL_TRANSACCIONAL` si posteriormente se configuran;
- con cero reglas configuradas no se genera alerta;
- la alerta futura conserva regla, valor detectado y umbral;
- crédito simple y revolvente siguen compartiendo el perfil a nivel cliente;
- documentación sincronizada.

## Pendiente para cierre funcional PLD

El cliente debe definir los umbrales y criterios que convierten una desviación en condición de revisión. Hasta entonces la infraestructura queda disponible pero sin reglas sembradas.
