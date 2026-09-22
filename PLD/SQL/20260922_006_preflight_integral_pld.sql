/*
    Preflight integral PLD — pendientes no bloqueados.
    Fecha: 2026-09-22

    Ejecutar después de:
      20260922_001_credito_revolvente.sql
      20260922_002_perfil_transaccional_cliente.sql
      20260922_004_listas_pld.sql
      20260922_005_alertas_investigacion.sql

    Este script no modifica estructura ni datos.
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
('dbo.catalogo_listas_pld','U'),
('dbo.listas_pld_cargas','U'),
('dbo.listas_pld_personas','U'),
('dbo.listas_pld_consultas','U'),
('dbo.listas_pld_consulta_resultados','U'),
('dbo.alertas_pld_investigacion','U');

INSERT INTO @errores(validacion,detalle)
SELECT N'OBJETO_FALTANTE',N'No existe ' + nombre + N'.'
FROM @objetos
WHERE OBJECT_ID(nombre,tipo) IS NULL;

IF OBJECT_ID('dbo.catalogo_listas_pld','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.catalogo_listas_pld WHERE clave='BLOQUEADAS' AND activo=1)
        INSERT INTO @errores(validacion,detalle)
        VALUES(N'LISTA_BLOQUEADAS',N'No existe catálogo activo BLOQUEADAS.');

    IF NOT EXISTS (SELECT 1 FROM dbo.catalogo_listas_pld WHERE clave='PEP' AND activo=1)
        INSERT INTO @errores(validacion,detalle)
        VALUES(N'LISTA_PEP',N'No existe catálogo activo PEP.');
END;

IF OBJECT_ID('dbo.listas_pld_cargas','U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT lista_id
        FROM dbo.listas_pld_cargas
        WHERE activo=1 AND vigente=1
        GROUP BY lista_id
        HAVING COUNT(*)>1
    )
        INSERT INTO @errores(validacion,detalle)
        VALUES(N'LISTA_MULTIPLE_VIGENTE',N'Existe más de una carga vigente para la misma lista.');
END;

IF OBJECT_ID('dbo.alertas_pld_investigacion','U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM dbo.alertas_pld_investigacion
        WHERE resultado NOT IN ('EN_ANALISIS','JUSTIFICADA','NO_JUSTIFICADA')
           OR LTRIM(RTRIM(ISNULL(comentario,'')))=''
    )
        INSERT INTO @errores(validacion,detalle)
        VALUES(N'INVESTIGACION_INVALIDA',N'Existen investigaciones con resultado o comentario inválido.');
END;

IF OBJECT_ID('dbo.seguridad_paginas','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM dbo.seguridad_paginas
        WHERE ruta='/secure/listas_pld.aspx' AND activo=1 AND es_handler=0
    )
        INSERT INTO @errores(validacion,detalle)
        VALUES(N'SEGURIDAD_LISTAS_PAGINA',N'No está registrada/activa la página Listas PLD.');

    IF NOT EXISTS (
        SELECT 1 FROM dbo.seguridad_paginas
        WHERE ruta='/handlers/handler_listas_pld.ashx' AND activo=1 AND es_handler=1
    )
        INSERT INTO @errores(validacion,detalle)
        VALUES(N'SEGURIDAD_LISTAS_HANDLER',N'No está registrado/activo el handler de Listas PLD.');
END;

IF OBJECT_ID('dbo.seguridad_pagina_handler','U') IS NOT NULL
   AND OBJECT_ID('dbo.seguridad_paginas','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.seguridad_pagina_handler ph
        JOIN dbo.seguridad_paginas p ON p.id=ph.pagina_id
        JOIN dbo.seguridad_paginas h ON h.id=ph.handler_id
        WHERE p.ruta='/secure/listas_pld.aspx'
          AND h.ruta='/handlers/handler_listas_pld.ashx'
          AND ph.activo=1
    )
        INSERT INTO @errores(validacion,detalle)
        VALUES(N'SEGURIDAD_LISTAS_RELACION',N'Falta relación activa página-handler para Listas PLD.');

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.seguridad_pagina_handler ph
        JOIN dbo.seguridad_paginas p ON p.id=ph.pagina_id
        JOIN dbo.seguridad_paginas h ON h.id=ph.handler_id
        WHERE p.ruta='/secure/captura_solicitud_credito.aspx'
          AND h.ruta='/handlers/handler_listas_pld.ashx'
          AND ph.activo=1
    )
        INSERT INTO @errores(validacion,detalle)
        VALUES(N'SEGURIDAD_IDENTIDAD_LISTAS',N'Falta relación del handler Listas PLD con Captura de Solicitud.');
END;

IF EXISTS(SELECT 1 FROM @errores)
BEGIN
    SELECT id,validacion,detalle FROM @errores ORDER BY id;
    THROW 51000,'PRECHECK INTEGRAL PLD FALLIDO. Revisar resultados.',1;
END;

SELECT CAST(1 AS bit) AS ok,
       N'PRECHECK INTEGRAL PLD OK' AS resultado,
       SYSDATETIME() AS fecha_validacion;
