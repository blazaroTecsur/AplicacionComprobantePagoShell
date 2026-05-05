// ============================================
// INDEX.JS - Consulta de Comprobantes
// ============================================

let tablaComprobantes;

$(document).ready(function () {
    if ($('#index_tblComprobantes').length > 0) {
        inicializarTabla();
        cargarTipos();
        cargarEstados();
        bindEventos();
        buscar();
    }
});

// ── Inicializar DataTable ─────────────────────
function inicializarTabla() {
    if ($.fn.DataTable.isDataTable('#index_tblComprobantes')) {
        $('#index_tblComprobantes').DataTable().destroy();
    }

    tablaComprobantes = $('#index_tblComprobantes').DataTable({
        language: { url: '/lib/datatables.net/i18n/es-ES.json' },
        searching: false,
        ordering: true,
        columns: [
            // ── Checkbox ──────────────────────
            {
                data: null,
                orderable: false,
                width: '30px',
                render: function (data, type, row) {
                    return `
                        <input type="checkbox"
                               class="chkComprobante form-check-input"
                               data-folio="${row.folio}"
                               data-estado="${row.estado}" />`;
                }
            },
            { data: 'folio' },
            { data: 'tipoComprobante' },
            { data: 'serie' },
            { data: 'numero' },
            { data: 'proveedor' },
            { data: 'fecha' },
            { data: 'moneda' },
            {
                data: 'montoTotal',
                render: d => CorporativoCore.formatearMonto(d)
            },
            {
                data: 'estado',
                render: d => renderEstado(d)
            },
            {
                data: 'voucherSyteline',
                render: d => d ? `<span class="badge bg-dark">${d}</span>` : ''
            },
            {
                data: 'folio',
                orderable: false,
                width: '60px',
                render: function (folio) {
                    return `
                        <button class="btn btn-sm btn-primary btn-ver"
                                data-folio="${folio}"
                                title="Ver detalle">
                            <i class="bi bi-eye"></i>
                        </button>`;
                }
            }
        ]
    });
}

// ── Cargar combos ─────────────────────────────
function cargarTipos() {
    CorporativoQuery.ajaxGet(
        '/Comprobante/ObtenerTiposDocumento',
        function (data) {
            let options = '<option value="">-- Todos --</option>';
            data.forEach(t =>
                options += `<option value="${t.codigo}">
                    ${t.descripcion}</option>`);
            $('#index_cboTipo').html(options);
        });
}

function cargarEstados() {
    CorporativoQuery.ajaxGet(
        '/Comprobante/ObtenerEstados',
        function (data) {
            let options = '<option value="">-- Todos --</option>';
            data.forEach(e =>
                options += `<option value="${e.codigo}">
                    ${e.descripcion}</option>`);
            $('#index_cboEstado').html(options);
        });
}

// ── Buscar comprobantes ───────────────────────
function buscar() {
    const filtros = {
        tipo: $('#index_cboTipo').val(),
        estado: $('#index_cboEstado').val(),
        proveedor: $('#index_txtProveedor').val(),
        folio: $('#index_txtFolio').val()
    };

    CorporativoQuery.ajaxPost(
        '/Comprobante/Buscar',
        filtros,
        function (data) {
            tablaComprobantes.clear().rows.add(data).draw();
            // Resetear checkboxes al recargar
            $('#chkTodos').prop('checked', false);
            actualizarBotonesExportar();
        }
    );
}

// ── Render estado (badge) ─────────────────────
function renderEstado(estado) {
    const colores = {
        'REGISTRADO': 'secondary',
        'ENVIADO': 'primary',
        'AUTORIZADO': 'info',
        'APROBADO': 'success',
        'ANULADO': 'danger',
        'DERIVADO': 'warning',
        'DERIVADO SYT': 'dark'
    };
    const color = colores[estado] || 'secondary';
    return `<span class="badge bg-${color}">${estado}</span>`;
}

// ── Seleccionar / deseleccionar todos ─────────
function bindChkTodos() {
    $('#chkTodos').off('change').on('change', function () {
        const checked = $(this).is(':checked');
        $('.chkComprobante').prop('checked', checked);
        actualizarBotonesExportar();
    });
}

