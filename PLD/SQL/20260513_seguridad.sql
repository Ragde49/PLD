/*
    Seguridad, usuarios, roles, puestos y permisos por pagina/menu.
    Script idempotente: se puede ejecutar mas de una vez.
    Usuario inicial: admin
    Contrasena temporal: Admin#2026!
*/

SET NOCOUNT ON;

IF OBJECT_ID('dbo.catalogo_roles_permisos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.catalogo_roles_permisos (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_catalogo_roles_permisos PRIMARY KEY,
        rol VARCHAR(100) NOT NULL,
        descripcion VARCHAR(250) NULL,
        activo BIT NOT NULL CONSTRAINT DF_catalogo_roles_permisos_activo DEFAULT (1),
        fecha_creacion DATETIME NOT NULL CONSTRAINT DF_catalogo_roles_permisos_fecha DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_catalogo_roles_permisos_rol'
      AND object_id = OBJECT_ID('dbo.catalogo_roles_permisos')
)
BEGIN
    CREATE UNIQUE INDEX UX_catalogo_roles_permisos_rol
    ON dbo.catalogo_roles_permisos(rol);
END;

IF OBJECT_ID('dbo.catalogo_puestos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.catalogo_puestos (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_catalogo_puestos PRIMARY KEY,
        puesto VARCHAR(150) NOT NULL,
        descripcion VARCHAR(250) NULL,
        activo BIT NOT NULL CONSTRAINT DF_catalogo_puestos_activo DEFAULT (1),
        fecha_creacion DATETIME NOT NULL CONSTRAINT DF_catalogo_puestos_fecha DEFAULT (GETDATE()),
        fecha_modificacion DATETIME NULL
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_catalogo_puestos_puesto'
      AND object_id = OBJECT_ID('dbo.catalogo_puestos')
)
BEGIN
    CREATE UNIQUE INDEX UX_catalogo_puestos_puesto
    ON dbo.catalogo_puestos(puesto);
END;

IF OBJECT_ID('dbo.seguridad_usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.seguridad_usuarios (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_seguridad_usuarios PRIMARY KEY,
        usuario VARCHAR(80) NOT NULL,
        nombre VARCHAR(150) NOT NULL,
        email VARCHAR(150) NULL,
        password_hash VARCHAR(300) NOT NULL,
        rol_id INT NOT NULL,
        puesto_id INT NULL,
        activo BIT NOT NULL CONSTRAINT DF_seguridad_usuarios_activo DEFAULT (1),
        debe_cambiar_password BIT NOT NULL CONSTRAINT DF_seguridad_usuarios_cambiar DEFAULT (1),
        ultimo_acceso DATETIME NULL,
        fecha_password DATETIME NULL,
        fecha_creacion DATETIME NOT NULL CONSTRAINT DF_seguridad_usuarios_fecha DEFAULT (GETDATE()),
        fecha_modificacion DATETIME NULL,
        CONSTRAINT FK_seguridad_usuarios_roles FOREIGN KEY (rol_id) REFERENCES dbo.catalogo_roles_permisos(id),
        CONSTRAINT FK_seguridad_usuarios_puestos FOREIGN KEY (puesto_id) REFERENCES dbo.catalogo_puestos(id)
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_seguridad_usuarios_usuario'
      AND object_id = OBJECT_ID('dbo.seguridad_usuarios')
)
BEGIN
    CREATE UNIQUE INDEX UX_seguridad_usuarios_usuario
    ON dbo.seguridad_usuarios(usuario);
END;

IF OBJECT_ID('dbo.seguridad_paginas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.seguridad_paginas (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_seguridad_paginas PRIMARY KEY,
        clave VARCHAR(120) NOT NULL,
        titulo VARCHAR(150) NOT NULL,
        ruta VARCHAR(260) NOT NULL,
        modulo VARCHAR(100) NOT NULL,
        es_menu BIT NOT NULL CONSTRAINT DF_seguridad_paginas_es_menu DEFAULT (0),
        es_handler BIT NOT NULL CONSTRAINT DF_seguridad_paginas_es_handler DEFAULT (0),
        activo BIT NOT NULL CONSTRAINT DF_seguridad_paginas_activo DEFAULT (1),
        fecha_creacion DATETIME NOT NULL CONSTRAINT DF_seguridad_paginas_fecha DEFAULT (GETDATE()),
        fecha_modificacion DATETIME NULL
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_seguridad_paginas_ruta'
      AND object_id = OBJECT_ID('dbo.seguridad_paginas')
)
BEGIN
    CREATE UNIQUE INDEX UX_seguridad_paginas_ruta
    ON dbo.seguridad_paginas(ruta);
