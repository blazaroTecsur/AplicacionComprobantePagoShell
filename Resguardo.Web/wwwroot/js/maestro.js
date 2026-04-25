var fncMaestro = {
    tablaCapataz: null,
    init: function () {
        
        $("#capataz_btnBuscar").click(function (e) {
            e.preventDefault();
            fncMaestro.buscarCapataces();
        });
        fncMaestro.crearTabla();

        $("#modalCapataz").modal("show");
        $("#modalCapataz").on("hidden.bs.modal", function () {
            if (fncRegistro.tablaCapataz !== null) {
                fncRegistro.tablaCapataz.destroy();
                fncRegistro.tablaCapataz = null;
            }
        });
    },
    crearTabla: function () {

        fncRegistro.tablaCapataz = CorporativoGrid.crear({
            element: "#tablaCapataces",
            url: BASE_URL + "/SolicitudVisualizar/ConsultarCapataces",
            pageSize: 10,            
            filtros: function () {
                return {
                    Filtro: $("#capataz_txtFiltro").val() || null,                    
                };
            },
            columns: [
                {
                    title: "", width: 50, hozAlign: "center",
                    formatter: function () {
                        return "<button class='btn btn-sm btn-primary'><i class='bi bi-check2-circle'></i></button>";
                    },
                    cellClick: function (e, cell) {
                        const row = cell.getRow();                        
                        $("#txtCodCapataz").val(row._row.data.codCapataz);
                        $("#txtNomCapataz").val(row._row.data.nomCapataz);    
                        $("#modalCapataz").modal("hide");
                    }
                },
                { title: "Cód.", field: "codCapataz", width: 100 },
                { title: "Dni", field: "dniCapataz", width: 100 },
                { title: "Nombres", field: "nomCapataz", width: 200 }
            ]
        });
    },
    buscarCapataces: function () {
        fncMaestro.tablaCapataz.buscar();
    }
}