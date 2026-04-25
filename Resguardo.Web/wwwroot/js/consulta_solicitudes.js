$(document).ready(function () {
    fncConsulta.init();
});

var fncConsulta = {
    tablaSolicitud: null,
    init: function () {

        CorporativoSelect.cargar({
            url: BASE_URL + "/SolicitudVisualizar/ListarGenerico",
            filtro: 'tipo',
            list: [
                { element: "#consulta_cboTipo", id: "id", text: "descripcion", type: 'TIPO' },
                { element: "#consulta_cboFlujo", id: "id", text: "descripcion", type: 'FLUJO' },
                { element: "#consulta_cboEstado", id: "id", text: "descripcion", type: 'ESTADO' }
            ]
        });
        $("#consulta_btnBuscar").click(function (e) {
            e.preventDefault();
            fncConsulta.buscarSolicitudes();
        });
        $("#consulta_btnNuevo").click(function () {
            fncConsulta.nuevaSolicitud();
        });

        fncConsulta.crearTabla();        
    },
    buscarSolicitudes: function () {
        fncConsulta.tablaSolicitud.buscar();
    },
    nuevaSolicitud: function () {
        $("#modalContainer").load(BASE_URL + "/SolicitudVisualizar/Formulario", function () {
            fncRegistro.id = 0;
            fncRegistro.accion = "N";
            fncRegistro.init();
        });
    },
    abrirFormulario: function (id, accion) {
        $("#modalContainer").load(BASE_URL + "/SolicitudVisualizar/Formulario", function () {
            fncRegistro.id = id;
            fncRegistro.accion = accion;
            fncRegistro.init();
        });
    },
    abrirServicios: function (id, accion) {
        $("#modalContainer").load(BASE_URL + "/SolicitudVisualizar/Servicio", function () {
            fncServicio.idSolicitud = id;            
            fncServicio.init();
        });
    },
    crearTabla: function () {

        fncConsulta.tablaSolicitud = CorporativoGrid.crear({
            element: "#consulta_tablaSolicitudes",
            height: $(window).height() * 0.7,
            url: BASE_URL + "/SolicitudVisualizar/ConsultarSolicitudes",
            pageSize: 10,
            filtros: function () {
                return {
                    Folio: $("#consulta_txtFolio").val() || null,
                    IdTipo: parseInt($("#consulta_cboTipo").val()) || null,
                    IdEstado: parseInt($("#consulta_cboEstado").val()) || null,
                    IdFlujo: parseInt($("#consulta_cboFlujo").val()) || null,
                    NumSro: $("#consulta_txtNroSro").val() || null
                };
            },
            columns: [
                {
                    title: "", width: 120, hozAlign: "center",
                    formatter: function (cell) {

                        var id = cell._cell.row.data.id;
                        var acciones = "";
                        if (cell._cell.row.data.modifica == true && CorporativoCore.tienePermiso(window.ROLES, ["SOLC.MODIF"])) {
                            acciones = acciones + "&nbsp;&nbsp;";
                            acciones = acciones + "<button class='btn btn-sm btn-warning' onclick='fncConsulta.abrirFormulario(" + id + ", \"M\")'><i class='bi bi-pencil'></i></button>";
                        }
                        if (cell._cell.row.data.edita == true && CorporativoCore.tienePermiso(window.ROLES, ["SOLC.EDITAR"])) {
                            acciones = acciones + "&nbsp;&nbsp;";
                            acciones = acciones + "<button class='btn btn-sm btn-warning' onclick='fncConsulta.abrirFormulario(" + id + ", \"E\")'><i class='bi bi-pencil'></i></button>";
                        }
                        if (cell._cell.row.data.aprueba == true && CorporativoCore.tienePermiso(window.ROLES, ["SOLC.APROBFULL", "SOLC.APROBAR"])) {
                            acciones = acciones + "&nbsp;&nbsp;";
                            acciones = acciones + "<button class='btn btn-sm btn-secondary' onclick='fncConsulta.abrirFormulario(" + id + ", \"A\")'><i class='bi bi-hand-thumbs-up'></i></button>";
                        }
                        if (cell._cell.row.data.confirma == true && CorporativoCore.tienePermiso(window.ROLES, ["SOLC.CONFIRMAR"])) {
                            acciones = acciones + "&nbsp;&nbsp;";
                            acciones = acciones + "<button class='btn btn-sm btn-primary' onclick='fncConsulta.abrirServicios(" + id + ", \"C\")'><i class='bi bi-check-circle'></i></button>";
                        }
                            return acciones;
                    }
                },
                { title: "Tipo", field: "tipo", width: 120 },
                { title: "Flujo", field: "flujo", width: 100 },
                {
                    title: "Folio", field: "folio", width: 100, formatter: function (cell) {
                        let value = cell.getValue();
                        var id = cell._cell.row.data.id;
                        return "<a href='#' onclick='fncConsulta.abrirFormulario(" + id + ", \"V\"); return false'>" + value + "</a>";
                    }
                },
                { title: "SRO", field: "numSro", width: 100 },
                { title: "Subcontratista", field: "sctta", width: 350 },
                { title: "Estado", field: "estado", width: 150 },
                { title: "Folio Ref.", field: "folioRef", width: 150 },
                { title: "Solicitante", field: "usuarioReg", width: 200 },
                {
                    title: "Fec. Registro", field: "fechaReg", width: 180, formatter: function (cell) {
                        let value = cell.getValue();
                        if (!value) return "";
                        let fecha = CorporativoCore.formatearFecha(value);
                        return fecha;
                    }
                }
            ]
        });
    }
};