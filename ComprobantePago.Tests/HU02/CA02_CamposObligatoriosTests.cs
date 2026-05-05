using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.Validations;
using Xunit;

namespace ComprobantePago.Tests.HU02
{
    /// <summary>
    /// CA-02: Todos los campos obligatorios deben estar completos antes de permitir el registro.
    ///
    /// Verifica las reglas de RegistrarComprobanteValidator para cada campo
    /// requerido, tanto en modo electrónico como en modo manual.
    /// </summary>
    public class CA02_CamposObligatoriosTests
    {
        private readonly RegistrarComprobanteValidator _validator = new();

        /// Construye un DTO válido base para todos los tests.
        private static RegistrarComprobanteDto DtoValido() => new()
        {
            Ruc             = "20206018411",
            RazonSocial     = "EMPRESA EMISORA SAC",
            TipoDocumento   = "FP",
            TipoSunat       = "01",
            Serie           = "F001",
            Numero          = "00000001",
            FechaEmision    = "01/03/2026",
            Moneda          = "PEN",
            TasaCambio      = 1m,
            MontoTotal      = 1180.00m,
            MontoNeto       = 1000.00m,
            MontoIGVCredito = 180.00m,
            TieneDetraccion = false
        };

        // ── RUC ───────────────────────────────────────────────────────────────

        [Fact]
        public void Validar_RucVacio_FallaConMensaje()
        {
            var dto = DtoValido(); dto.Ruc = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Ruc));
        }

        [Theory]
        [InlineData("2020601841")]    // 10 dígitos
        [InlineData("202060184111")]  // 12 dígitos
        public void Validar_RucConLongitudIncorrecta_Falla(string ruc)
        {
            var dto = DtoValido(); dto.Ruc = ruc;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Ruc));
        }

        [Fact]
        public void Validar_RucConLetras_Falla()
        {
            var dto = DtoValido(); dto.Ruc = "2020601841A";
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Ruc));
        }

        [Fact]
        public void Validar_RucValido_Pasa()
        {
            var dto = DtoValido();
            var result = _validator.Validate(dto);
            Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(dto.Ruc));
        }

        // ── Razón Social ──────────────────────────────────────────────────────

        [Fact]
        public void Validar_RazonSocialVacia_Falla()
        {
            var dto = DtoValido(); dto.RazonSocial = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.RazonSocial));
        }

        [Fact]
        public void Validar_RazonSocialSuperaMaximo_Falla()
        {
            var dto = DtoValido(); dto.RazonSocial = new string('A', 201);
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.RazonSocial));
        }

        // ── Tipo de documento ─────────────────────────────────────────────────

        [Fact]
        public void Validar_TipoDocumentoVacio_Falla()
        {
            var dto = DtoValido(); dto.TipoDocumento = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.TipoDocumento));
        }

        [Fact]
        public void Validar_TipoSunatVacio_Falla()
        {
            var dto = DtoValido(); dto.TipoSunat = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.TipoSunat));
        }

        // ── Serie y Número ────────────────────────────────────────────────────

        [Fact]
        public void Validar_SerieVacia_Falla()
        {
            var dto = DtoValido(); dto.Serie = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Serie));
        }

        [Fact]
        public void Validar_NumeroVacio_Falla()
        {
            var dto = DtoValido(); dto.Numero = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Numero));
        }

        // ── Fecha ─────────────────────────────────────────────────────────────

        [Fact]
        public void Validar_FechaEmisionVacia_Falla()
        {
            var dto = DtoValido(); dto.FechaEmision = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.FechaEmision));
        }

        // ── Moneda ────────────────────────────────────────────────────────────

        [Fact]
        public void Validar_MonedaVacia_Falla()
        {
            var dto = DtoValido(); dto.Moneda = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Moneda));
        }

        // ── Montos ────────────────────────────────────────────────────────────

        [Fact]
        public void Validar_MontoTotalCero_Falla()
        {
            var dto = DtoValido(); dto.MontoTotal = 0;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.MontoTotal));
        }

        [Fact]
        public void Validar_MontoNetoNegativo_Falla()
        {
            var dto = DtoValido(); dto.MontoNeto = -1;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.MontoNeto));
        }

        [Fact]
        public void Validar_MontoIGVNegativo_Falla()
        {
            var dto = DtoValido(); dto.MontoIGVCredito = -1;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.MontoIGVCredito));
        }

        // ── Detracción (condicional) ───────────────────────────────────────────

        [Fact]
        public void Validar_ConDetraccionPorcentajeSuperior100_Falla()
        {
            var dto = DtoValido();
            dto.TieneDetraccion      = true;
            dto.PorcentajeDetraccion = 101;
            dto.MontoDetraccion      = 100;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors,
                e => e.PropertyName == nameof(dto.PorcentajeDetraccion));
        }

        [Fact]
        public void Validar_SinDetraccion_PorcentajeNoSeValida()
        {
            var dto = DtoValido();
            dto.TieneDetraccion      = false;
            dto.PorcentajeDetraccion = 999; // valor inválido pero no debería validarse
            var result = _validator.Validate(dto);
            Assert.DoesNotContain(result.Errors,
                e => e.PropertyName == nameof(dto.PorcentajeDetraccion));
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