// ── Actualizar botones exportar ───────────────
function actualizarBotonesExportar() {
    const seleccionados = obtenerSeleccionados();
    const haySeleccion = seleccionados.length > 0;
    const soloAprobados = seleccionados.length > 0 &&
        seleccionados.every(s => s.estado === 'APROBADO');

    // Enviar a Syteline — solo aprobados
    $('#index_btnEnviarSyteline')
        .prop('disabled', !soloAprobados)
        .toggleClass('btn-info', soloAprobados)
        .toggleClass('btn-secondary', !soloAprobados)
        .attr('title', !haySeleccion
            ? 'Seleccione comprobantes para enviar'
            : !soloAprobados
                ? 'Solo se envían comprobantes APROBADOS'
                : `Enviar ${seleccionados.length} comprobante(s) a Syteline`);

    // Cabecera — solo aprobados
    $('#index_btnExportarCabecera')
        .prop('disabled', !soloAprobados)
        .toggleClass('btn-success', soloAprobados)
        .toggleClass('btn-secondary', !soloAprobados)
        .attr('title', !haySeleccion
            ? 'Seleccione comprobantes para exportar'
            : !soloAprobados
                ? 'Solo se exportan comprobantes APROBADOS'
                : `Exportar ${seleccionados.length} comprobante(s)`);

    // Imputación — solo aprobados
    $('#index_btnExportarImputacion')
        .prop('disabled', !soloAprobados)
        .toggleClass('btn-success', soloAprobados)
        .toggleClass('btn-secondary', !soloAprobados);

    // Contador seleccionados
    if (haySeleccion) {
        $('#lblSeleccionados')
            .text(`${seleccionados.length} seleccionado(s)`)
            .removeClass('d-none');
    } else {
        $('#lblSeleccionados').addClass('d-none');
    }
}

// ── Obtener folios seleccionados ──────────────
function obtenerSeleccionados() {
    const seleccionados = [];
    $('.chkComprobante:checked').each(function () {
        seleccionados.push({
            // .attr() garantiza string; .data() convierte "2026040001" a número
            // y JSON.stringify produciría [2026040001] que System.Text.Json
            // rechaza al deserializar List<string>.
            folio: $(this).attr('data-folio'),
            estado: $(this).attr('data-estado')
        });
    });
    return seleccionados;
}

// ── Enviar cabecera a Syteline (IDO SLAptrxs) ─
function enviarASyteline() {
    const seleccionados = obtenerSeleccionados();

    if (seleccionados.length === 0) {
        CorporativoCore.notificarAdvertencia(
            'Seleccione al menos un comprobante.');
        return;
    }

    const noAprobados = seleccionados.filter(s => s.estado !== 'APROBADO');
    if (noAprobados.length > 0) {
        CorporativoCore.notificarAdvertencia(
            'Solo se pueden enviar comprobantes APROBADOS. ' +
            `Hay ${noAprobados.length} comprobante(s) sin aprobar.`);
        return;
    }

    const folios = seleccionados.map(s => s.folio);
    const $btn   = $('#index_btnEnviarSyteline');
    const token  = $('input[name="__RequestVerificationToken"]').first().val();

    $btn.prop('disabled', true)
        .html('<span class="spinner-border spinner-border-sm me-1"></span> Enviando...');

    $.ajax({
        url: '/Comprobante/EnviarCabeceraASyteline',
        method: 'POST',
        contentType: 'application/json',
        headers: { 'RequestVerificationToken': token },
        data: JSON.stringify(folios),
        success: function (resp) {
            let msg = `Enviados: ${resp.enviados}`;
            if (resp.errores > 0) {
                const detalles = resp.fallos
                    .map(f => `${f.folio}: ${f.error}`)
                    .join('\n');
                CorporativoCore.notificarAdvertencia(
                    `${msg} — Errores: ${resp.errores}\n${detalles}`);
            } else {
                const vouchers = resp.detalle
                    .map(d => `${d.folio} → Voucher ${d.voucher}`)
                    .join(', ');
                CorporativoCore.notificarExito(
                    `${msg}. Vouchers: ${vouchers}`);
            }
            buscar();
        },
        error: function (xhr) {
            const err = xhr.responseJSON?.error
                ?? `Error ${xhr.status}: ${xhr.statusText}`;
            CorporativoCore.notificarError(
                `Error al enviar a Syteline: ${err}`);
        },
        complete: function () {
            $btn.prop('disabled', false)
                .html('<i class="bi bi-send"></i> Enviar a Syteline');
            actualizarBotonesExportar();
        }
    });
}

