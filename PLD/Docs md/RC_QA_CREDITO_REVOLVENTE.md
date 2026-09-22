# Instalación y checklist RC-QA — Crédito Revolvente

**Fecha:** 2026-09-22  
**Alcance:** RC operativa/técnica del crédito revolvente.  
**Importante:** esta guía no sustituye las reglas financieras pendientes de Fase 3 ni los formatos pendientes de Fases 5/6.

## 1. Prerrequisitos

- Windows con Visual Studio compatible con ASP.NET Web Forms.
- .NET Framework 4.5.2 Developer/Targeting Pack.
- Restauración de paquetes NuGet.
- IIS Express o IIS configurado.
- SQL Server con la base PLD existente.
- Cadena de conexión local denominada PLDConnection.
- Respaldo de la base antes de aplicar migraciones.
- Usuario de prueba administrador.
- Usuario de prueba con permiso.
- Usuario de prueba sin permiso.

## 2. Orden de instalación SQL

En ambientes donde la seguridad base ya existe:

1. PLD/SQL/20260922_001_credito_revolvente.sql
2. PLD/SQL/20260922_002_perfil_transaccional_cliente.sql
3. PLD/SQL/20260922_003_preflight_rc_revolvente.sql

En un ambiente nuevo que todavía no tenga seguridad inicializada, ejecutar previamente:

1. PLD/SQL/20260513_seguridad.sql
2. PLD/SQL/20260513_seguridad_pagina_handler.sql

El preflight es solo lectura y debe terminar con:

PRECHECK RC REVOLVENTE OK

Si falla, no liberar a QA hasta corregir la causa.

## 3. Compilación obligatoria

1. Restaurar paquetes NuGet.
2. Abrir PLD.sln.
3. Compilar en Debug.
4. Compilar en Release.
5. Confirmar cero errores introducidos por la entrega.

Registrar versión de Visual Studio, fecha, resultado, errores/warnings relevantes y SHA probado.

## 4. Smoke test revolvente

Usar un producto marcado como revolvente.

### RV-01 Producto/solicitud
- Crear o editar producto revolvente.
- Capturar solicitud.
- Confirmar que la UI indica producto revolvente.
- Confirmar que no se muestra ni ejecuta amortización fija CONDUSEF.
- Finalizar solicitud.

Esperado: solo una solicitud FINALIZADA puede configurar línea y registrar disposiciones.

### RV-02 Configurar línea
Ejemplo:
- límite: 100000.00;
- vigencia válida.

Esperado: utilizado 0.00 / disponible 100000.00.

### RV-03 Primera disposición
- Disponer 20000.00.

Esperado: utilizado 20000.00 / disponible 80000.00.

### RV-04 Segunda disposición
- Disponer 30000.00.

Esperado: utilizado 50000.00 / disponible 50000.00.

### RV-05 Sobregiro
- Intentar disponer 50000.01.

Esperado: rechazo; saldos sin cambio.

### RV-06 Pago con capital
- Registrar pago con 10000.00 aplicados a capital.

Esperado: utilizado 40000.00 / disponible 60000.00.

### RV-07 Nueva disposición después de pago
- Disponer 15000.00.

Esperado: utilizado 55000.00 / disponible 45000.00.

### RV-08 Reversa válida
Crear una disposición posterior sin movimientos dependientes y reversarla con motivo.

Esperado: reversa aplicada, trazabilidad conservada y saldos recalculados.

### RV-09 Reversa bloqueada
Intentar reversar una disposición de la que dependan movimientos posteriores.

Esperado: rechazo; historia y saldos sin cambio.

### RV-10 Cancelación válida
Cancelar un pago que no provoque sobregiro ni saldo histórico inválido. Motivo obligatorio.

Esperado: cancelación aplicada y saldos recalculados.

### RV-11 Cancelación bloqueada
Intentar cancelar un pago cuyo disponible recuperado ya haya sido reutilizado de forma incompatible.

Esperado: rechazo.

