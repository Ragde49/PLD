# Plan de cierre — Crédito revolvente hasta versión candidata para QA

**Estado:** EN_PROGRESO  
**Fecha:** 2026-09-22  
**Solicitante:** Edgar / proyecto PLD  
**Responsable:** ChatGPT / Bill + Codex  
**Objetivo:** ordenar el trabajo restante del crédito revolvente hasta llegar a una versión candidata para QA, separando lo ya implementado, lo pendiente por reglas de negocio y lo que no debe bloquear la primera entrega.

---

## 1. Línea base ya implementada

La implementación estructural y operativa básica ya existe en `main`:

- `catalogo_creditos.es_revolvente`.
- `solicitud_credito.monto_autorizado`.
- `solicitud_credito.fecha_vigencia_inicio`.
- `solicitud_credito.fecha_vigencia_fin`.
- Tabla `credito_disposiciones`.
- Vista `vw_credito_revolvente_saldo`.
- Handler `handler_credito_revolvente.ashx`.
- Pantalla `Secure/credito_revolvente.aspx`.
- Configuración de línea.
- Alta y reversa controlada de disposiciones.
- Cálculo derivado de:
  - capital dispuesto;
  - capital amortizado;
  - saldo utilizado;
  - disponible.
- Historial operativo de disposiciones y pagos.
- Integración con Consulta PF.
- Integración del saldo revolvente con Pagos.
- Controles de concurrencia para impedir sobregiros.
- Bloqueos de reversa/cancelación cuando existen movimientos posteriores incompatibles.
- Registro de seguridad página-handler.
- SQL incremental, README, estructura de BD y bitácora actualizados.

Esta línea base **no debe rediseñarse** salvo que una prueba revele un defecto real.

---

## 2. Fase 1 — Cierre técnico de integración

**Estado:** COMPLETADO — 2026-09-22  
**Clasificación:** BLOQUEA RC PARA QA  
**Dependencia de reglas nuevas:** NO

### Objetivo

Cerrar huecos técnicos entre el módulo revolvente y los flujos existentes sin introducir nuevas reglas financieras.

### Trabajo

1. Integrar explícitamente `es_revolvente` en la captura de solicitud.
2. Cuando el producto sea revolvente:
   - identificarlo visualmente;
   - conservar `monto_solicitado` como línea solicitada;
   - no ejecutar ni presentar una tabla de amortización fija como si fuera crédito simple;
   - no alterar el comportamiento del crédito simple.
3. Confirmar que la solicitud finalizada sea el único punto de entrada para configurar límite autorizado y vigencia.
4. Validar navegación:
   - Consulta PF → Crédito Revolvente;
   - Crédito Revolvente → Pagos;
   - retorno a Consulta PF.
5. Revisar autorización:
   - usuario autorizado;
   - usuario no autorizado;
   - handler 401/403 conforme al patrón actual.
6. Revisar que la migración `20260922_001_credito_revolvente.sql` sea segura para ejecución repetida donde sea razonable.
7. Limpiar documentación contradictoria del cambio 001 que todavía conserva texto histórico de estado `PENDIENTE`.

### Resultado

- [x] `es_revolvente` integrado en la captura.
- [x] Producto revolvente identificado visualmente.
- [x] `monto_solicitado` conservado como línea solicitada.
- [x] Amortización fija CONDUSEF oculta/bloqueada en UI para revolvente.
- [x] Backend rechaza `amortizacion_condusef` para revolvente.
- [x] Configuración de línea requiere solicitud `FINALIZADA`.
- [x] Disposiciones requieren solicitud `FINALIZADA`.
- [x] Navegación Consulta PF → Revolvente → Pagos cerrada.
- [x] Migración revisada y endurecida para reejecución de relación página-handler.
- [x] Crédito simple conserva el flujo de amortización actual.

### Criterio de salida

**CUMPLIDO.** El sistema distingue SIMPLE vs REVOLVENTE desde la solicitud y no aplica lógica de amortización fija al revolvente.

---

## 3. Fase 2 — Endurecimiento operativo

**Estado:** COMPLETADO — 2026-09-22  
**Clasificación:** BLOQUEA RC PARA QA  
**Dependencia de reglas nuevas:** NO, salvo casos contractuales no definidos que deben quedar excluidos

### Objetivo

Dejar estable el flujo operativo de línea → disposición → pago → disponible.

### Trabajo

1. Casos de disposición:
   - primera disposición;
   - múltiples disposiciones;
   - monto igual al disponible;
   - intento de sobregiro;
   - disposición fuera de vigencia;
   - línea sin límite configurado;
   - línea inactiva/no finalizada.
2. Casos de reversa:
   - reversa sin movimientos posteriores;
   - bloqueo de reversa con movimientos posteriores;
   - motivo obligatorio;
   - trazabilidad de usuario/fecha.
