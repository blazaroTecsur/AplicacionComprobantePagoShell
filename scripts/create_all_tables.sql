-- ============================================================
-- SCRIPT COMPLETO: Tablas e Inserts
-- Base de datos: ComprobantePago
-- Motor: MySQL 8.0+
-- Generado: 2026-05-05
-- ============================================================

-- ============================================================
-- TABLAS CATÁLOGO
-- ============================================================

CREATE TABLE IF NOT EXISTS rcoestadocomprobante (
    IdEstadoComprobante INT           NOT NULL AUTO_INCREMENT,
    Codigo              VARCHAR(20)   NOT NULL,
    Descripcion         VARCHAR(100)  NOT NULL,
    Activo              TINYINT(1)    NOT NULL DEFAULT 1,
    UsuarioReg          VARCHAR(50)   NOT NULL,
    FechaReg            DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct          VARCHAR(50)   NULL,
    FechaAct            DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdEstadoComprobante)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS rcotipodocumento (
    IdTipoDocumento INT          NOT NULL AUTO_INCREMENT,
    Codigo          VARCHAR(5)   NOT NULL,
    Descripcion     VARCHAR(100) NOT NULL,
    Activo          TINYINT(1)   NOT NULL DEFAULT 1,
    UsuarioReg      VARCHAR(50)  NOT NULL,
    FechaReg        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct      VARCHAR(50)  NULL,
    FechaAct        DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdTipoDocumento)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS rcotiposunat (
    IdTipoSunat INT          NOT NULL AUTO_INCREMENT,
    Codigo      VARCHAR(5)   NOT NULL,
    Descripcion VARCHAR(100) NOT NULL,
    Activo      TINYINT(1)   NOT NULL DEFAULT 1,
    UsuarioReg  VARCHAR(50)  NOT NULL,
    FechaReg    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct  VARCHAR(50)  NULL,
    FechaAct    DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdTipoSunat)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS rcoseriecorrelativo (
    IdSerieCorrelativo INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    CodigoEmpresa VARCHAR(50) NOT NULL,
    TipoDocumento VARCHAR(10) NOT NULL,
    Anio INT NOT NULL,
    Mes INT NOT NULL,
    UltimoCorrelativo INT NOT NULL DEFAULT 0,
    UNIQUE KEY uq_serie (CodigoEmpresa, TipoDocumento, Anio, Mes)
);

