using ComprobantePago.Application.DTOs.Comprobante.Response;
using ComprobantePago.Application.Interfaces;
using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Domain.Entities;
using ComprobantePago.Infrastructure.Repositories;
using ComprobantePago.Infrastructure.Services;
using ComprobantePago.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ComprobantePago.Tests.HU01
{
    /// <summary>
    /// CA-04: Generación de folio compuesto por año + mes + correlativo automático.
    ///
    /// Formato esperado: YYYYMMNNN donde:
    ///   - YYYY = año actual (4 dígitos)
    ///   - MM   = mes actual (2 dígitos con cero a la izquierda)
    ///   - NNNN = correlativo de 4 dígitos (0001, 0002, ...)
    ///
    /// Ejemplo: 202604 0001 → "2026040001"
    /// </summary>
    public class CA04_GeneracionFolioTests
    {
        private readonly string _prefijoEsperado =
            $"{DateTime.Now.Year}{DateTime.Now.Month:D2}";

        private (ComprobanteRepository repo, string dbNombre) ConstruirRepository(string dbNombre = "")
        {
            var nombre    = dbNombre.Length > 0 ? dbNombre : Guid.NewGuid().ToString();
            var db        = DbContextFactory.Crear(nombre);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var mockUsuario = new Mock<IUsuarioContexto>();
            mockUsuario.Setup(u => u.Correo).Returns("test@tecsur.com.pe");

            var mockSunat = new Mock<ISunatService>();
            mockSunat.Setup(s => s.ValidarComprobanteAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(new ValidacionSunatDto
                {
                    Exito = true, CodigoEstado = "1", EstadoSunat = "ACEPTADO"
                });

            return (new ComprobanteRepository(
                db, unitOfWork.Object,
                mockSunat.Object,
                new XmlComprobanteService(),
                new PdfComprobanteService(),
                mockUsuario.Object,
                NullLogger<ComprobanteRepository>.Instance), nombre);
        }

        [Fact]
        public async Task GenerarFolio_IniciaDesdeCeroUno_CuandoNoHayComprobantes()
        {
            var (repo, _) = ConstruirRepository(nameof(GenerarFolio_IniciaDesdeCeroUno_CuandoNoHayComprobantes));
            var archivo   = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.Equal($"{_prefijoEsperado}0001", resultado.Folio);
        }

        [Fact]
        public async Task GenerarFolio_IncremntaCorrelativamente_CuandaYaExistenComprobantes()
        {
            var nombre   = nameof(GenerarFolio_IncremntaCorrelativamente_CuandaYaExistenComprobantes);
            var db       = DbContextFactory.Crear(nombre);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var mockUsuario = new Mock<IUsuarioContexto>();
            mockUsuario.Setup(u => u.Correo).Returns("test@tecsur.com.pe");

            // Pre-cargar 2 comprobantes existentes con el mismo prefijo
            db.Comprobantes.AddRange(
                new Comprobante { Folio = $"{_prefijoEsperado}0001", RucReceptor = "20111111111", RazonSocialReceptor = "A", TipoDocumento = "01", TipoSunat = "01", Serie = "F001", Numero = "1", FechaEmision = DateTime.Today, CodigoEstado = "REGISTRADO", UsuarioReg = "sys" },
                new Comprobante { Folio = $"{_prefijoEsperado}0002", RucReceptor = "20222222222", RazonSocialReceptor = "B", TipoDocumento = "01", TipoSunat = "01", Serie = "F001", Numero = "2", FechaEmision = DateTime.Today, CodigoEstado = "REGISTRADO", UsuarioReg = "sys" }
            );
            await db.SaveChangesAsync();

            var mockSunat = new Mock<ISunatService>();
            mockSunat.Setup(s => s.ValidarComprobanteAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(new ValidacionSunatDto { Exito = true, CodigoEstado = "1", EstadoSunat = "ACEPTADO" });

            var repo    = new ComprobanteRepository(db, unitOfWork.Object, mockSunat.Object, new XmlComprobanteService(), new PdfComprobanteService(), mockUsuario.Object, NullLogger<ComprobanteRepository>.Instance);
            var archivo = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.Equal($"{_prefijoEsperado}0003", resultado.Folio);
        }

        [Fact]
        public async Task GenerarFolio_ContieneAnioActual()
        {
            var (repo, _) = ConstruirRepository(nameof(GenerarFolio_ContieneAnioActual));
            var archivo   = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.StartsWith(DateTime.Now.Year.ToString(), resultado.Folio);
        }

        [Fact]
        public async Task GenerarFolio_ContieneMesActualConDosDigitos()
        {
            var (repo, _) = ConstruirRepository(nameof(GenerarFolio_ContieneMesActualConDosDigitos));
            var archivo   = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            // Folio: YYYYMM + NNNN → posición 4..5 es el mes
            var mesFolio = resultado.Folio[4..6];
            Assert.Equal(DateTime.Now.Month.ToString("D2"), mesFolio);
        }

        [Fact]
        public async Task GenerarFolio_CorrelativoEsDeCuatroDigitos()
        {
            var (repo, _) = ConstruirRepository(nameof(GenerarFolio_CorrelativoEsDeCuatroDigitos));
            var archivo   = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            // Folio longitud total = 4 (año) + 2 (mes) + 4 (correlativo) = 10
            Assert.True(resultado.Folio.Length == 10,
                $"Folio '{resultado.Folio}' debe tener 10 caracteres (YYYYMMNNNN).");
        }

        [Fact]
        public async Task GenerarFolio_CuandoSunatRechaza_NoGeneraFolio()
        {
            var dbNombre  = nameof(GenerarFolio_CuandoSunatRechaza_NoGeneraFolio);
            var db        = DbContextFactory.Crear(dbNombre);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var mockUsuario = new Mock<IUsuarioContexto>();
            mockUsuario.Setup(u => u.Correo).Returns("test@tecsur.com.pe");

            var mockSunat = new Mock<ISunatService>();
            mockSunat.Setup(s => s.ValidarComprobanteAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(new ValidacionSunatDto { Exito = false, CodigoEstado = "0", EstadoSunat = "NO_EXISTE" });

            var repo    = new ComprobanteRepository(db, unitOfWork.Object, mockSunat.Object, new XmlComprobanteService(), new PdfComprobanteService(),  mockUsuario.Object, NullLogger<ComprobanteRepository>.Instance);
            var archivo = ArchivoTestFactory.CrearFormFileXml(ArchivoTestFactory.XmlFacturaSunat());

            var resultado = await repo.ValidarXmlSunatAsync(archivo);

            Assert.True(string.IsNullOrWhiteSpace(resultado.Folio),
                "No debe generarse folio cuando SUNAT rechaza el comprobante.");
        }
    }
}
