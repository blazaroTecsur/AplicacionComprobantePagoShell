using ComprobantePago.Application.Commands.Imputacion;
using ComprobantePago.Application.DTOs.Comprobante.Requests;
using ComprobantePago.Application.Interfaces.Services;
using ComprobantePago.Infrastructure.Persistence;
using ComprobantePago.Infrastructure.Repositories;
using ComprobantePago.Infrastructure.Services;
using ComprobantePago.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ComprobantePago.Tests.HU03
{
    /// <summary>
    /// HU-03 · CA-02
    /// Validar que se pueden agregar imputaciones contables con cuenta contable
    /// y códigos de unidad, y que los datos se persisten correctamente.
    /// </summary>
    public class CA02_AgregarImputacionTests
    {
        private static (ComprobanteRepository repo, AppDbContext db) Construir(string nombre)
        {
            var db      = DbContextFactory.Crear(nombre);
            var uow     = new TestUnitOfWork(db);
            var usuario = new Mock<ComprobantePago.Application.Interfaces.IUsuarioContexto>();
            usuario.Setup(u => u.Correo).Returns("digitador@tecsur.com.pe");

            var repo = new ComprobanteRepository(
                db, uow,
                new Mock<ISunatService>().Object,
                new XmlComprobanteService(),
                new PdfComprobanteService(),
                usuario.Object,
                NullLogger<ComprobanteRepository>.Instance);

            return (repo, db);
        }

        /// Crea un comprobante base y devuelve su folio.
        private static async Task<string> CrearComprobanteAsync(
            ComprobanteRepository repo, string dbNombre)
        {
            var command = new Application.Commands.Comprobante.RegistrarComprobanteCommand
            {
                Comprobante = new RegistrarComprobanteDto
                {
                    Ruc             = "20206018411",
                    RazonSocial     = "EMPRESA SAC",
                    TipoDocumento   = "FP",
                    TipoSunat       = "01",
                    Serie           = "F001",
                    Numero          = "00000200",
                    FechaEmision    = "01/04/2026",
                    FechaRecepcion  = "02/04/2026",
                    Moneda          = "PEN",
                    TasaCambio      = 1m,
                    MontoNeto       = 1000m,
                    MontoIGVCredito = 180m,
                    MontoTotal      = 1180m,
                    MontoBruto      = 1180m,
                    TieneDetraccion = false
                }
            };
            return await repo.GuardarAsync(command);
        }

        private static ImputacionDto ImputacionValida(string folio) => new()
        {
            Folio             = folio,
            CuentaContable    = "6201001",
            AliasCuenta       = "62010",
            DescripcionCuenta = "Remuneraciones básicas",
            Monto             = 500m,
            Descripcion       = "Servicio de consultoría",
            Proyecto          = string.Empty,
            CodUnidad1Cuenta  = "U1-001",
            CodUnidad3Cuenta  = string.Empty,
            CodUnidad4Cuenta  = string.Empty
        };

        // ── Agregar imputación básica ─────────────────────────────────────────

        [Fact]
        public async Task AgregarImputacion_CuentaYCodUnidad1_PersisteCorrecto()
        {
            var (repo, db) = Construir(nameof(AgregarImputacion_CuentaYCodUnidad1_PersisteCorrecto));
            var folio      = await CrearComprobanteAsync(repo, nameof(AgregarImputacion_CuentaYCodUnidad1_PersisteCorrecto));

            var cmd    = new AgregarImputacionCommand { Imputacion = ImputacionValida(folio) };
            var result = await repo.AgregarImputacionAsync(cmd);

            Assert.NotNull(result);
            var guardada = db.ImputacionesContables.FirstOrDefault(i => i.Folio == folio);
            Assert.NotNull(guardada);
            Assert.Equal("6201001",   guardada.CuentaContable);
            Assert.Equal("U1-001",    guardada.CodUnidad1Cuenta);
            Assert.Equal(500m,        guardada.Monto);
        }

        [Fact]
        public async Task AgregarImputacion_SoloCodUnidad3_PersisteCorrecto()
        {
            var (repo, db) = Construir(nameof(AgregarImputacion_SoloCodUnidad3_PersisteCorrecto));
            var folio      = await CrearComprobanteAsync(repo, nameof(AgregarImputacion_SoloCodUnidad3_PersisteCorrecto));

            var imp = ImputacionValida(folio);
            imp.CodUnidad1Cuenta = string.Empty;
            imp.CodUnidad3Cuenta = "U3-020";
            imp.CodUnidad4Cuenta = string.Empty;

            await repo.AgregarImputacionAsync(new AgregarImputacionCommand { Imputacion = imp });

            var guardada = db.ImputacionesContables.FirstOrDefault(i => i.Folio == folio);
            Assert.NotNull(guardada);
            Assert.Equal("U3-020", guardada.CodUnidad3Cuenta);
            Assert.Equal(string.Empty, guardada.CodUnidad1Cuenta);
        }

        [Fact]
        public async Task AgregarImputacion_LosTresCodUnidad_PersisteCorrecto()
        {
            var (repo, db) = Construir(nameof(AgregarImputacion_LosTresCodUnidad_PersisteCorrecto));
            var folio      = await CrearComprobanteAsync(repo, nameof(AgregarImputacion_LosTresCodUnidad_PersisteCorrecto));

            var imp = ImputacionValida(folio);
            imp.CodUnidad1Cuenta = "U1-001";
            imp.CodUnidad3Cuenta = "U3-010";
            imp.CodUnidad4Cuenta = "U4-050";

            await repo.AgregarImputacionAsync(new AgregarImputacionCommand { Imputacion = imp });

            var guardada = db.ImputacionesContables.FirstOrDefault(i => i.Folio == folio);
            Assert.NotNull(guardada);
            Assert.Equal("U1-001", guardada.CodUnidad1Cuenta);
            Assert.Equal("U3-010", guardada.CodUnidad3Cuenta);
            Assert.Equal("U4-050", guardada.CodUnidad4Cuenta);
        }

        // ── Secuencia automática ──────────────────────────────────────────────

        [Fact]
        public async Task AgregarImputacion_PrimeraLinea_SecuenciaEs1()
        {
            var (repo, db) = Construir(nameof(AgregarImputacion_PrimeraLinea_SecuenciaEs1));
            var folio      = await CrearComprobanteAsync(repo, nameof(AgregarImputacion_PrimeraLinea_SecuenciaEs1));

            var result = await repo.AgregarImputacionAsync(
                new AgregarImputacionCommand { Imputacion = ImputacionValida(folio) });

            Assert.Equal(1, result.Secuencia);
        }

        [Fact]
        public async Task AgregarImputacion_TresLineas_SecuenciaIncrementa()
        {
            var (repo, db) = Construir(nameof(AgregarImputacion_TresLineas_SecuenciaIncrementa));
            var folio      = await CrearComprobanteAsync(repo, nameof(AgregarImputacion_TresLineas_SecuenciaIncrementa));

            var cmd = new AgregarImputacionCommand { Imputacion = ImputacionValida(folio) };
            var r1  = await repo.AgregarImputacionAsync(cmd);
            var r2  = await repo.AgregarImputacionAsync(cmd);
            var r3  = await repo.AgregarImputacionAsync(cmd);

            Assert.Equal(1, r1.Secuencia);
            Assert.Equal(2, r2.Secuencia);
            Assert.Equal(3, r3.Secuencia);
        }

        // ── Usuario y fecha de registro ───────────────────────────────────────

        [Fact]
        public async Task AgregarImputacion_RegistraUsuarioDigitador()
        {
            var (repo, db) = Construir(nameof(AgregarImputacion_RegistraUsuarioDigitador));
            var folio      = await CrearComprobanteAsync(repo, nameof(AgregarImputacion_RegistraUsuarioDigitador));

            await repo.AgregarImputacionAsync(
                new AgregarImputacionCommand { Imputacion = ImputacionValida(folio) });

            var guardada = db.ImputacionesContables.FirstOrDefault(i => i.Folio == folio);
            Assert.NotNull(guardada);
            Assert.Equal("digitador@tecsur.com.pe", guardada.UsuarioReg);
            Assert.NotNull(guardada.FechaReg);
        }

        // ── Descripción y proyecto ────────────────────────────────────────────

        [Fact]
        public async Task AgregarImputacion_DescripcionYProyecto_PersisteCorrecto()
        {
            var (repo, db) = Construir(nameof(AgregarImputacion_DescripcionYProyecto_PersisteCorrecto));
            var folio      = await CrearComprobanteAsync(repo, nameof(AgregarImputacion_DescripcionYProyecto_PersisteCorrecto));

            var imp = ImputacionValida(folio);
            imp.Descripcion = "Pago honorarios consultoría";
            imp.Proyecto    = "PROY-2026-001";

            await repo.AgregarImputacionAsync(new AgregarImputacionCommand { Imputacion = imp });

            var guardada = db.ImputacionesContables.FirstOrDefault(i => i.Folio == folio);
            Assert.NotNull(guardada);
            Assert.Equal("Pago honorarios consultoría", guardada.Descripcion);
            Assert.Equal("PROY-2026-001", guardada.Proyecto);
        }
    }
}
