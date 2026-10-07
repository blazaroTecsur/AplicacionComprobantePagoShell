-- Migración: reemplaza el almacenamiento binario en BD por rutas al sistema de archivos.
-- Ejecutar ANTES de desplegar la nueva versión de la aplicación.
-- Los archivos existentes en `Contenido` NO se migran automáticamente;
-- si se requiere conservarlos, exportarlos manualmente al directorio configurado
-- en "Storages:Path" con la estructura <folio>/<NombreArchivo> antes de ejecutar este script.

ALTER TABLE rcodocumentoelectronico
    DROP COLUMN Contenido,
    ADD COLUMN RutaArchivo   VARCHAR(500) NOT NULL DEFAULT '' AFTER NombreArchivo,
    ADD COLUMN TamanioBytes  BIGINT       NOT NULL DEFAULT 0  AFTER RutaArchivo;
