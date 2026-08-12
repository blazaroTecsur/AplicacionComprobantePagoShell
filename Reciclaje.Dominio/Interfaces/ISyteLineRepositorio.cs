using Reciclaje.Dominio.Entidades;

namespace Reciclaje.Dominio.Interfaces;

public interface ISyteLineRepositorio
{
    // ── integracionsyteline ──────────────────────────────────────────────────
    Task<bool> ExisteIntegracion(string rowPointer);
    Task InsertarIntegracion(Integracionsyteline registro);

    // ── sro ──────────────────────────────────────────────────────────────────
    Task<Sro?> ObtenerSroPorNumero(string numeroSro);
    Task InsertarSro(Sro sro);

    // ── srolinea ─────────────────────────────────────────────────────────────
    Task<bool> ExisteSrolinea(string rowPointer);
    Task InsertarSrolinea(Srolinea linea);
    Task<IEnumerable<Srolinea>> ObtenerLineasConVales(IEnumerable<int>? sroLineaIds);

    // ── conversionarticulo ───────────────────────────────────────────────────
    Task<(string? ArticuloReciclaje, int? ConversionId)> ObtenerArticuloReciclaje(string articuloNoInventariado);

    Task<int> ObtenerSroidPorNumero(string numeroSro);

    // ── tareaordencompra ─────────────────────────────────────────────────────
    /// <summary>
    /// Devuelve el NombrePo de la TareaOrdenCompra para el año y mes dados.
    /// Retorna null si no existe registro.
    /// </summary>
    Task<string?> ObtenerNombrePoAsync(short anno, byte mes);

}