// ============================================
// IMPUTACION.JS - Imputación Contable
// ============================================

let tablaImputacion;
let modoEdicion = false;
let secuenciaEditando = null;
let listaImputaciones = [];

$(document).ready(function () {
    if ($('#tblImputacion').length > 0) {
        inicializarTablaImputacion();
        bindEventosImputacion();
    }
});

// ── Inicializar DataTable ─────────────────────
function inicializarTablaImputacion() {
    tablaImputacion = $('#tblImputacion').DataTable({
        language: { url: '/lib/datatables.net/i18n/es-ES.json' },
        paging: false,
        searching: false,
        info: false,
        ordering: false,
        columns: [
            { data: 'aliasCuenta' },
            {
                // Afectación / código impositivo — calculado por secuencia
                data: null,
                render: function (data, type, row) {
                    const af = obtenerAfectacion(row.secuencia);
                    if (!af) return '';
                    const clases = {
                        CABECERA:  'bg-secondary',
                        GRAVADO:   'bg-primary',
                        IGV:       'bg-info text-dark',
                        EXONERADO: 'bg-success',
                        'RETENCIÓN': 'bg-warning text-dark'
                    };
                    const cls = clases[af] ?? 'bg-secondary';
                    return `<span class="badge ${cls}">${af}</span>`;
                }
            },
            { data: 'proyecto' },
            { data: 'codUnidad1Cuenta' },
            { data: 'codUnidad3Cuenta' },
            { data: 'codUnidad4Cuenta' },
            {
                data: 'monto',
                render: d => CorporativoCore.formatearMonto(d),
                className: 'text-end'
            },
            { data: 'descripcion' },
            { data: 'cuentaContable', visible: false },
            {
                data: 'secuencia',
                orderable: false,
                render: function (secuencia, type, row) {
                    if (row.estado === 'pendiente') {
                        return `<button class="btn btn-sm btn-warning btn-configurar-imp"
                                        data-secuencia="${secuencia}"
                                        title="Configurar cuenta contable y códigos de unidad">
                                    <i class="bi bi-pencil-square"></i> Configurar
                                </button>`;
                    }
                    return `
                        <button class="btn btn-sm btn-primary btn-editar-imp"
                                data-secuencia="${secuencia}">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-danger btn-eliminar-imp"
                                data-secuencia="${secuencia}">
                            <i class="bi bi-trash"></i>
                        </button>`;
                }
            }
        ],
        createdRow: function (row, data) {
            if (data.estado === 'pendiente') {
                $(row).addClass('table-warning');
            }
        }
    });
}

// ── Cargar imputaciones del comprobante ───────
function cargarImputaciones(folio) {
    CorporativoQuery.ajaxGet(`/Comprobante/ObtenerImputaciones?folio=${folio}`, function (data) {
        listaImputaciones = data;
        refrescarTabla();
        calcularTotales();
    });
}

// ── Mostrar formulario nueva imputación ───────
function mostrarFormularioImputacion() {
    limpiarFormularioImputacion();
    modoEdicion = false;
    secuenciaEditando = null;

    // Pre-cargar monto y mostrar badge según la línea que se va a agregar
    const linea = obtenerLineaSiguiente();
    if (linea.monto !== null) {
        $('#txtMonto').val(CorporativoCore.formatearMonto(linea.monto));
    }
    $('#lblLineaImputacion')
        .text(linea.desc ? `Línea ${linea.seq} — ${linea.desc}` : `Línea ${linea.seq}`)
        .removeClass('d-none');

    $('#pnlDetalleImputacion').removeClass('d-none');
    $('#btnAgregarNuevaImputacion').removeClass('d-none');
    $('#btnCancelarDetalle').removeClass('d-none');
    $('#btnEliminarDetalle').addClass('d-none');
    $('#btnEditarDetalle').addClass('d-none');
    $('#txtAliasCuenta').focus();
}

