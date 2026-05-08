-- =============================================================
-- Permisos del módulo: Comprobante de Pago
-- Sistema: PlataformaInterna / ComprobantePago.Web
-- =============================================================

-- ─────────────────────────────────────────────────────────────
-- 1. Códigos de permiso
-- ─────────────────────────────────────────────────────────────
-- COMP.VER       Consultar comprobantes, ver detalle y descargar PDF/documentos
-- COMP.GUARDAR   Registrar comprobante, gestionar imputaciones y documentos adjuntos
-- COMP.ENVIAR    Enviar comprobante a revisión
-- COMP.ANULAR    Anular comprobante
-- COMP.AUTORIZAR Firmar y derivar comprobante
-- COMP.APROBAR   Aprobar y enviar a Syteline
-- COMP.RPT       Exportar reportes Excel (Cabecera y Distribución Syteline)

-- ─────────────────────────────────────────────────────────────
-- 2. Insertar permisos (tabla dbo.Permisos)
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'COMP.VER')
    INSERT INTO dbo.Permisos (Codigo, Descripcion, Modulo)
    VALUES ('COMP.VER', 'Consultar comprobantes de pago', 'COMPROBANTE');

IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'COMP.GUARDAR')
    INSERT INTO dbo.Permisos (Codigo, Descripcion, Modulo)
    VALUES ('COMP.GUARDAR', 'Registrar y editar comprobantes de pago', 'COMPROBANTE');

IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'COMP.ENVIAR')
    INSERT INTO dbo.Permisos (Codigo, Descripcion, Modulo)
    VALUES ('COMP.ENVIAR', 'Enviar comprobante a revisión', 'COMPROBANTE');

IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'COMP.ANULAR')
    INSERT INTO dbo.Permisos (Codigo, Descripcion, Modulo)
    VALUES ('COMP.ANULAR', 'Anular comprobante de pago', 'COMPROBANTE');

IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'COMP.AUTORIZAR')
    INSERT INTO dbo.Permisos (Codigo, Descripcion, Modulo)
    VALUES ('COMP.AUTORIZAR', 'Firmar y derivar comprobante de pago', 'COMPROBANTE');

IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'COMP.APROBAR')
    INSERT INTO dbo.Permisos (Codigo, Descripcion, Modulo)
    VALUES ('COMP.APROBAR', 'Aprobar y enviar comprobante a Syteline', 'COMPROBANTE');

IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'COMP.RPT')
    INSERT INTO dbo.Permisos (Codigo, Descripcion, Modulo)
    VALUES ('COMP.RPT', 'Exportar reportes Excel del módulo Comprobante', 'COMPROBANTE');

-- ─────────────────────────────────────────────────────────────
-- 3. Matriz perfil → permisos
-- ─────────────────────────────────────────────────────────────
--
--  Perfil              | VER | GUARDAR | ENVIAR | ANULAR | AUTORIZAR | APROBAR | RPT
--  ─────────────────── | ─── | ─────── | ────── | ────── | ───────── | ─────── | ───
--  SOLICITANTE         |  X  |    X    |   X    |   X    |           |         |
--  AUTORIZADOR         |  X  |         |        |        |     X     |         |
--  APROBADOR           |  X  |         |        |        |           |    X    |  X
--  ADMINISTRADOR       |  X  |    X    |   X    |   X    |     X     |    X    |  X
--

-- Perfil: SOLICITANTE
EXEC dbo.sp_AsignarPermisoPerfil 'SOLICITANTE', 'COMP.VER';
EXEC dbo.sp_AsignarPermisoPerfil 'SOLICITANTE', 'COMP.GUARDAR';
EXEC dbo.sp_AsignarPermisoPerfil 'SOLICITANTE', 'COMP.ENVIAR';
EXEC dbo.sp_AsignarPermisoPerfil 'SOLICITANTE', 'COMP.ANULAR';

