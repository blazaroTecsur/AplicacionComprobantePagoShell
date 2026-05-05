var fncSolicitud = {
    tablaServicio: null,
    id: 0,
    accion: null,
    init: function () {

        CorporativoSelect.cargar({
            url: BASE_URL + "/SolicitudVisualizar/ListarGenerico",
            filtro: 'tipo',
            list: [
                { element: "#cboServicio", id: "id", text: "descripcion", type: 'SERVICIO' },
                { element: "#cboFlujo", id: "id", text: "descripcion", type: 'FLUJO' },
                { element: "#cboEstado", id: "id", text: "descripcion", type: 'ESTADO' }
            ]
        });
        $("#btnAgregar").click(function () {
            fncSolicitud.agregarServicio();
        });
        $("#btnBuscarSro").click(function () {
            fncSolicitud.buscarSro();
        });
        $("#btnGrabar").click(function () {
            fncSolicitud.grabarSolicitud();
        });
        $("#btnAprobar").click(function () {
            $("#txtComentario").val("");
            var url = "/SolicitudAprobar/AprobarSolicitud";
            $("#modalTituloAprobar").html("Aprobar:");
            $("#modalAprobar").attr('url', url);
            $("#modalAprobar").modal("show");
        });
        $("#btnAnular").click(function () {
            $("#txtComentario").val("");
            var url = "/SolicitudAprobar/AnularSolicitud";
            $("#modalTituloAprobar").html("Anular:");
            $("#modalAprobar").attr('url', url);
            $("#modalAprobar").modal("show");
        });
        $("#btnAceptar").click(function () {
            if ($.trim($("#txtComentario").val()) == "") {
                CorporativoCore.mostrarToast('Debe ingresar el momentario', 'error');
                return;
            }
            var url = $("#modalAprobar").attr('url');
            fncSolicitud.aprobarSolicitud(url);
        });
        $("#lnkCapataz").click(function () {
            console.log("lnkCapataz");
            $("#modalContainerMaestro").load(BASE_URL + "/SolicitudVisualizar/Capataz", function () {
                fncMaestro.init();
            });
        });
        $("#btnVerMapa").click(function () {
            $("#modalContainerMapa").load(BASE_URL + "/SolicitudVisualizar/Mapa", function () {
                var coord = fncMapa.convert($("#txtCoordenada").attr("def"));
                fncMapa.lat = coord.lat;
                fncMapa.lng = coord.lng;
                fncMapa.init();
                $("#modalMapa").modal("show");
            });
        });

        fncSolicitud.crearTabla();
        fncSolicitud.controlarAccion();
    },
    controlarAccion: function () {

        var titulo = "";
        switch (fncSolicitud.accion) {
            case "N":
                titulo = "Nueva Solicitud";
                $(".div-new").attr('hidden', 'hidden');
                setTimeout(function () { fncSolicitud.tablaServicio.showColumn("eliminar") }, 1000);
                break;
            case "V":
                titulo = "Visualizar Solicitud";
                $(".div-new").removeAttr('hidden');
                $(".div-view").removeAttr('hidden');
                $(".div-servicio").attr('hidden', 'hidden');
                $("#txtCelular").attr('readonly', 'readonly');
                $("#txtTpoTrabajo").attr('readonly', 'readonly');
                $("#btnBuscarSro").attr('hidden', 'hidden');
                $("#btnAgregar").attr('hidden', 'hidden');
                $("#lnkCapataz").attr('hidden', 'hidden');
                $("#btnVerMapa").attr('hidden', 'hidden');
                $("#btnGrabar").attr('hidden', 'hidden');
                $("#cboFlujo").attr('disabled', 'disabled');
                $("#txtNroSro").attr('readonly', 'readonly');
                fncSolicitud.obtenerSolicitud(fncSolicitud.id);
                break;
            case "M":
                titulo = "Modificar Solicitud";
                $(".div-new").removeAttr('hidden');
                $(".div-view").removeAttr('hidden');
                $("#btnBuscarSro").attr('hidden', 'hidden');
                $("#cboFlujo").attr('disabled', 'disabled');
                $("#txtNroSro").attr('readonly', 'readonly');
                setTimeout(function () { fncSolicitud.tablaServicio.showColumn("eliminar") }, 1000);
                fncSolicitud.obtenerSolicitud(fncSolicitud.id);
                break;
            case "E":
                titulo = "Editar Solicitud";
                $(".div-new").removeAttr('hidden');
                $(".div-view").removeAttr('hidden');
                $(".div-servicio").attr('hidden', 'hidden');
                $("#btnBuscarSro").attr('hidden', 'hidden');
                $("#cboFlujo").attr('disabled', 'disabled');
                $("#txtNroSro").attr('readonly', 'readonly');
                fncSolicitud.obtenerSolicitud(fncSolicitud.id);
                break;
            case "A":
                titulo = "Aprobar Solicitud";
                $(".div-check").removeAttr('hidden');
                $(".div-new").removeAttr('hidden');
                $(".div-view").removeAttr('hidden');
                $(".div-servicio").attr('hidden', 'hidden');
                $("#txtCelular").attr('readonly', 'readonly');
                $("#txtTpoTrabajo").attr('readonly', 'readonly');
                $("#btnBuscarSro").attr('hidden', 'hidden');
                $("#lnkCapataz").attr('hidden', 'hidden');
                $("#btnAgregar").attr('hidden', 'hidden');
                $("#btnVerMapa").attr('hidden', 'hidden');
                $("#btnGrabar").attr('hidden', 'hidden');
                $("#cboFlujo").attr('disabled', 'disabled');
                $("#txtNroSro").attr('readonly', 'readonly');
                fncSolicitud.obtenerSolicitud(fncSolicitud.id);
                break;
            default:
        }

        $("#modalTitulo").html(titulo);
        $("#modalSolicitud").modal("show");
    },
    buscarSro: function () {

        var idFlujo = $.trim($("#cboFlujo").val());
        if (idFlujo == null || idFlujo == "") {
            CorporativoCore.mostrarToast("Debe ingresar el flujo", "error");
            return;
        }
        var nroSro = $.trim($("#txtNroSro").val());
        if (nroSro == null || nroSro == "") {
            CorporativoCore.mostrarToast("Debe ingresar el número de SRO", "error");
            return;
        }

        CorporativoQuery.submit({
            url: BASE_URL + '/SolicitudVisualizar/BuscarOrden?nroSro=' + nroSro,
            success: function (response) {

                var data = response;
                $("#txtDpto").val(data.codDpto + " = " + data.nomDpto);
                $('#txtActv').val(data.codActv + " = " + data.nomActv);
                $("#txtSctta").val(data.rucSctta + " = " + data.nomSctta);
                $("#txtCoordenada").val(data.coordenada);
                if (data.coordenada && data.coordenada.indexOf(',') > 0) {
                    $("#txtCoordenada").attr("def", data.coordenada);
                }
                $("#txtDireccion").val(data.direccion);

                $("#cboFlujo").attr('disabled', 'disabled');
                $("#txtNroSro").attr('readonly', 'readonly');
                $("#btnBuscarSro").attr('hidden', 'hidden');
                $(".div-new").removeAttr('hidden');
            }
        });
    },
    obtenerLimite: function (fecha, dpto) {

        CorporativoQuery.submit({
            url: BASE_URL + '/SolicitudAprobar/ObtenerLimite?fecha=' + fecha + "&dpto=" + dpto,
            success: function (response) {

                var data = response;
                var html = "";
                $.each(data, function (idx, obj) {
                    html += "<li><b>" + obj.tpoServicio + "</b>: Turno diurno: " + obj.diurnoConfig + " (" + obj.diurnoSolicitado + ") y nocturno: " + obj.nocturnoConfig + " (" + obj.nocturnoSolicitado + ")</li>";
                });
                $("#ulConfig").html(html);
                $("#cardLimites").removeAttr('hidden');
            }
        });
    },
    crearTabla: function () {

        fncSolicitud.tablaServicio = new Tabulator("#tablaServicios", {
            height: "200px",
            headerSortElement: null,
            columns: [
                { title: "Id", field: "id", visible: false },
                {
                    title: "", field: "eliminar", width: 50, hozAlign: "center", visible: false,
                    formatter: function () {
                        return "<button class='btn btn-sm btn-danger'><i class='bi bi-trash'></i></button>";
                    },
                    cellClick: function (e, cell) {
                        const row = cell.getRow();
                        if (confirm("¿Desea eliminar este servicio?")) {
                            row.delete();
                        }
                    }
                },
                { title: "IdTpoServicio", field: "idTpoServicio", width: 50, visible: false },
                { title: "Servicio", field: "tpoServicio", width: 200 },
                {
                    title: "Fecha", field: "fecha", width: 150, formatter: function (cell) {
                        let value = cell.getValue();
                        if (!value) return "";
                        let fecha = CorporativoCore.formatearFecha(value);
                        return fecha;
                    }
                },
                { title: "Hra. Inicio", field: "hraInicio", width: 150, editor: (fncSolicitud.accion == "E" ? 'time' : false) },
                { title: "Hra. Final", field: "hraFinal", width: 150, editor: (fncSolicitud.accion == "E" ? 'time' : false) },
                {
                    title: "Dia Sig.", field: "diaSig", width: 100, formatter: function (cell) {
                        let value = cell.getValue();
                        var día;
                        if (!value) día = "N"; else día = "S";
                        return día;
                    }
                },
                { title: "Cantidad", field: "cantidad", width: 150, editor: (fncSolicitud.accion == "A" ? 'number' : false) },
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
    },
    limpiarServicio: function () {

        $("#cboServicio").val("");
        $("#txtFecha").val("");
        $("#txtHraInicio").val("");
        $("#txtHraFin").val("");
        $("#chkDiaSig").prop('checked', false);
        $("#txtDireccion").val("");
        $("#txtCoordenada").val("");
        $("#txtCantidad").val("");
    },
    obtenerSolicitud: function (id) {

        CorporativoQuery.submit({
            url: BASE_URL + "/SolicitudVisualizar/ObtenerSolicitud?id=" + id,
            success: function (response) {
                if (response) {
                    var data = response;
                    $("#hdId").val(data.id);
                    $("#cboFlujo").val(data.idFlujo);
                    $("#txtNroSro").val(data.numSro);
                    $("#cboEstado").val(data.idEstado);
                    $("#txtFolio").val(data.folio);
                    $('#txtDpto').val(data.codDpto + " = " + data.nomDpto);
                    $('#txtActv').val(data.codActv + " = " + data.nomActv);
                    $("#txtSctta").val(data.rucSctta + " = " + data.nomSctta);
                    $("#txtCodCapataz").val(data.codCapataz);
                    $("#txtNomCapataz").val(data.nomCapataz);
                    $("#txtCelular").val(data.celular);
                    $("#txtTpoTrabajo").val(data.tpoTrabajo);
                    $("#txtCoordenada").attr('def', data.coordenada);
                    $("#txtTotHrasSolicitud").val(data.hrasSolicitadas);
                    $("#txtTotHrasAprobadas").val(data.hrasAprobadas);
                    fncSolicitud.tablaServicio.setData(data.servicios);
                    if (fncSolicitud.accion == "A") {
                        fncSolicitud.obtenerLimite(data.servicios[0].fecha, data.codDpto);
                    }
                }
            }
        });
    },
    agregarServicio: function () {

        if (!CorporativoValidator.validarFormulario("#frmServicio")) {
            return;
        }

        var servicio = {
            idTpoServicio: $("#cboServicio").val(),
            tpoServicio: $("#cboServicio option:selected").text(),
            fecha: $("#txtFecha").val(),
            hraInicio: $("#txtHraInicio").val(),
            hraFinal: $("#txtHraFin").val(),
            diaSig: $("#chkDiaSig").prop('checked'),
            direccion: $("#txtDireccion").val(),
            coordenada: $("#txtCoordenada").val(),
            cantidad: $("#txtCantidad").val(),
        };
        fncSolicitud.tablaServicio.addRow(servicio);
        fncSolicitud.limpiarServicio();
    },
    grabarSolicitud: function () {

        if (!CorporativoValidator.validarFormulario("#frmSolicitud")) {
            return;
        }
        else if (!fncSolicitud.tablaServicio || fncSolicitud.tablaServicio.getData() == null || fncSolicitud.tablaServicio.getData().length == 0) {
            CorporativoCore.mostrarToast("Debe ingresar al menos un servicio", "error");
            return;
        }

        var url = "";
        if (fncSolicitud.accion == "N") {
            url = "/SolicitudGestionar/RegistrarSolicitud";
        } else if (fncSolicitud.accion == "M") {
            url = "/SolicitudGestionar/ActualizarSolicitud";
        } else if (fncSolicitud.accion == "E") {
            url = "/SolicitudOperar/EditarSolicitud";
        }

        CorporativoForm.submit({
            url: BASE_URL + url,
            form: "#frmSolicitud",
            button: "#btnGrabar",
            tablas: [{ nombre: "servicios", tabla: fncSolicitud.tablaServicio }],
            beforeSend: function (payload) {
                var resultado = {
                    ...payload.formulario,
                    servicios: payload.servicios
                };
                delete payload.solicitud;
                delete payload.servicios;
                Object.assign(payload, resultado);
            },
            success: function (response) {
                var mensaje = "La operación se realizó con éxito. ";
                if (fncSolicitud.accion == "N") {
                    mensaje = mensaje + "Se ha generado el folio " + response.folio;
                }
                CorporativoCore.mostrarToast(mensaje, "success")
                $("#hdId").val("0");
                $("#modalSolicitud").modal("hide");
                $("#consulta_btnBuscar").click();
            }
        });
    },
    aprobarSolicitud: function (action) {

        var tabla = fncSolicitud.tablaServicio.getData();
        var servicios = [];
        $.each(tabla, function (idx, obj) {
            servicios.push({ id: obj.id, cantidad: obj.cantidad });
        });

        CorporativoQuery.submit({
            url: BASE_URL + action,
            type: 'POST',
            data: {
                id: $("#hdId").val(),
                comentario: $("#txtComentario").val(),
                servicios: servicios
            },
            success: function (response) {
                CorporativoCore.mostrarToast("La operación se realizó con éxito.", "success");
                $("#hdId").val("0");
                $("#modalAprobar").modal("hide");
                $("#modalSolicitud").modal("hide");
                $("#consulta_btnBuscar").click();
            }
        });
    }
};