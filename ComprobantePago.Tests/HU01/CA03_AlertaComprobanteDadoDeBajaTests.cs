using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Application.Interfaces;
using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Infrastructure.Repositories;
using ComprobantePago.Infrastructure.Services;
using ComprobantePago.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ComprobantePago.Tests.HU01
{
    /// <summary>
    /// CA-03: El sistema alerta cuando el comprobante está dado de baja en SUNAT.
    ///
    /// Según la API de SUNAT, código "2" indica que el comprobante fue
    /// comunicado en una baja (anulado). El sistema debe:
    ///   - Reportar ExitoSunat = false
    ///   - Reportar CodigoEstado = "2"
    ///   - NO generar folio (no registrar el comprobante anulado)
    ///   - El motivo de rechazo debe estar presente
    /// </summary>
    public class CA03_AlertaComprobanteDadoDeBajaTests
    {
        private ComprobanteRepository ConstruirRepository(Mock<ISunatService> mockSunat, string dbNombre = "")
        {
            var db         = DbContextFactory.Crear(dbNombre.Length > 0 ? dbNombre : null);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var mockUsuario = new Mock<IUsuarioContexto>();
            mockUsuario.Setup(u => u.Correo).Returns("test@tecsur.com.pe");

            return new ComprobanteRepository(
                db, unitOfWork.Object,
                mockSunat.Object,
                new XmlComprobanteService(),
                new PdfComprobanteService(),
                mockUsuario.Object,
                NullLogger<ComprobanteRepository>.Instance);
        }

        private Mock<ISunatService> MockSunatBaja()
        {
            var mock = new Mock<ISunatService>();
            mock.Setup(s => s.ValidarComprobanteAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(new ValidacionSunatDto
                {
                    Exito        = false,
                    CodigoEstado = "2",
                    EstadoSunat  = "ANULADO",
                    Motivo       = "El comprobante fue comunicado en una baja."
                });
            return mock;
        }

        [Fact]
        public async Task ValidarXml_CuandoComprobanteDadoDeBaja_ReportaCodigoAnulado()
        {
            var mock    = MockSunatBaja();
            var repo    = ConstruirRepository(mock, nameof(ValidarXml_CuandoComprobanteDadoDeBaja_ReportaCodigoAnulado));
            var archivo = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.Equal("2", resultado.CodigoEstado);
            Assert.Equal("ANULADO", resultado.EstadoSunat);
        }

        [Fact]
        public async Task ValidarXml_CuandoComprobanteDadoDeBaja_NoGeneraFolio()
        {
            var mock    = MockSunatBaja();
            var repo    = ConstruirRepository(mock, nameof(ValidarXml_CuandoComprobanteDadoDeBaja_NoGeneraFolio));
            var archivo = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.True(string.IsNullOrWhiteSpace(resultado.Folio),
                "Un comprobante dado de baja NO debe generar folio.");
        }

        [Fact]
        public async Task ValidarXml_CuandoComprobanteDadoDeBaja_ExitoEsFalso()
        {
            var mock    = MockSunatBaja();
            var repo    = ConstruirRepository(mock, nameof(ValidarXml_CuandoComprobanteDadoDeBaja_ExitoEsFalso));
            var archivo = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task ValidarXml_CuandoComprobanteDadoDeBaja_IncluyeMotivoRechazo()
        {
            var mock    = MockSunatBaja();
            var repo    = ConstruirRepository(mock, nameof(ValidarXml_CuandoComprobanteDadoDeBaja_IncluyeMotivoRechazo));
            var archivo = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.False(string.IsNullOrWhiteSpace(resultado.Motivo),
                "Debe incluirse el motivo de rechazo para alertar al usuario.");
        }

        [Fact]
        public async Task ValidarZip_CuandoComprobanteDadoDeBaja_NoGeneraFolio()
        {
            var mock    = MockSunatBaja();
            var repo    = ConstruirRepository(mock, nameof(ValidarZip_CuandoComprobanteDadoDeBaja_NoGeneraFolio));
            var xml     = ArchivoTestFactory.XmlFacturaSunat();
            var archivo = ArchivoTestFactory.CrearFormFileZip(xml);

            var resultado = await repo.ValidarZipSunatAsync(archivo);

            Assert.False(resultado.Exito);
            Assert.Equal("2", resultado.CodigoEstado);
            Assert.True(string.IsNullOrWhiteSpace(resultado.Folio));
        }
    }
}