CREATE TABLE IF NOT EXISTS rcomoneda (
    IdMoneda    INT          NOT NULL AUTO_INCREMENT,
    Codigo      VARCHAR(5)   NOT NULL,
    Descripcion VARCHAR(100) NOT NULL,
    Simbolo     VARCHAR(5)   NULL,
    Activo      TINYINT(1)   NOT NULL DEFAULT 1,
    UsuarioReg  VARCHAR(50)  NOT NULL,
    FechaReg    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct  VARCHAR(50)  NULL,
    FechaAct    DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdMoneda)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS rcolugarpago (
    IdLugarPago INT          NOT NULL AUTO_INCREMENT,
    Codigo      VARCHAR(5)   NOT NULL,
    Descripcion VARCHAR(100) NOT NULL,
    Activo      TINYINT(1)   NOT NULL DEFAULT 1,
    UsuarioReg  VARCHAR(50)  NOT NULL,
    FechaReg    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct  VARCHAR(50)  NULL,
    FechaAct    DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdLugarPago)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS rcotipodetraccion (
    IdTipoDetraccion INT           NOT NULL AUTO_INCREMENT,
    Codigo           VARCHAR(5)    NOT NULL,
    Descripcion      VARCHAR(200)  NOT NULL,
    Porcentaje       DECIMAL(5,2)  NOT NULL DEFAULT 0,
    Activo           TINYINT(1)    NOT NULL DEFAULT 1,
    UsuarioReg       VARCHAR(50)   NOT NULL,
    FechaReg         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct       VARCHAR(50)   NULL,
    FechaAct         DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdTipoDetraccion)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tmacuentacontable (
    IdCuentaContable INT          NOT NULL AUTO_INCREMENT,
    Codigo           VARCHAR(20)  NOT NULL,
    Descripcion      VARCHAR(200) NOT NULL,
    Activo           TINYINT(1)   NOT NULL DEFAULT 1,
    UsuarioReg       VARCHAR(50)  NOT NULL,
    FechaReg         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct       VARCHAR(50)  NULL,
    FechaAct         DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdCuentaContable)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tmacodigounidad1 (
    IdCodigoUnidad1 INT          NOT NULL AUTO_INCREMENT,
    Codigo          VARCHAR(10)  NOT NULL,
    Descripcion     VARCHAR(200) NOT NULL,
    Empresa         VARCHAR(50)  NOT NULL,
    Activo          TINYINT(1)   NOT NULL DEFAULT 1,
    UsuarioReg      VARCHAR(50)  NOT NULL,
    FechaReg        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct      VARCHAR(50)  NULL,
    FechaAct        DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdCodigoUnidad1)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tmacodigounidad2 (
    IdCodigoUnidad2 INT          NOT NULL AUTO_INCREMENT,
    Codigo          VARCHAR(10)  NOT NULL,
    Descripcion     VARCHAR(200) NOT NULL,
    Empresa         VARCHAR(50)  NOT NULL,
    Activo          TINYINT(1)   NOT NULL DEFAULT 1,
    UsuarioReg      VARCHAR(50)  NOT NULL,
    FechaReg        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct      VARCHAR(50)  NULL,
    FechaAct        DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdCodigoUnidad2)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tmacodigounidad3 (
    IdCodigoUnidad3 INT          NOT NULL AUTO_INCREMENT,
    Codigo          VARCHAR(10)  NOT NULL,
    Descripcion     VARCHAR(200) NOT NULL,
    Empresa         VARCHAR(50)  NOT NULL,
    Activo          TINYINT(1)   NOT NULL DEFAULT 1,
    UsuarioReg      VARCHAR(50)  NOT NULL,
    FechaReg        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct      VARCHAR(50)  NULL,
    FechaAct        DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdCodigoUnidad3)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tmacodigounidad4 (
    IdCodigoUnidad4 INT          NOT NULL AUTO_INCREMENT,
    Codigo          VARCHAR(10)  NOT NULL,
    Descripcion     VARCHAR(200) NOT NULL,
    Activo          TINYINT(1)   NOT NULL DEFAULT 1,
    UsuarioReg      VARCHAR(50)  NOT NULL,
    FechaReg        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct      VARCHAR(50)  NULL,
    FechaAct        DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdCodigoUnidad4)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tmaempleado (
    IdEmpleado              BIGINT          NOT NULL AUTO_INCREMENT,

    IdEmpleadoExternal      VARCHAR(50)     NOT NULL,
    Codigo                  VARCHAR(50)     NOT NULL,
    NombreCompleto          VARCHAR(200)    NOT NULL,

    -- datos principales
    Apellido                VARCHAR(100)    NULL,
    Nombre                  VARCHAR(100)    NULL,
    Alias                   VARCHAR(100)    NULL,
    Cargo                   VARCHAR(100)    NULL,
    Dpto                    VARCHAR(100)    NULL,
    Estado                  VARCHAR(20)     NULL,
    Turno                   VARCHAR(50)     NULL,
    Categoria               VARCHAR(50)     NULL,
    IdUsuario               VARCHAR(100)    NULL,
    FrecPago                VARCHAR(50)     NULL,
    TipEmp                  VARCHAR(50)     NULL,
    GenNomina               VARCHAR(10)     NULL,
    CtaSueldo               VARCHAR(50)     NULL,

    -- nombre fiscal
    PrimerNombre            VARCHAR(100)    NULL,
    SegundoNombre           VARCHAR(100)    NULL,
    PrimerApellido          VARCHAR(100)    NULL,
    SegundoApellido         VARCHAR(100)    NULL,

    -- contacto
    Direccion1              VARCHAR(200)    NULL,
    Direccion2              VARCHAR(200)    NULL,
    Direccion3              VARCHAR(200)    NULL,
    Direccion4              VARCHAR(200)    NULL,
    Ciudad                  VARCHAR(100)    NULL,
    CodProvincia            VARCHAR(50)     NULL,
    Cp                      VARCHAR(20)     NULL,
    Municipio               VARCHAR(100)    NULL,
    Telefono                VARCHAR(50)     NULL,
    TelComercial            VARCHAR(50)     NULL,
    ExtensionTel            VARCHAR(20)     NULL,
    CorreoElect             VARCHAR(150)    NULL,
    Correo                  VARCHAR(150)    NULL,

    -- recursos humanos
    FechaContr              DATETIME        NULL,
    FechaRevis              DATETIME        NULL,
    FechaRescis             DATETIME        NULL,

    UsuarioReg              VARCHAR(100)    NULL,
    FechaReg                DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct              VARCHAR(100)    NULL,
    FechaAct                DATETIME        NULL ON UPDATE CURRENT_TIMESTAMP,

    PRIMARY KEY (IdEmpleado),
    UNIQUE KEY UQ_tmaempleado_IdEmpleadoExternal (IdEmpleadoExternal)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tmaproveedor (
    IdProveedor              INT          NOT NULL AUTO_INCREMENT,
    IdProveedorExternal      VARCHAR(20)  NOT NULL,
    NombreProveedor          VARCHAR(255) NULL,
    TipoPersona              VARCHAR(20)  NOT NULL,
    Direccion1               VARCHAR(255) NULL,
    Direccion2               VARCHAR(255) NULL,
    Direccion3               VARCHAR(255) NULL,
    Direccion4               VARCHAR(255) NULL,
    Comprador                VARCHAR(150) NULL,
    Estado                   VARCHAR(20)  NOT NULL,
    Contacto                 VARCHAR(250) NULL,
    TelefonoContacto         VARCHAR(20)  NULL,
    CorreoExternoContacto    VARCHAR(100) NULL,
    CorreoInternoContacto    VARCHAR(100) NULL,
    Ruc                      VARCHAR(20)  NOT NULL,
    UsuarioReg               VARCHAR(30)  NULL,
    FechaReg                 DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct               VARCHAR(30)  NULL,
    FechaAct                 DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdProveedor),
    UNIQUE KEY UQ_tmaproveedor_IdProveedorExternal (IdProveedorExternal)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- TABLAS TRANSACCIONALES
-- ============================================================

CREATE TABLE IF NOT EXISTS rcocomprobante (
    IdComprobante        INT           NOT NULL AUTO_INCREMENT,
    Folio                VARCHAR(20)   NOT NULL,
    RucReceptor          VARCHAR(11)   NOT NULL,
    RazonSocialReceptor  VARCHAR(200)  NOT NULL,
    TipoDocumento        VARCHAR(5)    NOT NULL,
    TipoSunat            VARCHAR(5)    NOT NULL,
    Serie                VARCHAR(10)   NOT NULL,
    Numero               VARCHAR(20)   NOT NULL,
    FechaEmision         DATETIME      NOT NULL,
    FechaRecepcion       DATETIME      NULL,
    Moneda               VARCHAR(5)    NOT NULL,
    TasaCambio           DECIMAL(10,4) NOT NULL DEFAULT 1,
    LugarPago            VARCHAR(50)   NULL,
    PlazoPago            VARCHAR(10)   NULL,
    FechaVencimiento     DATETIME      NULL,
    RucBeneficiario      VARCHAR(11)   NULL,
    RazonSocialBenef     VARCHAR(200)  NULL,
    Observacion          VARCHAR(500)  NULL,
    OrdenCompra          VARCHAR(50)   NULL,
    FactMultiple         TINYINT(1)    NOT NULL DEFAULT 0,
    TipoDocAsociado      VARCHAR(5)    NULL,
    SerieAsociado        VARCHAR(10)   NULL,
    NumeroAsociado       VARCHAR(20)   NULL,
    TieneDetraccion      TINYINT(1)    NOT NULL DEFAULT 0,
    TipoDetraccion       VARCHAR(10)   NULL,
    PorcentajeDetraccion DECIMAL(5,2)  NULL,
    MontoDetraccion      DECIMAL(18,2) NOT NULL DEFAULT 0,
    ConstanciaDeposito   VARCHAR(20)   NULL,
    FechaDeposito        DATETIME      NULL,
    MontoNeto            DECIMAL(18,2) NOT NULL DEFAULT 0,
    MontoExento          DECIMAL(18,2) NOT NULL DEFAULT 0,
    PorcentajeIGV        DECIMAL(5,2)  NOT NULL DEFAULT 0,
    MontoIGVCosto        DECIMAL(18,2) NOT NULL DEFAULT 0,
    MontoIGVCredito      DECIMAL(18,2) NOT NULL DEFAULT 0,
    MontoTotal           DECIMAL(18,2) NOT NULL DEFAULT 0,
    MontoBruto           DECIMAL(18,2) NOT NULL DEFAULT 0,
    MontoRetencion       DECIMAL(18,2) NOT NULL DEFAULT 0,
    MontoMultas          DECIMAL(18,2) NOT NULL DEFAULT 0,
    ValorAduana          DECIMAL(18,2) NOT NULL DEFAULT 0,
    MontoRedondeo        DECIMAL(18,2) NOT NULL DEFAULT 0,
    EsDocumentoElectronico TINYINT(1)  NOT NULL DEFAULT 1,
    AplicaIGV            VARCHAR(1)    NOT NULL DEFAULT 'N',
    RequiereDetraccion   VARCHAR(1)    NOT NULL DEFAULT 'N',
    RequiereAduana       VARCHAR(1)    NOT NULL DEFAULT 'N',
    TienePdf             TINYINT(1)    NOT NULL DEFAULT 0,
    EdicionManual        TINYINT(1)    NOT NULL DEFAULT 0,
    EstaDerivado         TINYINT(1)    NOT NULL DEFAULT 0,
    CodigoEstado         VARCHAR(20)   NOT NULL DEFAULT 'NUEVO',
    EstadoSunat          VARCHAR(20)   NULL,
    RolDigitacion        VARCHAR(200)  NULL,
    FechaDigitacion      DATETIME      NULL,
    RolAutorizacion      VARCHAR(200)  NULL,
    FechaAutorizacion    DATETIME      NULL,
    RolAprobacion        VARCHAR(200)  NULL,
    FechaAprobacion      DATETIME      NULL,
    RolAnulacion         VARCHAR(200)  NULL,
    FechaAnulacion       DATETIME      NULL,
    Mensaje              VARCHAR(500)  NULL,
    Origen               VARCHAR(10)   NULL,
    SPO                  VARCHAR(50)   NULL,
    EsEmpleado           TINYINT(1)    NOT NULL DEFAULT 0,
    EmpleadoCodigo       VARCHAR(20)   NULL,
    EmpleadoNombre       VARCHAR(200)  NULL,
    VoucherSyteline      INT           NULL,
    CodigoEmpresa        VARCHAR(50)   NOT NULL DEFAULT '',
    Departamento         VARCHAR(50)   NOT NULL,
    UsuarioReg           VARCHAR(50)   NOT NULL,
    FechaReg             DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct           VARCHAR(50)   NULL,
    FechaAct             DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdComprobante),
    UNIQUE KEY UQ_rcocomprobante_Folio (Folio)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS rcoimputacioncontable (
    IdImputacionContable INT           NOT NULL AUTO_INCREMENT,
    Folio                VARCHAR(20)   NOT NULL,
    Secuencia            INT           NOT NULL DEFAULT 0,
    AliasCuenta          VARCHAR(50)   NULL,
    CuentaContable       VARCHAR(20)   NULL,
    DescripcionCuenta    VARCHAR(200)  NULL,
    Monto                DECIMAL(18,2) NOT NULL DEFAULT 0,
    Descripcion          VARCHAR(500)  NULL,
    Proyecto             VARCHAR(20)   NULL,
    CodUnidad1Cuenta     VARCHAR(10)   NULL,
    CodUnidad2Cuenta     VARCHAR(10)   NULL,
    CodUnidad3Cuenta     VARCHAR(10)   NULL,
    CodUnidad4Cuenta     VARCHAR(10)   NULL,
    TipoLinea            VARCHAR(20)   NULL,
    UsuarioReg           VARCHAR(50)   NOT NULL,
    FechaReg             DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct           VARCHAR(50)   NULL,
    FechaAct             DATETIME      NULL ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (IdImputacionContable),
    CONSTRAINT FK_rcoimputacioncontable_Folio
        FOREIGN KEY (Folio) REFERENCES rcocomprobante (Folio)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS rcodocumentoelectronico (
    IdDocumento   INT          NOT NULL AUTO_INCREMENT,
    Folio         VARCHAR(20)  NOT NULL,
    TipoArchivo   VARCHAR(10)  NOT NULL,
    SubTipo       VARCHAR(30)  NOT NULL DEFAULT '',
    NombreArchivo VARCHAR(255) NOT NULL,
    RutaArchivo   VARCHAR(500) NOT NULL DEFAULT '',
    TamanioBytes  BIGINT       NOT NULL DEFAULT 0,
    FechaReg      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioReg    VARCHAR(50)  NOT NULL,
    PRIMARY KEY (IdDocumento),
    INDEX IX_rcodocumentoelectronico_Folio (Folio)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;