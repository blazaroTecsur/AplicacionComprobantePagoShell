$(document).ready(function () {
    fncConsulta.init();
});

var fncConsulta = {
    tablaConfig: null,
    fechaSelect: null,
    init: function () {

        CorporativoSelect.cargar({
            url: BASE_URL + "/LimiteVisualizar/ListarDepartamento",
            filtro: 'tipo',
            list: [
                { element: "#consulta_cboServicio", id: "codigo", text: "nombre" }
            ]
        });
        $("#consulta_btnBuscar").click(function (e) {
            e.preventDefault();
            fncConsulta.buscarConfiguracion();
        });
        $("#consulta_btnCopiar").click(function (e) {      
            $("#modalContainer").load(BASE_URL + "/LimiteGestionar/Copiar", function () {
                fncConfig.init();
            });            
        });
        $("#consulta_btnGrabar").click(function (e) {
            e.preventDefault();
            fncConsulta.grabarConfiguracion();
        });

        fncConsulta.crearTabla();
    },
    crearTabla: function () {

        fncConsulta.tablaConfig = new Tabulator("#consulta_tablaConfiguracion", {
            height: "650px",
            headerSortElement: null,
            columnCalcs: "both",
            columns: [
                { title: "Cod.", field: "codDpto", width: 150 },
                { title: "Departamento", field: "nomDpto", width: 350 },
                { title: "IdTpoServicio", field: "idTpoServicio", visible: false },
                { title: "Servicio", field: "tpoServicio", width: 250 },
                { title: "Diurno", field: "diurno", width: 150, editor: 'number', bottomCalc: "sum" },
                { title: "Nocturno", field: "nocturno", width: 150, editor: 'number', bottomCalc: "sum" }
            ]
        });
    },
    buscarConfiguracion: function () {

        var fecha = $("#consulta_txtFecha").val();
        var dpto = $("#consulta_cboServicio").val();

        if (fecha == "") {
            CorporativoCore.mostrarToast('Debe seleccionar la fecha', 'error');
            return;
        }

        fncConsulta.fechaSelect = fecha;

        CorporativoQuery.submit({
            url: BASE_URL + "/LimiteVisualizar/ListarConfiguracion?fecha=" + fecha + "&dpto=" + dpto,
            success: function (response) {
                if (response) {
                    var data = response;
                    fncConsulta.tablaConfig.setData(data);
                }
            }
        });
    },
    grabarConfiguracion: function () {
        
        var data = fncConsulta.tablaConfig.getData();

        if (fncConsulta.fechaSelect == null || fncConsulta.fechaSelect == "") {
            CorporativoCore.mostrarToast("Debe seleccionar la fecha", "error");
            return;
        }
        else if (data == null || data.lenght == 0) {
            CorporativoCore.mostrarToast("Debe haber al menos un ítem", "error");
            return;
        }

        var formulario = {
            fecha: fncConsulta.fechaSelect,
            configs: data
        };

        CorporativoQuery.submit({
            url: BASE_URL + "/LimiteGestionar/RegistrarConfig",
            type: 'POST',
            data: formulario,
            success: function (response) {
                CorporativoCore.mostrarToast("La operación se realizó con éxito.", "success");                
                $("#consulta_btnBuscar").click();
            }
        });
    }
};