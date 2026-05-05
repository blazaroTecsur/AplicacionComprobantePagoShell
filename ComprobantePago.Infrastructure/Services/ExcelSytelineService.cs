using ClosedXML.Excel;
using ComprobantePago.Application.DTOs.Responses;
using ComprobantePago.Application.Interfaces.Services;

namespace ComprobantePago.Infrastructure.Services
{
    public class ExcelSytelineService : IExcelSytelineService
    {
        public byte[] GenerarCabecera(
            IEnumerable<SytelineCabeceraDto> datos)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Hoja1");

            // ── Encabezados exactos del CSV ───────
            var headers = new[]
            {
                "Proveedor", "Nombre", "Tipo", "Cancelación",
                "Comprobante", "Factura", "Fecha factura",
                "Fecha distribución", "OC",
                "Registrado desde OC", "Reg previo",
                "Facturad autom",
                "Estado de registro en segundo plano",
                "NRM", "Impo compra", "Flete", "Imp",
                "Corretaje", "Seguro", "Flete local",
                "Cargos varios", "Sales Tax", "Imp ventas2",
                "Mnto fctura", "Imp sin desc", "Imp desc",
                "Código prox", "Día prox", "% desc",
                "Días desc", "Fecha dcto", "Días vto",
                "Fecha ven", "Inclr imp en coste",
                "Tasa fija", "Moneda", "Tipo de cambio",
                "Cta CP", "Cta CP unid 1", "Cta CP unid 2",
                "Cta CP unid 3", "Cta CP unid 4",
                "Descripción cuenta", "Ref", "Tax  Code",
                "Descripción cód imp 1", "IGV",
                "Descripción cód imp 2",
                "Sitio orig OC creadr", "OC creador",
                "Sitio orig comp creador", "Comp creador",
                "Estado aut", "Doc.Soporte",
                "DerapatZLA_DocRembolsoIsActiveGridCol_SITE",
                "aptZLA_SeqFacGridCol_SITE", "Autorizó",
                "Notas",
                "Tipo de sistema de informes fiscales",
                "Usa Detracción", "Detracción", "Tasa",
                "Total detracción", "Total Det. local",
                "C(ZLA_DerDescDetraccionStatic_GROUP)",
                "C(aptZLA_TasaDetraccionStatic_GROUP)",
                "C(aptZLA_TotalDetraccionLocalStatic_GROUP)",
                "PLMulticurrencyInvoiceCheckBox",
                "Comprobante", "SAD", "SAD Voucher",
                "Manual VAT Entry", "Manual VAT Voucher",
                "PLSumForDistAmountEdit",
                "PLSumDomDistAmountEdit",
                "PLSumForDistTaxBasisEdit",
                "PLSumDomDistTaxBasisEdit",
                "Cuenta bancaria internacional",
                "Estado IVA", "VIES Status", "Fecha"
            };

            // Estilo encabezado
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#185FA5");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;
            }

