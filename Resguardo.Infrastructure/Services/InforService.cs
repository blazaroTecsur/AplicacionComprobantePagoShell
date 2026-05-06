using Infor.Abstractions.DTOs;
using Infor.Abstractions.Interfaces;
using Resguardo.Application.DTOs.Infor;
using Resguardo.Application.Exceptions;
using Resguardo.Application.Services;
using System.Text.Json;

namespace Resguardo.Infrastructure.Services
{
    public class InforService : IInforService
    {
        private readonly IInforIdoService _idoService;
        public InforService(IInforIdoService idoService)
        {
            _idoService = idoService;
        }
        public async Task<ObtenerOrdenResponse> ObtenerOrden(string numSro)
        {
            var infor = await _idoService.LoadAsync("FSSROs", "SroNum,Description,StatCode,Dept,SroType,PagerAddr,DeptDescription,SROTypeDesc", $"SroNum='{numSro}'", 1, null, true);
            var result = infor.Deserialize<IdoResponse>();

            if (result is null || result.Items.Count == 0)
                throw new NotFoundException($"SRO {numSro}");

            var items = result.Items.Select(row =>
            {
                var dict = row.ToDictionary(x => x.Name, x => x.Value);
                return new ObtenerOrdenResponse
                {
                    Id = dict.GetValueOrDefault("_ItemId"),
                    NumSro = dict.GetValueOrDefault("SroNum")?.Trim(),
                    Descripcion = dict.GetValueOrDefault("Description"),
                    CodDpto = dict.GetValueOrDefault("Dept"),
                    NomDpto = dict.GetValueOrDefault("DeptDescription"),
                    Estado = dict.GetValueOrDefault("StatCode"),
                    CodActv = dict.GetValueOrDefault("SroType"),
                    NomActv = dict.GetValueOrDefault("SROTypeDesc"),
                    CodSupr = "SUP001",
                    NomSupr = "Juan Pérez",
                    RucSctta = "12345678901",
                    NomSctta = "Constructora XYZ S.A.",
                    FechaFoc = DateTime.Now.AddDays(-10),
                    Coordenada = "-12.1726108,-76.9724007",
                    Direccion = dict.GetValueOrDefault("PagerAddr")
                };
            }).ToList();

            var sro = items.FirstOrDefault(x => x.NumSro == numSro);
            return sro;
        }
    }
}