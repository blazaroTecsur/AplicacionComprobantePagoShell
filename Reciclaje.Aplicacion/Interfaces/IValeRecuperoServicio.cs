using Reciclaje.Aplicacion.DTOs.Reciclaje;
using Reciclaje.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reciclaje.Aplicacion.Interfaces
{
    public interface IValeRecuperoServicio
    {
        Task<ValeRecuperoViewModel> Buscar(ValeRecuperoBusquedaDto filtros);
        Task GenerarValeEspecifico(List<int> srolineaIds, string usuarioActual);
        Task GenerarValeConsolidado(List<int> srolineaIds, string usuarioActual);

        // Recepcionar Vales
        Task<ValeRecuperoListaViewModel> BuscarVales(ValeRecuperoBuscarDto filtros);
        Task GuardarRecepcion(List<ValeRecepcionDto> recepciones, string usuarioActual);
        // Confirmar Vales
        Task<ValeConfirmacionListaViewModel> BuscarValesConfirmacion(ValeRecuperoBuscarDto filtros);
        Task GuardarConfirmacion(List<ValeConfirmacionGuardarDto> confirmaciones, string usuarioActual);
        Task RechazarVales(List<ValeConfirmacionGuardarDto> confirmaciones, string usuarioActual);
    }
}
