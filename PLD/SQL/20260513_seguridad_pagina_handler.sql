/*
    Relaciones pagina-handler para permisos PLD.
    Los handlers no se administran en la UI: heredan acceso desde sus paginas.
    Script idempotente: se puede ejecutar mas de una vez.
*/

SET NOCOUNT ON;

IF OBJECT_ID('dbo.seguridad_pagina_handler', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.seguridad_pagina_handler (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_seguridad_pagina_handler PRIMARY KEY,
        pagina_id INT NOT NULL,
        handler_id INT NOT NULL,
        activo BIT NOT NULL CONSTRAINT DF_seguridad_pagina_handler_activo DEFAULT (1),
        fecha_creacion DATETIME NOT NULL CONSTRAINT DF_seguridad_pagina_handler_fecha DEFAULT (GETDATE()),
        fecha_modificacion DATETIME NULL,
        CONSTRAINT FK_seguridad_pagina_handler_pagina FOREIGN KEY (pagina_id) REFERENCES dbo.seguridad_paginas(id),
        CONSTRAINT FK_seguridad_pagina_handler_handler FOREIGN KEY (handler_id) REFERENCES dbo.seguridad_paginas(id)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_seguridad_pagina_handler_activo'
      AND object_id = OBJECT_ID('dbo.seguridad_pagina_handler')
)
BEGIN
    CREATE UNIQUE INDEX UX_seguridad_pagina_handler_activo
    ON dbo.seguridad_pagina_handler(pagina_id, handler_id)
    WHERE activo = 1;
END;

DECLARE @recursos TABLE (
    ruta VARCHAR(260) NOT NULL PRIMARY KEY,
    titulo VARCHAR(150) NOT NULL,
    modulo VARCHAR(100) NOT NULL,
    es_handler BIT NOT NULL
);

INSERT INTO @recursos (ruta, titulo, modulo, es_handler)
VALUES
    ('/secure/seguridad.aspx', 'Seguridad', 'Configuracion', 0),
    ('/handlers/seguridad_handler.ashx', 'seguridad_handler', 'Handler', 1),
    ('/secure/alertas_pld.aspx', 'Alertas P.L.D.', 'PLD', 0),
    ('/handlers/handler_alertas_pld.ashx', 'handler_alertas_pld', 'Handler', 1),
    ('/secure/pagos_credito.aspx', 'Pagos de Credito P.L.D.', 'PLD', 0),
    ('/handlers/handler_pagos_credito.ashx', 'handler_pagos_credito', 'Handler', 1),
    ('/secure/captura_solicitud_credito.aspx', 'Solicitud PF', 'Captura de Clientes', 0),
    ('/handlers/solicitud_credito_handler.ashx', 'solicitud_credito_handler', 'Handler', 1),
    ('/handlers/catalogos_handler.ashx', 'catalogos_handler', 'Handler', 1),
    ('/handlers/contacto_solicitud_handler.ashx', 'contacto_solicitud_handler', 'Handler', 1),
    ('/handlers/pld_detalle_handler.ashx', 'pld_detalle_handler', 'Handler', 1),
    ('/handlers/clientes_handler.ashx', 'clientes_handler', 'Handler', 1),
    ('/handlers/calcular_pld.ashx', 'calcular_pld', 'Handler', 1),
    ('/secure/solicitud_pf.aspx', 'Presolicitud', 'Captura de Clientes', 0),
    ('/handlers/solicitudpfhandler.ashx', 'solicitudPFHandler', 'Handler', 1),
    ('/secure/catalogoproductofinanciero.aspx', 'Producto Financiero', 'Catalogos', 0),
    ('/handlers/producto_financiero_handler.ashx', 'producto_financiero_handler', 'Handler', 1),
    ('/handlers/producto_financiero_periodos_handler.ashx', 'producto_financiero_periodos_handler', 'Handler', 1),
    ('/handlers/catalogo_creditos_pld.ashx', 'catalogo_creditos_pld', 'Handler', 1),
    ('/handlers/handler_catalogo_moneda_divisa.ashx', 'handler_catalogo_moneda_divisa', 'Handler', 1),
    ('/secure/catalogonichomercado.aspx', 'Nicho de Mercado', 'Catalogos', 0),
    ('/handlers/handler_catalogo_nicho_mercado.ashx', 'handler_catalogo_nicho_mercado', 'Handler', 1),
    ('/secure/catalogos.aspx', 'Catalogos', 'Configuracion', 0),
    ('/handlers/handler_config_umbrales_pld.ashx', 'handler_config_umbrales_pld', 'Handler', 1),
    ('/secure/catalogo_aplicacion_pago.aspx', 'Aplicacion de Pago', 'Catalogos', 0),
    ('/handlers/handler_aplicacion_pago.ashx', 'handler_aplicacion_pago', 'Handler', 1),
    ('/handlers/handler_tipo_credito.ashx', 'handler_tipo_credito', 'Handler', 1),
    ('/secure/catalogo_canal_pago.aspx', 'Canal de Pago', 'Catalogos', 0),
    ('/handlers/handler_catalogo_canal_pago.ashx', 'handler_catalogo_canal_pago', 'Handler', 1),
    ('/secure/catalogo_centro_trabajo.aspx', 'Centro de Trabajo', 'Catalogos', 0),
    ('/handlers/handler_centro_trabajo.ashx', 'handler_centro_trabajo', 'Handler', 1),
    ('/secure/catalogo_colonia.aspx', 'Colonias', 'Catalogos', 0),
    ('/handlers/handler_colonia.ashx', 'handler_colonia', 'Handler', 1),
    ('/secure/catalogo_creditos.aspx', 'Creditos', 'Catalogos', 0),
    ('/handlers/handler_tipo_estado_cuenta.ashx', 'handler_tipo_estado_cuenta', 'Handler', 1),
    ('/secure/catalogo_destino_recursos.aspx', 'Destino de Recursos', 'Catalogos', 0),
    ('/handlers/handler_catalogo_destino_recursos.ashx', 'handler_catalogo_destino_recursos', 'Handler', 1),
    ('/secure/catalogo_empresas.aspx', 'Empresas', 'Catalogos', 0),
    ('/handlers/handler_empresas.ashx', 'handler_empresas', 'Handler', 1),
    ('/handlers/handler_paises.ashx', 'handler_paises', 'Handler', 1),
    ('/handlers/handler_sepomex.ashx', 'handler_sepomex', 'Handler', 1),
    ('/secure/catalogo_estados.aspx', 'Estados', 'Catalogos', 0),
    ('/handlers/handler_catalogo_estados.ashx', 'handler_catalogo_estados', 'Handler', 1),
    ('/secure/catalogo_fondeador.aspx', 'Fuente de Fondeo', 'Catalogos', 0),
    ('/handlers/handler_fondeador.ashx', 'handler_fondeador', 'Handler', 1),
    ('/secure/catalogo_giro_negocio.aspx', 'Giro del Negocio', 'Catalogos', 0),
    ('/handlers/handler_giro_negocio.ashx', 'handler_giro_negocio', 'Handler', 1),
    ('/secure/catalogo_instrumento_monetario.aspx', 'Instrumento Monetario', 'Catalogos', 0),
    ('/handlers/handler_catalogo_instrumento_monetario.ashx', 'handler_catalogo_instrumento_monetario', 'Handler', 1),
    ('/secure/catalogo_libor.aspx', 'Tasa Libor', 'Catalogos', 0),
    ('/handlers/handler_catalogo_libor.ashx', 'handler_catalogo_libor', 'Handler', 1),
    ('/secure/catalogo_medio_contacto.aspx', 'Medio de Contacto', 'Catalogos', 0),
    ('/handlers/handler_medios_contacto.ashx', 'handler_medios_contacto', 'Handler', 1),
    ('/secure/catalogo_moneda_divisa.aspx', 'Moneda Divisa', 'Catalogos', 0),
    ('/secure/catalogo_municipios.aspx', 'Municipios', 'Catalogos', 0),
    ('/handlers/handler_catalogo_municipios.ashx', 'handler_catalogo_municipios', 'Handler', 1),
    ('/secure/catalogo_nacionalidades.aspx', 'Nacionalidades', 'Catalogos', 0),
    ('/handlers/handler_nacionalidades.ashx', 'handler_nacionalidades', 'Handler', 1),
    ('/secure/catalogo_ocupacion.aspx', 'Ocupacion', 'Catalogos', 0),
    ('/handlers/handler_ocupacion.ashx', 'handler_ocupacion', 'Handler', 1),
    ('/secure/catalogo_origen_recursos.aspx', 'Origen de Recursos', 'Catalogos', 0),
    ('/handlers/handler_origen_recursos.ashx', 'handler_origen_recursos', 'Handler', 1),
    ('/secure/catalogo_paises.aspx', 'Paises', 'Catalogos', 0),
    ('/secure/catalogo_promotores.aspx', 'Promotores', 'Catalogos', 0),
    ('/handlers/handler_promotores.ashx', 'handler_promotores', 'Handler', 1),
    ('/handlers/handler_sucursales.ashx', 'handler_sucursales', 'Handler', 1),
    ('/secure/catalogo_propietario_real.aspx', 'Propietario Real', 'Catalogos', 0),
    ('/handlers/handler_catalogo_propietario_real.ashx', 'handler_catalogo_propietario_real', 'Handler', 1),
    ('/secure/catalogo_scoring.aspx', 'Scoring', 'Catalogos', 0),
    ('/handlers/scoring_atraso.ashx', 'scoring_atraso', 'Handler', 1),
    ('/handlers/scoring_pago.ashx', 'scoring_pago', 'Handler', 1),
    ('/handlers/scoring_monto.ashx', 'scoring_monto', 'Handler', 1),
    ('/handlers/scoring_circulo.ashx', 'scoring_circulo', 'Handler', 1),
    ('/handlers/scoring_producto.ashx', 'scoring_producto', 'Handler', 1),
    ('/secure/catalogo_sepomex.aspx', 'Codigos Postales', 'Catalogos', 0),
    ('/handlers/importar_sepomex.ashx', 'importar_sepomex', 'Handler', 1),
    ('/secure/catalogo_tasa_tie.aspx', 'Tasa TIE', 'Catalogos', 0),
    ('/handlers/handler_catalogo_tasa_tie.ashx', 'handler_catalogo_tasa_tie', 'Handler', 1),
    ('/secure/catalogo_tipo_documentos.aspx', 'Tipo de Documentos', 'Catalogos', 0),
    ('/handlers/handler_catalogo_tipo_documentos.ashx', 'handler_catalogo_tipo_documentos', 'Handler', 1),
    ('/secure/catalogo_tipo_pago.aspx', 'Tipo de Pago', 'Catalogos', 0),
    ('/handlers/handler_tipo_pago.ashx', 'handler_tipo_pago', 'Handler', 1),
    ('/secure/clasificacion_riesgo.aspx', 'Clasificacion de Riesgo', 'Configuracion', 0),
    ('/handlers/handler_peso_cliente_pf.ashx', 'handler_peso_cliente_pf', 'Handler', 1),
    ('/handlers/handler_peso_cliente_pm.ashx', 'handler_peso_cliente_pm', 'Handler', 1),
    ('/handlers/handler_peso_producto.ashx', 'handler_peso_producto', 'Handler', 1),
    ('/handlers/handler_peso_zona.ashx', 'handler_peso_zona', 'Handler', 1),
    ('/handlers/handler_peso_transacciones.ashx', 'handler_peso_transacciones', 'Handler', 1),
    ('/handlers/handler_peso_general.ashx', 'handler_peso_general', 'Handler', 1),
    ('/handlers/handler_puntaje_categoria.ashx', 'handler_puntaje_categoria', 'Handler', 1),
    ('/secure/config_puntaje_categoria.aspx', 'Puntaje por Categoria', 'Configuracion', 0),
    ('/secure/config_alertas_destinatarios.aspx', 'Destinatarios de Alertas', 'Configuracion', 0),
    ('/handlers/handler_config_alertas_destinatarios.ashx', 'handler_config_alertas_destinatarios', 'Handler', 1);

MERGE dbo.seguridad_paginas AS target
USING @recursos AS source
ON target.ruta = source.ruta
WHEN MATCHED THEN
    UPDATE SET
        target.titulo = source.titulo,
        target.modulo = source.modulo,
        target.es_handler = source.es_handler,
        target.activo = 1,
        target.fecha_modificacion = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (clave, titulo, ruta, modulo, es_menu, es_handler, activo, fecha_creacion)
    VALUES (
        LEFT(REPLACE(REPLACE(REPLACE(REPLACE(source.ruta, '/', '_'), '.', '_'), '-', '_'), '__', '_'), 120),
        source.titulo,
        source.ruta,
        source.modulo,
        CASE WHEN source.es_handler = 1 THEN 0 ELSE 1 END,
        source.es_handler,
        1,
        GETDATE()
    );

DECLARE @relaciones TABLE (
    pagina_ruta VARCHAR(260) NOT NULL,
    handler_ruta VARCHAR(260) NOT NULL
);

INSERT INTO @relaciones (pagina_ruta, handler_ruta)
VALUES
    ('/secure/seguridad.aspx', '/handlers/seguridad_handler.ashx'),
    ('/secure/alertas_pld.aspx', '/handlers/handler_alertas_pld.ashx'),
    ('/secure/pagos_credito.aspx', '/handlers/handler_pagos_credito.ashx'),
    ('/secure/pagos_credito.aspx', '/handlers/handler_alertas_pld.ashx'),
    ('/secure/captura_solicitud_credito.aspx', '/handlers/solicitud_credito_handler.ashx'),
    ('/secure/captura_solicitud_credito.aspx', '/handlers/catalogos_handler.ashx'),
    ('/secure/captura_solicitud_credito.aspx', '/handlers/contacto_solicitud_handler.ashx'),
    ('/secure/captura_solicitud_credito.aspx', '/handlers/pld_detalle_handler.ashx'),
    ('/secure/captura_solicitud_credito.aspx', '/handlers/clientes_handler.ashx'),
    ('/secure/captura_solicitud_credito.aspx', '/handlers/calcular_pld.ashx'),
    ('/secure/captura_solicitud_credito.aspx', '/handlers/handler_alertas_pld.ashx'),
    ('/secure/solicitud_pf.aspx', '/handlers/solicitudpfhandler.ashx'),
    ('/secure/catalogoproductofinanciero.aspx', '/handlers/producto_financiero_handler.ashx'),
    ('/secure/catalogoproductofinanciero.aspx', '/handlers/producto_financiero_periodos_handler.ashx'),
    ('/secure/catalogoproductofinanciero.aspx', '/handlers/catalogo_creditos_pld.ashx'),
    ('/secure/catalogoproductofinanciero.aspx', '/handlers/handler_catalogo_moneda_divisa.ashx'),
    ('/secure/catalogonichomercado.aspx', '/handlers/handler_catalogo_nicho_mercado.ashx'),
    ('/secure/catalogos.aspx', '/handlers/handler_config_umbrales_pld.ashx'),
    ('/secure/catalogo_aplicacion_pago.aspx', '/handlers/handler_aplicacion_pago.ashx'),
    ('/secure/catalogo_aplicacion_pago.aspx', '/handlers/handler_tipo_credito.ashx'),
    ('/secure/catalogo_canal_pago.aspx', '/handlers/handler_catalogo_canal_pago.ashx'),
    ('/secure/catalogo_centro_trabajo.aspx', '/handlers/handler_centro_trabajo.ashx'),
    ('/secure/catalogo_colonia.aspx', '/handlers/handler_colonia.ashx'),
    ('/secure/catalogo_creditos.aspx', '/handlers/catalogo_creditos_pld.ashx'),
    ('/secure/catalogo_creditos.aspx', '/handlers/handler_tipo_estado_cuenta.ashx'),
    ('/secure/catalogo_destino_recursos.aspx', '/handlers/handler_catalogo_destino_recursos.ashx'),
    ('/secure/catalogo_empresas.aspx', '/handlers/handler_empresas.ashx'),
    ('/secure/catalogo_empresas.aspx', '/handlers/handler_paises.ashx'),
    ('/secure/catalogo_empresas.aspx', '/handlers/handler_sepomex.ashx'),
    ('/secure/catalogo_estados.aspx', '/handlers/handler_catalogo_estados.ashx'),
    ('/secure/catalogo_fondeador.aspx', '/handlers/handler_fondeador.ashx'),
    ('/secure/catalogo_fondeador.aspx', '/handlers/handler_tipo_credito.ashx'),
    ('/secure/catalogo_giro_negocio.aspx', '/handlers/handler_giro_negocio.ashx'),
    ('/secure/catalogo_instrumento_monetario.aspx', '/handlers/handler_catalogo_instrumento_monetario.ashx'),
    ('/secure/catalogo_libor.aspx', '/handlers/handler_catalogo_libor.ashx'),
    ('/secure/catalogo_medio_contacto.aspx', '/handlers/handler_medios_contacto.ashx'),
    ('/secure/catalogo_moneda_divisa.aspx', '/handlers/handler_catalogo_moneda_divisa.ashx'),
    ('/secure/catalogo_municipios.aspx', '/handlers/handler_catalogo_municipios.ashx'),
    ('/secure/catalogo_nacionalidades.aspx', '/handlers/handler_nacionalidades.ashx'),
    ('/secure/catalogo_ocupacion.aspx', '/handlers/handler_ocupacion.ashx'),
    ('/secure/catalogo_origen_recursos.aspx', '/handlers/handler_origen_recursos.ashx'),
    ('/secure/catalogo_paises.aspx', '/handlers/handler_paises.ashx'),
    ('/secure/catalogo_promotores.aspx', '/handlers/handler_promotores.ashx'),
    ('/secure/catalogo_promotores.aspx', '/handlers/handler_tipo_credito.ashx'),
    ('/secure/catalogo_promotores.aspx', '/handlers/handler_sucursales.ashx'),
    ('/secure/catalogo_propietario_real.aspx', '/handlers/handler_catalogo_propietario_real.ashx'),
    ('/secure/catalogo_scoring.aspx', '/handlers/scoring_atraso.ashx'),
    ('/secure/catalogo_scoring.aspx', '/handlers/scoring_pago.ashx'),
    ('/secure/catalogo_scoring.aspx', '/handlers/scoring_monto.ashx'),
    ('/secure/catalogo_scoring.aspx', '/handlers/scoring_circulo.ashx'),
    ('/secure/catalogo_scoring.aspx', '/handlers/scoring_producto.ashx'),
    ('/secure/catalogo_sepomex.aspx', '/handlers/importar_sepomex.ashx'),
    ('/secure/catalogo_tasa_tie.aspx', '/handlers/handler_catalogo_tasa_tie.ashx'),
    ('/secure/catalogo_tipo_documentos.aspx', '/handlers/handler_catalogo_tipo_documentos.ashx'),
    ('/secure/catalogo_tipo_pago.aspx', '/handlers/handler_tipo_pago.ashx'),
    ('/secure/clasificacion_riesgo.aspx', '/handlers/handler_peso_cliente_pf.ashx'),
    ('/secure/clasificacion_riesgo.aspx', '/handlers/handler_peso_cliente_pm.ashx'),
    ('/secure/clasificacion_riesgo.aspx', '/handlers/handler_peso_producto.ashx'),
    ('/secure/clasificacion_riesgo.aspx', '/handlers/handler_peso_zona.ashx'),
    ('/secure/clasificacion_riesgo.aspx', '/handlers/handler_peso_transacciones.ashx'),
    ('/secure/clasificacion_riesgo.aspx', '/handlers/handler_peso_general.ashx'),
    ('/secure/clasificacion_riesgo.aspx', '/handlers/handler_puntaje_categoria.ashx'),
    ('/secure/config_puntaje_categoria.aspx', '/handlers/handler_puntaje_categoria.ashx'),
    ('/secure/config_alertas_destinatarios.aspx', '/handlers/handler_config_alertas_destinatarios.ashx');

MERGE dbo.seguridad_pagina_handler AS target
USING (
    SELECT DISTINCT
        p.id AS pagina_id,
        h.id AS handler_id
    FROM @relaciones r
    INNER JOIN dbo.seguridad_paginas p
        ON p.ruta = r.pagina_ruta
       AND ISNULL(p.activo, 0) = 1
       AND ISNULL(p.es_handler, 0) = 0
    INNER JOIN dbo.seguridad_paginas h
        ON h.ruta = r.handler_ruta
       AND ISNULL(h.activo, 0) = 1
       AND ISNULL(h.es_handler, 0) = 1
) AS source
ON target.pagina_id = source.pagina_id
AND target.handler_id = source.handler_id
WHEN MATCHED THEN
    UPDATE SET activo = 1, fecha_modificacion = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (pagina_id, handler_id, activo, fecha_creacion)
    VALUES (source.pagina_id, source.handler_id, 1, GETDATE());

-- Diagnostico: handlers activos que no heredan permisos desde ninguna pagina.
SELECT
    h.id AS handler_id,
    h.titulo,
    h.ruta
FROM dbo.seguridad_paginas h
WHERE ISNULL(h.es_handler, 0) = 1
  AND ISNULL(h.activo, 0) = 1
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.seguridad_pagina_handler ph
      INNER JOIN dbo.seguridad_paginas p
          ON p.id = ph.pagina_id
         AND ISNULL(p.activo, 0) = 1
         AND ISNULL(p.es_handler, 0) = 0
      WHERE ph.handler_id = h.id
        AND ISNULL(ph.activo, 0) = 1
  )
ORDER BY h.ruta;
