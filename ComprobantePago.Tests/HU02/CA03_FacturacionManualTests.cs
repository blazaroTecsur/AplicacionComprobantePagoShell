using ComprobantePago.Application.Commands.Comprobante;
using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Infrastructure.Persistence;
using ComprobantePago.Infrastructure.Repositories;
using ComprobantePago.Infrastructure.Services;
using ComprobantePago.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ComprobantePago.Tests.HU02
{
    /// <summary>
    /// CA-03: Permitir el ingreso de información manualmente para recibos
    /// (agua, luz, teléfono, vales de caja, vales de movilidad) y para
    /// la cabecera y detalle de montos.
    /// </summary>
    public class CA03_FacturacionManualTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        /// Crea repositorio + DbContext compartido. Usa TestUnitOfWork para que
        /// SaveChangesAsync persista realmente en la BD InMemory.
        private static (ComprobanteRepository repo, AppDbContext db) Construir(string nombre)
        {
            var db      = DbContextFactory.Crear(nombre);
            var uow     = new TestUnitOfWork(db);
            var usuario = new Mock<ComprobantePago.Application.Interfaces.IUsuarioContexto>();
            usuario.Setup(u => u.Correo).Returns("test@tecsur.com.pe");

            var repo = new ComprobanteRepository(
                db, uow,
                new Mock<ISunatService>().Object,
                new XmlComprobanteService(),
                new PdfComprobanteService(),
                usuario.Object,
                NullLogger<ComprobanteRepository>.Instance);

            return (repo, db);
        }

        private static RegistrarComprobanteCommand Comando(
            string tipoDocumento,
            string serie      = "001",
            string numero     = "00000001",
            string folio      = "",
            decimal montoNeto = 100m,
            decimal igv       = 0m,
            decimal total     = 100m) => new()
        {
            Comprobante = new RegistrarComprobanteDto
            {
                Folio           = folio,
                Ruc             = "10411309512",
                RazonSocial     = "PROVEEDOR MANUAL SAC",
                TipoDocumento   = tipoDocumento,
                TipoSunat       = "00",
                Serie           = serie,
                Numero          = numero,
                FechaEmision    = "01/04/2026",
                FechaRecepcion  = "02/04/2026",
                Moneda          = "PEN",
                TasaCambio      = 1m,
                MontoNeto       = montoNeto,
                MontoIGVCredito = igv,
                MontoTotal      = total,
                MontoBruto      = total,
                TieneDetraccion = false
            }
        };

        // ── Generación de folio ───────────────────────────────────────────────

        [Fact]
        public async Task GuardarManual_SinFolioPrevio_GeneraFolio()
        {
            var (repo, _) = Construir(nameof(GuardarManual_SinFolioPrevio_GeneraFolio));
            var folio     = await repo.GuardarAsync(Comando("RP"));
            Assert.False(string.IsNullOrWhiteSpace(folio),
                "Debe generarse un folio al guardar en modo manual.");
        }

        [Fact]
        public async Task GuardarManual_FolioTieneLongitudCorrecta()
        {
            var (repo, _) = Construir(nameof(GuardarManual_FolioTieneLongitudCorrecta));
            var folio     = await repo.GuardarAsync(Comando("RP"));
            Assert.True(folio.Length == 10,
                "El folio debe tener formato YYYYMMNNNN (10 caracteres).");
        }

        // ── Tipos de comprobante manual ───────────────────────────────────────

        [Theory]
        [InlineData("RP",  "Recibo de agua/luz/teléfono")]
        [InlineData("VC",  "Vale de caja")]
        [InlineData("VM",  "Vale de movilidad")]
        [InlineData("CI",  "Comprobante interno")]
        [InlineData("RH",  "Recibo por honorarios")]
        public async Task GuardarManual_AceptaTiposDeComprobanteInterno(
            string tipoDocumento, string descripcion)
        {
            var (repo, _) = Construir($"Tipo_{tipoDocumento}");
            var folio     = await repo.GuardarAsync(Comando(tipoDocumento));
            Assert.False(string.IsNullOrWhiteSpace(folio),
                $"{descripcion} ({tipoDocumento}) debe guardarse correctamente.");
        }

        // ── Actualización sin duplicado ───────────────────────────────────────

        [Fact]
        public async Task GuardarManual_ConFolioExistente_ActualizaSinCrearDuplicado()
        {
            var (repo, db) = Construir(nameof(GuardarManual_ConFolioExistente_ActualizaSinCrearDuplicado));

            // Primer guardado
            var folio = await repo.GuardarAsync(Comando("RP"));
            Assert.False(string.IsNullOrWhiteSpace(folio));

            // Segundo guardado con el mismo folio (actualización)
            var folioActualizado = await repo.GuardarAsync(
                Comando("RP", folio: folio, total: 150m, montoNeto: 150m));

            Assert.True(folioActualizado == folio,
                "El folio no debe cambiar al actualizar un comprobante existente.");

            var total = db.Comprobantes.Count(c => c.Folio == folio);
            Assert.True(total == 1,
                "No debe crearse un duplicado al actualizar.");
        }

        // ── Persistencia de datos ─────────────────────────────────────────────

        [Fact]
        public async Task GuardarManual_PersisteDatosDeMontos()
        {
            var (repo, db) = Construir(nameof(GuardarManual_PersisteDatosDeMontos));

            var folio = await repo.GuardarAsync(
                Comando("RP", montoNeto: 250m, igv: 45m, total: 295m));

            var registrado = db.Comprobantes.FirstOrDefault(c => c.Folio == folio);
            Assert.NotNull(registrado);
            Assert.Equal(250.00m, registrado.MontoNeto);
            Assert.Equal(45.00m,  registrado.MontoIGVCredito);
            Assert.Equal(295.00m, registrado.MontoTotal);
        }

        [Fact]
        public async Task GuardarManual_RegistraUsuarioYFechaDigitacion()
        {
            var db  = DbContextFactory.Crear(nameof(GuardarManual_RegistraUsuarioYFechaDigitacion));
            var uow = new TestUnitOfWork(db);

            var usuario = new Mock<ComprobantePago.Application.Interfaces.IUsuarioContexto>();
            usuario.Setup(u => u.Correo).Returns("operador@tecsur.com.pe");

            var repo = new ComprobanteRepository(
                db, uow,
                new Mock<ISunatService>().Object,
                new XmlComprobanteService(), new PdfComprobanteService(),
                usuario.Object,
                NullLogger<ComprobanteRepository>.Instance);

            var folio      = await repo.GuardarAsync(Comando("RP"));
            var registrado = db.Comprobantes.FirstOrDefault(c => c.Folio == folio);

            Assert.NotNull(registrado);
            Assert.Equal("operador@tecsur.com.pe", registrado.RolDigitacion);
            Assert.NotNull(registrado.FechaDigitacion);
        }

        [Fact]
        public async Task GuardarManual_EstadoInicialEsRegistrado()
        {
            var (repo, db) = Construir(nameof(GuardarManual_EstadoInicialEsRegistrado));

            var folio      = await repo.GuardarAsync(Comando("RP"));
            var registrado = db.Comprobantes.FirstOrDefault(c => c.Folio == folio);

            Assert.NotNull(registrado);
            Assert.Equal("REGISTRADO", registrado.CodigoEstado);
        }
    }
}
