var fncEfectivo = {
    tablaEfectivo: null,
    idServicioProv: 0,
    accion: null,
    posServicio: null,
    controlarAccion: function () {

        var data = fncConsulta.tablaServicio.getRowFromPosition(fncEfectivo.posServicio).getData();
        var titulo = "";
        console.log(data);
        switch (fncEfectivo.accion) {
            case "AS": titulo = "Asignar Efectivos"; break;
            case "AH": titulo = "Ampliar Horario de Efectivos"; break;
            case "AA":
                titulo = "Aprobar Ampliación de Horario";
                $("#efectivo_btnGrabar").attr('hidden', 'hidden');
                break;
            case "CE": titulo = "Cerrar Atención del Servicio"; break;
            case "VA":
                titulo = "Ver Detalles de la Atención";
                $("#efectivo_btnGrabar").attr('hidden', 'hidden');
                break;
            default:
        }

        $("#efectivo_txtFolio").val(data.folio);
        $("#efectivo_txtProveedor").val(data.proveedor);
        $("#efectivo_txtServicio").val(data.tpoServicio);
        $("#efectivo_txtFecha").val(data.fecha);
        $("#efectivo_txtHraInicio").val(data.hraInicio);
        $("#efectivo_txtHraFinal").val(data.hraFinal);

        fncEfectivo.idServicioProv = data.id;
        fncEfectivo.crearTabla();

        if (fncEfectivo.accion == "AS") {
            setTimeout(function () {
                for (var i = 0; i < data.cantidad; i++) {
                    fncEfectivo.tablaEfectivo.addRow();
                }
            }, 500);
        } else {
            fncEfectivo.listarEfectivos(data.id);
        }

        $("#modalTitulo").html(titulo);
        $("#modalEfectivo").modal("show");
    },
    init: function () {
        $("#efectivo_btnGrabar").click(function () {
            fncEfectivo.grabarEfectivos();
        });
        $("#modalEfectivo").on("hidden.bs.modal", function () {
            if (fncEfectivo.tablaEfectivo !== null) {
                fncEfectivo.tablaEfectivo.destroy();
                fncEfectivo.tablaEfectivo = null;
            }
        });
        fncEfectivo.controlarAccion();
    },
    crearTabla: function () {

        var cellEdited = function (cell) {

            var valor = cell.getValue();
            var tabla = cell.getTable();
            var posicion = cell.getRow().getPosition();

            setTimeout(function () {

                if (!valor) return;

                var filaActual = tabla.getRowFromPosition(posicion);

                if (!CorporativoCore.validarDni(valor)) {
                    CorporativoCore.mostrarToast('El DNI no es válido', 'error');
                    tabla.getEditedCells().forEach(function (c) { c.cancelEdit(); });
                    cell.setValue("", false);
                    filaActual.update({
                        nombres: "",
                        apellidos: "",
                        telefono: ""
                    });
                    return;
                }

                CorporativoQuery.submit({
                    url: BASE_URL + "/ServicioVisualizar/ObtenerPersonal?dni=" + valor,
                    success: function (response) {
                        if (response) {
                            if (response.idPersonal) {
                                tabla.blockRedraw();
                                filaActual.update({
                                    nombres: response.nombres,
                                    apellidos: response.apellidos,
                                    telefono: response.telefono
                                });
                                tabla.restoreRedraw();
                            } else {
                                CorporativoCore.mostrarToast('El DNI no existe. Complete los datos', 'success');
                            }
                        }
                    }
                });

            }, 200);
        };

        var columns = [];
        if (fncEfectivo.accion == "AS") {
            columns =
                [
                    { formatter: "rownum", hozAlign: "center", width: 40 },
                    { title: "DNI", field: "dni", width: 120, editor: true, cellEdited: cellEdited },
                    { title: "Nombres", field: "nombres", width: 200, editor: true },
                    { title: "Apellidos", field: "apellidos", width: 200, editor: true },
                    { title: "Celular", field: "telefono", width: 120, editor: true }
                ];
        } else if (fncEfectivo.accion == "AH") {
            columns =
                [
                    { formatter: "rownum", hozAlign: "center", width: 40 },
                    { title: "DNI", field: "dni", width: 120 },
                    { title: "Nombres", field: "nombres", width: 200 },
                    { title: "Apellidos", field: "apellidos", width: 200 },
                    { title: "Celular", field: "telefono", width: 120 },
                    { title: "H. Ampliada", field: "hraAmplia", width: 120, editor: "time" }
                ];
        } else if (fncEfectivo.accion == "AA") {
            columns =
                [
                    { formatter: "rownum", hozAlign: "center", width: 40 },
                    {
                        title: "", width: 100, hozAlign: "center",
                        formatter: function (cell) {
                            var acciones = "";
                            var pos = cell.getRow().getPosition();                            
                            if (cell._cell.row.data.estAmplia == "P") {
                                acciones = acciones + "<button class='btn btn-sm btn-success' onclick=\"fncEfectivo.aprobarAmpliacion(" + pos + ", 'A')\"><i class='bi bi-check-circle'></i></button>&nbsp;&nbsp;";
                                acciones = acciones + "<button class='btn btn-sm btn-danger' onclick=\"fncEfectivo.aprobarAmpliacion(" + pos + ", 'R')\"><i class='bi bi-x-circle'></i></button>";
                            }
                            return acciones;
                        }
                    },
                    { title: "DNI", field: "dni", width: 120 },
                    { title: "Nombres", field: "nombres", width: 200 },
                    { title: "Apellidos", field: "apellidos", width: 200 },
                    { title: "Celular", field: "telefono", width: 120 },
                    { title: "H. Ampliada", field: "hraAmplia", width: 120 },
                    { title: "Comentario", field: "comentApro", editor: "textarea", width: 300 }
                ];
        } else if (fncEfectivo.accion == "CE") {
            columns =
                [
                    { formatter: "rownum", hozAlign: "center", width: 40 },
                    { title: "IdEfectivo", field: "idEfectivo", visible: false },
                    { title: "DNI", field: "dni", width: 120 },
                    { title: "Nombres", field: "nombres", width: 200 },
                    { title: "Apellidos", field: "apellidos", width: 200 },
                    { title: "Celular", field: "telefono", width: 120 },
                    { title: "H.Ampliada", field: "hraAmplia", width: 120 },
                    { title: "Asistió? (S/N)", field: "asistio", editor: true, width: 150 },
                    { title: "H.Ini.Real", field: "hraInicio", editor: "time", width: 150 },
                    { title: "H.Fin.Real", field: "hraFinal", editor: "time", width: 150 },
                    { title: "SRO Ref.", field: "sroRefencia", editor: true, width: 150 },
                    { title: "Comentario", field: "comentario", editor: "textarea", width: 300 }
                ];
        } else if (fncEfectivo.accion == "VA") {
            columns =
                [
                    { formatter: "rownum", hozAlign: "center", width: 40 },
                    { title: "DNI", field: "dni", width: 120 },
                    { title: "Nombres", field: "nombres", width: 200 },
                    { title: "Apellidos", field: "apellidos", width: 200 },
                    { title: "Celular", field: "telefono", width: 120 },
                    {
                        title: "Asistió?", field: "asistio", width: 150, formatter: function (cell) {
                            let value = cell.getValue();
                            return (value ? "S" : "N");
                        }
                    },
                    { title: "H.Ampliada", field: "hraAmplia", width: 120 },
                    { title: "F.Aprobada", field: "fechaApro", width: 120 },
                    { title: "Aprobador", field: "usuarioApro", width: 120 },
                    { title: "Coment.Aprob", field: "comentApro", width: 300 },
                    { title: "H.Ini.Real", field: "hraInicio", width: 150 },
                    { title: "H.Fin.Real", field: "hraFinal", width: 150 },
                    { title: "SRO Ref.", field: "sroRefencia", width: 150 },
                    { title: "Coment.Cierre", field: "comentario", width: 300 }
                ];
        }

        fncEfectivo.tablaEfectivo = new Tabulator("#tablaEfectivos", {
            height: "300px",
            headerSortElement: null,
            columns: columns
        });
    },
    listarEfectivos: function (id) {

        CorporativoQuery.submit({
            url: BASE_URL + "/ServicioVisualizar/ListarEfectivos?id=" + id,
            success: function (response) {
                if (response) {
                    var data = response;
                    fncEfectivo.tablaEfectivo.setData(data);
                }
            }
        });
    },
    validarEfectivos: function () {

        var valido = true;
        var efectivos = fncEfectivo.tablaEfectivo.getData();
        $.each(efectivos, function (idx, efectivo) {

            if (fncEfectivo.accion == "AS") {
                if ($.trim(efectivo.dni) == "" ||
                    $.trim(efectivo.nombres) == "" ||
                    $.trim(efectivo.apellidos) == "" ||
                    $.trim(efectivo.telefono) == "") {
                    CorporativoCore.mostrarToast('Debe completar todos los datos de los efectivos', 'error');
                    valido = false;
                    return false;
                }
            } else if (fncEfectivo.accion == "CE") {
                if ($.trim(efectivo.asistio) != "S" && $.trim(efectivo.asistio) != "N") {
                    CorporativoCore.mostrarToast('Debe marcar la asistencia ingresando una "S" o "N".', 'error');
                    valido = false;
                    return false;
                } else if ($.trim(efectivo.asistio) == "S" &&
                    ($.trim(efectivo.hraInicio) == "" ||
                        $.trim(efectivo.hraFinal) == "")) {
                    CorporativoCore.mostrarToast('Debe completar las horas reales de atención, solo si el efectivo asistió', 'error');
                    valido = false;
                    return false;
                } else if (!($.trim(efectivo.asistio) == "S" || $.trim(efectivo.asistio) == "N")) {
                    CorporativoCore.mostrarToast('Debe marcar la asistencia de cada uno de los efectivos', 'error');
                    valido = false;
                    return false;
                }
            }
        });
        return valido;
    },
    grabarEfectivos: function () {

        if (!fncEfectivo.validarEfectivos()) {
            return;
        }

        var url = null;
        var tabla = fncEfectivo.tablaEfectivo.getData();
        var efectivos = [];
        if (fncEfectivo.accion == "AS") {
            url = BASE_URL + "/ServicioOperar/AsignarEfectivos";
            $.each(tabla, function (idx, obj) {
                efectivos.push({
                    dni: $.trim(obj.dni),
                    nombres: $.trim(obj.nombres),
                    apellidos: $.trim(obj.apellidos),
                    telefono: $.trim(obj.telefono)
                });
            });
        } else if (fncEfectivo.accion == "AH") {
            url = BASE_URL + "/ServicioOperar/AmpliarServicio";
            $.each(tabla, function (idx, obj) {
                if (obj.hraAmplia != null && $.trim(obj.hraAmplia) != "") {
                    efectivos.push({
                        idEfectivo: obj.idEfectivo,
                        hraAmplia: $.trim(obj.hraAmplia)
                    });
                }
            });
        } else if (fncEfectivo.accion == "CE") {
            url = BASE_URL + "/ServicioOperar/CerrarServicio";
            $.each(tabla, function (idx, obj) {
                efectivos.push({
                    idEfectivo: obj.idEfectivo,
                    asistio: ($.trim(obj.asistio) == "S" ? true : false),
                    hraInicio: $.trim(obj.hraInicio),
                    hraFinal: $.trim(obj.hraFinal),
                    sroRefencia: $.trim(obj.sroRefencia) ?? null,
                    comentario: $.trim(obj.comentario) ?? null
                });
            });
        }

        if (efectivos == null || efectivos.length == 0) {
            CorporativoCore.mostrarToast('Los datos solicitados del efectivo no han sido completados.', 'error');
            return;
        }

        var formulario = {
            idServicioProv: fncEfectivo.idServicioProv,
            efectivos: efectivos
        };

        CorporativoQuery.submit({
            url: url,
            type: 'POST',
            data: formulario,
            success: function (response) {
                CorporativoCore.mostrarToast("La operación se realizó con éxito.", "success");
                $("#modalEfectivo").modal("hide");
                $("#consulta_btnBuscar").click();
            }
        });
    },
    aprobarAmpliacion: function (pos, estado) {

        let confirma = confirm("¿Está seguro de realizar esta operación?");
        if (!confirma) {
            return;
        }
        var row = fncEfectivo.tablaEfectivo.getRowFromPosition(pos);
        var data = row.getData();
        var url = "";
        var formulario = {
            idServicioProv: fncEfectivo.idServicioProv,
            idEfectivo: data.idEfectivo,
            comentApro: $.trim(data.comentApro) ?? null
        };
        if (estado == "A") {
            url = "/ServicioOperar/AprobarAmpliacion";
        } else if (estado == "R") {
            url = "/ServicioOperar/RechazarAmpliacion";
        }
        CorporativoQuery.submit({
            url: BASE_URL + url,
            type: 'POST',
            data: formulario,
            success: function (response) {
                CorporativoCore.mostrarToast("La operación se realizó con éxito.", "success");
                $("#modalEfectivo").modal("hide");
                $("#consulta_btnBuscar").click();
            }
        });
    }
}