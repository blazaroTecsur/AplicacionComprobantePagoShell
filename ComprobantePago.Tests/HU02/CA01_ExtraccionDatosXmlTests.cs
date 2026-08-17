using ComprobantePago.Infrastructure.Services;
using ComprobantePago.Tests.Helpers;
using System.Text;
using Xunit;

namespace ComprobantePago.Tests.HU02
{
    /// <summary>
    /// CA-01: Los datos del XML se extraen correctamente y se muestran por defecto.
    ///
    /// Verifica que XmlComprobanteService.LeerDatosXml extraiga todos los campos
    /// necesarios para poblar la cabecera y los montos del comprobante sin intervención manual.
    /// </summary>
    public class CA01_ExtraccionDatosXmlTests
    {
        private readonly XmlComprobanteService _service = new();

        private static Stream ToStream(string xml)
            => new MemoryStream(Encoding.UTF8.GetBytes(xml));

        // ── Identificación ─────────────────────────────────────────────────────

        [Fact]
        public void LeerXml_ExtraeSerie()
        {
            var xml  = ArchivoTestFactory.XmlFacturaSunat(serie: "F557", numero: "00183136");
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.Equal("F557", datos.Serie);
        }

        [Fact]
        public void LeerXml_ExtraeNumero()
        {
            var xml  = ArchivoTestFactory.XmlFacturaSunat(serie: "F557", numero: "00183136");
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.Equal("00183136", datos.Numero);
        }

        [Fact]
        public void LeerXml_ExtraeFechaEnFormatoDdMmYyyy()
        {
            var xml  = ArchivoTestFactory.XmlFacturaSunat(fecha: "2026-03-01");
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.Equal("01/03/2026", datos.FechaEmision);
        }

        [Fact]
        public void LeerXml_ExtraeTipoDocumentoSunat()
        {
            var xml  = ArchivoTestFactory.XmlFacturaSunat(tipoDoc: "01");
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.Equal("01", datos.TipoSunat);
        }

        // ── Proveedor ──────────────────────────────────────────────────────────

        [Fact]
        public void LeerXml_ExtraeRucProveedor()
        {
            var xml  = ArchivoTestFactory.XmlFacturaSunat(ruc: "20206018411");
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.Equal("20206018411", datos.RucProveedor);
        }

        [Fact]
        public void LeerXml_ExtraeRazonSocialProveedor()
        {
            var xml  = ArchivoTestFactory.XmlFacturaSunat();
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.False(string.IsNullOrWhiteSpace(datos.RazonSocialProveedor),
                "La razón social del proveedor debe extraerse del XML.");
        }

        // ── Moneda y tasa de cambio ────────────────────────────────────────────

        [Fact]
        public void LeerXml_CuandoMonedaPEN_TasaCambioEsUno()
        {
            var xml  = ArchivoTestFactory.XmlFacturaSunat(moneda: "PEN");
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.Equal("PEN", datos.Moneda);
            Assert.Equal(1m, datos.TasaCambio);
        }

        [Fact]
        public void LeerXml_CuandoMonedaUSD_TasaCambioSeLeDelXml()
        {
            // XML con moneda USD y nodo PaymentExchangeRate
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
                         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2""
                         xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"">
                  <cbc:ID>F001-00000001</cbc:ID>
                  <cbc:IssueDate>2026-03-01</cbc:IssueDate>
                  <cbc:InvoiceTypeCode>01</cbc:InvoiceTypeCode>
                  <cbc:DocumentCurrencyCode>USD</cbc:DocumentCurrencyCode>
                  <cac:PaymentExchangeRate>
                    <cbc:SourceCurrencyCode>USD</cbc:SourceCurrencyCode>
                    <cbc:TargetCurrencyCode>PEN</cbc:TargetCurrencyCode>
                    <cbc:CalculationRate>3.750</cbc:CalculationRate>
                  </cac:PaymentExchangeRate>
                  <cac:AccountingSupplierParty>
                    <cac:Party>
                      <cac:PartyIdentification><cbc:ID>20111111111</cbc:ID></cac:PartyIdentification>
                      <cac:PartyLegalEntity><cbc:RegistrationName>PROVEEDOR USD</cbc:RegistrationName></cac:PartyLegalEntity>
                    </cac:Party>
                  </cac:AccountingSupplierParty>
                  <cac:LegalMonetaryTotal>
                    <cbc:PayableAmount>1000.00</cbc:PayableAmount>
                  </cac:LegalMonetaryTotal>
                </Invoice>";

            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.Equal("USD", datos.Moneda);
            Assert.Equal(3.750m, datos.TasaCambio);
        }

