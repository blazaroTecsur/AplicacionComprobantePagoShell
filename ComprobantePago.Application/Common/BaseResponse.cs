namespace ComprobantePago.Application.Common
{
    public class BaseResponse<T>
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Datos { get; set; }

        public static BaseResponse<T> Ok(T datos, string mensaje = "") =>
            new() { Exito = true, Datos = datos, Mensaje = mensaje };

        public static BaseResponse<T> Error(string mensaje) =>
            new() { Exito = false, Mensaje = mensaje };
    }

    public class BaseResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        public static BaseResponse Ok(string mensaje = "") =>
            new() { Exito = true, Mensaje = mensaje };

        public static BaseResponse Error(string mensaje) =>
            new() { Exito = false, Mensaje = mensaje };
    }
}
