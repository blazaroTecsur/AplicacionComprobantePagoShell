using AutoMapper;
using Resguardo.Application.Commands.ActualizarSolicitud;
using Resguardo.Application.Commands.ConfirmarServicio;
using Resguardo.Application.Commands.RegistrarConfig;
using Resguardo.Application.Commands.RegistrarSolicitud;
using Resguardo.Domain.Entities;

namespace Resguardo.Application.Mapeos
{
    public class SolicitudMapper : Profile
    {
        public SolicitudMapper()
        {
            CreateMap<RegistrarSolicitudCommand, Solicitud>().ReverseMap();
            CreateMap<RegistrarServicioItem, Servicio>().ReverseMap();
            CreateMap<ActualizarServicioItem, Servicio>().ReverseMap();
            CreateMap<ConfirmarServicioItem, ServicioProv>().ReverseMap();
            CreateMap<RegistrarConfigItem, Limite>().ReverseMap();
        }
    }
}