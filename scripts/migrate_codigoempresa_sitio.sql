-- =============================================================================
-- MIGRACIÓN: CodigoEmpresa → usar valor de Sitio (con prefijo S)
-- Ejecutar ANTES de desplegar la versión que elimina CodigoEmpresa()
--
-- Antes: TECSUR, GCI, ANDES
-- Después: STECSUR, SGCI, SANDES
-- =============================================================================

-- Verificar estado actual antes de ejecutar
SELECT 'rcocomprobante'       AS tabla, CodigoEmpresa, COUNT(*) AS registros
  FROM rcocomprobante
 GROUP BY CodigoEmpresa
UNION ALL
SELECT 'rcoserie_correlativo' AS tabla, CodigoEmpresa, COUNT(*) AS registros
  FROM rcoserie_correlativo
 GROUP BY CodigoEmpresa;

-- =============================================================================
-- ACTUALIZACIÓN (solo registros que aún no tienen el prefijo S)
-- =============================================================================

UPDATE rcocomprobante
   SET CodigoEmpresa = CONCAT('S', CodigoEmpresa)
 WHERE CodigoEmpresa NOT LIKE 'S%'
   AND CodigoEmpresa <> '';

UPDATE rcoserie_correlativo
   SET CodigoEmpresa = CONCAT('S', CodigoEmpresa)
 WHERE CodigoEmpresa NOT LIKE 'S%'
   AND CodigoEmpresa <> '';

-- Verificar resultado final
SELECT 'rcocomprobante'       AS tabla, CodigoEmpresa, COUNT(*) AS registros
  FROM rcocomprobante
 GROUP BY CodigoEmpresa
UNION ALL
SELECT 'rcoserie_correlativo' AS tabla, CodigoEmpresa, COUNT(*) AS registros
  FROM rcoserie_correlativo
 GROUP BY CodigoEmpresa;
