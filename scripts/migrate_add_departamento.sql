-- =============================================================================
-- MIGRACIÓN: Agregar columna Departamento a rcocomprobante
-- Ejecutar ANTES de desplegar la versión que usa Departamento
--
-- Los registros históricos quedan con NULL (visible para todos los usuarios)
-- Los nuevos registros se poblan automáticamente desde el claim "dpto"
-- =============================================================================

ALTER TABLE rcocomprobante
  ADD COLUMN Departamento VARCHAR(50) NULL;

-- Verificar
SELECT Departamento, COUNT(*) AS registros
  FROM rcocomprobante
 GROUP BY Departamento;
