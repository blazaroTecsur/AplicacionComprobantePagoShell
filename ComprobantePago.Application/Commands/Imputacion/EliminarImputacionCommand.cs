namespace ComprobantePago.Application.Commands.Imputacion
{
    public sealed class EliminarImputacionCommand
    {
        public string Folio { get; init; } = string.Empty;
        public int Secuencia { get; init; }
    }
}
