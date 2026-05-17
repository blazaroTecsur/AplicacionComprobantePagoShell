using ComprobantePago.Application.DTOs.Comprobante.Common;
using ComprobantePago.Application.Interfaces.Services.Maestros;
using ComprobantePago.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Seguridad.Abstractions.Interfaces;

namespace ComprobantePago.Infrastructure.Services.Maestros
{
    public class DbCuentaContableService(AppDbContext contexto, IUsuarioContexto usuario) : ICuentaContableService
    {
        private readonly AppDbContext _contexto = contexto;
        private readonly IUsuarioContexto _usuario = usuario;

        public async Task<IEnumerable<ComboDto>> ObtenerCuentasContablesAsync(string filtro = "")
        {
            var empresa = _usuario.Empresa;
            bool filtrarEmpresa = !string.IsNullOrWhiteSpace(empresa);
            bool tieneFiltro = !string.IsNullOrWhiteSpace(filtro);

            return await _contexto.CuentasContables
                .Where(x => x.Activo &&
                    (!filtrarEmpresa || x.Empresa == empresa) &&
                    (!tieneFiltro || x.Codigo.Contains(filtro) || x.Descripcion.Contains(filtro)))
                .OrderBy(x => x.Codigo)
                .Select(x => new ComboDto { Codigo = x.Codigo, Descripcion = x.Descripcion })
                .ToListAsync();
        }
    }
}