END;

IF OBJECT_ID('dbo.seguridad_menu', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.seguridad_menu (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_seguridad_menu PRIMARY KEY,
        parent_id INT NULL,
        page_id INT NULL,
        titulo VARCHAR(150) NOT NULL,
        icono VARCHAR(80) NULL,
        url VARCHAR(260) NULL,
        orden INT NOT NULL CONSTRAINT DF_seguridad_menu_orden DEFAULT (0),
        activo BIT NOT NULL CONSTRAINT DF_seguridad_menu_activo DEFAULT (1),
        fecha_creacion DATETIME NOT NULL CONSTRAINT DF_seguridad_menu_fecha DEFAULT (GETDATE()),
        CONSTRAINT FK_seguridad_menu_parent FOREIGN KEY (parent_id) REFERENCES dbo.seguridad_menu(id),
        CONSTRAINT FK_seguridad_menu_page FOREIGN KEY (page_id) REFERENCES dbo.seguridad_paginas(id)
    );
END;

IF OBJECT_ID('dbo.seguridad_rol_pagina', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.seguridad_rol_pagina (
        rol_id INT NOT NULL,
        pagina_id INT NOT NULL,
        puede_ver BIT NOT NULL CONSTRAINT DF_seguridad_rol_pagina_ver DEFAULT (1),
        puede_crear BIT NOT NULL CONSTRAINT DF_seguridad_rol_pagina_crear DEFAULT (0),
        puede_editar BIT NOT NULL CONSTRAINT DF_seguridad_rol_pagina_editar DEFAULT (0),
        puede_eliminar BIT NOT NULL CONSTRAINT DF_seguridad_rol_pagina_eliminar DEFAULT (0),
        fecha_creacion DATETIME NOT NULL CONSTRAINT DF_seguridad_rol_pagina_fecha DEFAULT (GETDATE()),
        CONSTRAINT PK_seguridad_rol_pagina PRIMARY KEY (rol_id, pagina_id),
        CONSTRAINT FK_seguridad_rol_pagina_rol FOREIGN KEY (rol_id) REFERENCES dbo.catalogo_roles_permisos(id),
        CONSTRAINT FK_seguridad_rol_pagina_pagina FOREIGN KEY (pagina_id) REFERENCES dbo.seguridad_paginas(id)
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.catalogo_roles_permisos WHERE rol = 'Administrador')
BEGIN
    INSERT INTO dbo.catalogo_roles_permisos (rol, descripcion, activo, fecha_creacion)
    VALUES ('Administrador', 'Acceso completo al sistema', 1, GETDATE());
END
ELSE
BEGIN
    UPDATE dbo.catalogo_roles_permisos
    SET activo = 1
    WHERE rol = 'Administrador';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.catalogo_puestos WHERE puesto = 'Administrador del sistema')
BEGIN
    INSERT INTO dbo.catalogo_puestos (puesto, descripcion, activo, fecha_creacion)
    VALUES ('Administrador del sistema', 'Puesto inicial para administracion de seguridad', 1, GETDATE());
END;

DECLARE @paginas TABLE (
    ruta VARCHAR(260) NOT NULL,
    titulo VARCHAR(150) NOT NULL,
    modulo VARCHAR(100) NOT NULL,
    es_menu BIT NOT NULL,
    es_handler BIT NOT NULL
);

INSERT INTO @paginas (ruta, titulo, modulo, es_menu, es_handler)
VALUES
    ('/default.aspx', 'Inicio', 'Sistema', 1, 0),
    ('/secure/seguridad.aspx', 'Seguridad', 'Configuracion', 1, 0),
    ('/secure/alertas_pld.aspx', 'Alertas P.L.D.', 'PLD', 1, 0),
    ('/secure/catalogonichomercado.aspx', 'Nicho de Mercado', 'Catalogos', 1, 0),
    ('/secure/captura_solicitud_credito.aspx', 'Solicitud PF', 'Captura de Clientes', 1, 0),
    ('/secure/catalogoproductofinanciero.aspx', 'Producto Financiero', 'Catalogos', 1, 0),
    ('/secure/solicitud_pf.aspx', 'Presolicitud', 'Captura de Clientes', 1, 0),
    ('/secure/presolicitud_form.aspx', 'Formulario de Presolicitud', 'Captura de Clientes', 1, 0),
    ('/secure/pagos_credito.aspx', 'Pagos de Credito P.L.D.', 'PLD', 1, 0),
    ('/secure/config_puntaje_categoria.aspx', 'Puntaje por Categoria', 'Configuracion', 1, 0),
    ('/secure/clasificacion_riesgo.aspx', 'Clasificacion de Riesgo', 'Configuracion', 1, 0),
    ('/secure/catalogo_tipo_pago.aspx', 'Tipo de Pago', 'Catalogos', 1, 0),
    ('/secure/catalogo_tipo_documentos.aspx', 'Tipo de Documentos', 'Catalogos', 1, 0),
    ('/secure/catalogo_tasa_tie.aspx', 'Tasa TIE', 'Catalogos', 1, 0),
    ('/secure/catalogo_sepomex.aspx', 'Codigos Postales', 'Catalogos', 1, 0),
    ('/secure/catalogo_scoring.aspx', 'Scoring', 'Catalogos', 1, 0),
    ('/secure/catalogo_propietario_real.aspx', 'Propietario Real', 'Catalogos', 1, 0),
    ('/secure/catalogo_promotores.aspx', 'Promotores', 'Catalogos', 1, 0),
    ('/secure/catalogo_paises.aspx', 'Paises', 'Catalogos', 1, 0),
    ('/secure/catalogo_origen_recursos.aspx', 'Origen de Recursos', 'Catalogos', 1, 0),
    ('/secure/catalogo_ocupacion.aspx', 'Ocupacion', 'Catalogos', 1, 0),
    ('/secure/catalogo_nacionalidades.aspx', 'Nacionalidades', 'Catalogos', 1, 0),
    ('/secure/catalogo_municipios.aspx', 'Municipios', 'Catalogos', 1, 0),
    ('/secure/catalogo_moneda_divisa.aspx', 'Moneda Divisa', 'Catalogos', 1, 0),
    ('/secure/catalogo_medio_contacto.aspx', 'Medio de Contacto', 'Catalogos', 1, 0),
    ('/secure/catalogo_libor.aspx', 'Tasa Libor', 'Catalogos', 1, 0),
    ('/secure/catalogo_instrumento_monetario.aspx', 'Instrumento Monetario', 'Catalogos', 1, 0),
    ('/secure/catalogo_giro_negocio.aspx', 'Giro del Negocio', 'Catalogos', 1, 0),
    ('/secure/catalogo_fondeador.aspx', 'Fuente de Fondeo', 'Catalogos', 1, 0),
    ('/secure/catalogo_estados.aspx', 'Estados', 'Catalogos', 1, 0),
    ('/secure/catalogo_empresas.aspx', 'Empresas', 'Catalogos', 1, 0),
    ('/secure/catalogo_destino_recursos.aspx', 'Destino de Recursos', 'Catalogos', 1, 0),
    ('/secure/catalogo_creditos.aspx', 'Creditos', 'Catalogos', 1, 0),
    ('/secure/catalogo_colonia.aspx', 'Colonias', 'Catalogos', 1, 0),
    ('/secure/catalogo_centro_trabajo.aspx', 'Centro de Trabajo', 'Catalogos', 1, 0),
    ('/secure/catalogo_canal_pago.aspx', 'Canal de Pago', 'Catalogos', 1, 0),
    ('/secure/catalogo_aplicacion_pago.aspx', 'Aplicacion de Pago', 'Catalogos', 1, 0),
    ('/secure/catalogos.aspx', 'Catalogos', 'Configuracion', 1, 0),
    ('/handlers/utilidades.ashx', 'utilidades', 'Handler', 0, 1),
    ('/handlers/solicitud_credito_handler.ashx', 'solicitud_credito_handler', 'Handler', 0, 1),
    ('/handlers/solicitudpfhandler.ashx', 'solicitudPFHandler', 'Handler', 0, 1),
    ('/handlers/scoring_producto.ashx', 'scoring_producto', 'Handler', 0, 1),
    ('/handlers/scoring_pago.ashx', 'scoring_pago', 'Handler', 0, 1),
    ('/handlers/scoring_monto.ashx', 'scoring_monto', 'Handler', 0, 1),
    ('/handlers/scoring_circulo.ashx', 'scoring_circulo', 'Handler', 0, 1),
    ('/handlers/scoring_atraso.ashx', 'scoring_atraso', 'Handler', 0, 1),
    ('/handlers/producto_financiero_periodos_handler.ashx', 'producto_financiero_periodos_handler', 'Handler', 0, 1),
    ('/handlers/producto_financiero_handler.ashx', 'producto_financiero_handler', 'Handler', 0, 1),
    ('/handlers/pld_detalle_handler.ashx', 'pld_detalle_handler', 'Handler', 0, 1),
    ('/handlers/importar_sepomex.ashx', 'importar_sepomex', 'Handler', 0, 1),
    ('/handlers/handler_tipo_pago.ashx', 'handler_tipo_pago', 'Handler', 0, 1),
    ('/handlers/handler_tipo_estado_cuenta.ashx', 'handler_tipo_estado_cuenta', 'Handler', 0, 1),
    ('/handlers/handler_tipo_credito.ashx', 'handler_tipo_credito', 'Handler', 0, 1),
    ('/handlers/handler_sucursales.ashx', 'handler_sucursales', 'Handler', 0, 1),
    ('/handlers/handler_sepomex.ashx', 'handler_sepomex', 'Handler', 0, 1),
    ('/handlers/handler_puntaje_categoria.ashx', 'handler_puntaje_categoria', 'Handler', 0, 1),
    ('/handlers/handler_promotores.ashx', 'handler_promotores', 'Handler', 0, 1),
    ('/handlers/handler_peso_zona.ashx', 'handler_peso_zona', 'Handler', 0, 1),
    ('/handlers/handler_peso_transacciones.ashx', 'handler_peso_transacciones', 'Handler', 0, 1),
    ('/handlers/handler_peso_producto.ashx', 'handler_peso_producto', 'Handler', 0, 1),
    ('/handlers/handler_peso_general.ashx', 'handler_peso_general', 'Handler', 0, 1),
    ('/handlers/handler_peso_cliente_pm.ashx', 'handler_peso_cliente_pm', 'Handler', 0, 1),
    ('/handlers/handler_peso_cliente_pf.ashx', 'handler_peso_cliente_pf', 'Handler', 0, 1),
    ('/handlers/handler_peso_alertas.ashx', 'handler_peso_alertas', 'Handler', 0, 1),
    ('/handlers/handler_paises.ashx', 'handler_paises', 'Handler', 0, 1),
    ('/handlers/handler_pagos_credito.ashx', 'handler_pagos_credito', 'Handler', 0, 1),
    ('/handlers/handler_origen_recursos.ashx', 'handler_origen_recursos', 'Handler', 0, 1),
    ('/handlers/handler_ocupacion.ashx', 'handler_ocupacion', 'Handler', 0, 1),
    ('/handlers/handler_nacionalidades.ashx', 'handler_nacionalidades', 'Handler', 0, 1),
    ('/handlers/handler_medios_contacto.ashx', 'handler_medios_contacto', 'Handler', 0, 1),
    ('/handlers/handler_giro_negocio.ashx', 'handler_giro_negocio', 'Handler', 0, 1),
    ('/handlers/handler_fondeador.ashx', 'handler_fondeador', 'Handler', 0, 1),
    ('/handlers/handler_empresas.ashx', 'handler_empresas', 'Handler', 0, 1),
    ('/handlers/handler_config_umbrales_pld.ashx', 'handler_config_umbrales_pld', 'Handler', 0, 1),
    ('/handlers/handler_colonia.ashx', 'handler_colonia', 'Handler', 0, 1),
    ('/handlers/handler_centro_trabajo.ashx', 'handler_centro_trabajo', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_tipo_documentos.ashx', 'handler_catalogo_tipo_documentos', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_tasa_tie.ashx', 'handler_catalogo_tasa_tie', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_propietario_real.ashx', 'handler_catalogo_propietario_real', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_nicho_mercado.ashx', 'handler_catalogo_nicho_mercado', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_municipios.ashx', 'handler_catalogo_municipios', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_moneda_divisa.ashx', 'handler_catalogo_moneda_divisa', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_libor.ashx', 'handler_catalogo_libor', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_instrumento_monetario.ashx', 'handler_catalogo_instrumento_monetario', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_estados.ashx', 'handler_catalogo_estados', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_destino_recursos.ashx', 'handler_catalogo_destino_recursos', 'Handler', 0, 1),
    ('/handlers/handler_catalogo_canal_pago.ashx', 'handler_catalogo_canal_pago', 'Handler', 0, 1),
    ('/handlers/handler_aplicacion_pago.ashx', 'handler_aplicacion_pago', 'Handler', 0, 1),
    ('/handlers/handler_alertas_pld.ashx', 'handler_alertas_pld', 'Handler', 0, 1),
    ('/handlers/contacto_solicitud_handler.ashx', 'contacto_solicitud_handler', 'Handler', 0, 1),
    ('/handlers/clientes_handler.ashx', 'clientes_handler', 'Handler', 0, 1),
    ('/handlers/catalogo_scoring.ashx', 'catalogo_scoring', 'Handler', 0, 1),
    ('/handlers/catalogo_creditos_pld.ashx', 'catalogo_creditos_pld', 'Handler', 0, 1),
    ('/handlers/catalogo_colonia.ashx', 'catalogo_colonia', 'Handler', 0, 1),
    ('/handlers/catalogos_identidad_handler.ashx', 'catalogos_identidad_handler', 'Handler', 0, 1),
    ('/handlers/catalogos_handler.ashx', 'catalogos_handler', 'Handler', 0, 1),
    ('/handlers/calcular_pld.ashx', 'calcular_pld', 'Handler', 0, 1);

MERGE dbo.seguridad_paginas AS target
USING @paginas AS source
ON target.ruta = source.ruta
WHEN MATCHED THEN
    UPDATE SET
        target.titulo = source.titulo,
        target.modulo = source.modulo,
        target.es_menu = source.es_menu,
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
        source.es_menu,
        source.es_handler,
        1,
        GETDATE()
    );

