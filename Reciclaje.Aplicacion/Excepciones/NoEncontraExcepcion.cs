namespace Reciclaje.Aplicacion.Excepciones
{   
    public sealed class UsuarioNoEncontradoExcepcion : AplicacionExcepcion
    {
        public UsuarioNoEncontradoExcepcion(string codigo)
            : base($"El usuario no existe.")
        {
        }
    }
    public sealed class TenantNoEncontradoExcepcion : AplicacionExcepcion
    {        
        public TenantNoEncontradoExcepcion(string codigo)
            : base($"El tenant no existe.")
        {
        }
    }
}