// ── Mostrar formulario editar imputación ──────
function mostrarFormularioEditar(secuencia) {
    const imp = listaImputaciones.find(i => i.secuencia == secuencia);
    if (!imp) return;

    modoEdicion = true;
    secuenciaEditando = secuencia;

    $('#txtSecuencia').val(imp.secuencia);
    $('#txtAliasCuenta').val(imp.aliasCuenta);
    $('#txtCuentaContable').val(imp.cuentaContable);
    $('#txtDescripcionCuenta').val(imp.descripcionCuenta);
    $('#txtDescripcion').val(imp.descripcion);
    $('#txtMonto').val(CorporativoCore.formatearMonto(imp.monto));
    $('#txtProyecto').val(imp.proyecto);

    if (imp.cuentaContable) {
        mostrarCodigosUnidad('Cuenta');
        $('#txtCodUnidad1Cuenta').val(imp.codUnidad1Cuenta);
        $('#txtCodUnidad3Cuenta').val(imp.codUnidad3Cuenta);
        $('#txtCodUnidad4Cuenta').val(imp.codUnidad4Cuenta);
    }

    // Badge de línea: muestra la descripción si la secuencia tiene un nombre conocido
    const desc = obtenerDescripcionSecuencia(imp.secuencia);
    $('#lblLineaImputacion')
        .text(desc ? `Línea ${imp.secuencia} — ${desc}` : `Línea ${imp.secuencia}`)
        .removeClass('d-none');

    $('#pnlDetalleImputacion').removeClass('d-none');
    $('#btnAgregarNuevaImputacion').addClass('d-none');
    $('#btnEditarDetalle').removeClass('d-none');
    $('#btnEliminarDetalle').removeClass('d-none');
    $('#btnCancelarDetalle').removeClass('d-none');
}

// ── Obtener datos del formulario ──────────────
function obtenerDatosFormulario() {
    return {
        secuencia: $('#txtSecuencia').val(),
        folio: $('#hdnFolio').val(),
        aliasCuenta: $('#txtAliasCuenta').val(),
        cuentaContable: $('#txtCuentaContable').val(),
        descripcionCuenta: $('#txtDescripcionCuenta').val(),
        descripcion: $('#txtDescripcion').val(),
        monto: CorporativoCore.limpiarMonto($('#txtMonto').val()),
        proyecto: $('#txtProyecto').val(),
        codUnidad1Cuenta: $('#txtCodUnidad1Cuenta').val(),
        codUnidad3Cuenta: $('#txtCodUnidad3Cuenta').val(),
        codUnidad4Cuenta: $('#txtCodUnidad4Cuenta').val(),
    };
}

// ── Agregar imputación ────────────────────────
function agregarImputacion() {
    if (!validarFormularioImputacion()) return;

    const datos = obtenerDatosFormulario();

    CorporativoQuery.ajaxPost('/Comprobante/AgregarImputacion',
        { imputacion: datos },
        function (response) {
            if (response.exito) {
                listaImputaciones.push(response.imputacion);
                refrescarTabla();
                calcularTotales();
                limpiarFormularioImputacion();
                CorporativoCore.notificarExito('Imputación agregada correctamente.');
            } else {
                CorporativoCore.notificarError(response.mensaje);
            }
        }
    );
}

// ── Guardar edición imputación ────────────────
function guardarEdicionImputacion() {
    if (!validarFormularioImputacion()) return;

    const datos = obtenerDatosFormulario();

    CorporativoQuery.ajaxPost('/Comprobante/EditarImputacion',
        { imputacion: datos },
        function (response) {
            if (response.exito) {
                const idx = listaImputaciones
                    .findIndex(i => i.secuencia == secuenciaEditando);
                if (idx >= 0) listaImputaciones[idx] = response.imputacion;

                refrescarTabla();
                calcularTotales();
                ocultarFormularioImputacion();
                CorporativoCore.notificarExito('Imputación actualizada correctamente.');
            } else {
                CorporativoCore.notificarError(response.mensaje);
            }
        }
    );
}

