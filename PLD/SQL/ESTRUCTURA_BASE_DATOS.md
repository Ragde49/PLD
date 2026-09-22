# Estructura de Base de Datos — PLD

> Documento canónico de la estructura de base de datos conocida por el repositorio.
>
> **Obligatorio:** todo cambio de esquema realizado por ChatGPT, Codex u otro agente debe actualizar este archivo y crear el script incremental correspondiente en `PLD/SQL/`, conforme a `AGENTS.md`.

## Estado actual de la documentación

El repositorio **no contiene actualmente el DDL completo de toda la base operativa**. Por lo tanto, este documento no pretende inventar ni reconstruir columnas que no estén confirmadas.

La estructura completa de producción debe contrastarse con la base de datos real antes de ejecutar migraciones de alto impacto.

## Conexión utilizada por la aplicación

Nombre lógico de la cadena de conexión:

`PLDConnection`

Motor observado:

**Microsoft SQL Server**

Acceso principal desde código:

- `System.Data.SqlClient.SqlConnection`
- `System.Data.SqlClient.SqlCommand`

## Scripts incrementales actualmente versionados

1. `20260513_seguridad.sql`
2. `20260513_seguridad_pagina_handler.sql`
3. `20260922_001_credito_revolvente.sql`
4. `20260922_002_perfil_transaccional_cliente.sql`
5. `20260922_004_listas_pld.sql`
6. `20260922_005_alertas_investigacion.sql`
7. `20260922_006_preflight_integral_pld.sql` (solo validación)

Estos scripts documentan principalmente la estructura de seguridad y no representan por sí solos la creación completa de la base de datos PLD.

## Objetos de seguridad confirmados por scripts versionados

- `catalogo_roles_permisos`
- `catalogo_puestos`
- `seguridad_usuarios`
- `seguridad_paginas`
- `seguridad_menu`
- `seguridad_rol_pagina`
- `seguridad_pagina_handler`

## Objetos operativos confirmados por uso en el código

Los siguientes objetos aparecen utilizados directamente por handlers o módulos del sistema. Su existencia está confirmada por el código, pero este repositorio no contiene todavía su DDL completo:

### Crédito y cliente

- `solicitud_credito`
- `cliente_persona_fisica`
- `contacto_solicitud`
- `contacto_solicitud_telefono`
- `contacto_solicitud_email`
- `contacto_solicitud_domicilio`
- `referencias`
- `pagos_credito`
- `credito_disposiciones`
- `vw_credito_revolvente_saldo`
- `vw_cliente_perfil_transaccional_mensual`

### Producto financiero

- `catalogo_producto_financiero`
- `catalogo_producto_financiero_periodos`
- `detalles_del_producto`
- `producto_planeacion`
- `vw_producto_financiero_completo`

### PLD y alertas

- `solicitud_pld_detalle`
- `vw_pld_factores`
- `alertas_pld`
- `alertas_pld_bitacora`
- `alertas_pld_investigacion`
- `catalogo_listas_pld`
- `listas_pld_cargas`
- `listas_pld_personas`
- `listas_pld_consultas`
- `listas_pld_consulta_resultados`
- `catalogo_alerta_categoria`
- `catalogo_alerta_motivo`
- `catalogo_alerta_regla`
- `config_umbrales_pld`
- `config_puntaje_categoria`
- `config_peso_cliente_pf`
- `config_peso_cliente_pm`
- `config_peso_producto`
- `config_peso_zona`
- `config_peso_general`
- `config_peso_alertas`
- `config_peso_transacciones`
- `vw_pagos_credito_pld`
- `vw_pagos_credito_pld_mensual`
- `vw_pagos_credito_pld_credito`
- `vw_pagos_credito_pld_cliente_periodo`

### Catálogos observados

- `catalogo_paises`
- `catalogo_estados`
- `catalogo_municipios`
- `catalogo_nacionalidad` / `catalogo_nacionalidades` según módulo existente
- `catalogo_ocupacion`
- `catalogo_actividad_economica`
- `catalogo_origen_recursos`
- `catalogo_destino_recursos`
- `catalogo_moneda_divisa`
- `catalogo_canal_pago`
- `catalogo_tipo_pago`
- `catalogo_aplicacion_pago`
- otros catálogos expuestos por handlers del proyecto

> Nota: cuando exista discrepancia de nombre entre módulos, se debe verificar contra SQL Server antes de normalizar o renombrar. No corregir nombres por intuición.

