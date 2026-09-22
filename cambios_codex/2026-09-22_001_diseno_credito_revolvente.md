# Cambio: Diseño técnico-funcional de crédito revolvente / cuenta corriente

**Estado:** COMPLETADO  
**Fecha:** 2026-09-22  
**Solicitante:** Edgar / proyecto PLD  
**Responsable/agente:** ChatGPT / Bill  
**Tipo:** ANÁLISIS, DISEÑO E IMPLEMENTACIÓN ESTRUCTURAL APROBADA

## Objetivo

Definir cómo incorporar crédito revolvente al sistema PLD sin romper el crédito simple actual, considerando que una misma línea/contrato vigente puede tener múltiples disposiciones, pagos periódicos de intereses y abonos opcionales a capital.

El diseño fue aprobado y se implementaron las Fases 1 y 2 estructurales. Las reglas financieras no confirmadas permanecen fuera de alcance.

---

## Contexto funcional confirmado

El requerimiento recibido establece que, para un crédito revolvente:

- Un cliente puede tener una línea, por ejemplo, de $100,000.
- Puede disponer parcialmente de ella en diferentes fechas.
- Puede realizar nuevas disposiciones mientras el contrato/línea siga vigente y exista monto disponible.
- Puede pagar mensualmente intereses y, opcionalmente, abonar capital.
- Se requiere consultar el historial de disposiciones y pagos como cargos y abonos.
- Desde la consulta del cliente se debe poder visualizar dicho historial y/o acceder a la consulta de pagos por cliente.
- No debe tratarse como un crédito simple con una sola disposición y una tabla fija de amortización.

No se definieron todavía:

- orden contractual de aplicación del pago;
- forma exacta de cálculo/devengo de intereses;
- fecha de corte;
- pago mínimo;
- vigencia contractual exacta;
- reglas de mora;
- formato del estado de cuenta.

Por lo anterior, **esas reglas no deben inventarse en la implementación**.

---

## Estado actual del sistema

### 1. Solicitud de crédito

`solicitud_credito` funciona hoy como la cabecera operativa del crédito y concentra, entre otros datos usados por código:

- cliente;
- producto financiero;
- periodo del producto;
- moneda;
- monto solicitado;
- plazo;
- tasa;
- canal;
- origen/destino de recursos;
- resultado PLD;
- estatus.

Los pagos actuales se relacionan directamente con `solicitud_credito.id`.

### 2. Producto y tipo de crédito

El catálogo de productos financieros ya posee `tipo_credito_id` y existe `catalogo_creditos`.

Por lo tanto, **no se recomienda crear otro campo de texto para identificar SIMPLE/REVOLVENTE en cada operación**. La modalidad debe resolverse a partir del producto/tipo de crédito configurado.

### 3. Pagos

`pagos_credito` ya conserva información útil para ambos modelos:

- solicitud/crédito;
- cliente;
- monto;
- capital;
- interés;
- IVA;
- moratorio;
- comisiones;
- otros;
- saldo antes/después;
- fecha;
- moneda;
- canal/tipo de pago;
- banderas PLD;
- auditoría.

Esto permite conservar la tabla y el módulo de pagos.

### 4. Limitación actual del saldo

En el buscador de pagos el saldo vigente se obtiene así:

1. último `saldo_despues_pago` aplicado;
2. si no existe pago, `monto_solicitado`.

Ese mecanismo funciona razonablemente para un crédito con una sola disposición, pero **no sirve como fuente de verdad de un revolvente**.

Ejemplo:

- línea: $100,000;
- disposición inicial: $20,000;
- pago a capital: $5,000;
- saldo después del pago: $15,000;
- nueva disposición posterior: $20,000.

El saldo correcto sería $35,000, pero el esquema actual seguiría tomando $15,000 del último pago mientras no exista una nueva lógica de disposiciones.

### 5. No existe actualmente

No se identificó en el repositorio un modelo funcional para:

- línea de crédito;
- disposiciones;
- límite autorizado;
- disponible revolvente;
- historial de cargos por disposición;
- contrato revolvente separado;
- movimientos específicos de una línea revolvente.

---

