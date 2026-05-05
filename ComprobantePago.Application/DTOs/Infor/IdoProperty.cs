namespace ComprobantePago.Application.DTOs.Infor
{
    /// <summary>
    /// Propiedad individual en el formato Action/ItemId/Properties del IDO REST de Syteline.
    /// </summary>
    public sealed class IdoProperty
    {
        public bool   IsNull   { get; init; }
        public bool   Modified { get; init; } = true;
        public required string Name  { get; init; }
        public string Value    { get; init; } = string.Empty;
    }
}