        // ── Montos ─────────────────────────────────────────────────────────────

        [Fact]
        public void LeerXml_ExtraeMontoTotal()
        {
            var xml  = ArchivoTestFactory.XmlFacturaSunat(montoTotal: 1180.00m);
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.Equal(1180.00m, datos.MontoTotal);
        }

        [Fact]
        public void LeerXml_ExtraeMontoNeto()
        {
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
                         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2""
                         xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"">
                  <cbc:ID>F001-00000001</cbc:ID>
                  <cbc:IssueDate>2026-03-01</cbc:IssueDate>
                  <cbc:InvoiceTypeCode>01</cbc:InvoiceTypeCode>
                  <cbc:DocumentCurrencyCode>PEN</cbc:DocumentCurrencyCode>
                  <cac:AccountingSupplierParty>
                    <cac:Party>
                      <cac:PartyIdentification><cbc:ID>20206018411</cbc:ID></cac:PartyIdentification>
                      <cac:PartyLegalEntity><cbc:RegistrationName>EMPRESA SAC</cbc:RegistrationName></cac:PartyLegalEntity>
                    </cac:Party>
                  </cac:AccountingSupplierParty>
                  <cac:LegalMonetaryTotal>
                    <cbc:LineExtensionAmount>1000.00</cbc:LineExtensionAmount>
                    <cbc:PayableAmount>1180.00</cbc:PayableAmount>
                  </cac:LegalMonetaryTotal>
                </Invoice>";

            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.Equal(1000.00m, datos.MontoNeto);
        }

        // ── Detracción ─────────────────────────────────────────────────────────

        [Fact]
        public void LeerXml_CuandoTieneDetraccion_DetectaFlag()
        {
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
                         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2""
                         xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"">
                  <cbc:ID>F001-00000001</cbc:ID>
                  <cbc:IssueDate>2026-03-01</cbc:IssueDate>
                  <cbc:InvoiceTypeCode>01</cbc:InvoiceTypeCode>
                  <cbc:DocumentCurrencyCode>PEN</cbc:DocumentCurrencyCode>
                  <cac:PaymentTerms>
                    <cbc:ID>Detraccion</cbc:ID>
                    <cbc:PaymentMeansID>030</cbc:PaymentMeansID>
                    <cbc:PaymentPercent>12.00</cbc:PaymentPercent>
                    <cbc:Amount>141.60</cbc:Amount>
                  </cac:PaymentTerms>
                  <cac:AccountingSupplierParty>
                    <cac:Party>
                      <cac:PartyIdentification><cbc:ID>20206018411</cbc:ID></cac:PartyIdentification>
                      <cac:PartyLegalEntity><cbc:RegistrationName>EMPRESA SAC</cbc:RegistrationName></cac:PartyLegalEntity>
                    </cac:Party>
                  </cac:AccountingSupplierParty>
                  <cac:LegalMonetaryTotal>
                    <cbc:PayableAmount>1180.00</cbc:PayableAmount>
                  </cac:LegalMonetaryTotal>
                </Invoice>";

            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.True(datos.TieneDetraccion);
            Assert.Equal("030", datos.CodigoDetraccion);
            Assert.Equal(12.00m, datos.PorcentajeDetraccion);
            Assert.Equal(141.60m, datos.MontoDetraccion);
        }

        [Fact]
        public void LeerXml_CuandoNoTieneDetraccion_FlagEsFalso()
        {
            var xml  = ArchivoTestFactory.XmlFacturaSunat();
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.False(datos.TieneDetraccion);
        }

        [Fact]
        public void LeerXml_XmlVacio_RetornaDtoSinExcepcion()
        {
            var xml  = @"<?xml version=""1.0""?><Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""/>";
            var datos = _service.LeerDatosXml(ToStream(xml));
            Assert.NotNull(datos);
        }
    }
}
