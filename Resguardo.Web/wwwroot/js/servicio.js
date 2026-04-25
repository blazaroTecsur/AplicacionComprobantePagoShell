var fncServicio = {
    tablaServicio: null,
    idSolicitud: 0,
    init: function () {

        $("#servicio_btnGrabar").click(function () {
            fncServicio.confirmarServicios();
        });

        fncServicio.crearTabla();
        fncServicio.listarServicios();
        $("#modalTitulo").html("Servicios");
        $("#modalServicio").modal("show");
        $("#modalServicio").on("hidden.bs.modal", function () {
            if (fncServicio.tablaServicio !== null) {
                fncServicio.tablaServicio.destroy();
                fncServicio.tablaServicio = null;
            }
        });
    },
    crearTabla: function () {

        fncServicio.tablaServicio = new Tabulator("#tablaServicios", {
            height: "600px",
            headerSortElement: null,
            columns: [
                { title: "Id", field: "id", visible: false },
                { title: "Servicio", field: "tpoServicio", width: 200 },
                {
                    title: "Fecha", field: "fecha", width: 150, formatter: function (cell) {
                        let value = cell.getValue();
                        if (!value) return "";
                        let fecha = CorporativoCore.formatearFecha(value);
                        return fecha;
                    }
                },
                { title: "Hra. Inicio", field: "hraInicio", width: 150 },
                { title: "Hra. Final", field: "hraFinal", width: 150 },
                { title: "Cantidad", field: "cantidad", width: 150 },
                {
                    title: "Dirección", field: "direccion", width: 500, formatter: function (cell) {
                        let value = cell.getValue();
                        var coordenada = cell._cell.row.data.coordenada;
                        return "<a href='#' onclick='CorporativoCore.abrirGoogleMaps(\"" + coordenada + "\")'><i class='bi bi-geo-alt-fill text-danger'></i></a> " + value;
                    }
                },
                { title: "Coordenada", field: "coordenada", visible: false }
            ],
            rowFormatter: function (row) {

                var holderEl = document.createElement("div");
                var tableEl = document.createElement("div");

                holderEl.style.boxSizing = "border-box";
                holderEl.style.padding = "10px 30px 10px 10px";
                holderEl.style.borderTop = "1px solid #333";
                holderEl.style.borderBotom = "1px solid #333";

                tableEl.style.border = "1px solid #333";

                holderEl.appendChild(tableEl);

                row.getElement().appendChild(holderEl);

                var subTable = new Tabulator(tableEl, {
                    layout: "fitColumns",
                    data: row.getData().servicioProvs,
                    columns: [
                        { title: "Proveedor", field: "proveedor", width: 250 },
                        { title: "Cantidad", field: "cantidad", width: 150, editor: "input", editor: true, validator: ["min:0", "max:100", "numeric"] },
                        { title: "Id", field: "id", visible: false },
                        { title: "IdServicio", field: "idServicio", visible: false },
                        { title: "IdProveedor", field: "idProveedor", visible: false }
                    ]
                })
            }
        });
    },
    listarServicios: function () {

        CorporativoQuery.submit({
            url: BASE_URL + "/SolicitudVisualizar/ListarServicios?id=" + fncServicio.idSolicitud,
            success: function (response) {
                if (response) {
                    var data = response;
                    console.log(data);
                    fncServicio.tablaServicio.setData(data);
                }
            }
        });
    },
    confirmarServicios: function () {

        var servicios = fncServicio.tablaServicio.getData();
        var cantAcu = 0, cantSer = 0, cantProv = 0;
        var provs = { idSolicitud: fncServicio.idSolicitud, servicios: [] };
        var error = false;
        $.each(servicios, function (idxs, servicio) {
            cantAcu = 0;
            cantSer = servicio.cantidad;
            $.each(servicio.servicioProvs, function (idxc, ctta) {                
                cantProv = parseInt(ctta.cantidad);
                if ($.isNumeric(cantProv) && cantProv > 0) {                    
                    cantAcu = cantAcu + cantProv;
                    provs.servicios.push(ctta);
                }
            });            
            if (cantSer != cantAcu) {
                CorporativoCore.mostrarToast("La cantidad imputada de efectivos por proveedor difiere de la cantidad solicitada.", "error");
                error = true;
                return false;
            }
        });        
        if (!error) {
            CorporativoQuery.submit({
                url: BASE_URL + '/SolicitudOperar/ConfirmarServicios',
                type: 'POST',
                data: provs,
                success: function (response) {
                    CorporativoCore.mostrarToast("La operación se realizó con éxito.", "success");
                    $("#modalServicio").modal("hide");
                    $("#consulta_btnBuscar").click();
                }
            });
        }
    }
}