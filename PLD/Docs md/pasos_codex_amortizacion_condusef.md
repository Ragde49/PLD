# Implementación CONDUSEF: Documento/Tabla de Amortización al final de Captura de Solicitud de Crédito

> **Fuente de orden (obligatoria):** El orden y nombres de columnas deben quedar **exactamente** como en el Excel `Crenor CS.xlsx` (hojas: **Semanal**, **Quincenal**, **Mensual**).  
> **Reglas de negocio ya confirmadas:**  
> - **NO** incluir **comisión por gestión** en la tabla ni en totales.  
> - **NO** amortizar **comisión por apertura** (se cobra **Día 0**).  
> - **SÍ** calcular y mostrar **IVA de la comisión por apertura** (en “Crédito simple”).  

---

## 1) Dónde se muestra
- En la pantalla de **Captura de Solicitud de Crédito** (módulo existente).
- Agregar al **final del formulario** una sección fija:
  - Título: **“Tabla de amortización (CONDUSEF)”**
  - Debe mostrarse **solo cuando exista** una solicitud guardada (tenga `solicitud_id`) y tenga definido `producto_financiero_id` + `producto_financiero_periodo_id`.

---

## 2) Datos que se deben leer (SQL)
Para una solicitud dada `solicitud_credito.id = @solicitud_id`:

### 2.1 De `dbo.solicitud_credito`
- `id`
- `producto_financiero_id`
- `producto_financiero_periodo_id`
- `monto_solicitado`
- `plazo` *(si existe; si no, usar el del periodo)*
- `tasa_entrada` *(si se usa; si no, usar la del periodo)*

### 2.2 De `dbo.catalogo_producto_financiero_periodos` (por `producto_financiero_periodo_id`)
- `tipo_periodo`  *(Mensual / Quincenal / Semanal)*
- `plazo`
- `tasa_interes`

> **Regla:** La periodicidad elegida para calcular **SIEMPRE** se toma de `producto_financiero_periodo_id`.

### 2.3 De `dbo.detalles_del_producto` (por `producto_id = producto_financiero_id`)
Campos mínimos:
- `calculo` *(ej. “SALDOS INSOLUTOS”)*
- `iva`
- `iva_comision`
- `base_calculo` *(ej. 360)*
- `redondear_pago_fijo`
- `centavos_para_redondeo`
- `comision_apertura`
- `amortizar_comision_apertura` *(debe respetarse como switch; para CONDUSEF aquí debe ser 0 / NO amortizar)*
- **Ignorar para tabla:** `comision_gestion`, `amortizar_comision_gestion` (NO se consideran en CONDUSEF)

---

## 3) Reglas exactas de cálculo (CONDUSEF)

### 3.1 Capital a financiar (PV)
- `capital_base = solicitud.monto_solicitado`
- **NO** sumar comisión de apertura al capital (PV) porque **no se amortiza**.

### 3.2 Comisión de apertura (Día 0)
- `comision_apertura_monto = capital_base * (comision_apertura / 100)`
- `iva_comision_apertura = comision_apertura_monto * (iva_comision / 100)`

> Mostrar estos 2 importes en un bloque **“Cargos iniciales (Día 0)”** fuera de la tabla principal.  
> No se distribuyen en los pagos.

### 3.3 Intereses / IVA por periodo (tabla)
- Mantener cálculo de amortización **por saldos insolutos** con pago fijo por periodo.
- `tasa_periodo`:
  - Mensual: `tasa_anual / 12`
  - Quincenal: `tasa_anual / 24`
  - Semanal: `tasa_anual / 52`
- `IVA_interes_periodo = interes_periodo * (iva / 100)`
- **No incluir comisión de gestión** en ningún renglón ni total.

### 3.4 Redondeo
- Si `redondear_pago_fijo = 1`:
  - Redondear el **pago fijo** según `centavos_para_redondeo` (ej. 10 centavos = 0.10).
- Mantener el mismo criterio en el último pago para cerrar saldo.

---

## 4) Orden exacto de columnas (tal cual Excel `Crenor CS.xlsx`)
La tabla a renderizar (en pantalla y/o exportable) debe llevar **exactamente** estas columnas **en este orden**:

1. **Número de Pagos**
2. **Fecha**
3. **Saldo inicial**
4. **Capital + Interés**
5. **Capital**
6. **Interés**
7. **IVA de los Intereses**
8. **Pago Semanal** *(solo en hoja Semanal)* **/ Pago Mensual** *(en Quincenal y Mensual, tal cual el Excel)*
9. **Saldo pendiente de pago**

> Nota crítica: En el Excel `Quincenal` el encabezado aparece como **“Pago Mensual”**; respetar el texto **exacto** del Excel.

---

## 5) Renglones exactos (estructura de la tabla)

### 5.1 Renglón 0 (igual al Excel)
Crear un primer renglón con:
- **Número de Pagos** = 0  
- **Fecha** = vacío o fecha inicio (si el Excel lo deja en blanco, replicarlo)  
- **Saldo inicial** = 0  
- **Capital + Interés** = 0  
- **Capital** = 0  
- **Interés** = 0  
- **IVA de los Intereses** = 0  
- **Pago ...** = 0  
- **Saldo pendiente de pago** = `capital_base`

### 5.2 Renglones 1..N
Para cada periodo `i=1..plazo`:
- **Saldo inicial** = saldo del periodo anterior
- **Interés** = saldo_inicial * tasa_periodo
- **IVA de los Intereses** = Interés * (iva/100)
- **Capital** = pago_fijo - Interés
- **Capital + Interés** = pago_fijo *(sin IVA; esto debe coincidir con el Excel)*
- **Pago ...** = (Capital + Interés) + IVA de los Intereses
- **Saldo pendiente de pago** = saldo_inicial - capital

---

## 6) Fechas (si aplica en captura)
- Si el sistema tiene **fecha de desembolso/inicio**, usarla como base.
- Si no existe aún en solicitud, **mostrar la tabla sin fechas reales** (o con fechas calculadas desde `fecha_creacion`) hasta que se agregue el campo.
- Si aplica `calculo_fecha_exigible = SIGUIENTE_DIA_HABIL`, ajustar sábados/domingo al siguiente lunes (sin calendario de feriados por ahora).

---

## 7) UI/UX requerido en Captura (sin postback)
Al final de la captura:
- Bloque 1: **Cargos iniciales (Día 0)**  
  - Comisión por apertura (monto)  
  - IVA comisión por apertura (monto)  
  - Total cargos iniciales  
- Bloque 2: **Tabla de amortización (CONDUSEF)** con las 9 columnas en el orden anterior.
- Botón: **“Descargar Excel CONDUSEF”** (type="button") que exporte exactamente ese layout.

> Importante: **NO** mostrar comisión por gestión en ninguna parte del documento CONDUSEF.

---

## 8) Criterios de aceptación (QA)
- La tabla generada debe coincidir con `Crenor CS.xlsx` en:
  - Orden y nombres de columnas
  - Existencia del renglón 0
  - Cálculo por periodo (capital/interés/IVA)
  - Totales por fila (Pago = Capital+Interés + IVA)
- Comisión apertura:
  - No aparece amortizada en pagos
  - Se muestra como cargo Día 0 con su IVA
- Comisión gestión:
  - No aparece en tabla ni en totales

---

## 9) Alcance exacto del cambio
- Solo agregar esta sección/documento al final de la **captura de solicitud**.
- No cambiar lógica de otros módulos aún (instrumento crediticio, cuenta corriente, etc.).
