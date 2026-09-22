# Fase 3 — Reglas financieras de crédito revolvente

**Estado:** BLOQUEADO  
**Fecha:** 2026-09-22  
**Solicitante:** Edgar / proyecto PLD  
**Responsable:** ChatGPT / Bill + Codex  
**Objetivo:** definir e implementar las reglas financieras del crédito revolvente sin inventar fórmulas ni reutilizar reglas de crédito simple que no correspondan.

## Base existente confirmada

- `catalogo_producto_financiero_periodos.tasa_interes` existe y el código actual la trata como fracción: ejemplo 0.21 = 21%.
- `solicitud_credito.tasa_entrada` existe y el código actual la trata como porcentaje: ejemplo 21 = 21%.
- `pagos_credito` ya maneja componentes separados:
  - `monto_capital`
  - `monto_interes`
  - `monto_iva`
  - `monto_moratorio`
  - `monto_comisiones`
  - `monto_otros`
- `detalles_del_producto` ya contiene parámetros financieros usados por crédito simple, incluyendo IVA y diversas comisiones. No se asumirá que aplican al revolvente hasta confirmarlo.
- El saldo revolvente estructural ya se calcula como disposiciones aplicadas menos capital amortizado.

## Definiciones necesarias antes de programar

1. **Tasa ordinaria**
   - ¿Se usa la tasa de la solicitud/producto actual?
   - ¿Es una sola tasa para toda la línea o cada disposición conserva una tasa propia?
   - Si la tasa del producto cambia después, ¿las líneas existentes conservan la tasa contratada?

2. **Base de cálculo del interés**
   - saldo insoluto diario;
   - saldo promedio diario;
   - saldo al corte;
   - por disposición;
   - otra regla.

3. **Convención de días**
   - año de 360 o 365 días;
   - días naturales o alguna otra convención.

4. **Fecha de corte**
   - día fijo del mes;
   - fecha basada en apertura;
   - periodicidad distinta;
   - qué ocurre cuando el día no existe en un mes.

5. **Fecha límite de pago**
   - número de días después del corte;
   - días naturales o hábiles;
   - tratamiento de fin de semana/inhábil.

6. **Pago mínimo**
   - fórmula exacta;
   - si incluye capital, interés, IVA, comisiones y mora;
   - mínimo monetario absoluto, si existe.

7. **IVA**
   - porcentaje aplicable;
   - sobre qué conceptos se calcula;
   - si debe usar el IVA ya configurado en `detalles_del_producto`.

8. **Interés moratorio**
   - tasa/fórmula;
   - evento que activa mora;
   - base sobre la que se calcula;
   - desde qué fecha y hasta qué fecha;
   - si genera IVA.

9. **Prelación de pagos**
   - orden exacto de aplicación entre moratorio, comisiones, IVA, interés ordinario, capital y otros.

10. **Comisiones**
    - cuáles de las comisiones existentes aplican al revolvente;
    - cuándo se cobran;
    - si llevan IVA;
    - si se cargan a la línea o se cobran aparte.

11. **Pagos mayores al saldo exigible/utilizado**
    - rechazar;
    - aceptar solo hasta el adeudo y devolver excedente;
    - mantener saldo a favor;
    - otra regla.

12. **Pagos anticipados**
    - si reducen únicamente capital;
    - si liberan disponible inmediatamente;
    - si existe alguna comisión/restricción.

13. **Redondeo**
    - confirmar 2 decimales para importes finales;
    - precisión intermedia de intereses;
    - regla de redondeo.

14. **Ejemplo de control**
    Proporcionar al menos un caso esperado, aunque sea manual:
    - línea autorizada;
    - disposiciones y fechas;
    - tasa;
    - corte;
    - pago;
    - interés esperado;
    - IVA esperado;
    - pago mínimo esperado.

## Criterio para desbloquear

La fase se cambia de `BLOQUEADO` a `EN_PROGRESO` cuando estén confirmadas como mínimo:

- tasa y conservación de tasa;
- base de cálculo;
- convención de días;
- corte;
- fecha límite;
- pago mínimo;
- IVA;
- mora;
- prelación;
- tratamiento de excedentes;
- redondeo.

No se implementará ninguna fórmula por inferencia.
