/*
    Credito revolvente / cuenta corriente - Fases 1 y 2.
    Fecha: 2026-09-22

    Objetivos:
    - Marcar tipos de credito que operan como revolventes.
    - Agregar limite autorizado y vigencia a solicitud_credito sin cambiar monto_solicitado.
    - Registrar disposiciones multiples por la misma solicitud/linea.
    - Calcular saldo utilizado y disponible de forma derivada.
    - Registrar pagina/handler y heredar permiso desde Consulta PF.

    IMPORTANTE:
    Este script NO implementa calculo de intereses, pago minimo, prelacion de pagos,
    mora ni estado de cuenta contractual.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH('dbo.catalogo_creditos', 'es_revolvente') IS NULL
    BEGIN
        ALTER TABLE dbo.catalogo_creditos
        ADD es_revolvente BIT NOT NULL
            CONSTRAINT DF_catalogo_creditos_es_revolvente DEFAULT (0);
    END;

    IF COL_LENGTH('dbo.solicitud_credito', 'monto_autorizado') IS NULL
    BEGIN
        ALTER TABLE dbo.solicitud_credito
        ADD monto_autorizado DECIMAL(18,2) NULL;
    END;

    IF COL_LENGTH('dbo.solicitud_credito', 'fecha_vigencia_inicio') IS NULL
    BEGIN
        ALTER TABLE dbo.solicitud_credito
        ADD fecha_vigencia_inicio DATE NULL;
    END;

    IF COL_LENGTH('dbo.solicitud_credito', 'fecha_vigencia_fin') IS NULL
    BEGIN
        ALTER TABLE dbo.solicitud_credito
        ADD fecha_vigencia_fin DATE NULL;
    END;

    IF OBJECT_ID('dbo.CK_solicitud_credito_monto_autorizado', 'C') IS NULL
    BEGIN
        ALTER TABLE dbo.solicitud_credito
        ADD CONSTRAINT CK_solicitud_credito_monto_autorizado
            CHECK (monto_autorizado IS NULL OR monto_autorizado > 0);
    END;

    IF OBJECT_ID('dbo.CK_solicitud_credito_vigencia', 'C') IS NULL
    BEGIN
        ALTER TABLE dbo.solicitud_credito
        ADD CONSTRAINT CK_solicitud_credito_vigencia
            CHECK (
                fecha_vigencia_inicio IS NULL
                OR fecha_vigencia_fin IS NULL
                OR fecha_vigencia_fin >= fecha_vigencia_inicio
            );
    END;

    IF OBJECT_ID('dbo.credito_disposiciones', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.credito_disposiciones (
            id INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_credito_disposiciones PRIMARY KEY,
            solicitud_credito_id INT NOT NULL,
            fecha_disposicion DATETIME2(0) NOT NULL
                CONSTRAINT DF_credito_disposiciones_fecha_disposicion DEFAULT (SYSDATETIME()),
            monto DECIMAL(18,2) NOT NULL,
            moneda_id INT NULL,
            referencia NVARCHAR(100) NULL,
            estatus NVARCHAR(20) NOT NULL
                CONSTRAINT DF_credito_disposiciones_estatus DEFAULT (N'APLICADA'),
            activo BIT NOT NULL
                CONSTRAINT DF_credito_disposiciones_activo DEFAULT (1),
            observaciones NVARCHAR(1000) NULL,
            motivo_reversa NVARCHAR(500) NULL,
            creado_por NVARCHAR(100) NOT NULL,
            fecha_creacion DATETIME2(0) NOT NULL
                CONSTRAINT DF_credito_disposiciones_fecha_creacion DEFAULT (SYSDATETIME()),
            modificado_por NVARCHAR(100) NULL,
            fecha_modificacion DATETIME2(0) NULL,

            CONSTRAINT FK_credito_disposiciones_solicitud
                FOREIGN KEY (solicitud_credito_id)
                REFERENCES dbo.solicitud_credito(id),

            CONSTRAINT FK_credito_disposiciones_moneda
                FOREIGN KEY (moneda_id)
                REFERENCES dbo.catalogo_moneda_divisa(id),

            CONSTRAINT CK_credito_disposiciones_monto
                CHECK (monto > 0),

            CONSTRAINT CK_credito_disposiciones_estatus
                CHECK (estatus IN (N'APLICADA', N'REVERSADA'))
        );
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.credito_disposiciones')
          AND name = 'IX_credito_disposiciones_solicitud_fecha'
    )
    BEGIN
        CREATE INDEX IX_credito_disposiciones_solicitud_fecha
            ON dbo.credito_disposiciones
            (solicitud_credito_id, fecha_disposicion, id);
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.credito_disposiciones')
          AND name = 'IX_credito_disposiciones_solicitud_estatus'
    )
    BEGIN
        CREATE INDEX IX_credito_disposiciones_solicitud_estatus
            ON dbo.credito_disposiciones
            (solicitud_credito_id, activo, estatus)
            INCLUDE (monto);
    END;

    IF OBJECT_ID('dbo.vw_credito_revolvente_saldo', 'V') IS NULL
    BEGIN
        EXEC(N'
            CREATE VIEW dbo.vw_credito_revolvente_saldo
            AS
            SELECT CAST(0 AS INT) AS solicitud_credito_id
            WHERE 1 = 0;
        ');
    END;

    EXEC(N'
        ALTER VIEW dbo.vw_credito_revolvente_saldo
        AS
        SELECT
            sc.id AS solicitud_credito_id,
            sc.cliente_id,
            sc.producto_financiero_id,
            pf.tipo_credito_id,
            c.nombre_credito AS tipo_credito,
            CONVERT(BIT, ISNULL(c.es_revolvente, 0)) AS es_revolvente,
            sc.moneda_id,
            sc.monto_solicitado,
            sc.monto_autorizado,
            sc.fecha_vigencia_inicio,
            sc.fecha_vigencia_fin,
            sc.estatus AS estatus_solicitud,
            sc.activo AS activo_solicitud,
            CAST(ISNULL(d.capital_dispuesto, 0) AS DECIMAL(18,2)) AS capital_dispuesto,
            CAST(ISNULL(p.capital_amortizado, 0) AS DECIMAL(18,2)) AS capital_amortizado,
            CAST(ISNULL(d.capital_dispuesto, 0) - ISNULL(p.capital_amortizado, 0) AS DECIMAL(18,2)) AS saldo_utilizado,
            CAST(
                CASE
                    WHEN sc.monto_autorizado IS NULL THEN NULL
                    ELSE sc.monto_autorizado
                         - (ISNULL(d.capital_dispuesto, 0) - ISNULL(p.capital_amortizado, 0))
                END
                AS DECIMAL(18,2)
            ) AS disponible
        FROM dbo.solicitud_credito sc
        INNER JOIN dbo.catalogo_producto_financiero pf
            ON pf.id = sc.producto_financiero_id
        INNER JOIN dbo.catalogo_creditos c
            ON c.id = pf.tipo_credito_id
        OUTER APPLY (
            SELECT SUM(cd.monto) AS capital_dispuesto
            FROM dbo.credito_disposiciones cd
            WHERE cd.solicitud_credito_id = sc.id
              AND cd.activo = 1
              AND cd.estatus = N''APLICADA''
        ) d
        OUTER APPLY (
            SELECT SUM(ISNULL(pc.monto_capital, 0)) AS capital_amortizado
            FROM dbo.pagos_credito pc
            WHERE pc.solicitud_credito_id = sc.id
              AND pc.activo = 1
              AND pc.estatus = N''APLICADO''
        ) p
        WHERE ISNULL(c.es_revolvente, 0) = 1;
    ');

    -- Seguridad: pagina operativa + handler.
    IF NOT EXISTS (
        SELECT 1 FROM dbo.seguridad_paginas
        WHERE ruta = '/secure/credito_revolvente.aspx'
    )
    BEGIN
        INSERT INTO dbo.seguridad_paginas
            (clave, titulo, ruta, modulo, es_menu, es_handler, activo, fecha_creacion)
        VALUES
            ('secure_credito_revolvente_aspx', 'Credito Revolvente', '/secure/credito_revolvente.aspx',
             'Credito', 0, 0, 1, GETDATE());
    END
    ELSE
    BEGIN
        UPDATE dbo.seguridad_paginas
        SET titulo = 'Credito Revolvente',
            modulo = 'Credito',
            es_menu = 0,
            es_handler = 0,
            activo = 1,
            fecha_modificacion = GETDATE()
        WHERE ruta = '/secure/credito_revolvente.aspx';
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.seguridad_paginas
        WHERE ruta = '/handlers/handler_credito_revolvente.ashx'
    )
    BEGIN
        INSERT INTO dbo.seguridad_paginas
            (clave, titulo, ruta, modulo, es_menu, es_handler, activo, fecha_creacion)
        VALUES
            ('handlers_handler_credito_revolvente_ashx', 'handler_credito_revolvente',
             '/handlers/handler_credito_revolvente.ashx',
             'Handler', 0, 1, 1, GETDATE());
    END
    ELSE
    BEGIN
        UPDATE dbo.seguridad_paginas
        SET titulo = 'handler_credito_revolvente',
            modulo = 'Handler',
            es_menu = 0,
            es_handler = 1,
            activo = 1,
            fecha_modificacion = GETDATE()
        WHERE ruta = '/handlers/handler_credito_revolvente.ashx';
    END;

    DECLARE @paginaRevId INT =
        (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/secure/credito_revolvente.aspx');
    DECLARE @handlerRevId INT =
        (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/handlers/handler_credito_revolvente.ashx');
    DECLARE @consultaPfId INT =
        (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/secure/solicitud_pf.aspx');

    IF @paginaRevId IS NOT NULL AND @handlerRevId IS NOT NULL
       AND NOT EXISTS (
            SELECT 1
            FROM dbo.seguridad_pagina_handler
            WHERE pagina_id = @paginaRevId
              AND handler_id = @handlerRevId
       )
    BEGIN
        INSERT INTO dbo.seguridad_pagina_handler
            (pagina_id, handler_id, activo, fecha_creacion)
        VALUES
            (@paginaRevId, @handlerRevId, 1, GETDATE());
    END
    ELSE IF @paginaRevId IS NOT NULL AND @handlerRevId IS NOT NULL
    BEGIN
        UPDATE dbo.seguridad_pagina_handler
        SET activo = 1,
            fecha_modificacion = GETDATE()
        WHERE pagina_id = @paginaRevId
          AND handler_id = @handlerRevId;
    END;

    -- Heredar acceso desde Consulta PF; no se crean permisos CRUD visibles.
    IF @consultaPfId IS NOT NULL AND @paginaRevId IS NOT NULL
    BEGIN
        INSERT INTO dbo.seguridad_rol_pagina
            (rol_id, pagina_id, puede_ver, puede_crear, puede_editar, puede_eliminar, fecha_creacion)
        SELECT
            rp.rol_id,
            @paginaRevId,
            1,
            0,
            0,
            0,
            GETDATE()
        FROM dbo.seguridad_rol_pagina rp
        WHERE rp.pagina_id = @consultaPfId
          AND rp.puede_ver = 1
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.seguridad_rol_pagina x
              WHERE x.rol_id = rp.rol_id
                AND x.pagina_id = @paginaRevId
          );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
