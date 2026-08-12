using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reciclaje.Aplicacion.Excepciones
{
    public sealed class UsuarioNoCreadoExcepcion : AplicacionExcepcion
    {
        public UsuarioNoCreadoExcepcion(string usuario)
            : base($"El usuario '{usuario}' no ha sido creado.")
        {
        }
    }
    public sealed class UsuarioTenantNoCreadoExcepcion : AplicacionExcepcion
    {
        public UsuarioTenantNoCreadoExcepcion(string usuario)
            : base($"El tenant del usuario '{usuario}' no ha sido creado.")
        {
        }
    }
}