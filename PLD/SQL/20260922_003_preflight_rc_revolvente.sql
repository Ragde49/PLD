/*
    Preflight de Release Candidate — Crédito revolvente / PLD.
    Fecha: 2026-09-22

    USO:
    1. Ejecutar después de las migraciones requeridas.
    2. No modifica datos ni estructura.
    3. Si una validación crítica falla, termina con THROW.
*/

SET NOCOUNT ON;

DECLARE @errores TABLE (
    id INT IDENTITY(1,1) PRIMARY KEY,
    validacion NVARCHAR(200) NOT NULL,
    detalle NVARCHAR(1000) NOT NULL
);

DECLARE @objetos TABLE (nombre SYSNAME NOT NULL, tipo CHAR(2) NOT NULL);
INSERT INTO @objetos(nombre,tipo)
VALUES
('dbo.catalogo_creditos','U'),
('dbo.catalogo_producto_financiero','U'),
('dbo.catalogo_moneda_divisa','U'),
('dbo.solicitud_credito','U'),
('dbo.pagos_credito','U'),
('dbo.cliente_persona_fisica','U'),
('dbo.seguridad_paginas','U'),
('dbo.seguridad_pagina_handler','U'),
('dbo.seguridad_rol_pagina','U'),
('dbo.credito_disposiciones','U'),
('dbo.vw_credito_revolvente_saldo','V'),
('dbo.vw_cliente_perfil_transaccional_mensual','V');

INSERT INTO @errores(validacion,detalle)
SELECT N'OBJETO_FALTANTE', N'No existe ' + nombre + N' (' + tipo + N').'
FROM @objetos o
WHERE OBJECT_ID(o.nombre,o.tipo) IS NULL;

DECLARE @columnas TABLE (tabla SYSNAME NOT NULL, columna SYSNAME NOT NULL);
INSERT INTO @columnas(tabla,columna)
VALUES
('dbo.catalogo_creditos','es_revolvente'),
('dbo.solicitud_credito','monto_autorizado'),
('dbo.solicitud_credito','fecha_vigencia_inicio'),
('dbo.solicitud_credito','fecha_vigencia_fin'),
('dbo.cliente_persona_fisica','perfil_pagos_mensuales_esperados'),
('dbo.cliente_persona_fisica','perfil_monto_mensual_esperado'),
('dbo.cliente_persona_fisica','perfil_transaccional_modificado_por'),
('dbo.cliente_persona_fisica','perfil_transaccional_fecha_modificacion');

INSERT INTO @errores(validacion,detalle)
SELECT N'COLUMNA_FALTANTE', N'No existe ' + tabla + N'.' + columna + N'.'
FROM @columnas
WHERE COL_LENGTH(tabla,columna) IS NULL;

IF OBJECT_ID('dbo.seguridad_paginas','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM dbo.seguridad_paginas
        WHERE ruta='/secure/credito_revolvente.aspx'
          AND ISNULL(activo,0)=1
          AND ISNULL(es_handler,0)=0
    )
    INSERT INTO @errores(validacion,detalle)
    VALUES(N'SEGURIDAD_PAGINA',N'No está activa /secure/credito_revolvente.aspx como página.');

    IF NOT EXISTS (
        SELECT 1 FROM dbo.seguridad_paginas
        WHERE ruta='/handlers/handler_credito_revolvente.ashx'
          AND ISNULL(activo,0)=1
          AND ISNULL(es_handler,0)=1
    )
    INSERT INTO @errores(validacion,detalle)
    VALUES(N'SEGURIDAD_HANDLER',N'No está activo /handlers/handler_credito_revolvente.ashx como handler.');
END;

IF OBJECT_ID('dbo.seguridad_paginas','U') IS NOT NULL
   AND OBJECT_ID('dbo.seguridad_pagina_handler','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.seguridad_pagina_handler ph
        INNER JOIN dbo.seguridad_paginas p ON p.id=ph.pagina_id
        INNER JOIN dbo.seguridad_paginas h ON h.id=ph.handler_id
        WHERE p.ruta='/secure/credito_revolvente.aspx'
          AND h.ruta='/handlers/handler_credito_revolvente.ashx'
          AND ISNULL(ph.activo,0)=1
    )
    INSERT INTO @errores(validacion,detalle)
    VALUES(N'SEGURIDAD_RELACION',N'No existe relación activa página-handler para Crédito Revolvente.');

    IF EXISTS (
        SELECT ph.pagina_id,ph.handler_id
        FROM dbo.seguridad_pagina_handler ph
        WHERE ISNULL(ph.activo,0)=1
        GROUP BY ph.pagina_id,ph.handler_id
        HAVING COUNT(*)>1
    )
    INSERT INTO @errores(validacion,detalle)
    VALUES(N'SEGURIDAD_DUPLICADA',N'Existen relaciones página-handler activas duplicadas.');
END;

IF COL_LENGTH('dbo.cliente_persona_fisica','perfil_pagos_mensuales_esperados') IS NOT NULL
   AND COL_LENGTH('dbo.cliente_persona_fisica','perfil_monto_mensual_esperado') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1 FROM dbo.cliente_persona_fisica
        WHERE perfil_pagos_mensuales_esperados < 0
           OR perfil_monto_mensual_esperado < 0
    )
    INSERT INTO @errores(validacion,detalle)
    VALUES(N'PERFIL_NEGATIVO',N'Existen clientes con perfil transaccional esperado negativo.');
END;

IF OBJECT_ID('dbo.vw_credito_revolvente_saldo','V') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1 FROM dbo.vw_credito_revolvente_saldo
        WHERE saldo_utilizado < 0
           OR (monto_autorizado IS NOT NULL AND saldo_utilizado > monto_autorizado)
           OR (disponible IS NOT NULL AND disponible < 0)
    )
    INSERT INTO @errores(validacion,detalle)
    VALUES(N'SALDO_REVOLVENTE',N'Existen líneas con saldo utilizado negativo, superior al límite o disponible negativo.');
END;

IF OBJECT_ID('dbo.credito_disposiciones','U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1 FROM dbo.credito_disposiciones
        WHERE monto <= 0
           OR estatus NOT IN (N'APLICADA',N'REVERSADA')
    )
    INSERT INTO @errores(validacion,detalle)
    VALUES(N'DISPOSICION_INVALIDA',N'Existen disposiciones con monto/estatus inválido.');

    IF EXISTS (
        SELECT 1
        FROM dbo.credito_disposiciones d
        INNER JOIN dbo.solicitud_credito sc ON sc.id=d.solicitud_credito_id
        WHERE d.activo=1
          AND d.estatus=N'APLICADA'
          AND (
              (sc.fecha_vigencia_inicio IS NOT NULL AND CAST(d.fecha_disposicion AS date)<sc.fecha_vigencia_inicio)
              OR
              (sc.fecha_vigencia_fin IS NOT NULL AND CAST(d.fecha_disposicion AS date)>sc.fecha_vigencia_fin)
          )
    )
    INSERT INTO @errores(validacion,detalle)
    VALUES(N'DISPOSICION_FUERA_VIGENCIA',N'Existen disposiciones activas fuera de la vigencia configurada.');
END;

IF EXISTS (SELECT 1 FROM @errores)
BEGIN
    SELECT id,validacion,detalle FROM @errores ORDER BY id;
    THROW 51000, 'PRECHECK RC REVOLVENTE FALLIDO. Revisar el conjunto de resultados.', 1;
END;

SELECT CAST(1 AS bit) AS ok,
       N'PRECHECK RC REVOLVENTE OK' AS resultado,
       SYSDATETIME() AS fecha_validacion;