// ── Eliminar imputación ───────────────────────
async function eliminarImputacion(secuencia) {
    const ok = await CorporativoCore.confirmar('¿Desea eliminar esta imputación?');
    if (!ok) return;

    const folio = $('#hdnFolio').val();

    CorporativoQuery.ajaxPost('/Comprobante/EliminarImputacion',
        { folio, secuencia },
        function (response) {
            if (response.exito) {
                listaImputaciones = listaImputaciones
                    .filter(i => i.secuencia != secuencia);
                refrescarTabla();
                calcularTotales();
                ocultarFormularioImputacion();
                CorporativoCore.notificarExito('Imputación eliminada correctamente.');
            } else {
                CorporativoCore.notificarError(response.mensaje);
            }
        }
    );
}

// ── Calcular totales ──────────────────────────
function calcularTotales() {
    const montoTotal = CorporativoCore.limpiarMonto($('#txtMontoTotal').val());

    // La línea 1 es cabecera SyteLine y no cuenta como imputación financiera.
    // Solo se suman las líneas con secuencia > 1.
    const totalImputado = listaImputaciones
        .filter(i => i.secuencia > 1)
        .reduce((sum, imp) => sum + (parseFloat(imp.monto) || 0), 0);

    const diferencia = montoTotal - totalImputado;

    $('#txtPorImputar').val(CorporativoCore.formatearMonto(montoTotal));
    $('#txtTotalImputacion').val(CorporativoCore.formatearMonto(totalImputado));
    $('#txtDiferenciaImputacion').val(CorporativoCore.formatearMonto(diferencia));

    if (Math.abs(diferencia) > 0.01) {
        $('#txtDiferenciaImputacion')
            .addClass('text-danger fw-bold')
            .removeClass('text-success');
    } else {
        $('#txtDiferenciaImputacion')
            .addClass('text-success')
            .removeClass('text-danger fw-bold');
    }

    // Bloquear el botón "Agregar Imputación" cuando la diferencia llega a 0.
    const completa = Math.abs(diferencia) <= 0.01;
    $('#btnAgregarDetalle')
        .prop('disabled', completa)
        .attr('title', completa ? 'Imputación completa: la diferencia es 0.00' : '');
}

// ── Validar formulario ────────────────────────
function validarFormularioImputacion() {
    if (CorporativoCore.esVacio($('#txtCuentaContable').val())) {
        CorporativoCore.notificarAdvertencia('Debe seleccionar una cuenta contable.');
        $('#txtAliasCuenta').focus();
        return false;
    }
    return true;
}

// ── Limpiar formulario imputación ─────────────
function limpiarFormularioImputacion() {
    $('#txtSecuencia').val('');
    $('#txtAliasCuenta, #txtCuentaContable, #txtDescripcionCuenta').val('');
    $('#txtDescripcion').val('');
    $('#txtMonto').val('0.00');
    $('#txtProyecto').val('');

    ocultarCodigosUnidad('Cuenta');
}

// ── Ocultar formulario ────────────────────────
function ocultarFormularioImputacion() {
    $('#pnlDetalleImputacion').addClass('d-none');
    $('#btnAgregarNuevaImputacion').addClass('d-none');
    $('#btnEditarDetalle').addClass('d-none');
    $('#btnEliminarDetalle').addClass('d-none');
    $('#btnCancelarDetalle').addClass('d-none');
    $('#lblLineaImputacion').addClass('d-none');
    limpiarFormularioImputacion();
    modoEdicion = false;
    secuenciaEditando = null;
}

// ════════════════════════════════════════════
// LÓGICA DE SECUENCIAS DE IMPUTACIÓN
// ════════════════════════════════════════════

/**
 * Construye la lista de líneas esperadas según los montos del comprobante.
 * Índice 0 → seq 1 (cabecera, sin monto requerido)
 * Índice 1 → seq 2 (Monto Neto)
 * Índice 2 → seq 3 (IGV Crédito Fiscal)
 * Índice 3 → seq 4 (Exento, solo si > 0)
 * Índice 3/4 → seq 4/5 (Retención, solo si > 0)
 */
