using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.Validations;
using Xunit;

namespace ComprobantePago.Tests.HU03
{
    /// <summary>
    /// HU-03 · CA-01
    /// Validar que los campos de detracción son obligatorios cuando "TieneDetraccion = true",
    /// y que NO se validan cuando la detracción no aplica.
    /// </summary>
    public class CA01_ValidacionDetraccionTests
    {
        private readonly RegistrarComprobanteValidator _validator = new();

        private static RegistrarComprobanteDto DtoBase() => new()
        {
            Ruc             = "20206018411",
            RazonSocial     = "EMPRESA PROVEEDORA SAC",
            TipoDocumento   = "FP",
            TipoSunat       = "01",
            Serie           = "F001",
            Numero          = "00000100",
            FechaEmision    = "01/04/2026",
            Moneda          = "PEN",
            TasaCambio      = 1m,
            MontoTotal      = 11600.00m,
            MontoNeto       = 10000.00m,
            MontoIGVCredito = 1800.00m,
            TieneDetraccion = false
        };

        private static RegistrarComprobanteDto DtoConDetraccion() => new()
        {
            Ruc                  = "20206018411",
            RazonSocial          = "EMPRESA PROVEEDORA SAC",
            TipoDocumento        = "FP",
            TipoSunat            = "01",
            Serie                = "F001",
            Numero               = "00000100",
            FechaEmision         = "01/04/2026",
            Moneda               = "PEN",
            TasaCambio           = 1m,
            MontoTotal           = 11600.00m,
            MontoNeto            = 10000.00m,
            MontoIGVCredito      = 1800.00m,
            TieneDetraccion      = true,
            TipoDetraccion       = "030",
            PorcentajeDetraccion = 4.00m,
            MontoDetraccion      = 464.00m
        };

        // ── Sin detracción: campos opcionales ─────────────────────────────────

        [Fact]
        public void SinDetraccion_TipoDetraccionVacia_Pasa()
        {
            var dto = DtoBase();
            dto.TieneDetraccion = false;
            dto.TipoDetraccion  = string.Empty;
            var result = _validator.Validate(dto);
            Assert.DoesNotContain(result.Errors,
                e => e.PropertyName == nameof(dto.TipoDetraccion));
        }

        [Fact]
        public void SinDetraccion_PorcentajeFueraRango_NoSeValida()
        {
            var dto = DtoBase();
            dto.TieneDetraccion      = false;
            dto.PorcentajeDetraccion = 999;
            var result = _validator.Validate(dto);
            Assert.DoesNotContain(result.Errors,
                e => e.PropertyName == nameof(dto.PorcentajeDetraccion));
        }

        [Fact]
        public void SinDetraccion_MontoNegativo_NoSeValida()
        {
            var dto = DtoBase();
            dto.TieneDetraccion = false;
            dto.MontoDetraccion = -100;
            var result = _validator.Validate(dto);
            Assert.DoesNotContain(result.Errors,
                e => e.PropertyName == nameof(dto.MontoDetraccion));
        }

        // ── Con detracción: TipoDetraccion requerido ──────────────────────────

        [Fact]
        public void ConDetraccion_TipoDetraccionVacio_Falla()
        {
            var dto = DtoConDetraccion();
            dto.TipoDetraccion = string.Empty;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors,
                e => e.PropertyName == nameof(dto.TipoDetraccion));
        }

        [Fact]
        public void ConDetraccion_TipoDetraccionVacio_MensajeEsCorrecto()
        {
            var dto = DtoConDetraccion();
            dto.TipoDetraccion = string.Empty;
            var result = _validator.Validate(dto);
            Assert.Contains(result.Errors,
                e => e.ErrorMessage.Contains("tipo de detracción") &&
                     e.PropertyName == nameof(dto.TipoDetraccion));
        }

        // ── Con detracción: porcentaje fuera de rango ─────────────────────────

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void ConDetraccion_PorcentajeFueraDeRango_Falla(decimal porcentaje)
        {
            var dto = DtoConDetraccion();
            dto.PorcentajeDetraccion = porcentaje;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors,
                e => e.PropertyName == nameof(dto.PorcentajeDetraccion));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        [InlineData(15)]
        [InlineData(100)]
        public void ConDetraccion_PorcentajeDentroDeRango_Pasa(decimal porcentaje)
        {
            var dto = DtoConDetraccion();
            dto.PorcentajeDetraccion = porcentaje;
            var result = _validator.Validate(dto);
            Assert.DoesNotContain(result.Errors,
                e => e.PropertyName == nameof(dto.PorcentajeDetraccion));
        }

        // ── Con detracción: monto negativo ────────────────────────────────────

        [Fact]
        public void ConDetraccion_MontoNegativo_Falla()
        {
            var dto = DtoConDetraccion();
            dto.MontoDetraccion = -1;
            var result = _validator.Validate(dto);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors,
                e => e.PropertyName == nameof(dto.MontoDetraccion));
        }

        [Fact]
        public void ConDetraccion_MontoCero_Pasa()
        {
            var dto = DtoConDetraccion();
            dto.MontoDetraccion = 0;
            var result = _validator.Validate(dto);
            Assert.DoesNotContain(result.Errors,
                e => e.PropertyName == nameof(dto.MontoDetraccion));
        }

        // ── DTO con detracción completo y válido ──────────────────────────────

        [Fact]
        public void ConDetraccion_DtoCompletoYValido_Pasa()
        {
            var result = _validator.Validate(DtoConDetraccion());
            Assert.True(result.IsValid,
                $"DTO con detracción válida no debería tener errores. " +
                $"Errores: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
        }
    }
}
