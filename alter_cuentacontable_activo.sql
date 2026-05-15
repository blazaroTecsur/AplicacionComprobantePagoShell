-- Agregar columna Activo a tmacuentacontable (por defecto todas activas)
ALTER TABLE tmacuentacontable
    ADD COLUMN Activo TINYINT(1) NOT NULL DEFAULT 1;
