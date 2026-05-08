-- ============================================================
-- PERMISOS COMPROBANTE DE PAGO
-- Aplicación: COMPROBANTE  (codApp = 'COMPROBANTE')
-- ============================================================

-- ── 1. Códigos de permiso ───────────────────────────────────
INSERT INTO permisos (cod_permiso, cod_app, descripcion, nivel) VALUES
  ('COMP.VER',       'COMPROBANTE', 'Consultar comprobantes',            'CONTROLADOR'),
  ('COMP.GUARDAR',   'COMPROBANTE', 'Registrar / editar comprobante',    'ACTION'),
  ('COMP.ENVIAR',    'COMPROBANTE', 'Enviar comprobante a revisión',     'ACTION'),
  ('COMP.AUTORIZAR', 'COMPROBANTE', 'Firmar y derivar comprobantes',     'CONTROLADOR'),
  ('COMP.APROBAR',   'COMPROBANTE', 'Aprobar y enviar a Syteline',       'CONTROLADOR'),
  ('COMP.ANULAR',    'COMPROBANTE', 'Anular comprobante',                'ACTION'),
  ('COMP.RPT',       'COMPROBANTE', 'Exportar reportes Syteline',        'ACTION')
ON DUPLICATE KEY UPDATE descripcion = VALUES(descripcion);

-- ── 2. Matriz perfil → permiso ──────────────────────────────
--
--  Perfil              COMP.VER  COMP.GUARDAR  COMP.ENVIAR  COMP.AUTORIZAR  COMP.APROBAR  COMP.ANULAR  COMP.RPT
--  ASISTENT_ADM          ✓           ✓             ✓
--  JEFATUR_ARE           ✓                                       ✓
--  ANALIST_JR            ✓                                                       ✓           ✓*          ✓
--  JEFE_CONT             ✓                                                       ✓                       ✓
--
--  ✓* = asignación explícita vía API de seguridad (no genérico)

INSERT INTO perfil_permisos (cod_perfil, cod_permiso) VALUES
  ('ASISTENT_ADM', 'COMP.VER'),
  ('ASISTENT_ADM', 'COMP.GUARDAR'),
  ('ASISTENT_ADM', 'COMP.ENVIAR'),
  ('JEFATUR_ARE',  'COMP.VER'),
  ('JEFATUR_ARE',  'COMP.AUTORIZAR'),
  ('ANALIST_JR',   'COMP.VER'),
  ('ANALIST_JR',   'COMP.APROBAR'),
  ('ANALIST_JR',   'COMP.RPT'),
  ('JEFE_CONT',    'COMP.VER'),
  ('JEFE_CONT',    'COMP.APROBAR'),
  ('JEFE_CONT',    'COMP.RPT')
ON DUPLICATE KEY UPDATE cod_permiso = VALUES(cod_permiso);

-- ── 3. Detalle de acciones cubiertas por código ─────────────
--
--  COMP.VER (clase ComprobanteConsultarController)
--    Index · Detalle · Buscar · ObtenerDetalle · ObtenerPdf
--    DocumentosElectronicos · Catálogos (TiposDoc, Proveedores,
--    CuentasContables, Empleados, UnidadesMedida)
--    Imputaciones (lectura)
--
--  COMP.GUARDAR (actions en ComprobanteGestionarController)
--    Nuevo · Editar · Guardar · SubirDocumentos · EliminarDocumento
--    AgregarImputacion · EditarImputacion · EliminarImputacion
--    ValidarSunat
--
--  COMP.ENVIAR (action en ComprobanteGestionarController)
--    Enviar
--
--  COMP.AUTORIZAR (clase ComprobanteAutorizarController)
--    Autorizar · Firmar · Derivar
--
--  COMP.APROBAR (clase ComprobanteAprobarController)
--    Aprobar · AprobarConfirmar · EnviarASyteline
--
--  COMP.ANULAR (action en ComprobanteGestionarController)
--    Anular
--
--  COMP.RPT (actions en ComprobanteReporteController)
--    ExportarDistribucion · ExportarCabecera
