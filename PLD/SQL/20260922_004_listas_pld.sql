/*
    Listas PLD / PEP - carga manual versionada y consulta exacta.
    Fecha: 2026-09-22

    Alcance:
    - Catálogo genérico de listas PLD.
    - Versiones/cargas históricas.
    - Una versión vigente por lista.
    - Personas con NOMBRE, RFC y CURP opcionales.
    - Auditoría de consultas y resultados.
    - Registro de página/handler y permisos.

    NO define:
    - similitud/fonética;
    - porcentaje de coincidencia;
    - bloqueo automático;
    - proveedor externo.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.catalogo_listas_pld','U') IS NULL
    BEGIN
        CREATE TABLE dbo.catalogo_listas_pld (
            id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_catalogo_listas_pld PRIMARY KEY,
            clave VARCHAR(50) NOT NULL,
            nombre NVARCHAR(150) NOT NULL,
            descripcion NVARCHAR(300) NULL,
            activo BIT NOT NULL CONSTRAINT DF_catalogo_listas_pld_activo DEFAULT(1),
            fecha_creacion DATETIME2(0) NOT NULL CONSTRAINT DF_catalogo_listas_pld_fecha DEFAULT(SYSDATETIME()),
            fecha_modificacion DATETIME2(0) NULL
        );

        CREATE UNIQUE INDEX UX_catalogo_listas_pld_clave
        ON dbo.catalogo_listas_pld(clave);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.catalogo_listas_pld WHERE clave='BLOQUEADAS')
        INSERT INTO dbo.catalogo_listas_pld(clave,nombre,descripcion)
        VALUES('BLOQUEADAS',N'Personas bloqueadas',N'Lista manual de personas bloqueadas recibida por la institución.');

    IF NOT EXISTS (SELECT 1 FROM dbo.catalogo_listas_pld WHERE clave='PEP')
        INSERT INTO dbo.catalogo_listas_pld(clave,nombre,descripcion)
        VALUES('PEP',N'Personas Políticamente Expuestas (PEP)',N'Lista manual PEP recibida o mantenida por la institución.');

    IF OBJECT_ID('dbo.listas_pld_cargas','U') IS NULL
    BEGIN
        CREATE TABLE dbo.listas_pld_cargas (
            id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_listas_pld_cargas PRIMARY KEY,
            lista_id INT NOT NULL,
            nombre_archivo NVARCHAR(260) NOT NULL,
            extension VARCHAR(10) NOT NULL,
            referencia_fuente NVARCHAR(250) NULL,
            fecha_recepcion DATE NULL,
            hash_sha256 VARCHAR(64) NULL,
            total_registros INT NOT NULL CONSTRAINT DF_listas_pld_cargas_total DEFAULT(0),
            vigente BIT NOT NULL CONSTRAINT DF_listas_pld_cargas_vigente DEFAULT(0),
            activo BIT NOT NULL CONSTRAINT DF_listas_pld_cargas_activo DEFAULT(1),
            creado_por NVARCHAR(100) NOT NULL,
            fecha_creacion DATETIME2(0) NOT NULL CONSTRAINT DF_listas_pld_cargas_fecha DEFAULT(SYSDATETIME()),
            activado_por NVARCHAR(100) NULL,
            fecha_activacion DATETIME2(0) NULL,
            CONSTRAINT FK_listas_pld_cargas_catalogo FOREIGN KEY(lista_id) REFERENCES dbo.catalogo_listas_pld(id)
        );

        CREATE INDEX IX_listas_pld_cargas_lista_fecha
        ON dbo.listas_pld_cargas(lista_id, fecha_creacion DESC);

        CREATE INDEX IX_listas_pld_cargas_lista_vigente
        ON dbo.listas_pld_cargas(lista_id, vigente, activo);
    END;

    IF OBJECT_ID('dbo.listas_pld_personas','U') IS NULL
    BEGIN
        CREATE TABLE dbo.listas_pld_personas (
            id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_listas_pld_personas PRIMARY KEY,
            carga_id BIGINT NOT NULL,
            nombre NVARCHAR(250) NOT NULL,
            nombre_normalizado NVARCHAR(250) NOT NULL,
            rfc VARCHAR(20) NULL,
            rfc_normalizado VARCHAR(20) NULL,
            curp VARCHAR(30) NULL,
            curp_normalizado VARCHAR(30) NULL,
            fila_origen INT NULL,
            activo BIT NOT NULL CONSTRAINT DF_listas_pld_personas_activo DEFAULT(1),
            fecha_creacion DATETIME2(0) NOT NULL CONSTRAINT DF_listas_pld_personas_fecha DEFAULT(SYSDATETIME()),
            CONSTRAINT FK_listas_pld_personas_carga FOREIGN KEY(carga_id) REFERENCES dbo.listas_pld_cargas(id)
        );

        CREATE INDEX IX_listas_pld_personas_carga
        ON dbo.listas_pld_personas(carga_id, activo);

        CREATE INDEX IX_listas_pld_personas_nombre
        ON dbo.listas_pld_personas(nombre_normalizado)
        INCLUDE(carga_id, nombre, rfc, curp, activo);

        CREATE INDEX IX_listas_pld_personas_rfc
        ON dbo.listas_pld_personas(rfc_normalizado)
        INCLUDE(carga_id, nombre, rfc, curp, activo);

        CREATE INDEX IX_listas_pld_personas_curp
        ON dbo.listas_pld_personas(curp_normalizado)
        INCLUDE(carga_id, nombre, rfc, curp, activo);
    END;

    IF OBJECT_ID('dbo.listas_pld_consultas','U') IS NULL
    BEGIN
        CREATE TABLE dbo.listas_pld_consultas (
            id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_listas_pld_consultas PRIMARY KEY,
            cliente_id INT NULL,
            nombre_consultado NVARCHAR(250) NULL,
            rfc_consultado VARCHAR(20) NULL,
            curp_consultado VARCHAR(30) NULL,
            coincidencias INT NOT NULL CONSTRAINT DF_listas_pld_consultas_coinc DEFAULT(0),
            usuario NVARCHAR(100) NOT NULL,
            fecha_consulta DATETIME2(0) NOT NULL CONSTRAINT DF_listas_pld_consultas_fecha DEFAULT(SYSDATETIME())
        );

        CREATE INDEX IX_listas_pld_consultas_cliente_fecha
        ON dbo.listas_pld_consultas(cliente_id, fecha_consulta DESC);
    END;

    IF OBJECT_ID('dbo.listas_pld_consulta_resultados','U') IS NULL
    BEGIN
        CREATE TABLE dbo.listas_pld_consulta_resultados (
            id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_listas_pld_consulta_resultados PRIMARY KEY,
            consulta_id BIGINT NOT NULL,
            persona_id BIGINT NOT NULL,
            tipo_coincidencia VARCHAR(50) NOT NULL,
            fecha_creacion DATETIME2(0) NOT NULL CONSTRAINT DF_listas_pld_consulta_resultados_fecha DEFAULT(SYSDATETIME()),
            CONSTRAINT FK_listas_pld_consulta_resultados_consulta FOREIGN KEY(consulta_id) REFERENCES dbo.listas_pld_consultas(id),
            CONSTRAINT FK_listas_pld_consulta_resultados_persona FOREIGN KEY(persona_id) REFERENCES dbo.listas_pld_personas(id)
        );

        CREATE INDEX IX_listas_pld_consulta_resultados_consulta
        ON dbo.listas_pld_consulta_resultados(consulta_id);
    END;

    /* Seguridad */
    IF OBJECT_ID('dbo.seguridad_paginas','U') IS NOT NULL
    BEGIN
        MERGE dbo.seguridad_paginas AS target
        USING (
            SELECT '/secure/listas_pld.aspx' AS ruta, 'Listas PLD / PEP' AS titulo, 'PLD' AS modulo, CAST(0 AS bit) AS es_handler
            UNION ALL
            SELECT '/handlers/handler_listas_pld.ashx', 'handler_listas_pld', 'Handler', CAST(1 AS bit)
        ) AS source
        ON target.ruta=source.ruta
        WHEN MATCHED THEN
            UPDATE SET target.titulo=source.titulo,target.modulo=source.modulo,target.es_handler=source.es_handler,target.activo=1,target.fecha_modificacion=GETDATE()
        WHEN NOT MATCHED THEN
            INSERT(clave,titulo,ruta,modulo,es_menu,es_handler,activo,fecha_creacion)
            VALUES(LEFT(REPLACE(REPLACE(source.ruta,'/','_'),'.','_'),120),source.titulo,source.ruta,source.modulo,0,source.es_handler,1,GETDATE());

        DECLARE @paginaListas INT=(SELECT id FROM dbo.seguridad_paginas WHERE ruta='/secure/listas_pld.aspx');
        DECLARE @handlerListas INT=(SELECT id FROM dbo.seguridad_paginas WHERE ruta='/handlers/handler_listas_pld.ashx');
        DECLARE @paginaCatalogos INT=(SELECT id FROM dbo.seguridad_paginas WHERE ruta='/secure/catalogos.aspx');
        DECLARE @paginaSolicitud INT=(SELECT id FROM dbo.seguridad_paginas WHERE ruta='/secure/captura_solicitud_credito.aspx');

        IF OBJECT_ID('dbo.seguridad_pagina_handler','U') IS NOT NULL
        BEGIN
            IF @paginaListas IS NOT NULL AND @handlerListas IS NOT NULL
               AND NOT EXISTS(SELECT 1 FROM dbo.seguridad_pagina_handler WHERE pagina_id=@paginaListas AND handler_id=@handlerListas)
                INSERT INTO dbo.seguridad_pagina_handler(pagina_id,handler_id,activo,fecha_creacion)
                VALUES(@paginaListas,@handlerListas,1,GETDATE());

            IF @paginaSolicitud IS NOT NULL AND @handlerListas IS NOT NULL
               AND NOT EXISTS(SELECT 1 FROM dbo.seguridad_pagina_handler WHERE pagina_id=@paginaSolicitud AND handler_id=@handlerListas)
                INSERT INTO dbo.seguridad_pagina_handler(pagina_id,handler_id,activo,fecha_creacion)
                VALUES(@paginaSolicitud,@handlerListas,1,GETDATE());
        END;

        IF OBJECT_ID('dbo.seguridad_rol_pagina','U') IS NOT NULL AND @paginaCatalogos IS NOT NULL AND @paginaListas IS NOT NULL
        BEGIN
            INSERT INTO dbo.seguridad_rol_pagina(rol_id,pagina_id,puede_ver,puede_crear,puede_editar,puede_eliminar,fecha_creacion)
            SELECT rp.rol_id,@paginaListas,1,0,0,0,GETDATE()
            FROM dbo.seguridad_rol_pagina rp
            WHERE rp.pagina_id=@paginaCatalogos AND rp.puede_ver=1
              AND NOT EXISTS(
                SELECT 1 FROM dbo.seguridad_rol_pagina x
                WHERE x.rol_id=rp.rol_id AND x.pagina_id=@paginaListas
              );
        END;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
