-- Agregar columna CodUnidad2Cuenta a la tabla rcoimputacioncontable
ALTER TABLE rcoimputacioncontable
    ADD COLUMN CodUnidad2Cuenta VARCHAR(20) NULL
    AFTER CodUnidad1Cuenta;
