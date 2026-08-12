namespace Reciclaje.Aplicacion.Interfaces
{
    public interface ISyteLinePoServicio
    {
        /// <summary>
        /// Crea la Orden de Compra en SyteLine para el registro dado.
        /// Devuelve true si el POST fue exitoso (HTTP 200).
        /// </summary>
        Task<(bool exitoso, string mensaje)> CrearOrdenCompraAsync(
            int tareaId,
            short anno,
            byte mes,
            string nombrePo,
            string? sitio,
            string accessToken,
            string mongooseConfig);

        /// <summary>
        /// Consulta SLPoItems en SyteLine para el PoNum dado y devuelve
        /// el último PoLine (número entero) y el RowPointer de ese ítem.
        /// </summary>
        Task<(bool exitoso, string mensaje, int? ultimaLinea, string? rowPointer)>
            ObtenerUltimaLineaPoAsync(string poNum, string accessToken, string mongooseConfig);

        /// <summary>
        /// Inserta una nueva línea en la PO de SyteLine (SLPoItems/additem).
        /// PoLine = ultimaLinea + 1. Devuelve éxito, mensaje y la nueva PoLine usada.
        /// </summary>
        Task<(bool exitoso, string mensaje, int nuevaPoLine)> AgregarLineaPoAsync(
            string poNum,
            int ultimaLinea,
            string codigoArticulo,
            string descripcionArticulo,
            decimal cantidad,
            string unidadMedida,
            short anno,
            byte mes,
            string accessToken,
            string mongooseConfig,
            string articuloNoInv,
            decimal costoUnitario);
    }
}