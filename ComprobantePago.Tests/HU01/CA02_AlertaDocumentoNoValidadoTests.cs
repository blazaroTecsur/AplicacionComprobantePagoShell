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
    /// CA-02: El sistema alerta cuando el documento no está validado por SUNAT.
    ///
    /// Cubre los casos:
    ///   - Código "0": comprobante no existe en SUNAT.
    ///   - Código "4": no autorizado por imprenta.
    ///   - Código "ERROR": fallo de conexión con el servicio SUNAT.
    ///   - ZIP sin XML de comprobante → error antes de llamar a SUNAT.
    ///   - En todos los casos: Folio NO debe generarse.
    /// </summary>
    public class CA02_AlertaDocumentoNoValidadoTests
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

        private Mock<ISunatService> MockSunatCodigo(string codigo, bool exito = false)
        {
            var mock = new Mock<ISunatService>();
            mock.Setup(s => s.ValidarComprobanteAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(new ValidacionSunatDto
                {
                    Exito        = exito,
                    CodigoEstado = codigo,
                    EstadoSunat  = codigo,
                    Motivo       = $"El comprobante tiene estado {codigo}"
                });
            return mock;
        }

        [Fact]
        public async Task ValidarXml_CuandoNoExisteEnSunat_NoGeneraFolio()
        {
            var mock    = MockSunatCodigo("0");
            var repo    = ConstruirRepository(mock, nameof(ValidarXml_CuandoNoExisteEnSunat_NoGeneraFolio));
            var archivo = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.False(resultado.Exito);
            Assert.Equal("0", resultado.CodigoEstado);
            Assert.True(string.IsNullOrWhiteSpace(resultado.Folio),
                "No debe generarse folio cuando SUNAT devuelve código 0.");
        }

        [Fact]
        public async Task ValidarXml_CuandoNoAutorizadoImprenta_NoGeneraFolio()
        {
            var mock    = MockSunatCodigo("4");
            var repo    = ConstruirRepository(mock, nameof(ValidarXml_CuandoNoAutorizadoImprenta_NoGeneraFolio));
            var archivo = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.False(resultado.Exito);
            Assert.Equal("4", resultado.CodigoEstado);
            Assert.True(string.IsNullOrWhiteSpace(resultado.Folio));
        }

        [Fact]
        public async Task ValidarXml_CuandoFallaConexionSunat_RetornaError()
        {
            var mock    = MockSunatCodigo("ERROR");
            var repo    = ConstruirRepository(mock, nameof(ValidarXml_CuandoFallaConexionSunat_RetornaError));
            var archivo = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.False(resultado.Exito);
            Assert.Equal("ERROR", resultado.CodigoEstado);
            Assert.True(string.IsNullOrWhiteSpace(resultado.Folio));
        }

        [Fact]
        public async Task ValidarZip_SinXmlInterno_RetornaErrorSinLlamarSunat()
        {
            var mock = new Mock<ISunatService>();
            var repo = ConstruirRepository(mock, nameof(ValidarZip_SinXmlInterno_RetornaErrorSinLlamarSunat));

            // ZIP vacío (sin XML de comprobante)
            using var zipMs = new System.IO.MemoryStream();
            using (var zip = new System.IO.Compression.ZipArchive(zipMs, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
                zip.CreateEntry("documento.pdf"); // solo PDF, sin XML
            var archivo = ArchivoTestFactory.CrearFormFile(zipMs.ToArray(), "sin_xml.zip");

            var resultado = await repo.ValidarZipSunatAsync(archivo);

            Assert.False(resultado.Exito);
            Assert.False(string.IsNullOrWhiteSpace(resultado.Motivo),
                "Debe informar el motivo del error.");
            mock.Verify(s => s.ValidarComprobanteAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<decimal>()), Times.Never,
                "No debe llamarse a SUNAT si el ZIP no contiene XML.");
        }

        [Fact]
        public async Task ValidarZip_CuandoSunatNoEncuentraComprobante_NoGeneraFolio()
        {
            var mock    = MockSunatCodigo("0");
            var repo    = ConstruirRepository(mock, nameof(ValidarZip_CuandoSunatNoEncuentraComprobante_NoGeneraFolio));
            var xml     = ArchivoTestFactory.XmlFacturaSunat();
            var archivo = ArchivoTestFactory.CrearFormFileZip(xml);

            var resultado = await repo.ValidarZipSunatAsync(archivo);

            Assert.False(resultado.Exito);
            Assert.True(string.IsNullOrWhiteSpace(resultado.Folio));
        }
    }
}
