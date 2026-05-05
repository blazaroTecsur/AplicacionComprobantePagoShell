using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Domain.Entities;
using Mapster;

namespace ComprobantePago.Application.Mapping
{
    public static class MapsterConfig
    {
        public static void Configure()
        {
            // ── Comprobante → ComprobanteDto (listado) ─────────────────────────
            TypeAdapterConfig<Comprobante, ComprobanteDto>.NewConfig()
                .Map(d => d.TipoComprobante, s => s.TipoDocumento)
                .Map(d => d.Proveedor,       s => s.RazonSocialReceptor)
                .Map(d => d.Fecha,           s => s.FechaEmision.ToString("dd/MM/yyyy"))
                .Map(d => d.Estado,          s => s.CodigoEstado);

            // ── ImputacionContable → ImputacionDetalleDto ──────────────────────
            TypeAdapterConfig<ImputacionContable, ImputacionDetalleDto>.NewConfig()
                .Map(d => d.AliasCuenta,      s => s.AliasCuenta      ?? string.Empty)
                .Map(d => d.CuentaContable,   s => s.CuentaContable   ?? string.Empty)
                .Map(d => d.DescripcionCuenta,s => s.DescripcionCuenta ?? string.Empty)
                .Map(d => d.Descripcion,      s => s.Descripcion      ?? string.Empty)
                .Map(d => d.Proyecto,         s => s.Proyecto         ?? string.Empty)
                .Map(d => d.CodUnidad1Cuenta, s => s.CodUnidad1Cuenta ?? string.Empty)
                .Map(d => d.CodUnidad3Cuenta, s => s.CodUnidad3Cuenta ?? string.Empty)
                .Map(d => d.CodUnidad4Cuenta, s => s.CodUnidad4Cuenta ?? string.Empty);

            // ── ImputacionDto → ImputacionContable (crear / editar) ────────────
            TypeAdapterConfig<ImputacionDto, ImputacionContable>.NewConfig()
                .Map(d => d.Secuencia, s => ParsearSecuencia(s.Secuencia))
                .Ignore(d => d.IdImputacionContable)
                .Ignore(d => d.UsuarioReg)
                .Ignore(d => d.FechaReg)
                .Ignore(d => d.UsuarioAct)
                .Ignore(d => d.FechaAct)
                .Ignore(d => d.Comprobante);
        }

        private static int ParsearSecuencia(string value) =>
            int.TryParse(value, out var n) ? n : 0;
    }
}
