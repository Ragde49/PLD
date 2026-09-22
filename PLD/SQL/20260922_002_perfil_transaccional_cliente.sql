/*
    Perfil transaccional esperado del cliente + comparación mensual PLD.
    Fecha: 2026-09-22

    Alcance confirmado:
    - Número esperado de pagos por mes.
    - Monto esperado de pagos por mes.
    - Comparación mensual real vs esperado.
    - Sin sembrar umbrales ni reglas de alerta nuevas.

    IMPORTANTE:
    Esta migración no define cuándo una desviación es inusual.
    Los umbrales deben ser aprobados y configurados posteriormente.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH('dbo.cliente_persona_fisica', 'perfil_pagos_mensuales_esperados') IS NULL
    BEGIN
        ALTER TABLE dbo.cliente_persona_fisica
        ADD perfil_pagos_mensuales_esperados INT NULL;
    END;

    IF COL_LENGTH('dbo.cliente_persona_fisica', 'perfil_monto_mensual_esperado') IS NULL
    BEGIN
        ALTER TABLE dbo.cliente_persona_fisica
        ADD perfil_monto_mensual_esperado DECIMAL(18,2) NULL;
    END;

    IF COL_LENGTH('dbo.cliente_persona_fisica', 'perfil_transaccional_modificado_por') IS NULL
    BEGIN
        ALTER TABLE dbo.cliente_persona_fisica
        ADD perfil_transaccional_modificado_por NVARCHAR(100) NULL;
    END;

    IF COL_LENGTH('dbo.cliente_persona_fisica', 'perfil_transaccional_fecha_modificacion') IS NULL
    BEGIN
        ALTER TABLE dbo.cliente_persona_fisica
        ADD perfil_transaccional_fecha_modificacion DATETIME2(0) NULL;
    END;

    IF OBJECT_ID('dbo.CK_cliente_pf_perfil_pagos_esperados', 'C') IS NULL
    BEGIN
        ALTER TABLE dbo.cliente_persona_fisica
        ADD CONSTRAINT CK_cliente_pf_perfil_pagos_esperados
            CHECK (perfil_pagos_mensuales_esperados IS NULL OR perfil_pagos_mensuales_esperados >= 0);
    END;

    IF OBJECT_ID('dbo.CK_cliente_pf_perfil_monto_esperado', 'C') IS NULL
    BEGIN
        ALTER TABLE dbo.cliente_persona_fisica
        ADD CONSTRAINT CK_cliente_pf_perfil_monto_esperado
            CHECK (perfil_monto_mensual_esperado IS NULL OR perfil_monto_mensual_esperado >= 0);
    END;

    IF OBJECT_ID('dbo.vw_cliente_perfil_transaccional_mensual', 'V') IS NULL
    BEGIN
        EXEC(N'
            CREATE VIEW dbo.vw_cliente_perfil_transaccional_mensual
            AS
            SELECT CAST(0 AS INT) AS cliente_id
            WHERE 1 = 0;
        ');
    END;

    EXEC(N'
        ALTER VIEW dbo.vw_cliente_perfil_transaccional_mensual
        AS
        WITH pagos AS
        (
            SELECT
                pc.cliente_id,
                YEAR(pc.fecha_pago) AS anio,
                MONTH(pc.fecha_pago) AS mes,
                COUNT_BIG(*) AS pagos_realizados,
                CAST(SUM(ISNULL(pc.monto_pago, 0)) AS DECIMAL(18,2)) AS monto_pagado,
                MAX(pc.id) AS pago_credito_id_referencia,
                MAX(pc.solicitud_credito_id) AS solicitud_credito_id_base
            FROM dbo.pagos_credito pc
            WHERE pc.activo = 1
              AND pc.estatus = N''APLICADO''
              AND pc.cliente_id IS NOT NULL
            GROUP BY
                pc.cliente_id,
                YEAR(pc.fecha_pago),
                MONTH(pc.fecha_pago)
        )
        SELECT
            c.id_cliente AS cliente_id,
            p.solicitud_credito_id_base,
            p.pago_credito_id_referencia,
            p.anio,
            p.mes,
            c.perfil_pagos_mensuales_esperados,
            c.perfil_monto_mensual_esperado,
            CAST(p.pagos_realizados AS INT) AS pagos_realizados,
            p.monto_pagado,
            CAST(
                CASE
                    WHEN c.perfil_pagos_mensuales_esperados IS NULL THEN NULL
                    ELSE p.pagos_realizados - c.perfil_pagos_mensuales_esperados
                END AS INT
            ) AS desviacion_pagos_cantidad,
            CAST(
                CASE
                    WHEN c.perfil_pagos_mensuales_esperados IS NULL
                      OR c.perfil_pagos_mensuales_esperados = 0 THEN NULL
                    ELSE ((CAST(p.pagos_realizados AS DECIMAL(18,4))
                          - c.perfil_pagos_mensuales_esperados)
                          / c.perfil_pagos_mensuales_esperados) * 100.0
                END AS DECIMAL(18,4)
            ) AS desviacion_pagos_pct,
            CAST(
                CASE
                    WHEN c.perfil_monto_mensual_esperado IS NULL THEN NULL
                    ELSE p.monto_pagado - c.perfil_monto_mensual_esperado
                END AS DECIMAL(18,2)
            ) AS desviacion_monto,
            CAST(
                CASE
                    WHEN c.perfil_monto_mensual_esperado IS NULL
                      OR c.perfil_monto_mensual_esperado = 0 THEN NULL
                    ELSE ((p.monto_pagado - c.perfil_monto_mensual_esperado)
                          / c.perfil_monto_mensual_esperado) * 100.0
                END AS DECIMAL(18,4)
            ) AS desviacion_monto_pct,
            c.perfil_transaccional_modificado_por,
            c.perfil_transaccional_fecha_modificacion
        FROM pagos p
        INNER JOIN dbo.cliente_persona_fisica c
            ON c.id_cliente = p.cliente_id;
    ');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
