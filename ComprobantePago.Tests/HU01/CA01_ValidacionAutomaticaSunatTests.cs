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
    /// CA-01: Validación automática de comprobantes de pago contra SUNAT.
    ///
    /// Verifica que al cargar un XML o ZIP válido el sistema:
    ///   - invoca el servicio SUNAT con los datos extraídos del documento,
    ///   - retorna Folio generado cuando SUNAT acepta (código 1),
    ///   - retorna Folio generado cuando SUNAT autoriza por imprenta (código 3),
    ///   - procesa correctamente los tipos: Factura (01), Boleta (03),
    ///     Nota de Crédito (07) y Nota de Débito (08).
    /// </summary>
    public class CA01_ValidacionAutomaticaSunatTests
    {
        private ComprobanteRepository ConstruirRepository(
            Mock<ISunatService> mockSunat,
            string dbNombre = "")
        {
            var db        = DbContextFactory.Crear(dbNombre.Length > 0 ? dbNombre : null);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var mockUsuario = new Mock<IUsuarioContexto>();
            mockUsuario.Setup(u => u.Correo).Returns("test@tecsur.com.pe");

            return new ComprobanteRepository(
                db,
                unitOfWork.Object,
                mockSunat.Object,
                new XmlComprobanteService(),
                new PdfComprobanteService(),
                mockUsuario.Object,
                NullLogger<ComprobanteRepository>.Instance);
        }

        private Mock<ISunatService> MockSunatConCodigo(string codigo) =>
            MockSunatConCodigo(codigo, $"Estado {codigo}");

        private Mock<ISunatService> MockSunatConCodigo(string codigo, string estado)
        {
            var mock = new Mock<ISunatService>();
            mock.Setup(s => s.ValidarComprobanteAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(new ValidacionSunatDto
                {
                    Exito        = codigo is "1" or "3",
                    CodigoEstado = codigo,
                    EstadoSunat  = estado
                });
            return mock;
        }

        // ── XML ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidarXml_CuandoSunatAcepta_RetornaFolioGenerado()
        {
            var mockSunat = MockSunatConCodigo("1", "ACEPTADO");
            var repo      = ConstruirRepository(mockSunat, nameof(ValidarXml_CuandoSunatAcepta_RetornaFolioGenerado));
            var archivo   = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.True(resultado.Exito);
            Assert.Equal("1", resultado.CodigoEstado);
            Assert.False(string.IsNullOrWhiteSpace(resultado.Folio),
                "Debe generarse un folio cuando SUNAT acepta.");
        }

        [Fact]
        public async Task ValidarXml_CuandoSunatAutorizaImprenta_RetornaFolioGenerado()
        {
            var mockSunat = MockSunatConCodigo("3", "AUTORIZADO");
            var repo      = ConstruirRepository(mockSunat, nameof(ValidarXml_CuandoSunatAutorizaImprenta_RetornaFolioGenerado));
            var archivo   = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.True(resultado.Exito);
            Assert.Equal("3", resultado.CodigoEstado);
            Assert.False(string.IsNullOrWhiteSpace(resultado.Folio));
        }

        [Fact]
        public async Task ValidarXml_InvocaSunatConDatosExtraidos()
        {
            var mockSunat = MockSunatConCodigo("1");
            var repo      = ConstruirRepository(mockSunat, nameof(ValidarXml_InvocaSunatConDatosExtraidos));
            var xml       = ArchivoTestFactory.XmlFacturaSunat(
                ruc: "20206018411", serie: "F001", numero: "00000099", tipoDoc: "01");
            var archivo   = ArchivoTestFactory.CrearFormFileXml(xml);

            await repo.ValidarXmlSunatAsync(archivo);

            mockSunat.Verify(s => s.ValidarComprobanteAsync(
                "20206018411", "01", "F001", "00000099",
                It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        }

        // ── ZIP ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidarZip_CuandoSunatAcepta_RetornaFolioGenerado()
        {
            var mockSunat = MockSunatConCodigo("1", "ACEPTADO");
            var repo      = ConstruirRepository(mockSunat, nameof(ValidarZip_CuandoSunatAcepta_RetornaFolioGenerado));
            var xml       = ArchivoTestFactory.XmlFacturaSunat();
            var archivo   = ArchivoTestFactory.CrearFormFileZip(xml);

            var resultado = await repo.ValidarZipSunatAsync(archivo);

            Assert.True(resultado.Exito);
            Assert.False(string.IsNullOrWhiteSpace(resultado.Folio));
        }

        [Fact]
        public async Task ValidarZip_InvocaSunatConDatosDelXmlInterno()
        {
            var mockSunat = MockSunatConCodigo("1");
            var repo      = ConstruirRepository(mockSunat, nameof(ValidarZip_InvocaSunatConDatosDelXmlInterno));
            var xml       = ArchivoTestFactory.XmlFacturaSunat(
                ruc: "20123456789", serie: "B001", numero: "00000005", tipoDoc: "03");
            var archivo   = ArchivoTestFactory.CrearFormFileZip(xml, "B001-00000005.xml");

            await repo.ValidarZipSunatAsync(archivo);

            mockSunat.Verify(s => s.ValidarComprobanteAsync(
                "20123456789", "03", "B001", "00000005",
                It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        }

        // ── Tipos de documento ────────────────────────────────────────────────

        [Theory]
        [InlineData("01", "F001", "Factura")]
        [InlineData("03", "B001", "Boleta de venta")]
        [InlineData("07", "FC01", "Nota de crédito")]
        [InlineData("08", "FD01", "Nota de débito")]
        public async Task ValidarXml_AceptaTiposDeDocumentoPermitidos(
            string tipoDoc, string serie, string descripcion)
        {
            var mockSunat = MockSunatConCodigo("1");
            var repo      = ConstruirRepository(mockSunat, $"TipoDoc_{tipoDoc}");
            var xml       = ArchivoTestFactory.XmlFacturaSunat(tipoDoc: tipoDoc, serie: serie);
            var archivo   = ArchivoTestFactory.CrearFormFileXml(xml);

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.True(resultado.CodigoEstado == "1",
                $"{descripcion} debe validarse correctamente.");
        }
    }
}
