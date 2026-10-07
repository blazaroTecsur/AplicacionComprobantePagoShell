-- ============================================================
-- INSERTS CATÁLOGO — rcoestadocomprobante
-- ============================================================

INSERT INTO rcoestadocomprobante (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'NUEVO', 'Nuevo', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcoestadocomprobante WHERE Codigo = 'NUEVO');

INSERT INTO rcoestadocomprobante (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'REGISTRADO', 'Registrado', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcoestadocomprobante WHERE Codigo = 'REGISTRADO');

INSERT INTO rcoestadocomprobante (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'ENVIADO', 'Enviado', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcoestadocomprobante WHERE Codigo = 'ENVIADO');

INSERT INTO rcoestadocomprobante (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'AUTORIZADO', 'Autorizado', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcoestadocomprobante WHERE Codigo = 'AUTORIZADO');

INSERT INTO rcoestadocomprobante (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'APROBADO', 'Aprobado', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcoestadocomprobante WHERE Codigo = 'APROBADO');

INSERT INTO rcoestadocomprobante (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'ANULADO', 'Anulado', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcoestadocomprobante WHERE Codigo = 'ANULADO');

INSERT INTO rcoestadocomprobante (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'DERIVADO', 'Derivado', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcoestadocomprobante WHERE Codigo = 'DERIVADO');

INSERT INTO rcoestadocomprobante (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'DERIVADO SYT', 'Derivado a Syteline', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcoestadocomprobante WHERE Codigo = 'DERIVADO SYT');

-- ============================================================
-- INSERTS CATÁLOGO — rcotipodocumento
-- ============================================================

INSERT INTO rcotipodocumento (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'FAC', 'Factura', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodocumento WHERE Codigo = 'FAC');

INSERT INTO rcotipodocumento (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'NCR', 'Nota de Crédito', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodocumento WHERE Codigo = 'NCR');

INSERT INTO rcotipodocumento (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'NDB', 'Nota de Débito', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodocumento WHERE Codigo = 'NDB');

INSERT INTO rcotipodocumento (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'LIQ', 'Liquidación de Compra', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodocumento WHERE Codigo = 'LIQ');

INSERT INTO rcotipodocumento (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'REC', 'Recibo por Honorarios', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodocumento WHERE Codigo = 'REC');

-- ============================================================
-- INSERTS CATÁLOGO — rcotiposunat
-- ============================================================

INSERT INTO rcotiposunat (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT '01', 'Factura', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotiposunat WHERE Codigo = '01');

INSERT INTO rcotiposunat (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT '07', 'Nota de Crédito', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotiposunat WHERE Codigo = '07');

INSERT INTO rcotiposunat (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT '08', 'Nota de Débito', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotiposunat WHERE Codigo = '08');

INSERT INTO rcotiposunat (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT '40', 'Recibo por Honorarios', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotiposunat WHERE Codigo = '40');

INSERT INTO rcotiposunat (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT '03', 'Boleta de Venta', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotiposunat WHERE Codigo = '03');

INSERT INTO rcotiposunat (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT '04', 'Liquidación de Compra', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotiposunat WHERE Codigo = '04');

INSERT INTO rcotiposunat (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'R1', 'Recibo por Honorarios', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotiposunat WHERE Codigo = 'R1');

INSERT INTO rcotiposunat (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT '14', 'Recibos Servicios Públicos', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotiposunat WHERE Codigo = '14');

INSERT INTO rcotiposunat (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
SELECT 'VALES', 'Vales', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotiposunat WHERE Codigo = 'VALES');

-- ============================================================
-- INSERTS CATÁLOGO — rcomoneda
-- ============================================================

INSERT INTO rcomoneda (Codigo, Descripcion, Simbolo, Activo, UsuarioReg, FechaReg)
SELECT 'PEN', 'Sol Peruano', 'S/.', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcomoneda WHERE Codigo = 'PEN');

INSERT INTO rcomoneda (Codigo, Descripcion, Simbolo, Activo, UsuarioReg, FechaReg)
SELECT 'USD', 'Dólar Americano', '$', 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcomoneda WHERE Codigo = 'USD');

-- ============================================================
-- INSERTS CATÁLOGO — rcolugarpago
-- ============================================================

INSERT INTO rcolugarpago (Codigo, Descripcion, Activo, UsuarioReg, FechaReg)
VALUES
    ('01', 'CAJA CHICA',        1, 'SISTEMA', NOW()),
    ('02', 'CUENTAS A RENDIR',  1, 'SISTEMA', NOW()),
    ('03', 'IMPORTACIONES',     1, 'SISTEMA', NOW());

-- ============================================================
-- INSERTS CATÁLOGO — rcotipodetraccion (SUNAT)
-- ============================================================

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '001', 'Azúcar y melaza de caña', 10.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '001');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '003', 'Alcohol etílico', 10.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '003');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '004', 'Recursos hidrobiológicos', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '004');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '005', 'Maíz amarillo duro', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '005');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '006', 'Algodón', 10.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '006');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '007', 'Caña de azúcar', 10.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '007');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '008', 'Madera', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '008');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '009', 'Arena y piedra', 10.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '009');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '010', 'Residuos, subproductos, desechos, recortes y desperdicios', 15.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '010');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '011', 'Bienes gravados con el IGV por renuncia a la exoneración', 10.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '011');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '012', 'Intermediación laboral y tercerización', 12.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '012');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '014', 'Carnes y despojos comestibles', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '014');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '017', 'Harina, polvo y "pellets" de pescado', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '017');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '019', 'Arrendamiento de bienes', 12.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '019');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '020', 'Mantenimiento y reparación de bienes muebles', 12.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '020');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '021', 'Movimiento de carga', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '021');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '022', 'Otros servicios empresariales', 12.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '022');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '023', 'Leche', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '023');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '024', 'Comisión mercantil', 12.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '024');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '025', 'Fabricación de bienes por encargo', 12.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '025');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '026', 'Servicio de transporte de personas', 10.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '026');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '030', 'Contratos de construcción', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '030');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '031', 'Oro gravado con el IGV', 12.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '031');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '032', 'Páprika y otros frutos de los géneros capsicum o pimienta', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '032');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '034', 'Minerales metálicos no auríferos', 10.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '034');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '035', 'Bienes exonerados del IGV', 1.50, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '035');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '036', 'Oro y demás minerales metálicos exonerados del IGV', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '036');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '039', 'Minerales no metálicos', 12.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '039');

INSERT INTO rcotipodetraccion (Codigo, Descripcion, Porcentaje, Activo, UsuarioReg, FechaReg)
SELECT '040', 'Bien inmueble gravado con IGV', 4.00, 1, 'SYSTEM', NOW()
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM rcotipodetraccion WHERE Codigo = '040');