### RV-12 Fecha retroactiva
Intentar capturar un pago o disposición retroactiva que provoque saldo negativo o sobregiro en un punto intermedio.

Esperado: rechazo por integridad histórica.

### RV-13 Moneda
Intentar aplicar capital a la línea con moneda distinta a la del crédito.

Esperado: rechazo mientras no exista regla aprobada de conversión.

## 5. Perfil transaccional / PLD

### PLD-01 Perfil
Capturar pagos esperados por mes y monto mensual esperado.

Esperado: se guardan y recuperan con el cliente.

### PLD-02 Valores negativos
Intentar guardar cantidad o monto esperado negativo.

Esperado: rechazo en UI, backend y BD.

### PLD-03 Comparación mensual
Registrar pagos y consultar revolvente.

Esperado: mostrar esperado vs real y desviación.

### PLD-04 Sin reglas configuradas
No configurar reglas PERFIL_TRANSACCIONAL.

Esperado: evaluación completa del pago genera cero alertas por este origen.

### PLD-05 Regla configurable
Solo cuando el cliente apruebe un umbral, configurar una regla de prueba en QA.

Esperado: alerta explicable, con valor detectado, umbral y contexto esperado vs real; nunca rechazo automático.

## 6. Regresión crédito simple

### RS-01 Solicitud simple
Crear solicitud de producto no revolvente.

Esperado: comportamiento previo intacto.

### RS-02 Amortización CONDUSEF
Ejecutar tabla del crédito simple.

Esperado: continúa disponible y usa las reglas existentes.

### RS-03 Pago simple
Registrar y cancelar pago simple.

Esperado: conserva su lógica previa; no aplica validaciones exclusivas del revolvente.

### RS-04 Alertas existentes
Ejecutar evaluación completa de pagos.

Esperado: reglas existentes siguen evaluándose.

## 7. Seguridad

### SEG-01 No autenticado
Abrir página y handler sin sesión.

Esperado: login o 401 según patrón actual.

### SEG-02 Rol autorizado
Usuario con acceso correspondiente.

Esperado: página y handler accesibles.

### SEG-03 Rol no autorizado
Usuario autenticado sin permiso.

Esperado: página no autorizada y handler 403.

### SEG-04 Administrador
Administrador.

Esperado: acceso según bypass existente.

## 8. Trazabilidad

Confirmar:
- usuario y fecha de disposición;
- motivo, usuario y fecha de reversa;
- usuario y fecha de pago;
- motivo, usuario y fecha de cancelación;
- perfil transaccional modificado por y fecha.

## 9. Exclusiones conocidas de esta RC

No forman parte de esta RC operativa mientras el cliente no entregue o autorice reglas:

- interés ordinario automático del revolvente;
- fecha de corte automática;
- fecha límite;
- pago mínimo;
- prelación automática;
- mora o interés moratorio automático;
- comisiones específicas del revolvente;
- estado de cuenta contractual o regulatorio final;
- TXT regulatorios;
- umbrales PLD nuevos del perfil transaccional;
- perfil esperado de disposiciones;
- integración externa Quién es Quién.

## 10. Criterio de liberación a QA

Marcar GO únicamente cuando:

- compilación Debug y Release: PASS;
- migraciones 001 y 002 aplicadas en DB de pruebas: PASS;
- preflight 003: PASS;
- smoke test revolvente: PASS;
- regresión simple: PASS;
- seguridad: PASS;
- SHA exacto registrado.

Si cualquiera falla: NO-GO.

## 11. Registro de ejecución

| Campo | Resultado |
| --- | --- |
| SHA | |
| Ambiente | |
| DB | |
| Fecha/hora | |
| Responsable | |
| Build Debug | PENDIENTE |
| Build Release | PENDIENTE |
| Migraciones | PENDIENTE |
| Preflight SQL | PENDIENTE |
| Smoke revolvente | PENDIENTE |
| Regresión simple | PENDIENTE |
| Seguridad | PENDIENTE |
| Decisión GO/NO-GO | NO-GO hasta completar gates |