3. Casos de pago:
   - pago con capital;
   - pago sin capital cuando corresponda operativamente;
   - capital mayor al saldo utilizado;
   - actualización de disponible;
   - cancelación compatible;
   - bloqueo de cancelación que provocaría sobregiro.
4. Confirmar consistencia entre:
   - `credito_disposiciones`;
   - `pagos_credito`;
   - `vw_credito_revolvente_saldo`;
   - UI de revolvente;
   - UI de pagos.
5. Revisar precisión decimal y redondeo de los saldos derivados.
6. Verificar regresión del crédito simple.

### Resultado

- [x] Primera y múltiples disposiciones validadas.
- [x] Disposición igual al disponible permitida.
- [x] Sobregiro bloqueado bajo transacción serializable.
- [x] Disposición fuera de vigencia bloqueada.
- [x] Línea sin límite/vigencia bloqueada.
- [x] Línea no finalizada/inactiva bloqueada.
- [x] Reversa exige motivo.
- [x] Reversa bloqueada cuando hay movimientos posteriores incompatibles.
- [x] Reversa bloqueada si dejaría saldo histórico negativo.
- [x] Cambio de vigencia bloqueado si deja disposiciones aplicadas fuera del periodo.
- [x] Pago a capital validado contra monto del pago, saldo actual y saldo histórico en fecha.
- [x] Pago con capital 0 permitido; no se inventó prelación financiera.
- [x] Componentes monetarios negativos bloqueados.
- [x] Capital revolvente en moneda distinta al crédito bloqueado mientras no exista regla de conversión aprobada.
- [x] Saldo cero de línea revolvente no se interpreta como liquidación contractual.
- [x] Cancelación exige motivo y mantiene auditoría.
- [x] Cancelación bloqueada cuando dejaría la línea fuera de límites en cualquier punto de la historia.
- [x] Saldos posteriores a pago/cancelación devueltos por servidor.
- [x] Listado e historial del handler revolvente validan que la solicitud realmente sea revolvente.
- [x] Importes de principal normalizados a 2 decimales.
- [x] Crédito simple conserva su rama de cálculo y comportamiento existente.

### Criterio de salida

**CUMPLIDO por revisión estática.**

Para una línea dada:

`saldo_utilizado = disposiciones aplicadas - capital amortizado`

`disponible = límite autorizado - saldo_utilizado`

La integridad se valida además sobre la secuencia histórica de movimientos para impedir saldos negativos o sobregiros intermedios.

La compilación y ejecución contra SQL Server siguen reservadas para la preparación de RC/QA porque el repositorio no dispone de CI ni de una instancia SQL accesible desde este conector.

---

## 4. Fase 3 — Reglas financieras del revolvente

**Estado:** BLOQUEADO — esperando definiciones funcionales de 2026-09-22  
**Clasificación:** PENDIENTE POR REGLAS DE NEGOCIO  
**Bloqueo:** BLOQUEA UNA RC FINANCIERA COMPLETA, pero no una RC técnica/operativa si QA acepta explícitamente el alcance excluido.

### Definiciones requeridas antes de programar

1. ¿La tasa es única para toda la línea o por disposición?
2. ¿Cómo se devenga el interés?
   - saldo diario;
   - saldo promedio;
   - corte;
   - otra metodología.
3. Fecha de corte.
4. Fecha límite de pago.
5. Fórmula de pago mínimo.
6. Prelación exacta del pago:
   - moratorio;
   - comisiones;
   - interés;
   - IVA;
   - capital;
   - cualquier otro componente.
7. Regla de mora.
8. Tratamiento de saldo a favor.
9. Comisiones periódicas.
10. Tratamiento de pagos anticipados.
11. Tratamiento de pagos mayores al saldo utilizado.
12. Precisión y redondeo de cada componente.

### Trabajo una vez confirmadas las reglas

- Implementar cálculo reproducible.
- Parametrizar lo que corresponda.
- No depender del cálculo JavaScript para integridad.
- Registrar componentes del pago.
- Generar pruebas numéricas con casos conocidos.
- Documentar fórmulas y ejemplos aprobados.

### Criterio de salida

Un mismo conjunto de entradas debe producir siempre el mismo interés, pago mínimo y aplicación del pago, con explicación auditable.

---

## 5. Fase 4 — Perfil transaccional y PLD del revolvente

**Clasificación:** DESEABLE PARA CIERRE PLD, NO DEBE BLOQUEAR LA PRIMERA RC OPERATIVA DEL CRÉDITO  
**Dependencia:** reglas/umbrales PLD aprobados

### Trabajo

1. Incorporar al perfil transaccional esperado:
   - número esperado de pagos;
   - monto esperado de pagos;
   - cuando se apruebe, número/monto esperado de disposiciones.
2. Comparar comportamiento real vs esperado.
3. Evaluar reglas de alertamiento por:
   - frecuencia;
   - monto;
   - desviación del perfil;
   - patrones de disposiciones/pagos.
