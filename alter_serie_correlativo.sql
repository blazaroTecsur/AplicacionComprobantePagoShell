CREATE TABLE IF NOT EXISTS rcoseriecorrelativo (
    IdSerieCorrelativo INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    CodigoEmpresa VARCHAR(50) NOT NULL,
    TipoDocumento VARCHAR(10) NOT NULL,
    Anio INT NOT NULL,
    Mes INT NOT NULL,
    UltimoCorrelativo INT NOT NULL DEFAULT 0,
    UNIQUE KEY uq_serie (CodigoEmpresa, TipoDocumento, Anio, Mes)
);