## Decisión de diseño recomendada

### Conservar `solicitud_credito` como cabecera del crédito/contrato operativo

No se recomienda crear en esta fase una segunda cabecera de “contrato” que duplique cliente, producto, moneda, tasa y estado.

La solicitud finalizada seguirá siendo la referencia principal del crédito para:

- crédito simple;
- crédito revolvente.

Esto mantiene compatibilidad con:

- captura actual;
- pagos;
- PLD;
- alertas;
- consultas;
- relaciones existentes.

### Diferenciar comportamiento por tipo de crédito del producto

Flujo lógico:

`solicitud_credito -> catalogo_producto_financiero -> tipo_credito_id -> catalogo_creditos`

El sistema debe resolver si la modalidad es SIMPLE o REVOLVENTE sin duplicar el tipo en pagos o disposiciones.

---

## Modelo conceptual propuesto

### A. Cabecera actual: `solicitud_credito`

Se conserva.

Para revolvente será necesario evaluar agregar, como mínimo, conceptos equivalentes a:

- monto/límite autorizado;
- fecha inicio de vigencia;
- fecha fin de vigencia.

**No se deben crear aún estos campos.** Antes de la migración se debe confirmar si existen equivalentes en la base real que no estén versionados actualmente.

`monto_solicitado` debe conservar su semántica actual y no debe convertirse silenciosamente en “límite autorizado”.

### B. Nueva entidad necesaria: disposiciones

La arquitectura actual no tiene una estructura que represente correctamente una salida adicional de dinero dentro del mismo contrato.

Se propone una entidad específica de disposiciones, conceptualmente:

`credito_disposiciones`

Campos funcionales mínimos a confirmar al implementar:

- identificador;
- solicitud/crédito al que pertenece;
- fecha de disposición;
- monto;
- moneda, si puede diferir del contrato;
- referencia/folio;
- estatus;
- observaciones;
- usuario de creación;
- fecha de creación;
- usuario/fecha de modificación.

No realizar borrado físico de una disposición aplicada; usar estatus/reverso trazable.

### C. Pagos

Se conserva `pagos_credito`.

No se recomienda obligar a que cada pago apunte a una sola disposición, porque el requerimiento describe pagos sobre la línea utilizada y no liquidación individual de cada disposición.

El desglose:

- capital;
- interés;
- IVA;
- moratorio;
- comisiones;
- otros;

debe seguir siendo trazable en el pago.

### D. Saldo y disponible

Para revolvente, la fuente de verdad recomendada es un cálculo derivado.

**Capital dispuesto vigente**

`SUM(disposiciones aplicadas)`

**Capital amortizado**

`SUM(pagos aplicados.monto_capital)`

**Saldo de capital utilizado**

`capital dispuesto - capital amortizado`

**Disponible**

`límite autorizado - saldo de capital utilizado`

No se recomienda mantener manualmente columnas duplicadas de `saldo_utilizado` y `saldo_disponible` como fuente principal, porque pueden quedar desincronizadas.

Se recomienda una vista/consulta de saldo revolvente cuando se implemente.

### E. Historial tipo cargos y abonos

Primera versión recomendada:

- **Cargo:** disposición aplicada.
- **Abono:** pago aplicado.

El historial puede construirse por consulta unificando disposiciones y pagos ordenados por fecha.

No se recomienda crear todavía una tabla genérica de movimientos únicamente para mostrar el historial.

Si el futuro estado de cuenta requiere registrar:

- intereses devengados;
- comisiones periódicas;
- impuestos;
- ajustes;
- cargos que existan aunque no haya pago;

entonces sí deberá evaluarse un subledger/movimiento formal. Esa decisión debe esperar la muestra del estado de cuenta y las reglas financieras correspondientes.

---

## Regla crítica de compatibilidad

### Crédito simple

Debe conservar exactamente el comportamiento actual hasta que exista una tarea específica que lo modifique:

`solicitud_credito -> pagos_credito`

La amortización CONDUSEF y pagos fijos siguen aplicando al producto simple cuando corresponda.

### Crédito revolvente

Flujo propuesto:

