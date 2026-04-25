using Resguardo.Domain.Entities;
using Resguardo.Domain.Interfaces;
using Resguardo.Infrastructure.Data;

namespace Resguardo.Infrastructure.Repositorios
{
    public class EfectivoRepositorio(DBContexto contexto) : RepositorioBase<Efectivo>(contexto), IEfectivoRepositorio
    {        
    }
}