            // ── Datos ─────────────────────────────
            int row = 2;
            foreach (var d in datos)
            {
                int col = 1;

                // 1-Proveedor
                ws.Cell(row, col++).Value = d.Proveedor;
                // 2-Nombre
                ws.Cell(row, col++).Value = d.Nombre;
                // 3-Tipo (fijo)
                ws.Cell(row, col++).Value = "Comprobante";
                // 4-Cancelación (fijo)
                ws.Cell(row, col++).Value = 0;
                // 5-Comprobante (voucher Syteline pre-asignado; si no hay, usa IdComprobante)
                ws.Cell(row, col++).Value = d.VoucherSyteline > 0 ? d.VoucherSyteline : d.Comprobante;
                // 6-Factura
                ws.Cell(row, col++).Value = d.Factura;
                // 7-Fecha factura
                ws.Cell(row, col++).Value = d.FechaFactura;
                // 8-Fecha distribución
                ws.Cell(row, col++).Value = d.FechaDistribucion;
                // 9-OC (fijo)
                ws.Cell(row, col++).Value = "";
                // 10-Registrado desde OC (fijo)
                ws.Cell(row, col++).Value = 0;
                // 11-Reg previo (fijo)
                ws.Cell(row, col++).Value = "";
                // 12-Facturad autom (fijo)
                ws.Cell(row, col++).Value = 0;
                // 13-Estado registro (fijo)
                ws.Cell(row, col++).Value = "Permitir";
                // 14-NRM (fijo)
                ws.Cell(row, col++).Value = "";
                // 15-Impo compra
                SetMonto(ws.Cell(row, col++), d.ImpoCompra);
                // 16-Flete (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 17-Imp (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 18-Corretaje (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 19-Seguro (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 20-Flete local (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 21-Cargos varios
                SetMonto(ws.Cell(row, col++), d.CargosVarios);
                // 22-Sales Tax (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 23-Imp ventas2
                SetMonto(ws.Cell(row, col++), d.ImpVentas2);
                // 24-Mnto fctura
                SetMonto(ws.Cell(row, col++), d.MntoFactura);
                // 25-Imp sin desc
                SetMonto(ws.Cell(row, col++), d.ImpSinDesc);
                // 26-Imp desc (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 27-Código prox (fijo)
                ws.Cell(row, col++).Value = 99;
                // 28-Día prox (fijo)
                ws.Cell(row, col++).Value = 0;
                // 29-% desc (fijo)
                ws.Cell(row, col++).Value = 0.000;
                // 30-Días desc (fijo)
                ws.Cell(row, col++).Value = 0;
                // 31-Fecha dcto
                ws.Cell(row, col++).Value = d.FechaDcto;
                // 32-Días vto
                ws.Cell(row, col++).Value = d.DiasVto;
                // 33-Fecha ven
                ws.Cell(row, col++).Value = d.FechaVen;
                // 34-Inclr imp en coste (fijo)
                ws.Cell(row, col++).Value = 0;
                // 35-Tasa fija (fijo)
                ws.Cell(row, col++).Value = 0;
                // 36-Moneda
                ws.Cell(row, col++).Value = d.Moneda;
                // 37-Tipo de cambio
                ws.Cell(row, col++).Value = d.TipoCambio;
                // 38-Cta CP
                ws.Cell(row, col++).Value = d.CtaCP;
                // 39-Cta CP unid 1
                ws.Cell(row, col++).Value = d.CtaCPUnid1;
                // 40-Cta CP unid 2
                ws.Cell(row, col++).Value = d.CtaCPUnid2;
                // 41-Cta CP unid 3
                ws.Cell(row, col++).Value = d.CtaCPUnid3;
                // 42-Cta CP unid 4
                ws.Cell(row, col++).Value = d.CtaCPUnid4;
                // 43-Descripción cuenta
                ws.Cell(row, col++).Value = ""; // d.DescripcionCuenta; // ← opcional, puede causar problemas si tiene comas o saltos de línea
                // 44-Ref
                ws.Cell(row, col++).Value = d.Ref;
                // 45-Tax Code: Exento si PorcentajeIGV=0 y MontoExento>0, sino NR
                ws.Cell(row, col++).Value =
                    d.PorcentajeIGV == 0 && d.MontoExento > 0 ? "Exento" : "NR";
                // 46-Descripción cód imp 1 (fijo)
                ws.Cell(row, col++).Value = "";
                // 47-IGV (fijo)
                ws.Cell(row, col++).Value = "";
                // 48-Descripción cód imp 2 (fijo)
                ws.Cell(row, col++).Value = "";
                // 49-Sitio orig OC (fijo)
                ws.Cell(row, col++).Value = "";
                // 50-OC creador (fijo)
                ws.Cell(row, col++).Value = "";
                // 51-Sitio orig comp (fijo)
                ws.Cell(row, col++).Value = "";
                // 52-Comp creador (fijo)
                ws.Cell(row, col++).Value = "";
                // 53-Estado aut
                ws.Cell(row, col++).Value = d.EstadoAut;
                // 54-Doc.Soporte (fijo)
                ws.Cell(row, col++).Value = 0;
                // 55-DerapatZLA (fijo)
                ws.Cell(row, col++).Value = "";
                // 56-aptZLA_SeqFac (fijo)
                ws.Cell(row, col++).Value = "001";
                // 57-Autorizó
                ws.Cell(row, col++).Value = d.Autorizo;
                // 58-Notas
                ws.Cell(row, col++).Value = d.Notas;
                // 59-Tipo sistema informes (fijo)
                ws.Cell(row, col++).Value = "";
                // 60-Usa Detracción
                ws.Cell(row, col++).Value = d.UsaDetraccion;
                // 61-Detracción
                ws.Cell(row, col++).Value = d.Detraccion;
                // 62-Tasa
                SetMonto(ws.Cell(row, col++), d.Tasa);
                // 63-Total detracción
                SetMonto(ws.Cell(row, col++), d.TotalDetraccion);
                // 64-Total Det. local
                SetMonto(ws.Cell(row, col++), d.TotalDetLocal);
                // 65-C(ZLA_DerDesc...) (fijo)
                ws.Cell(row, col++).Value = "";
                // 66-C(aptZLA_Tasa...) (fijo)
                ws.Cell(row, col++).Value = "";
                // 67-C(aptZLA_Total...) (fijo)
                ws.Cell(row, col++).Value = "";
                // 68-PLMulticurrency (fijo)
                ws.Cell(row, col++).Value = 0;
                // 69-Comprobante (fijo)
                ws.Cell(row, col++).Value = "";
                // 70-SAD (fijo)
                ws.Cell(row, col++).Value = 0;
                // 71-SAD Voucher (fijo)
                ws.Cell(row, col++).Value = 0;
                // 72-Manual VAT Entry (fijo)
                ws.Cell(row, col++).Value = 0;
                // 73-Manual VAT Voucher (fijo)
                ws.Cell(row, col++).Value = 0;
                // 74-PLSumForDistAmount (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 75-PLSumDomDistAmount (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 76-PLSumForDistTaxBasis (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 77-PLSumDomDistTaxBasis (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 78-Cuenta bancaria (fijo)
                ws.Cell(row, col++).Value = "";
                // 79-Estado IVA (fijo)
                ws.Cell(row, col++).Value = "";
                // 80-VIES Status (fijo)
                ws.Cell(row, col++).Value = "";
                // 81-Fecha (fijo)
                ws.Cell(row, col++).Value = "";

                // Filas alternas
                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor =
                        XLColor.FromHtml("#EBF2FA");

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public byte[] GenerarDistribucion(
            IEnumerable<SytelineDistribucionDto> datos)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Hoja1");

            var headers = new[]
            {
                "Proveedor", "Comprobante", "Nombre", "Tipo",
                "Fecha dist", "OC", "NRM", "Reg previo",
                "Reg desde OC", "Factura", "Fecha factura",
                "Sec dist", "Proyecto", "Tar", "Cód coste",
                "Sist impst", "Cód imp", "Desc cód imp",
                "Cód imp exento", "Desc exento",
                "Base imp", "Importe", "Tipo cambio",
                "Cuenta", "Desc cuenta",
                "Unidad1", "Unid2", "Unidad3", "Unidad4",
                "Moneda",
                "Impo compra", "Flete", "Imp", "Corretaje",
                "Seguro", "Flete local", "Cargos varios",
                "Sales Tax", "IGV", "Mnto fctura", "Total dist",
                "Is Third Party?", "Nº proveedor", "Nombre prov",
                "Núm reg fiscal", "Dígito cheque",
                "Nacional", "Doc.Soporte", "Tipo doc",
                "Número Doc", "Establecimiento", "Secuencial",
                "Pago Loc Ext", "Punto emisión", "Pago exterior",
                "Tipo Régimen", "Tipo doc", "Tipo núm reg",
                "Aplica Cont", "País", "¿Régimen pref?", "País",
                "C(NumAutorizacion)", "C(FechaEmision)"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#185FA5");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;
            }

            int row = 2;
            foreach (var d in datos)
            {
                int col = 1;

                // 1-Proveedor
                ws.Cell(row, col++).Value = d.Proveedor;
                // 2-Comprobante (voucher Syteline pre-asignado; si no hay, usa IdComprobante)
                ws.Cell(row, col++).Value = d.VoucherSyteline > 0 ? d.VoucherSyteline : d.Comprobante;
                // 3-Nombre
                ws.Cell(row, col++).Value = d.Nombre;
                // 4-Tipo (fijo)
                ws.Cell(row, col++).Value = "Comprobante";
                // 5-Fecha dist
                ws.Cell(row, col++).Value = d.FechaDistribucion;
                // 6-OC (fijo)
                ws.Cell(row, col++).Value = "";
                // 7-NRM (fijo)
                ws.Cell(row, col++).Value = "";
                // 8-Reg previo (fijo)
                ws.Cell(row, col++).Value = "";
                // 9-Reg desde OC (fijo)
                ws.Cell(row, col++).Value = 0;
                // 10-Factura
                ws.Cell(row, col++).Value = d.Factura;
                // 11-Fecha factura
                ws.Cell(row, col++).Value = d.FechaFactura;
                // 12-Sec dist
                ws.Cell(row, col++).Value = d.SecDist;
                // 13-Proyecto
                ws.Cell(row, col++).Value = d.Proyecto;
                // 14-Tar (fijo)
                ws.Cell(row, col++).Value = 0;
                // 15-Cód coste (fijo)
                ws.Cell(row, col++).Value = 0;
                // 16-Sist impst
                ws.Cell(row, col++).Value = d.SistImpst;
                // 17-Cód imp
                ws.Cell(row, col++).Value = d.CodImp;
                // 18-Desc cód imp
                ws.Cell(row, col++).Value = d.DescCodImp;
                // 19-Cód imp exento (fijo)
                ws.Cell(row, col++).Value = "";
                // 20-Desc exento (fijo)
                ws.Cell(row, col++).Value = "";
                // 21-Base imp
                SetMonto(ws.Cell(row, col++), d.BaseImp);
                // 22-Importe
                SetMonto(ws.Cell(row, col++), d.Importe);
                // 23-Tipo cambio
                ws.Cell(row, col++).Value = d.TasaCambio;
                // 24-Cuenta
                ws.Cell(row, col++).Value = d.CuentaContable;
                // 25-Desc cuenta
                ws.Cell(row, col++).Value = d.DescripcionCuenta;
                // 26-Unidad1
                ws.Cell(row, col++).Value = d.CodUnidad1;
                // 27-Unid2 (fijo)
                ws.Cell(row, col++).Value = "";
                // 28-Unidad3
                ws.Cell(row, col++).Value = d.CodUnidad3;
                // 29-Unidad4
                ws.Cell(row, col++).Value = d.CodUnidad4;
                // 30-Moneda
                ws.Cell(row, col++).Value = d.Moneda;
                // 31-Impo compra
                SetMonto(ws.Cell(row, col++), d.ImpoCompra);
                // 32-Flete (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 33-Imp (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 34-Corretaje (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 35-Seguro (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 36-Flete local (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 37-Cargos varios (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 38-Sales Tax (fijo)
                SetMonto(ws.Cell(row, col++), 0);
                // 39-IGV
                SetMonto(ws.Cell(row, col++), d.IGV);
                // 40-Mnto fctura
                SetMonto(ws.Cell(row, col++), d.MntoFactura);
                // 41-Total dist
                SetMonto(ws.Cell(row, col++), d.TotalDistribucion);
                // 42-Is Third Party? (1 solo en línea principal)
                ws.Cell(row, col++).Value = d.EsLineaPrincipal ? 1 : 0;
                // 43-Nº proveedor
                ws.Cell(row, col++).Value = d.NroProveedor;
                // 44-Nombre prov
                ws.Cell(row, col++).Value = d.NombreProv;
                // 45-Núm reg fiscal
                ws.Cell(row, col++).Value = d.NumRegFiscal;
                // 46-Dígito cheque (fijo)
                ws.Cell(row, col++).Value = "";
                // 47-Nacional (fijo)
                ws.Cell(row, col++).Value = "";
                // 48-Doc.Soporte (fijo)
                ws.Cell(row, col++).Value = "";
                // 49-Tipo doc (SUNAT)
                ws.Cell(row, col++).Value = d.TipoDoc;
                // 50-64: campos fijos vacíos
                for (int f = 50; f <= 64; f++)
                    ws.Cell(row, col++).Value = "";

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor =
                        XLColor.FromHtml("#EBF2FA");

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ── Helper formato monto ──────────────────
        private static void SetMonto(IXLCell cell, decimal valor)
        {
            cell.Value = valor;
            cell.Style.NumberFormat.Format =
                "_(* #,##0.00_);" +
                "_(* (#,##0.00);" +
                "_(* \"-\"??_);" +
                "_(@_)";
            cell.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Right;
        }
    }
}