`solicitud_credito / línea -> disposiciones -> pagos_credito`

La amortización fija del crédito simple **no debe ejecutarse automáticamente** para un producto revolvente.

---

## Reglas de negocio propuestas para implementación

Estas reglas son estructurales y no sustituyen condiciones contractuales pendientes.

### Nueva disposición

Antes de aplicar una disposición:

1. Crédito activo.
2. Modalidad configurada como revolvente.
3. Línea dentro de vigencia.
4. Monto > 0.
5. Disponible suficiente.
6. Moneda compatible con el contrato/producto.
7. Usuario autorizado.
8. Registrar auditoría.

Operación:

`monto disposición <= disponible antes de disposición`

La validación y el alta deben ocurrir dentro de una transacción SQL para impedir sobregiros por concurrencia.

### Cancelación/reverso de disposición

- No borrar físicamente.
- Registrar usuario, fecha y motivo.
- Recalcular saldo/disponible a partir de operaciones vigentes.
- No permitir reverso que deje inconsistencias con pagos posteriores sin una regla explícita.

### Pago revolvente

Antes de aplicar:

1. Crédito activo.
2. Cliente corresponde al crédito.
3. Monto válido.
4. No aplicar más capital que el capital utilizado, salvo regla aprobada de saldo a favor/devolución.
5. Conservar motor PLD actual después del pago.

### Aplicación del pago

**PENDIENTE DE DEFINICIÓN FUNCIONAL.**

No se debe programar todavía una prelación automática como:

`moratorio -> comisiones -> interés -> IVA -> capital`

hasta que la institución confirme la regla contractual.

El sistema actual ya tiene campos para los componentes, por lo que puede adaptarse sin destruir el modelo de pagos.

---

## Cambios previstos por módulo

### Captura de solicitud

Para producto SIMPLE:

- sin cambio conceptual.

Para producto REVOLVENTE:

- mostrar límite solicitado;
- posteriormente mostrar límite autorizado y vigencia cuando exista etapa de autorización;
- ocultar/no aplicar tabla fija de amortización si no corresponde.

### Consulta de solicitudes/clientes

Agregar para revolvente:

- límite autorizado;
- capital utilizado;
- disponible;
- vigencia;
- historial de disposiciones;
- historial de pagos.

La pantalla actual `solicitud_pf.aspx` puede evolucionar como punto de entrada sin crear de inicio un segundo catálogo de clientes.

### Pagos

Conservar el buscador actual por:

- cliente;
- RFC;
- CURP;
- solicitud/crédito.

Para revolvente, mostrar en el resumen seleccionado:

- tipo de crédito;
- límite;
- utilizado;
- disponible.

El cálculo de saldo no debe usar únicamente el último `saldo_despues_pago`.

### Alertas PLD

Los pagos seguirán utilizando el motor actual.

Las disposiciones deben quedar disponibles para monitoreo futuro, pero **no se deben inventar reglas PLD nuevas para disposiciones** sin requerimiento aprobado.

### Perfil transaccional

El diseño revolvente afecta el futuro perfil transaccional:

- pagos esperados;
- disposiciones esperadas;
- montos;
- frecuencia.

Por eso conviene diseñar ese punto después de aprobar la estructura revolvente.

### Estado de cuenta

Se pospone el diseño final hasta recibir la muestra solicitada.

El modelo de disposiciones + pagos permitirá construir una primera secuencia de cargos y abonos, pero no se debe asumir que eso cubre el formato regulatorio/contractual requerido.

---

## Alternativas evaluadas

### Alternativa A — Crear una nueva cabecera de línea/contrato

**Ventajas**

- separación semántica entre solicitud y contrato;
- modelo más cercano a algunos core bancarios.

**Desventajas**

- duplicaría cliente, producto, moneda, tasa, PLD y estatus;
- exigiría migrar pagos actuales o manejar dos llaves principales;
- incrementa considerablemente el impacto en UI, handlers y alertas;
- mayor riesgo de retrabajo.

**Conclusión:** no recomendada en esta etapa.

### Alternativa B — Conservar `solicitud_credito` y agregar disposiciones

**Ventajas**

