using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reciclaje.Aplicacion.Interfaces
{
    public interface IConversionarticuloServicio
    {
        Task<IEnumerable<ConversionarticuloDto>> Listar();
        Task<ConversionarticuloDto> ObtenerPorId(int id);
        Task Crear(ConversionarticuloCrearDto dto, string usuarioActual);
        Task Editar(ConversionarticuloEditarDto dto, string usuarioActual);
        Task Eliminar(int id);
    }
}