4. Toda alerta debe indicar qué dato/regla la originó.
5. No tratar una alerta como rechazo automático del crédito.

### Nota

El motor actual de alertas sobre pagos se conserva. No introducir umbrales nuevos sin aprobación.

---

## 6. Fase 5 — Estado de cuenta

**Clasificación:** NO DEBE BLOQUEAR LA PRIMERA RC OPERATIVA  
**Dependencia:** muestra/formato y reglas financieras confirmadas

### Ya disponible

El sistema ya puede construir un historial operativo básico:

- disposición = cargo;
- pago = abono;
- saldo principal.

### Pendiente

Esperar muestra de estado de cuenta para definir:

- encabezados;
- periodos/cortes;
- saldo inicial/final;
- intereses;
- IVA;
- comisiones;
- pago mínimo;
- fecha límite;
- movimientos;
- formato de generación/impresión.

No llamar “estado de cuenta contractual/regulatorio” al historial operativo actual.

---

## 7. Fase 6 — Reportes regulatorios / TXT

**Clasificación:** NO DEBE BLOQUEAR LA PRIMERA RC DEL CRÉDITO REVOLVENTE  
**Dependencia:** layout vigente confirmado

### Pendiente

- layout exacto aplicable;
- claves;
- periodicidad;
- reglas de inclusión;
- validaciones;
- nombre/formato del archivo;
- proceso de corrección/reenvío.

No inventar TXT CNBV/SITI.

---

## 8. Fase 7 — Preparación de versión candidata para QA

**Clasificación:** BLOQUEA RC PARA QA

### Requisitos mínimos

1. SQL incremental consolidado y con orden de ejecución documentado.
2. Compilación exitosa de `PLD.sln`.
3. Migración aplicada en base de pruebas.
4. Pruebas de humo del flujo:
   - producto revolvente;
   - solicitud;
   - límite/vigencia;
   - disposición;
   - segunda disposición;
   - pago;
   - nueva disposición;
   - reversa válida;
   - reversa bloqueada;
   - cancelación válida;
   - cancelación bloqueada.
5. Regresión:
   - solicitud simple;
   - amortización simple;
   - pago simple;
   - alertas de pagos existentes.
6. Seguridad:
   - administrador;
   - rol autorizado;
   - rol no autorizado;
   - handler 401/403.
7. Confirmar que no existan secretos.
8. README actualizado.
9. `ESTRUCTURA_BASE_DATOS.md` actualizado.
10. Bitácora actualizada.
11. Lista explícita de exclusiones conocidas para QA.
12. Crear commit/tag o referencia inequívoca de la RC.

---

## 9. Qué debe incluir la primera RC para QA

### Obligatorio

- identificación del producto como revolvente;
- solicitud compatible con revolvente;
- límite autorizado;
- vigencia;
- disposiciones;
- reversas controladas;
- saldo utilizado;
- disponible;
- pagos;
- actualización de disponible por capital pagado;
- cancelaciones controladas;
- historial operativo;
- consulta por cliente/crédito;
- seguridad;
- crédito simple sin regresiones.

### Si ya fueron confirmadas las reglas financieras

También incluir:

- interés;
- corte;
- fecha límite;
- pago mínimo;
- prelación;
- mora;
- comisiones.

### Si todavía NO fueron confirmadas

QA debe recibir la RC con exclusión explícita de automatización financiera y validar únicamente el alcance estructural/operativo. No programar reglas supuestas.

---

## 10. Lo que NO debe bloquear la primera entrega

Siempre que quede documentado como fuera de alcance de esa RC:

- formato final del estado de cuenta;
- generación PDF/impresión del estado de cuenta;
- TXT regulatorios;
- integración externa con Quién es Quién;
- alertamiento PLD avanzado sobre disposiciones;
- perfil transaccional avanzado del revolvente;
- reportes ejecutivos adicionales;
- automatizaciones regulatorias que dependan de layouts aún no entregados.

---

## 11. Orden de ejecución recomendado

**Ahora:**

Fase 1 → Fase 2.

**En paralelo, solicitar definiciones para:**

Fase 3.

**Cuando lleguen las reglas:**

Fase 3 → ajustes de Fase 4 que sí estén aprobados.

**Antes de QA:**

Fase 7.

**Después o como incremento independiente si aún no hay insumos:**

Fase 5 → Fase 6 → PLD avanzado restante de Fase 4.

---

## 12. Definición de RC-QA

La versión se considera **candidata para QA** cuando:

- no tiene huecos técnicos conocidos en el flujo revolvente incluido;
- mantiene el crédito simple;
- compila;
- la migración se aplica correctamente en pruebas;
- las operaciones incluidas son trazables;
- el saldo/disponible es reproducible;
- las exclusiones por reglas no recibidas están identificadas;
- la documentación está sincronizada;
- existe un SHA/tag exacto para que QA pruebe siempre la misma versión.

No debe declararse RC-QA antes de compilar y aplicar la migración en un ambiente de pruebas.
