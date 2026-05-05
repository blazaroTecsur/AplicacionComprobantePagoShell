var fncReporte = {
    tipo: null,
    flujo: null,
    servicio: null,
    init: function () {
        $("#reporte_btnGenerar").click(function () {
            fncReporte.generar();
        });

        if (fncReporte.flujo != null) {
            $("#reporte_cboFlujo").append(fncReporte.flujo.clone());
            $(".reporte_cboFlujo").removeAttr('hidden');
        }
        if (fncReporte.servicio != null) {
            $("#reporte_cboServicio").append(fncReporte.servicio.clone());
            $(".reporte_cboServicio").removeAttr('hidden');
        }

        $("#modalFiltro").modal("show");
    },
    generar: function () {

        var fechaDsd = $("#reporte_txtFechaDsd").val();
        var fechaHst = $("#reporte_txtFechaHst").val();

        if (fechaDsd == null || fechaDsd == "" || fechaHst == null || fechaHst == "") {
            CorporativoCore.mostrarToast('Debe seleccionar ambas fechas', 'error');
            return;
        }

        var url = '#';
        var flujo = $("#reporte_cboFlujo").val() == "0" ? null : $("#reporte_cboFlujo").val();
        var servicio = $("#reporte_cboServicio").val() == "0" ? null : $("#reporte_cboServicio").val();

        if (fncReporte.tipo == "SO") {
            url = '/Reporte/ReporteSolicitud?fechaDsd=' + fechaDsd + '&fechaHst=' + fechaHst + '&flujo=' + flujo;
        } else if (fncReporte.tipo == "EF") {
            url = '/Reporte/ReporteEfectivo?fechaDsd=' + fechaDsd + '&fechaHst=' + fechaHst + '&servicio=' + servicio;
        }
        
        CorporativoFile.submit({
            url: BASE_URL + url,   
            file: "reporte.xls",
            button: "#reporte_btnGenerar",
            //success: function (data) {
            //    let blob = new Blob([data]);
            //    let link = document.createElement('a');
            //    link.href = window.URL.createObjectURL(blob);
            //    //link.download = "usuarios.xlsx";
            //    link.click();
            //}
        });

    }
};