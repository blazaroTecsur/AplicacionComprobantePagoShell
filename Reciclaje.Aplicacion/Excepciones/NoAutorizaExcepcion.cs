namespace Reciclaje.Aplicacion.Excepciones
{
    public sealed class UsuarioNoAutorizadoExcepcion : AplicacionExcepcion
    {
        public UsuarioNoAutorizadoExcepcion(string usuario)
            : base($"El usuario no está autenticado.")
        {
        }
    }
}