// ── Exportar cabecera Syteline ────────────────
function exportarCabecera() {
    const seleccionados = obtenerSeleccionados();

    if (seleccionados.length === 0) {
        CorporativoCore.notificarAdvertencia(
            'Seleccione al menos un comprobante.');
        return;
    }

    const noAprobados = seleccionados.filter(
        s => s.estado !== 'APROBADO');
    if (noAprobados.length > 0) {
        CorporativoCore.notificarAdvertencia(
            'Solo se pueden exportar comprobantes APROBADOS. ' +
            `Hay ${noAprobados.length} comprobante(s) sin aprobar.`);
        return;
    }

    const folios = seleccionados.map(s => s.folio);
    enviarFormExportar('/Comprobante/ExportarCabeceraSyteline', folios);
}

// ── Exportar imputación Syteline ──────────────
function exportarImputacion() {
    const seleccionados = obtenerSeleccionados();

    if (seleccionados.length === 0) {
        CorporativoCore.notificarAdvertencia(
            'Seleccione al menos un comprobante.');
        return;
    }

    const noAprobados = seleccionados.filter(
        s => s.estado !== 'APROBADO');
    if (noAprobados.length > 0) {
        CorporativoCore.notificarAdvertencia(
            'Solo se pueden exportar comprobantes APROBADOS.');
        return;
    }

    const folios = seleccionados.map(s => s.folio);
    enviarFormExportar('/Comprobante/ExportarDistribucionSyteline', folios);
}

// ── Enviar form dinámico para descarga ────────
function enviarFormExportar(url, folios) {
    const token = $('input[name="__RequestVerificationToken"]')
        .first().val();

    const form = $('<form>', {
        method: 'POST',
        action: url
    });

    // Token antiforgery
    if (token) {
        form.append($('<input>', {
            type: 'hidden',
            name: '__RequestVerificationToken',
            value: token
        }));
    }

    // Folios seleccionados
    folios.forEach(folio => {
        form.append($('<input>', {
            type: 'hidden',
            name: 'folios',
            value: folio
        }));
    });

    $('body').append(form);
    form.submit();
    form.remove();
}

// ── Eventos ───────────────────────────────────
function bindEventos() {

    // Buscar
    $('#index_btnBuscar').on('click', buscar);

    // Buscar con Enter
    $('#index_txtFolio, #index_txtProveedor').on('keypress', function (e) {
        if (e.which === 13) buscar();
    });

    // Nuevo
    $('#index_btnNuevo').on('click', function () {
        window.location.href = '/Comprobante/Detalle';
    });

    // Ver detalle
    $('#index_tblComprobantes').on('click', '.btn-ver', function () {
        const folio = $(this).data('folio');
        window.location.href = `/Comprobante/Detalle?folio=${folio}`;
    });

    // Checkbox individual → actualizar botones
    $('#index_tblComprobantes').on(
        'change', '.chkComprobante', function () {
            const total = $('.chkComprobante').length;
            const seleccionados = $('.chkComprobante:checked').length;
            $('#chkTodos').prop(
                'checked', total === seleccionados && total > 0);
            actualizarBotonesExportar();
        }
    );

    // Checkbox seleccionar todos — re-bind después de draw
    tablaComprobantes.on('draw', function () {
        bindChkTodos();
        actualizarBotonesExportar();
    });

    // Enviar a Syteline
    $('#index_btnEnviarSyteline').on('click', enviarASyteline);

    // Exportar cabecera
    $('#index_btnExportarCabecera').on('click', exportarCabecera);

    // Exportar imputación
    $('#index_btnExportarImputacion').on('click', exportarImputacion);
}