function obtenerLineasEsperadas() {
    const neto      = CorporativoCore.limpiarMonto($('#txtMontoNeto').val());
    const igv       = CorporativoCore.limpiarMonto($('#txtMontoIGVCredito').val());
    const exento    = CorporativoCore.limpiarMonto($('#txtMontoExento').val());
    const retencion = CorporativoCore.limpiarMonto($('#txtMontoRetencion').val());

    const lineas = [
        { monto: null,     desc: 'Cabecera SyteLine',      afectacion: 'CABECERA'   }, // seq 1
        { monto: neto,     desc: 'Monto Neto',             afectacion: 'GRAVADO'    }, // seq 2
        { monto: igv,      desc: 'IGV Crédito Fiscal',     afectacion: 'IGV'        }, // seq 3
    ];
    if (exento > 0)    lineas.push({ monto: exento,    desc: 'Monto Exento / Exonerado', afectacion: 'EXONERADO' });
    if (retencion > 0) lineas.push({ monto: retencion, desc: 'Retención',                afectacion: 'RETENCIÓN' });

    return lineas;
}

/** Devuelve la etiqueta de afectación/código impositivo para una secuencia dada. */
function obtenerAfectacion(seq) {
    const lineas = obtenerLineasEsperadas();
    const item   = lineas[seq - 1];
    return item ? item.afectacion : '';
}

/**
 * Genera filas "pendientes" para las secuencias esperadas que aún no tienen
 * imputación guardada en listaImputaciones.
 */
function construirFilasPendientes() {
    const lineas              = obtenerLineasEsperadas();
    const secuenciasGuardadas = new Set(listaImputaciones.map(i => i.secuencia));

    return lineas
        .map((linea, idx) => ({ seq: idx + 1, ...linea }))
        .filter(l => !secuenciasGuardadas.has(l.seq))
        .map(l => ({
            secuencia:         l.seq,
            aliasCuenta:       '',
            cuentaContable:    '',
            descripcionCuenta: '',
            monto:             l.monto ?? 0,
            descripcion:       l.desc,
            proyecto:          '',
            codUnidad1Cuenta:  '',
            codUnidad3Cuenta:  '',
            codUnidad4Cuenta:  '',
            estado:            'pendiente'
        }));
}

/**
 * Redibuja la tabla fusionando imputaciones guardadas (de listaImputaciones)
 * con filas pendientes, ordenadas por secuencia.
 */
function refrescarTabla() {
    const guardadas  = listaImputaciones.map(i => ({ ...i, estado: 'guardado' }));
    const pendientes = construirFilasPendientes();
    const todas = [...guardadas, ...pendientes].sort((a, b) => a.secuencia - b.secuencia);
    tablaImputacion.clear().rows.add(todas).draw();
}

/** Devuelve { seq, monto, desc } para la próxima línea a agregar. */
function obtenerLineaSiguiente() {
    const lineas  = obtenerLineasEsperadas();
    const nextSeq = listaImputaciones.length + 1;
    const idx     = nextSeq - 1; // 0-based
    return idx < lineas.length
        ? { seq: nextSeq, ...lineas[idx] }
        : { seq: nextSeq, monto: null, desc: null };
}

/** Devuelve la descripción de una secuencia ya guardada (para el modo edición). */
function obtenerDescripcionSecuencia(seq) {
    const lineas = obtenerLineasEsperadas();
    const item   = lineas[seq - 1];
    return item ? item.desc : null;
}