- reutiliza la arquitectura actual;
- crédito simple sigue funcionando;
- pagos siguen asociados al mismo crédito;
- PLD y alertas existentes se conservan;
- menor cambio de esquema y menor riesgo;
- soporta múltiples disposiciones dentro del mismo contrato operativo.

**Desventajas**

- `solicitud_credito` asume también el papel de cuenta/contrato después de finalizarse;
- requerirá documentar claramente la transición de solicitud a crédito activo.

**Conclusión:** **alternativa recomendada**.

---

## Backend recomendado

Cuando se autorice la implementación existen dos opciones.

### Opción 1 — Extender `solicitud_credito_handler.ashx`

Acciones potenciales:

- resumen revolvente;
- listar disposiciones;
- crear disposición;
- cancelar/reversar disposición.

**Ventaja:** menor número de componentes y conserva el contexto del crédito.

**Desventaja:** el handler ya concentra bastante lógica.

### Opción 2 — Handler específico de revolvente

Ejemplo conceptual:

`handler_credito_revolvente.ashx`

**Ventaja:** separación funcional y mantenimiento más claro.

**Desventaja:** requiere alta de seguridad y relación página-handler conforme a `AGENTS.md`.

### Recomendación

Para implementación se recomienda **handler específico de revolvente**, manteniendo `handler_pagos_credito.ashx` para pagos y `solicitud_credito_handler.ashx` para la solicitud/cabecera.

Esto evita aumentar aún más la responsabilidad del handler de solicitudes y hace explícita la lógica de disponibilidad/disposición.

---

## Impacto previsto de base de datos

**No aplicado. Diseño solamente.**

Cuando se apruebe, la migración deberá revisar la base real y probablemente abarcar:

1. ampliación mínima de `solicitud_credito` para datos de autorización/vigencia si no existen;
2. nueva entidad de disposiciones;
3. vista o consulta reproducible de saldo/utilizado/disponible;
4. índices por solicitud, fecha y estatus;
5. registro de la nueva estructura en `PLD/SQL/ESTRUCTURA_BASE_DATOS.md`.

No se debe generar el SQL antes de confirmar los campos reales de producción.

---

## Integridad y concurrencia

El disponible es una condición financiera crítica.

Dos disposiciones simultáneas no deben poder consumir el mismo disponible.

La implementación debe:

1. iniciar transacción;
2. obtener/calcular utilizado con bloqueo apropiado;
3. validar disponible;
4. insertar disposición;
5. confirmar transacción.

No confiar en el valor mostrado en JavaScript como fuente de verdad.

---

## Impacto en los puntos pequeños del documento recibido

### Se pueden resolver antes del revolvente

- corrección de entidad de nacimiento;
- asteriscos/campos obligatorios;
- listas bloqueadas;
- consulta de listas;
- investigación/comentarios/bitácora de alertas.

### Conviene esperar a este diseño/implementación

- simplificación final de captura de pagos;
- saldo mostrado en pagos;
- perfil transaccional contractual;
- estado de cuenta;
- historial financiero cliente;
- reportes que dependan de movimientos;
- presentación definitiva del tipo de crédito en operación/pagos.

---

## Fases recomendadas de implementación

### Fase 1 — Modelo revolvente

- confirmar tipo de crédito REVOLVENTE en catálogo;
- validar campos reales de `solicitud_credito`;
- migración de vigencia/límite si es necesaria;
- entidad de disposiciones;
- cálculo de utilizado/disponible;
- handler seguro de disposiciones.

### Fase 2 — Operación

- captura de disposición;
- consulta de línea;
- historial de disposiciones;
- integrar buscador/consulta por cliente;
- pagos revolventes con saldo derivado correctamente.

### Fase 3 — Cálculo financiero

Solo después de confirmar:

- cálculo de interés;
- fecha de corte;
- pago mínimo;
- prelación de pagos;
- mora;
- reglas de capital.

### Fase 4 — Estado de cuenta

Después de recibir la muestra:

- formato;
- cargos/abonos;
- cortes;
- saldos;
- intereses;
- comisiones;
- generación de documento.

### Fase 5 — PLD ampliado

