$(document).ready(function () {
    fncConsulta.init();
});

var fncConsulta = {
    tablaServicio: null,
    init: function () {

        CorporativoSelect.cargar({
            url: BASE_URL + "/ServicioVisualizar/ListarGenerico",
            filtro: 'tipo',
            list: [
                { element: "#consulta_cboProveedor", id: "id", text: "descripcion", type: 'PROVEEDOR' },
                { element: "#consulta_cboServicio", id: "id", text: "descripcion", type: 'SERVICIO' }
            ]
        });
        $("#consulta_btnBuscar").click(function (e) {
            e.preventDefault();
            fncConsulta.buscarServicios();
        });
        $("#consulta_btnReporte").click(function () {
            fncConsulta.abrirReporte();
        });

        fncConsulta.crearTabla();
    },
    abrirReporte: function () {
        $("#modalContainer").load(BASE_URL + "/Reporte/Filtros", function () {
            fncReporte.tipo = "EF";
            fncReporte.servicio = $("#consulta_cboServicio option");
            fncReporte.init();
        });
    },
    buscarServicios: function () {
        fncConsulta.tablaServicio.buscar();
    },
    ejecutarAccion: function (pos, accion) {
        
        $("#modalContainer").load(BASE_URL + "/ServicioVisualizar/Efectivo", function () {
            fncEfectivo.posServicio = pos;
            fncEfectivo.accion = accion;            
            fncEfectivo.init();
        });
    },    
    crearTabla: function () {

        fncConsulta.tablaServicio = CorporativoGrid.crear({
            element: "#consulta_tablaServicios",
            height: $(window).height() * 0.7,
            url: BASE_URL + "/ServicioVisualizar/ConsultarServicios",
            pageSize: 10,
            filtros: function () {
                return {
                    IdProveedor: parseInt($("#consulta_cboProveedor").val()) || null,
                    IdTpoServicio: parseInt($("#consulta_cboServicio").val()) || null,
                    Folio: $("#consulta_txtFolio").val() || null,                    
                    FechaIni: $("#consulta_txtFechaIni").val() || null,
                    FechaFin: $("#consulta_txtFechaFin").val() || null
                };
            },
            columns: [
                {                    
                    title: "", width: 150, hozAlign: "center",
                    formatter: function (cell) {                        
                        var acciones = "";
                        var pos = cell.getRow().getPosition();
                        
                        if (cell._cell.row.data.asignar && CorporativoCore.tienePermiso(window.ROLES, ["SERV.ASIGNAR"])) {                            
                            acciones = acciones + "<button class='btn btn-sm btn-success' title='Asignar efectivos' onclick=\"fncConsulta.ejecutarAccion(" + pos + ", 'AS')\"><i class='bi bi-person-plus'></i></button>";
                        }
                        if (cell._cell.row.data.ampliar && CorporativoCore.tienePermiso(window.ROLES, ["SERV.AMPLIAR"])) {
                            acciones = acciones + "<button class='btn btn-sm btn-secondary' title='Ampliar horario' onclick=\"fncConsulta.ejecutarAccion(" + pos + ", 'AH')\"><i class='bi bi-clock-history'></i></button>&nbsp;&nbsp;";
                        }
                        if (cell._cell.row.data.aprobar && CorporativoCore.tienePermiso(window.ROLES, ["SERV.APROBAMP"])) {
                            acciones = acciones + "<button class='btn btn-sm btn-warning' title='Aprobar amplicación' onclick=\"fncConsulta.ejecutarAccion(" + pos + ", 'AA')\"><i class='bi bi-calendar-check'></i></button>&nbsp;&nbsp;";
                        }
                        if (cell._cell.row.data.cerrar && CorporativoCore.tienePermiso(window.ROLES, ["SERV.CERRAR"])) {
                            acciones = acciones + "<button class='btn btn-sm btn-success' title='Cerrar atención' onclick=\"fncConsulta.ejecutarAccion(" + pos + ", 'CE')\"><i class='bi bi-hand-thumbs-up'></i></button>&nbsp;&nbsp;";
                        }                        
                        
                        return acciones;
                    }
                },
                { title: "Id", field: "id", visible: false },
                { title: "Proveedor", field: "proveedor", width: 200 },
                {
                    title: "Folio", field: "folio", width: 110, formatter: function (cell) {
                        let value = cell.getValue();
                        var pos = cell.getRow().getPosition();                        
                        return "<a title='Ver atención' onclick=\"fncConsulta.ejecutarAccion(" + pos + ", 'VA'); return false;\" href='#'>" + value + "</a>";
                    }
                },
                { title: "Servicio", field: "tpoServicio", width: 200 },
                {
                    title: "Fecha", field: "fecha", width: 150, formatter: function (cell) {
                        let value = cell.getValue();
                        if (!value) return "";
                        let fecha = CorporativoCore.formatearFecha(value);
                        return fecha;
                    }
                },
                { title: "Estado", field: "estado", width: 100 },
                { title: "Hra. Inicio", field: "hraInicio", width: 150 },
                { title: "Hra. Final", field: "hraFinal", width: 150 },
                { title: "Cantidad", field: "cantidad", width: 120 },
                {
                    title: "Dirección", field: "direccion", width: 500, formatter: function (cell) {
                        let value = cell.getValue();
                        var coordenada = cell._cell.row.data.coordenada;
                        return "<a href='#' onclick='CorporativoCore.abrirGoogleMaps(\"" + coordenada + "\"); return false;'><i class='bi bi-geo-alt-fill text-danger'></i></a> " + value;
                    }
                },
                { title: "Coordenada", field: "coordenada", visible: false }
            ]
        });
    }
};