-- Cambiar IdProveedorExternal de INT a VARCHAR(20) en tmaproveedor
ALTER TABLE tmaproveedor
    MODIFY COLUMN IdProveedorExternal VARCHAR(20) NULL;
