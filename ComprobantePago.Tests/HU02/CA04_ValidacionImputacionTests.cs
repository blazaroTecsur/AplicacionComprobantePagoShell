using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.Validations;
using Xunit;

namespace ComprobantePago.Tests.HU02
{
    /// <summary>
    /// CA-04: Validar que las imputaciones contables requieren al menos un
    /// código de unidad (1, 3 o 4) antes de ser registradas.
    /// </summary>
    public class CA04_ValidacionImputacionTests
    {
        private readonly ImputacionValidator _validator = new();

        private static ImputacionDto DtoValido() => new()
        {
            Folio             = "2026040001",
            CuentaContable    = "6201001",
            DescripcionCuenta = "Remuneraciones",
            CodUnidad1Cuenta  = "U1-001",
            CodUnidad3Cuenta  = string.Empty,
            CodUnidad4Cuenta  = string.Empty,
            Monto             = 500m,
            Descripcion       = "Pago servicios"
        };

        // ── Folio ─────────────────────────────────────────────────────────────

        [Fact]
        public void Validar_FolioVacio_Falla()
        {
            var dto = DtoValido(); dto.Folio = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Folio));
        }

        // ── Cuenta Contable ───────────────────────────────────────────────────

        [Fact]
        public void Validar_CuentaContableVacia_Falla()
        {
            var dto = DtoValido(); dto.CuentaContable = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.CuentaContable));
        }

        // ── Descripción ───────────────────────────────────────────────────────

        [Fact]
        public void Validar_DescripcionCuentaVacia_Falla()
        {
            var dto = DtoValido(); dto.DescripcionCuenta = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.DescripcionCuenta));
        }

        [Fact]
        public void Validar_DescripcionCuentaSuperaMaximo_Falla()
        {
            var dto = DtoValido(); dto.DescripcionCuenta = new string('X', 201);
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.DescripcionCuenta));
        }

        // ── Códigos de Unidad — al menos uno obligatorio ──────────────────────

        [Fact]
        public void Validar_TodosLosCodUnidadVacios_Falla()
        {
            var dto = DtoValido();
            dto.CodUnidad1Cuenta = string.Empty;
            dto.CodUnidad3Cuenta = string.Empty;
            dto.CodUnidad4Cuenta = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("al menos un código de unidad"));
        }

        [Fact]
        public void Validar_SoloCodUnidad1Presente_Pasa()
        {
            var dto = DtoValido();
            dto.CodUnidad1Cuenta = "U1-001";
            dto.CodUnidad3Cuenta = string.Empty;
            dto.CodUnidad4Cuenta = string.Empty;
            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validar_SoloCodUnidad3Presente_Pasa()
        {
            var dto = DtoValido();
            dto.CodUnidad1Cuenta = string.Empty;
            dto.CodUnidad3Cuenta = "U3-010";
            dto.CodUnidad4Cuenta = string.Empty;
            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validar_SoloCodUnidad4Presente_Pasa()
        {
            var dto = DtoValido();
            dto.CodUnidad1Cuenta = string.Empty;
            dto.CodUnidad3Cuenta = string.Empty;
            dto.CodUnidad4Cuenta = "U4-050";
            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validar_LosTresCodUnidadPresentes_Pasa()
        {
            var dto = DtoValido();
            dto.CodUnidad1Cuenta = "U1-001";
            dto.CodUnidad3Cuenta = "U3-010";
            dto.CodUnidad4Cuenta = "U4-050";
            var result = _validator.Validate(dto);
            Assert.True(result.IsValid);
        }

        // ── DTO completo válido ───────────────────────────────────────────────

        [Fact]
        public void Validar_DtoCompletoYValido_Pasa()
        {
            var result = _validator.Validate(DtoValido());
            Assert.True(result.IsValid,
                $"DTO válido no debería tener errores. Errores: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
        }
    }
}