-- Perfil: AUTORIZADOR
EXEC dbo.sp_AsignarPermisoPerfil 'AUTORIZADOR', 'COMP.VER';
EXEC dbo.sp_AsignarPermisoPerfil 'AUTORIZADOR', 'COMP.AUTORIZAR';

-- Perfil: APROBADOR
EXEC dbo.sp_AsignarPermisoPerfil 'APROBADOR', 'COMP.VER';
EXEC dbo.sp_AsignarPermisoPerfil 'APROBADOR', 'COMP.APROBAR';
EXEC dbo.sp_AsignarPermisoPerfil 'APROBADOR', 'COMP.RPT';

-- Perfil: ADMINISTRADOR
EXEC dbo.sp_AsignarPermisoPerfil 'ADMINISTRADOR', 'COMP.VER';
EXEC dbo.sp_AsignarPermisoPerfil 'ADMINISTRADOR', 'COMP.GUARDAR';
EXEC dbo.sp_AsignarPermisoPerfil 'ADMINISTRADOR', 'COMP.ENVIAR';
EXEC dbo.sp_AsignarPermisoPerfil 'ADMINISTRADOR', 'COMP.ANULAR';
EXEC dbo.sp_AsignarPermisoPerfil 'ADMINISTRADOR', 'COMP.AUTORIZAR';
EXEC dbo.sp_AsignarPermisoPerfil 'ADMINISTRADOR', 'COMP.APROBAR';
EXEC dbo.sp_AsignarPermisoPerfil 'ADMINISTRADOR', 'COMP.RPT';

-- ─────────────────────────────────────────────────────────────
-- 4. Cobertura de acciones por permiso
-- ─────────────────────────────────────────────────────────────
--
--  COMP.VER
--    GET  /Comprobante/Index
--    GET  /Comprobante/Detalle
--    GET  /Comprobante/ObtenerTiposDocumento
--    GET  /Comprobante/ObtenerTiposSunat
--    GET  /Comprobante/ObtenerMonedas
--    GET  /Comprobante/ObtenerLugaresPago
--    GET  /Comprobante/ObtenerTiposDetraccion
--    GET  /Comprobante/ObtenerTipos
--    GET  /Comprobante/ObtenerEstados
--    GET  /Comprobante/ObtenerEmpleados
--    GET  /Comprobante/ObtenerProveedores
--    POST /Comprobante/Buscar
--    GET  /Comprobante/ObtenerDetalle
--    GET  /Comprobante/ObtenerPdf
--    GET  /Comprobante/DocumentosElectronicos
--    GET  /Comprobante/ObtenerImputaciones
--    GET  /Comprobante/ObtenerCuentasContables
--    GET  /Comprobante/ObtenerCodigosUnidad
--    GET  /Comprobante/DescargarPlantillaImputacion
--    GET  /Comprobante/DescargarDocumento
--
--  COMP.GUARDAR
--    POST /Comprobante/Guardar
--    POST /Comprobante/AgregarImputacion
--    POST /Comprobante/EditarImputacion
--    POST /Comprobante/EliminarImputacion
--    POST /Comprobante/CargarImputacionMasiva
--    POST /Comprobante/SubirDocumentos
--    POST /Comprobante/EliminarDocumento
--    POST /Comprobante/ValidarXmlSunat
--    POST /Comprobante/ValidarPdfSunat
--    POST /Comprobante/ValidarZipSunat
--
--  COMP.ENVIAR
--    POST /Comprobante/Enviar
--
--  COMP.ANULAR
--    POST /Comprobante/Anular
--
--  COMP.AUTORIZAR
--    POST /Comprobante/Firmar
--    POST /Comprobante/Derivar
--
--  COMP.APROBAR
--    POST /Comprobante/Aprobar
--    POST /Comprobante/EnviarCabeceraASyteline
--
--  COMP.RPT
--    POST /Comprobante/ExportarDistribucionSyteline
--    POST /Comprobante/ExportarCabeceraSyteline
--