DECLARE @adminRolId INT = (SELECT id FROM dbo.catalogo_roles_permisos WHERE rol = 'Administrador');
DECLARE @adminPuestoId INT = (SELECT id FROM dbo.catalogo_puestos WHERE puesto = 'Administrador del sistema');
DECLARE @adminHash VARCHAR(300) = 'PBKDF2$100000$gdGX+9haK2XlDwzvZqLVjQ==$VPkDSy1ZHc1NFC+6FU2QzJfxiFTcpvQ88XIApWp1Htw=';

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_usuarios WHERE usuario = 'admin')
BEGIN
    INSERT INTO dbo.seguridad_usuarios (
        usuario, nombre, email, password_hash, rol_id, puesto_id,
        activo, debe_cambiar_password, fecha_password, fecha_creacion
    )
    VALUES (
        'admin',
        'Administrador',
        NULL,
        @adminHash,
        @adminRolId,
        @adminPuestoId,
        1,
        1,
        GETDATE(),
        GETDATE()
    );
END
ELSE
BEGIN
    UPDATE dbo.seguridad_usuarios
    SET rol_id = @adminRolId,
        puesto_id = ISNULL(puesto_id, @adminPuestoId),
        activo = 1
    WHERE usuario = 'admin';
END;

INSERT INTO dbo.seguridad_rol_pagina (rol_id, pagina_id, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT @adminRolId, p.id, 1, 0, 0, 0
FROM dbo.seguridad_paginas p
WHERE p.activo = 1
  AND ISNULL(p.es_handler, 0) = 0
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.seguridad_rol_pagina rp
      WHERE rp.rol_id = @adminRolId
        AND rp.pagina_id = p.id
  );