// ── Cargar imputación masiva ──────────────────
function procesarImputacionMasiva(archivo) {
    const formData = new FormData();
    formData.append('file', archivo);
    formData.append('folio', $('#hdnFolio').val());

    CorporativoCore.showLoading();
    $.ajax({
        url: '/Comprobante/CargarImputacionMasiva',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: {
            'RequestVerificationToken': CorporativoCore.obtenerToken()
        },
        success: function (response) {
            CorporativoCore.hideLoading();
            if (response.exito) {
                listaImputaciones = response.imputaciones;
                refrescarTabla();
                calcularTotales();
                CorporativoCore.notificarExito('Imputación masiva cargada correctamente.');
            } else {
                CorporativoCore.notificarError(response.mensaje);
            }
        },
        error: function (xhr) {
            CorporativoCore.hideLoading();
            CorporativoCore.handleError(xhr, {
                onCustom: function () {
                    CorporativoCore.notificarError('Error al procesar el archivo.');
                }
            });
        }
    });
}

// ── Descargar plantilla ───────────────────────
function descargarPlantillaImputacion() {
    window.location.href = '/Comprobante/DescargarPlantillaImputacion';
}

// ════════════════════════════════════════════
// CÓDIGOS DE UNIDAD
// ════════════════════════════════════════════

function mostrarCodigosUnidad(campo) {
    $(`#divCodigosUnidad${campo}`).removeClass('d-none');
}

function ocultarCodigosUnidad(campo) {
    $(`#divCodigosUnidad${campo}`).addClass('d-none');
    $(`#txtCodUnidad1${campo}`).val('');
    $(`#txtCodUnidad3${campo}`).val('');
    $(`#txtCodUnidad4${campo}`).val('');
}

function buscarCuentaContable() {
    CorporativoQuery.ajaxGet('/Comprobante/ObtenerCuentasContables',
        function (data) {
            if (!data || data.length === 0) {
                CorporativoCore.notificarInfo('No hay cuentas contables disponibles.');
                return;
            }
            mostrarModalBusqueda(data, 'Seleccionar Cuenta Contable', 'seleccionar-cuenta');
        }
    );
}

function buscarCodigoUnidad(campo, unidad, inputId) {
    const codigoOrigen = $('#txtCuentaContable').val();
    CorporativoQuery.ajaxGet(
        `/Comprobante/ObtenerCodigosUnidad?campo=${campo}&unidad=${unidad}&codigo=${codigoOrigen}`,
        function (data) {
            if (!data || data.length === 0) {
                CorporativoCore.notificarInfo('No hay códigos de unidad disponibles.');
                return;
            }
            mostrarModalBusqueda(data, 'Seleccionar Código de Unidad', 'seleccionar-unidad', inputId);
        }
    );
}

function mostrarModalBusqueda(data, titulo, claseItem, inputId) {
    const renderItem = (d) => {
        const $a = $('<a>', {
            class: `list-group-item list-group-item-action ${claseItem}`,
            'data-codigo': d.codigo,
            href: '#'
        });
        if (inputId) $a.attr('data-input', inputId);
        $a.append($('<span>', { class: 'fw-bold' }).text(d.codigo));
        $a.append($('<span>', { class: 'text-muted ms-2' }).text(d.descripcion));
        return $a[0].outerHTML;
    };

    const html = `
        <div class="modal fade" id="modalBusqueda" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header py-2" style="background-color:#5b74ad;">
                        <h6 class="modal-title text-white mb-0">
                            <i class="bi bi-search"></i> ${titulo}
                        </h6>
                        <button type="button" class="btn-close btn-close-white"
                                data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body pb-0 pt-2 px-2">
                        <input type="text" class="form-control form-control-sm mb-2"
                               id="txtFiltroBusqueda" placeholder="Filtrar..." autocomplete="off" />
                    </div>
                    <div class="modal-body p-0" style="max-height:360px; overflow-y:auto;">
                        <div class="list-group list-group-flush" id="listaBusqueda">
                            ${data.map(renderItem).join('')}
                        </div>
                    </div>
                </div>
            </div>
        </div>`;

    $('#modalBusqueda').remove();
    $('#modalContainer').append(html);

    $('#txtFiltroBusqueda').on('keyup', function () {
        const texto = $(this).val().toLowerCase();
        $('#listaBusqueda a').each(function () {
            const coincide = $(this).text().toLowerCase().includes(texto);
            $(this).toggleClass('d-none', !coincide);
        });
    });

    const modal = new bootstrap.Modal(document.getElementById('modalBusqueda'));
    modal.show();
    setTimeout(() => $('#txtFiltroBusqueda').focus(), 300);
}