- perfil transaccional para revolvente;
- alertas por comportamiento de disposiciones/pagos;
- reportes requeridos.

---

## Decisiones que requieren confirmación antes de programar cálculo financiero

1. ¿`monto_solicitado` será la línea solicitada y existirá un `monto_autorizado` distinto?
2. ¿Cuál es la vigencia de la línea y de dónde se obtiene?
3. ¿La tasa es única para toda la línea o puede variar por disposición?
4. ¿Cómo se calcula el interés: saldo diario, promedio, corte u otra metodología?
5. ¿Cuál es la fecha de corte?
6. ¿Existe fecha límite de pago?
7. ¿Cómo se determina el pago mínimo?
8. ¿Cuál es la prelación exacta de aplicación de pagos?
9. ¿Se permite saldo a favor?
10. ¿Qué sucede con una disposición cancelada cuando existen pagos posteriores?
11. ¿Qué cargos/comisiones periódicas debe generar la línea?
12. ¿Qué campos debe mostrar el estado de cuenta?

Estas preguntas **no bloquean el diseño estructural**, pero sí bloquean una implementación financiera completa.

---

## Criterios de aceptación de este diseño

- [x] Preserva el crédito simple actual.
- [x] Reutiliza `solicitud_credito`.
- [x] Reutiliza `pagos_credito`.
- [x] Evita duplicar cabecera de contrato sin necesidad.
- [x] Identifica la necesidad real de una entidad de disposiciones.
- [x] Define saldo utilizado y disponible de forma reproducible.
- [x] Evita usar último pago como saldo de revolvente.
- [x] No inventa reglas de interés/pago mínimo.
- [x] Separa diseño estructural de estado de cuenta.
- [x] Mantiene PLD y crédito como dominios relacionados pero distintos.
- [x] No modifica código ni base de datos hasta aprobación.

---

## Resultado del diseño inicial

**Estado histórico del diseño:** SUPERADO POR IMPLEMENTACIÓN POSTERIOR  
**Commit/PR:** Ver commit que incorpora este documento.

### Archivos modificados

- `cambios_codex/2026-09-22_001_diseno_credito_revolvente.md`
- `BITACORA_CAMBIOS.md`

### SQL aplicado/generado

N/A — análisis y diseño únicamente.

### Estructura de BD actualizada

No aplica. No hubo cambio real de esquema.

### README actualizado

No aplica todavía. El sistema no cambió; solamente se documentó una propuesta pendiente de aprobación.

### Validación realizada

- revisión de `solicitud_credito_handler.ashx.vb`;
- revisión de `handler_pagos_credito.ashx.vb`;
- revisión de `producto_financiero_handler.ashx.vb`;
- revisión de UI de captura de solicitud y pagos;
- revisión de objetos actualmente documentados en `PLD/SQL/ESTRUCTURA_BASE_DATOS.md`;
- búsqueda de estructuras existentes de línea, disposición, contrato y disponible, sin encontrar implementación actual equivalente.

### Pendientes / riesgos

- confirmar reglas financieras listadas en este documento;
- contrastar la base de datos real antes de cualquier migración;
- recibir muestra de estado de cuenta antes de diseñar su formato definitivo;
- la aprobación ya fue otorgada posteriormente; ver sección de implementación ejecutada.


---

## Implementación ejecutada — 2026-09-22

**Estado final:** COMPLETADO para Fases 1 y 2 estructurales.

Implementado: bandera de tipo revolvente, límite autorizado y vigencia, tabla de disposiciones, vista de saldo, handler y pantalla operativa, historial cargos/abonos, integración con Consulta PF y pagos, protección contra sobregiros por concurrencia y contra cancelaciones que invaliden disposiciones posteriores, además del registro de seguridad página-handler.

**SQL:** `PLD/SQL/20260922_001_credito_revolvente.sql`

Pendiente por definición funcional: cálculo de intereses, fecha de corte, pago mínimo, prelación de pagos, mora y estado de cuenta contractual/regulatorio.

**Validación:** revisión estática integral. La migración debe ejecutarse primero en una base de pruebas antes de publicar el código en IIS.
