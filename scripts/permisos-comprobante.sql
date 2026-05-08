-- =============================================================
-- Menú y permisos del módulo: Comprobante de Pago
-- =============================================================

-- 1. Menú
INSERT INTO segmenu (IdMenu, IdApp, Codigo, Nombre, Ruta, Icono, Estado, UsuarioReg, FechaReg, UsuarioAct, FechaAct)
VALUES (NULL, '1', 'COMPROBANTE', 'Registro de Comprobantes de Pago', '/Comprobante/Index', 'fa-file-invoice-dollar', '1', 'DBO', NOW(), NULL, NULL);

-- 2. Permisos
INSERT INTO segpermiso (IdMenu, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdMenu FROM segmenu x WHERE x.Codigo = 'COMPROBANTE'), 'COMP.VER', 'VER COMPROBANTE DE PAGO', 'DBO', NOW());

INSERT INTO segpermiso (IdMenu, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdMenu FROM segmenu x WHERE x.Codigo = 'COMPROBANTE'), 'COMP.GUARDAR', 'REGISTRAR COMPROBANTE DE PAGO', 'DBO', NOW());

INSERT INTO segpermiso (IdMenu, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdMenu FROM segmenu x WHERE x.Codigo = 'COMPROBANTE'), 'COMP.ENVIAR', 'ENVIAR COMPROBANTE DE PAGO', 'DBO', NOW());

INSERT INTO segpermiso (IdMenu, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdMenu FROM segmenu x WHERE x.Codigo = 'COMPROBANTE'), 'COMP.ANULAR', 'ANULAR COMPROBANTE DE PAGO', 'DBO', NOW());

INSERT INTO segpermiso (IdMenu, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdMenu FROM segmenu x WHERE x.Codigo = 'COMPROBANTE'), 'COMP.AUTORIZAR', 'AUTORIZAR COMPROBANTE DE PAGO', 'DBO', NOW());

INSERT INTO segpermiso (IdMenu, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdMenu FROM segmenu x WHERE x.Codigo = 'COMPROBANTE'), 'COMP.APROBAR', 'APROBAR COMPROBANTE DE PAGO', 'DBO', NOW());

INSERT INTO segpermiso (IdMenu, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdMenu FROM segmenu x WHERE x.Codigo = 'COMPROBANTE'), 'COMP.RPT', 'REPORTE COMPROBANTE DE PAGO', 'DBO', NOW());
