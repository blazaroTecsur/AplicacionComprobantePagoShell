using ComprobantePago.Application.Commands.Comprobante;
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
    /// HU-03 · CA-03
    /// Validar que las imputaciones contables se pueden editar y eliminar,
    /// y que los cambios se reflejan correctamente en la base de datos.
    /// </summary>
    public class CA03_EditarEliminarImputacionTests
    {
        private static (ComprobanteRepository repo, AppDbContext db) Construir(string nombre)
        {
            var db      = DbContextFactory.Crear(nombre);
            var uow     = new TestUnitOfWork(db);
            var usuario = new Mock<ComprobantePago.Application.Interfaces.IUsuarioContexto>();
            usuario.Setup(u => u.Correo).Returns("editor@tecsur.com.pe");

            var repo = new ComprobanteRepository(
                db, uow,
                new Mock<ISunatService>().Object,
                new XmlComprobanteService(),
                new PdfComprobanteService(),
                usuario.Object,
                NullLogger<ComprobanteRepository>.Instance);

            return (repo, db);
        }

        private static async Task<string> CrearComprobanteAsync(ComprobanteRepository repo)
        {
            return await repo.GuardarAsync(new RegistrarComprobanteCommand
            {
                Comprobante = new RegistrarComprobanteDto
                {
                    Ruc             = "20206018411",
                    RazonSocial     = "EMPRESA SAC",
                    TipoDocumento   = "FP",
                    TipoSunat       = "01",
                    Serie           = "F001",
                    Numero          = "00000300",
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
            });
        }

        private static async Task<int> AgregarImputacionAsync(
            ComprobanteRepository repo, string folio,
            string cuenta = "6201001", decimal monto = 400m,
            string codU1 = "U1-001")
        {
            var result = await repo.AgregarImputacionAsync(new AgregarImputacionCommand
            {
                Imputacion = new ImputacionDto
                {
                    Folio             = folio,
                    CuentaContable    = cuenta,
                    AliasCuenta       = cuenta.Substring(0, 5),
                    DescripcionCuenta = "Cuenta de prueba",
                    Monto             = monto,
                    Descripcion       = "Línea de prueba",
                    CodUnidad1Cuenta  = codU1,
                    CodUnidad3Cuenta  = string.Empty,
                    CodUnidad4Cuenta  = string.Empty
                }
            });
            return result.Secuencia;
        }

        // ── Editar imputación ─────────────────────────────────────────────────

        [Fact]
        public async Task EditarImputacion_CambiaMontoYCodigos_PersisteCorrecto()
        {
            var (repo, db) = Construir(nameof(EditarImputacion_CambiaMontoYCodigos_PersisteCorrecto));
            var folio      = await CrearComprobanteAsync(repo);
            var secuencia  = await AgregarImputacionAsync(repo, folio);

            await repo.EditarImputacionAsync(new EditarImputacionCommand
            {
                Imputacion = new ImputacionDto
                {
                    Folio             = folio,
                    Secuencia         = secuencia.ToString(),
                    CuentaContable    = "6201001",
                    AliasCuenta       = "62010",
                    DescripcionCuenta = "Remuneraciones actualizadas",
                    Monto             = 750m,
                    Descripcion       = "Descripción editada",
                    CodUnidad1Cuenta  = "U1-002",
                    CodUnidad3Cuenta  = "U3-015",
                    CodUnidad4Cuenta  = string.Empty
                }
            });

            var editada = db.ImputacionesContables
                .FirstOrDefault(i => i.Folio == folio && i.Secuencia == secuencia);

            Assert.NotNull(editada);
            Assert.Equal(750m,    editada.Monto);
            Assert.Equal("U1-002", editada.CodUnidad1Cuenta);
            Assert.Equal("U3-015", editada.CodUnidad3Cuenta);
            Assert.Equal("Remuneraciones actualizadas", editada.DescripcionCuenta);
        }

        [Fact]
        public async Task EditarImputacion_RegistraUsuarioActualizacion()
        {
            var (repo, db) = Construir(nameof(EditarImputacion_RegistraUsuarioActualizacion));
            var folio      = await CrearComprobanteAsync(repo);
            var secuencia  = await AgregarImputacionAsync(repo, folio);

            await repo.EditarImputacionAsync(new EditarImputacionCommand
            {
                Imputacion = new ImputacionDto
                {
                    Folio             = folio,
                    Secuencia         = secuencia.ToString(),
                    CuentaContable    = "6201001",
                    AliasCuenta       = "62010",
                    DescripcionCuenta = "Cuenta actualizada",
                    Monto             = 400m,
                    CodUnidad1Cuenta  = "U1-001",
                    CodUnidad3Cuenta  = string.Empty,
                    CodUnidad4Cuenta  = string.Empty
                }
            });

            var editada = db.ImputacionesContables
                .FirstOrDefault(i => i.Folio == folio && i.Secuencia == secuencia);

            Assert.NotNull(editada);
            Assert.Equal("editor@tecsur.com.pe", editada.UsuarioAct);
            Assert.NotNull(editada.FechaAct);
        }

        [Fact]
        public async Task EditarImputacion_CambiaCodUnidad4_PersisteCorrecto()
        {
            var (repo, db) = Construir(nameof(EditarImputacion_CambiaCodUnidad4_PersisteCorrecto));
            var folio      = await CrearComprobanteAsync(repo);
            var secuencia  = await AgregarImputacionAsync(repo, folio);

            await repo.EditarImputacionAsync(new EditarImputacionCommand
            {
                Imputacion = new ImputacionDto
                {
                    Folio             = folio,
                    Secuencia         = secuencia.ToString(),
                    CuentaContable    = "6201001",
                    AliasCuenta       = "62010",
                    DescripcionCuenta = "Cuenta de prueba",
                    Monto             = 400m,
                    CodUnidad1Cuenta  = string.Empty,
                    CodUnidad3Cuenta  = string.Empty,
                    CodUnidad4Cuenta  = "U4-099"
                }
            });

            var editada = db.ImputacionesContables
                .FirstOrDefault(i => i.Folio == folio && i.Secuencia == secuencia);

            Assert.NotNull(editada);
            Assert.Equal("U4-099", editada.CodUnidad4Cuenta);
            Assert.Equal(string.Empty, editada.CodUnidad1Cuenta);
        }

        // ── Editar imputación inexistente ─────────────────────────────────────

        [Fact]
        public async Task EditarImputacion_SecuenciaInexistente_LanzaExcepcion()
        {
            var (repo, _) = Construir(nameof(EditarImputacion_SecuenciaInexistente_LanzaExcepcion));
            var folio     = await CrearComprobanteAsync(repo);

            await Assert.ThrowsAnyAsync<Exception>(() =>
                repo.EditarImputacionAsync(new EditarImputacionCommand
                {
                    Imputacion = new ImputacionDto
                    {
                        Folio             = folio,
                        Secuencia         = "999",
                        CuentaContable    = "6201001",
                        AliasCuenta       = "62010",
                        DescripcionCuenta = "Cuenta X",
                        Monto             = 100m,
                        CodUnidad1Cuenta  = "U1-001"
                    }
                }));
        }

        // ── Eliminar imputación ───────────────────────────────────────────────

        [Fact]
        public async Task EliminarImputacion_ExistePreviamente_YaNoExisteEnDb()
        {
            var (repo, db) = Construir(nameof(EliminarImputacion_ExistePreviamente_YaNoExisteEnDb));
            var folio      = await CrearComprobanteAsync(repo);
            var secuencia  = await AgregarImputacionAsync(repo, folio);

            Assert.True(db.ImputacionesContables
                .Any(i => i.Folio == folio && i.Secuencia == secuencia));

            await repo.EliminarImputacionAsync(new EliminarImputacionCommand
            {
                Folio     = folio,
                Secuencia = secuencia
            });

            Assert.False(db.ImputacionesContables
                .Any(i => i.Folio == folio && i.Secuencia == secuencia));
        }

        [Fact]
        public async Task EliminarImputacion_VariasLineas_SoloEliminaLaIndicada()
        {
            var (repo, db) = Construir(nameof(EliminarImputacion_VariasLineas_SoloEliminaLaIndicada));
            var folio      = await CrearComprobanteAsync(repo);
            var sec1       = await AgregarImputacionAsync(repo, folio, monto: 300m);
            var sec2       = await AgregarImputacionAsync(repo, folio, monto: 400m);
            var sec3       = await AgregarImputacionAsync(repo, folio, monto: 500m);

            await repo.EliminarImputacionAsync(new EliminarImputacionCommand
            {
                Folio     = folio,
                Secuencia = sec2
            });

            var restantes = db.ImputacionesContables
                .Where(i => i.Folio == folio)
                .Select(i => i.Secuencia)
                .ToList();

            Assert.Equal(2, restantes.Count);
            Assert.Contains(sec1, restantes);
            Assert.Contains(sec3, restantes);
            Assert.DoesNotContain(sec2, restantes);
        }
    }
}