// ════════════════════════════════════════════
// BIND EVENTOS
// ════════════════════════════════════════════

function bindEventosImputacion() {

    $('#btnAgregarDetalle').on('click', mostrarFormularioImputacion);
    $('#btnAgregarNuevaImputacion').on('click', agregarImputacion);
    $('#btnEditarDetalle').on('click', guardarEdicionImputacion);
    $('#btnCancelarDetalle').on('click', ocultarFormularioImputacion);

    $('#btnLimpiarImputacion').on('click', async function () {
        const ok = await CorporativoCore.confirmar('¿Desea limpiar el formulario de imputación?');
        if (ok) limpiarFormularioImputacion();
    });

    $('#tblImputacion').on('click', '.btn-editar-imp', function () {
        mostrarFormularioEditar($(this).data('secuencia'));
    });

    $('#tblImputacion').on('click', '.btn-eliminar-imp', function () {
        eliminarImputacion($(this).data('secuencia'));
    });

    // Configurar fila pendiente: abre el formulario pre-cargado para la siguiente línea esperada.
    $('#tblImputacion').on('click', '.btn-configurar-imp', function () {
        mostrarFormularioImputacion();
    });

    $('#btnExplorar').on('click', function () {
        $('#inpFile').trigger('click');
    });

    $('#inpFile').on('change', function () {
        if (this.files.length > 0) {
            procesarImputacionMasiva(this.files[0]);
            $(this).val('');
        }
    });

    $('#btnDescargarPlantillaImputacion').on('click', descargarPlantillaImputacion);

    $('a[href="#tabImputacion"]').on('shown.bs.tab', function () {
        const folio = $('#hdnFolio').val();
        if (folio) {
            $('#barraOpcionesImputacion').removeClass('d-none');
            $('#barraAccionesImputacion').removeClass('d-none');
            cargarImputaciones(folio);
        }
    });

    // ── Códigos Unidad ────────────────────────

    $('#btnBuscarCuentaContable').on('click', buscarCuentaContable);

    $('#txtCuentaContable').on('change', function () {
        if (!CorporativoCore.esVacio($(this).val())) mostrarCodigosUnidad('Cuenta');
        else ocultarCodigosUnidad('Cuenta');
    });

    $('#btnBuscarCodUnidad1Cuenta').on('click', () => buscarCodigoUnidad('Cuenta', 1, '#txtCodUnidad1Cuenta'));
    $('#btnBuscarCodUnidad3Cuenta').on('click', () => buscarCodigoUnidad('Cuenta', 3, '#txtCodUnidad3Cuenta'));
    $('#btnBuscarCodUnidad4Cuenta').on('click', () => buscarCodigoUnidad('Cuenta', 4, '#txtCodUnidad4Cuenta'));

    $(document).on('click', '.seleccionar-cuenta', function (e) {
        e.preventDefault();
        const codigo = $(this).data('codigo');
        $('#txtAliasCuenta').val(codigo);
        $('#txtCuentaContable').val(codigo);
        const descripcion = $(this).find('.text-muted').text().trim();
        $('#txtDescripcionCuenta').val(descripcion);
        $('#txtCuentaContable').trigger('change');
        bootstrap.Modal.getInstance(document.getElementById('modalBusqueda')).hide();
    });

    $(document).on('click', '.seleccionar-unidad', function (e) {
        e.preventDefault();
        const codigo = $(this).data('codigo');
        const inputId = $(this).data('input');
        $(inputId).val(codigo);
        bootstrap.Modal.getInstance(document.getElementById('modalBusqueda')).hide();
    });
}
