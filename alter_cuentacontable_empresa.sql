-- Agrega columna Empresa a tmacuentacontable para filtrar cuentas por empresa
ALTER TABLE tmacuentacontable
    ADD COLUMN Empresa VARCHAR(10) NOT NULL DEFAULT '' AFTER Descripcion;

-- Índice para acelerar los filtros por empresa
CREATE INDEX idx_cuentacontable_empresa ON tmacuentacontable (Empresa);