UPDATE rp
SET puede_ver = 1,
    puede_crear = 0,
    puede_editar = 0,
    puede_eliminar = 0
FROM dbo.seguridad_rol_pagina rp
INNER JOIN dbo.seguridad_paginas p ON p.id = rp.pagina_id
WHERE rp.rol_id = @adminRolId
  AND ISNULL(p.es_handler, 0) = 0;

DECLARE @inicioId INT = (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/default.aspx');
DECLARE @presolicitudId INT = (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/secure/solicitud_pf.aspx');
DECLARE @solicitudId INT = (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/secure/captura_solicitud_credito.aspx');
DECLARE @alertasId INT = (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/secure/alertas_pld.aspx');
DECLARE @pagosId INT = (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/secure/pagos_credito.aspx');
DECLARE @catalogosId INT = (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/secure/catalogos.aspx');
DECLARE @seguridadId INT = (SELECT id FROM dbo.seguridad_paginas WHERE ruta = '/secure/seguridad.aspx');

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_menu WHERE titulo = 'Inicio' AND parent_id IS NULL)
    INSERT INTO dbo.seguridad_menu (parent_id, page_id, titulo, icono, url, orden, activo)
    VALUES (NULL, @inicioId, 'Inicio', 'home', '/default.aspx', 10, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_menu WHERE titulo = 'Captura de Clientes' AND parent_id IS NULL)
    INSERT INTO dbo.seguridad_menu (parent_id, page_id, titulo, icono, url, orden, activo)
    VALUES (NULL, NULL, 'Captura de Clientes', 'users', NULL, 20, 1);

DECLARE @menuClientesId INT = (SELECT TOP 1 id FROM dbo.seguridad_menu WHERE titulo = 'Captura de Clientes' AND parent_id IS NULL ORDER BY id);

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_menu WHERE parent_id = @menuClientesId AND titulo = 'Presolicitud')
    INSERT INTO dbo.seguridad_menu (parent_id, page_id, titulo, icono, url, orden, activo)
    VALUES (@menuClientesId, @presolicitudId, 'Presolicitud', NULL, '/secure/solicitud_pf.aspx', 10, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_menu WHERE parent_id = @menuClientesId AND titulo = 'Solicitud PF')
    INSERT INTO dbo.seguridad_menu (parent_id, page_id, titulo, icono, url, orden, activo)
    VALUES (@menuClientesId, @solicitudId, 'Solicitud PF', NULL, '/secure/captura_solicitud_credito.aspx', 20, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_menu WHERE titulo = 'Alertas P.L.D.' AND parent_id IS NULL)
    INSERT INTO dbo.seguridad_menu (parent_id, page_id, titulo, icono, url, orden, activo)
    VALUES (NULL, @alertasId, 'Alertas P.L.D.', 'alert-triangle', '/secure/alertas_pld.aspx', 30, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_menu WHERE titulo = 'Pagos de Credito P.L.D.' AND parent_id IS NULL)
    INSERT INTO dbo.seguridad_menu (parent_id, page_id, titulo, icono, url, orden, activo)
    VALUES (NULL, @pagosId, 'Pagos de Credito P.L.D.', 'dollar-sign', '/secure/pagos_credito.aspx', 40, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_menu WHERE titulo = 'Configuracion' AND parent_id IS NULL)
    INSERT INTO dbo.seguridad_menu (parent_id, page_id, titulo, icono, url, orden, activo)
    VALUES (NULL, NULL, 'Configuracion', 'settings', NULL, 90, 1);

DECLARE @menuConfigId INT = (SELECT TOP 1 id FROM dbo.seguridad_menu WHERE titulo = 'Configuracion' AND parent_id IS NULL ORDER BY id);

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_menu WHERE parent_id = @menuConfigId AND titulo = 'Catalogos')
    INSERT INTO dbo.seguridad_menu (parent_id, page_id, titulo, icono, url, orden, activo)
    VALUES (@menuConfigId, @catalogosId, 'Catalogos', NULL, '/secure/catalogos.aspx', 10, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.seguridad_menu WHERE parent_id = @menuConfigId AND titulo = 'Seguridad')
    INSERT INTO dbo.seguridad_menu (parent_id, page_id, titulo, icono, url, orden, activo)
    VALUES (@menuConfigId, @seguridadId, 'Seguridad', NULL, '/secure/seguridad.aspx', 20, 1);

PRINT 'Seguridad PLD lista. Usuario inicial: admin / Admin#2026!';
