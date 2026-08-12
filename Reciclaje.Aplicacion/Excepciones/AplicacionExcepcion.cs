namespace Reciclaje.Aplicacion.Excepciones
{
    public abstract class AplicacionExcepcion : Exception
    {
        protected AplicacionExcepcion(string mensaje) : base(mensaje)
        {
        }
    }
    public sealed class UsuarioNoExisteException : ApplicationException
    {
        public UsuarioNoExisteException(string usuario)
            : base($"El usuario '{usuario}' no existe.")
        {
        }
    }

    public sealed class ValidacionExcepcion : AplicacionExcepcion
    {
        public ValidacionExcepcion(string mensaje) : base(mensaje) { }
    }

    public sealed class NoEncontraExcepcion : AplicacionExcepcion
    {
        public NoEncontraExcepcion(string mensaje) : base(mensaje) { }
    }

}