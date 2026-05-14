-- ============================================================
-- Estado PAGADO – perfil ASISTENT_TES, rol COMP_PAGAR,
-- permiso COMP.PAGAR
-- Aplicar en la base de datos de Seguridad
-- ============================================================

-- 1. Permiso
INSERT INTO segpermiso (CodPermiso, Descripcion, CodApp, UsuarioReg, FechaReg)
VALUES ('COMP.PAGAR', 'Marcar comprobante como pagado', 'COMP', 'DBO', NOW())
ON DUPLICATE KEY UPDATE Descripcion = VALUES(Descripcion);

-- 2. Rol
INSERT INTO segrol (CodRol, Descripcion, CodApp, UsuarioReg, FechaReg)
VALUES ('COMP_PAGAR', 'Tesorero asistente – pago de comprobantes', 'COMP', 'DBO', NOW())
ON DUPLICATE KEY UPDATE Descripcion = VALUES(Descripcion);

-- 3. Rol ↔ Permiso
INSERT INTO segrolpermiso (CodRol, CodPermiso, UsuarioReg, FechaReg)
VALUES ('COMP_PAGAR', 'COMP.PAGAR', 'DBO', NOW())
ON DUPLICATE KEY UPDATE UsuarioReg = VALUES(UsuarioReg);

-- También requiere ver la lista de comprobantes
INSERT INTO segrolpermiso (CodRol, CodPermiso, UsuarioReg, FechaReg)
VALUES ('COMP_PAGAR', 'COMP.VER', 'DBO', NOW())
ON DUPLICATE KEY UPDATE UsuarioReg = VALUES(UsuarioReg);

-- 4. Perfil
INSERT INTO segperfil (CodPerfil, Descripcion, CodApp, UsuarioReg, FechaReg)
VALUES ('ASISTENT_TES', 'Asistente de Tesorería', 'COMP', 'DBO', NOW())
ON DUPLICATE KEY UPDATE Descripcion = VALUES(Descripcion);

-- 5. Perfil ↔ Rol
INSERT INTO segperfilrol (CodPerfil, CodRol, UsuarioReg, FechaReg)
VALUES ('ASISTENT_TES', 'COMP_PAGAR', 'DBO', NOW())
ON DUPLICATE KEY UPDATE UsuarioReg = VALUES(UsuarioReg);
