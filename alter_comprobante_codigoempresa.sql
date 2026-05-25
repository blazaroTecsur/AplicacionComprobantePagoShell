-- Agrega la columna CodigoEmpresa a rcocomprobante para soporte multiempresa.
-- Los registros existentes quedan con '' (vacío); actualizar manualmente si es necesario.
ALTER TABLE rcocomprobante
    ADD COLUMN CodigoEmpresa VARCHAR(50) NOT NULL DEFAULT '';