## Regla de mantenimiento

Cada migración nueva debe agregar aquí, según corresponda:

- objeto afectado;
- columnas nuevas/modificadas/eliminadas;
- tipo de dato;
- nullability;
- default;
- PK/FK;
- índices y constraints relevantes;
- vistas/procedimientos/funciones afectados;
- dependencias;
- script incremental que introdujo el cambio;
- fecha del cambio.

## Historial de estructura documentada

### 2026-09-22

Se crea este documento como referencia canónica. No se realizaron cambios de esquema en esta fecha; únicamente se formalizó la obligación de mantener sincronizados:

**código + SQL incremental + estructura documentada + README (cuando aplique) + bitácora**.


### 2026-09-22 — Crédito revolvente

Script: `20260922_001_credito_revolvente.sql`

- `catalogo_creditos.es_revolvente BIT NOT NULL DEFAULT 0`.
- `solicitud_credito.monto_autorizado DECIMAL(18,2) NULL`.
- `solicitud_credito.fecha_vigencia_inicio DATE NULL`.
- `solicitud_credito.fecha_vigencia_fin DATE NULL`.
- Nueva tabla `credito_disposiciones`, relacionada con `solicitud_credito` y `catalogo_moneda_divisa`, con estatus APLICADA/REVERSADA y auditoría.
- Índices por solicitud/fecha y solicitud/estatus.
- Nueva vista `vw_credito_revolvente_saldo` para capital dispuesto, capital amortizado, saldo utilizado y disponible.
- Saldo utilizado y disponible son valores derivados, no fuentes de verdad almacenadas.


### 2026-09-22 — Perfil transaccional del cliente

Script: `20260922_002_perfil_transaccional_cliente.sql`

Cambios en `cliente_persona_fisica`:

- `perfil_pagos_mensuales_esperados INT NULL`.
- `perfil_monto_mensual_esperado DECIMAL(18,2) NULL`.
- `perfil_transaccional_modificado_por NVARCHAR(100) NULL`.
- `perfil_transaccional_fecha_modificacion DATETIME2(0) NULL`.
- Constraints para impedir valores negativos en cantidad/monto esperados.

Nueva vista `vw_cliente_perfil_transaccional_mensual`:

- agrega pagos aplicados por cliente, año y mes;
- expone cantidad y monto esperados;
- expone cantidad y monto reales;
- calcula desviaciones absolutas y porcentuales;
- no contiene umbrales ni clasificación PLD.


### 2026-09-22 — Listas PLD / PEP

Script: `20260922_004_listas_pld.sql`

Nuevos objetos:

- `catalogo_listas_pld`: catálogo de tipos de lista. Se crean las claves confirmadas `BLOQUEADAS` y `PEP`.
- `listas_pld_cargas`: histórico de archivos/versiones, fuente, fecha de recepción, SHA-256, conteo y bandera `vigente`.
- `listas_pld_personas`: registros asociados a cada carga con nombre/RFC/CURP y valores normalizados.
- `listas_pld_consultas`: auditoría de cada consulta.
- `listas_pld_consulta_resultados`: detalle de coincidencias devueltas.
- Índices por lista/versión y por nombre, RFC y CURP normalizados.
- Registro de seguridad de `/secure/listas_pld.aspx` y `/handlers/handler_listas_pld.ashx`.

La búsqueda implementada es exacta sobre valores normalizados. No existe porcentaje de similitud, búsqueda fonética ni bloqueo automático.

### 2026-09-22 — Investigación de alertas PLD

Script: `20260922_005_alertas_investigacion.sql`

Nueva tabla `alertas_pld_investigacion`:

- `id BIGINT IDENTITY` PK.
- `alerta_id INT NOT NULL` FK a `alertas_pld(id)`.
- `resultado VARCHAR(30) NOT NULL`, limitado a `EN_ANALISIS`, `JUSTIFICADA` o `NO_JUSTIFICADA`.
- `comentario NVARCHAR(MAX) NOT NULL`.
- `categoria_alerta NVARCHAR(150) NULL`.
- `origen_evento NVARCHAR(100) NULL`.
- `usuario NVARCHAR(100) NOT NULL`.
- `fecha_investigacion DATETIME2(0) NOT NULL`.
- Índice por alerta y fecha.

El resultado de investigación no modifica automáticamente `alertas_pld.estatus_alerta`; el estatus operativo y la conclusión de investigación permanecen desacoplados.
