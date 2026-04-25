var fncConfig = {    
    init: function () {        
        $("#config_btnGrabar").click(function () {
            fncConfig.copiarConfig();
        });     
        $("#modalCopia").modal("show");
    },    
    copiarConfig: function () {

        var fechaOrigen = $("#config_txtFechaOrigen").val();
        var fechaDestino = $("#config_txtFechaDestino").val();

        if (fechaOrigen == "") {
            CorporativoCore.mostrarToast('Debe seleccionar la fecha de origen', 'error');
            return;
        } else if (fechaDestino == "") {
            CorporativoCore.mostrarToast('Debe seleccionar la fecha de destino', 'error');
            return;
        } else if (fechaOrigen == fechaDestino) {
            CorporativoCore.mostrarToast('Las fechas no deben ser iguales', 'error');
            return;
        }

        var formulario = {
            fechaOrigen: fechaOrigen,
            fechaDestino: fechaDestino
        };

        CorporativoQuery.submit({
            url: BASE_URL + "/ConfigGestionar/CopiarConfig",
            type: 'POST',
            data: formulario,
            success: function (response) {
                CorporativoCore.mostrarToast("La operación se realizó con éxito.", "success");
                $("#modalCopia").modal("hide");
                $("#consulta_txtFecha").val(fechaDestino);
                $("#consulta_btnBuscar").click();
            }
        });
    }
}