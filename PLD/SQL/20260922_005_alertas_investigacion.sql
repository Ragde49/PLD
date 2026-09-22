/*
    Investigación trazable de alertas PLD.
    Fecha: 2026-09-22

    Permite documentar análisis y resultado sin imponer
    una equivalencia automática con el estatus operativo de la alerta.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.alertas_pld_investigacion','U') IS NULL
    BEGIN
        CREATE TABLE dbo.alertas_pld_investigacion (
            id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_alertas_pld_investigacion PRIMARY KEY,
            alerta_id INT NOT NULL,
            resultado VARCHAR(30) NOT NULL,
            comentario NVARCHAR(MAX) NOT NULL,
            categoria_alerta NVARCHAR(150) NULL,
            origen_evento NVARCHAR(100) NULL,
            usuario NVARCHAR(100) NOT NULL,
            fecha_investigacion DATETIME2(0) NOT NULL CONSTRAINT DF_alertas_pld_investigacion_fecha DEFAULT(SYSDATETIME()),
            CONSTRAINT FK_alertas_pld_investigacion_alerta FOREIGN KEY(alerta_id) REFERENCES dbo.alertas_pld(id),
            CONSTRAINT CK_alertas_pld_investigacion_resultado CHECK (resultado IN ('EN_ANALISIS','JUSTIFICADA','NO_JUSTIFICADA'))
        );

        CREATE INDEX IX_alertas_pld_investigacion_alerta_fecha
        ON dbo.alertas_pld_investigacion(alerta_id, fecha_investigacion DESC, id DESC);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
