using System.ComponentModel.DataAnnotations;

namespace Reciclaje.Aplicacion.DTOs.Reciclaje
{
    // ── DTO de listado ───────────────────────────────────────────────
    public class TareaOrdenCompraDto
    {
        public int Id { get; set; }
        public short Anno { get; set; }
        public byte Mes { get; set; }
        public string NombrePo { get; set; } = null!;
        public string? Sitio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; } = null!;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        public string? Estado { get; set; }
        public string? UidSyteLine { get; set; }
    }

    // ── DTO para crear ───────────────────────────────────────────────
    public class TareaOrdenCompraCrearDto
    {
        [Required(ErrorMessage = "El año es obligatorio.")]
        [Range(2000, 2999, ErrorMessage = "Año inválido.")]
        public short Anno { get; set; }

        [Required(ErrorMessage = "El mes es obligatorio.")]
        [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")]
        public byte Mes { get; set; }

        [StringLength(100, ErrorMessage = "El sitio no puede superar los 100 caracteres.")]
        public string? Sitio { get; set; }

        [StringLength(15, ErrorMessage = "El estado no puede superar los 15 caracteres.")]
        public string? Estado { get; set; }

        [StringLength(36, ErrorMessage = "El UID de SyteLine debe tener máximo 36 caracteres.")]
        public string? UidSyteLine { get; set; }
    }

    // ── DTO para editar ──────────────────────────────────────────────
    public class TareaOrdenCompraEditarDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El año es obligatorio.")]
        [Range(2000, 2999, ErrorMessage = "Año inválido.")]
        public short Anno { get; set; }

        [Required(ErrorMessage = "El mes es obligatorio.")]
        [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")]
        public byte Mes { get; set; }

        [StringLength(100, ErrorMessage = "El sitio no puede superar los 100 caracteres.")]
        public string? Sitio { get; set; }

        [StringLength(15, ErrorMessage = "El estado no puede superar los 15 caracteres.")]
        public string? Estado { get; set; }

        [StringLength(36, ErrorMessage = "El UID de SyteLine debe tener máximo 36 caracteres.")]
        public string? UidSyteLine { get; set; }

        // Solo lectura — se muestra en la vista
        public string NombrePo { get; set; } = null!;
    }
}
