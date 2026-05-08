-- =============================================================
-- Menú, permisos, roles, perfiles y asignaciones
-- Módulo: Comprobante de Pago
-- =============================================================

-- ── 1. Menú ───────────────────────────────────────────────────
INSERT INTO segmenu (IdMenu, IdApp, Codigo, Nombre, Ruta, Icono, Estado, UsuarioReg, FechaReg, UsuarioAct, FechaAct)
VALUES (NULL, '1', 'COMPROBANTE', 'Registro de Comprobantes de Pago', '/Comprobante/Index', 'fa-file-invoice-dollar', '1', 'DBO', NOW(), NULL, NULL);

-- ── 2. Permisos ───────────────────────────────────────────────
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

-- ── 3. Roles ──────────────────────────────────────────────────
INSERT INTO segrol (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'COMP_CONSULTA',  'CONSULTAR COMPROBANTE DE PAGO',   'DBO', NOW());

INSERT INTO segrol (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'COMP_EDITOR',    'EDITAR COMPROBANTE DE PAGO',      'DBO', NOW());

INSERT INTO segrol (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'COMP_ENVIAR',    'ENVIAR COMPROBANTE DE PAGO',      'DBO', NOW());

INSERT INTO segrol (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'COMP_AUTORIZAR', 'AUTORIZAR COMPROBANTE DE PAGO',   'DBO', NOW());

INSERT INTO segrol (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'COMP_APROBAR',   'APROBAR COMPROBANTE DE PAGO',     'DBO', NOW());

INSERT INTO segrol (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'COMP_ANULAR',    'ANULAR COMPROBANTE DE PAGO',      'DBO', NOW());

INSERT INTO segrol (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'COMP_REPORT',    'REPORTE COMPROBANTE DE PAGO',     'DBO', NOW());

-- ── 4. Rol → Permiso ──────────────────────────────────────────
INSERT INTO segrolpermiso (IdRol, IdPermiso)
SELECT (SELECT y.IdRol FROM segrol y WHERE y.IdTenant = 1 AND y.Codigo = 'COMP_CONSULTA'),
       x.IdPermiso FROM segpermiso x WHERE x.Codigo IN ('COMP.VER');

INSERT INTO segrolpermiso (IdRol, IdPermiso)
SELECT (SELECT y.IdRol FROM segrol y WHERE y.IdTenant = 1 AND y.Codigo = 'COMP_EDITOR'),
       x.IdPermiso FROM segpermiso x WHERE x.Codigo IN ('COMP.GUARDAR');

INSERT INTO segrolpermiso (IdRol, IdPermiso)
SELECT (SELECT y.IdRol FROM segrol y WHERE y.IdTenant = 1 AND y.Codigo = 'COMP_ENVIAR'),
       x.IdPermiso FROM segpermiso x WHERE x.Codigo IN ('COMP.ENVIAR');

INSERT INTO segrolpermiso (IdRol, IdPermiso)
SELECT (SELECT y.IdRol FROM segrol y WHERE y.IdTenant = 1 AND y.Codigo = 'COMP_AUTORIZAR'),
       x.IdPermiso FROM segpermiso x WHERE x.Codigo IN ('COMP.AUTORIZAR');

INSERT INTO segrolpermiso (IdRol, IdPermiso)
SELECT (SELECT y.IdRol FROM segrol y WHERE y.IdTenant = 1 AND y.Codigo = 'COMP_APROBAR'),
       x.IdPermiso FROM segpermiso x WHERE x.Codigo IN ('COMP.APROBAR');

INSERT INTO segrolpermiso (IdRol, IdPermiso)
SELECT (SELECT y.IdRol FROM segrol y WHERE y.IdTenant = 1 AND y.Codigo = 'COMP_ANULAR'),
       x.IdPermiso FROM segpermiso x WHERE x.Codigo IN ('COMP.ANULAR');

INSERT INTO segrolpermiso (IdRol, IdPermiso)
SELECT (SELECT y.IdRol FROM segrol y WHERE y.IdTenant = 1 AND y.Codigo = 'COMP_REPORT'),
       x.IdPermiso FROM segpermiso x WHERE x.Codigo IN ('COMP.RPT');

-- ── 5. Perfiles ───────────────────────────────────────────────
INSERT INTO segperfil (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'ASISTENT_ADM', 'ASISTENTE ADMINISTRATIVO', 'DBO', NOW());

INSERT INTO segperfil (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'JEFATUR_ARE',  'JEFATURA DE AREA',         'DBO', NOW());

INSERT INTO segperfil (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'ANALIST_JR',   'ANALISTA CONTABLE JUNIOR', 'DBO', NOW());

INSERT INTO segperfil (IdTenant, Codigo, Nombre, UsuarioReg, FechaReg)
VALUES ((SELECT x.IdTenant FROM segtenant x WHERE x.IdTenant = 1), 'JEFE_CONT',    'JEFE DE CONTABILIDAD',     'DBO', NOW());

-- ── 6. Perfil → Rol ───────────────────────────────────────────
-- ASISTENT_ADM: consulta + edición + envío
INSERT INTO segperfilrol (IdPerfil, IdRol)
SELECT (SELECT y.IdPerfil FROM segperfil y WHERE y.IdTenant = 1 AND y.Codigo = 'ASISTENT_ADM'),
       x.IdRol FROM segrol x WHERE x.Codigo IN ('COMP_CONSULTA', 'COMP_EDITOR', 'COMP_ENVIAR');

-- JEFATUR_ARE: consulta + autorización
INSERT INTO segperfilrol (IdPerfil, IdRol)
SELECT (SELECT y.IdPerfil FROM segperfil y WHERE y.IdTenant = 1 AND y.Codigo = 'JEFATUR_ARE'),
       x.IdRol FROM segrol x WHERE x.Codigo IN ('COMP_CONSULTA', 'COMP_AUTORIZAR');

-- ANALIST_JR: consulta + aprobación + anulación + reportes
INSERT INTO segperfilrol (IdPerfil, IdRol)
SELECT (SELECT y.IdPerfil FROM segperfil y WHERE y.IdTenant = 1 AND y.Codigo = 'ANALIST_JR'),
       x.IdRol FROM segrol x WHERE x.Codigo IN ('COMP_CONSULTA', 'COMP_APROBAR', 'COMP_ANULAR', 'COMP_REPORT');

-- JEFE_CONT: consulta + aprobación + reportes
INSERT INTO segperfilrol (IdPerfil, IdRol)
SELECT (SELECT y.IdPerfil FROM segperfil y WHERE y.IdTenant = 1 AND y.Codigo = 'JEFE_CONT'),
       x.IdRol FROM segrol x WHERE x.Codigo IN ('COMP_CONSULTA', 'COMP_APROBAR', 'COMP_REPORT');
