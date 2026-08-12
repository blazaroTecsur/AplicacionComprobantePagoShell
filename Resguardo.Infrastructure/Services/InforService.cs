using Infor.Abstractions.DTOs;
using Infor.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
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
            var infor = await _idoService.LoadAsync("FSSROs", "SroNum,Description,StatCode,Dept,SroType,PagerAddr,DeptDescription,SROTypeDesc", $"SroNum='{numSro}'", 1);
            var result = infor.Deserialize<IdoResponse>();

            if (result is null || result.Items.Count == 0)
                throw new BusinessException(StatusCodes.Status400BadRequest.ToString(), $"La SRO {numSro} no existe");

            var item = result.Items.First();
            return new ObtenerOrdenResponse
            {
                Id = item.GetProperty("_ItemId").GetString(),
                NumSro = item.GetProperty("SroNum").GetString()?.Trim(),
                Descripcion = item.GetProperty("Description").GetString(),
                CodDpto = item.GetProperty("Dept").GetString(),
                NomDpto = item.GetProperty("DeptDescription").GetString(),
                Estado = item.TryGetProperty("StatCode", out var stat) ? stat.GetString() : null,
                CodActv = item.GetProperty("SroType").GetString(),
                NomActv = item.GetProperty("SROTypeDesc").GetString(),
                CodSupr = "SUP001",
                NomSupr = "Juan Pérez",
                CodSctta = "GR00003",
                NomSctta = "GRUPO DE CONTRATISTAS INTERNACIONALES",
                FechaFoc = DateTime.Now.AddDays(-10),
                Coordenada = "-12.1726108,-76.9724007",
                Direccion = item.TryGetProperty("PagerAddr", out var addr) ? addr.GetString() : null
            };            
        }
    }
}