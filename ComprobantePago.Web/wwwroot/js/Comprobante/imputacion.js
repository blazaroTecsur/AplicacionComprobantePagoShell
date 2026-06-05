// ============================================
// IMPUTACION.JS - Imputación Contable
// ============================================

let tablaImputacion;
let modoEdicion = false;
let secuenciaEditando = null;
let listaImputaciones = [];

$(function () {
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
                // Afectación / código impositivo — tipoLinea (RP) o secuencia (otros)
                data: null,
                render: function (data, type, row) {
                    const af = obtenerAfectacion(row.secuencia, row.tipoLinea);
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
            { data: 'codUnidad2Cuenta' },
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
                    const estado   = $('#hdnCodigoEstado').val();
                    const editable = estado === '' || estado === 'NUEVO' || estado === 'REGISTRADO';
                    if (!editable) return '';
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
    CorporativoQuery.ajaxGet(BASE_URL+`/Comprobante/ObtenerImputaciones?folio=${folio}`, function (data) {
        listaImputaciones = data;
        refrescarTabla();
        calcularTotales();
        actualizarBotonesRP();
    });
}

// ── Mostrar/ocultar botones según tipo de documento ──
function actualizarBotonesRP() {
    const esRP = $('#hdnTipoDocumento').val() === 'RP';
    $('#btnFraccionar').removeClass('d-none');
    $('#btnExplorar').removeClass('d-none');
    $('#btnDescargarPlantillaImputacion').removeClass('d-none');
}

// ── Mostrar formulario nueva imputación ───────
function mostrarFormularioImputacion(secuenciaObjetivo) {
    limpiarFormularioImputacion();
    modoEdicion = false;
    secuenciaEditando = null;

    // Si viene de "Configurar" usamos la secuencia del pendiente; si es "Nueva" la siguiente disponible
    let linea;
    if (secuenciaObjetivo != null) {
        const lineas = obtenerLineasEsperadas();
        const item   = lineas[secuenciaObjetivo - 1];
        linea = item ? { seq: secuenciaObjetivo, ...item } : { seq: secuenciaObjetivo, monto: null, desc: null };
    } else {
        linea = obtenerLineaSiguiente();
    }

    $('#txtSecuencia').val(linea.seq);
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
    $('#txtAliasCuenta').trigger('focus');
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
        $('#txtCodUnidad2Cuenta').val(imp.codUnidad2Cuenta);
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
        codUnidad2Cuenta: $('#txtCodUnidad2Cuenta').val(),
        codUnidad3Cuenta: $('#txtCodUnidad3Cuenta').val(),
        codUnidad4Cuenta: $('#txtCodUnidad4Cuenta').val(),
    };
}

// ── Agregar imputación ────────────────────────
function agregarImputacion() {
    if (!validarFormularioImputacion()) return;

    const datos = obtenerDatosFormulario();

    CorporativoQuery.ajaxPost(BASE_URL+'/Comprobante/AgregarImputacion',
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

    CorporativoQuery.ajaxPost(BASE_URL+'/Comprobante/EditarImputacion',
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

    CorporativoQuery.ajaxPost(BASE_URL+'/Comprobante/EliminarImputacion',
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
    const cuenta = $('#txtCuentaContable').val();
    if (CorporativoCore.esVacio(cuenta)) {
        CorporativoCore.notificarAdvertencia('Debe seleccionar una cuenta contable.');
        $('#txtAliasCuenta').trigger('focus');
        return false;
    }
    if (cuenta.startsWith('6')) {
        if (CorporativoCore.esVacio($('#txtCodUnidad1Cuenta').val())) {
            CorporativoCore.notificarAdvertencia('Las cuentas que inician con 6 requieren Código de Unidad 1.');
            $('#txtCodUnidad1Cuenta').trigger('focus');
            return false;
        }
        if (CorporativoCore.esVacio($('#txtCodUnidad4Cuenta').val())) {
            CorporativoCore.notificarAdvertencia('Las cuentas que inician con 6 requieren Código de Unidad 4.');
            $('#txtCodUnidad4Cuenta').trigger('focus');
            return false;
        }
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
    const esRP  = $('#hdnTipoDocumento').val() === 'RP';

    // Para RP fraccionado: usar las líneas dinámicas de listaImputaciones (tipoLinea)
    if (esRP && listaImputaciones.length > 0 &&
        listaImputaciones.some(i => i.tipoLinea)) {

        const lineas = [];
        // Seq 1 = Cabecera (siempre)
        lineas.push({ monto: null, desc: 'Cabecera SyteLine', afectacion: 'CABECERA' });

        // Resto: construir desde listaImputaciones que tienen tipoLinea
        const conTipo = listaImputaciones
            .filter(i => i.secuencia > 1 && i.tipoLinea)
            .sort((a, b) => a.secuencia - b.secuencia);

        conTipo.forEach(imp => {
            const desc = imp.tipoLinea === 'GRAVADO' ? 'Monto Neto'
                       : imp.tipoLinea === 'IGV'     ? 'IGV Crédito Fiscal'
                       : imp.tipoLinea === 'EXENTO'  ? 'Monto Exento'
                       : imp.tipoLinea;
            lineas.push({ monto: imp.monto, desc, afectacion: imp.tipoLinea });
        });

        return lineas;
    }

    const neto      = CorporativoCore.limpiarMonto($('#txtMontoNeto').val());
    const igv       = CorporativoCore.limpiarMonto($('#txtMontoIGVCredito').val());
    const exento    = CorporativoCore.limpiarMonto($('#txtMontoExento').val());
    const retencion = CorporativoCore.limpiarMonto($('#txtMontoRetencion').val());

    // Facturación solo exenta: neto=0 e igv=0 → solo 2 afectaciones (Cabecera + Exento)
    const soloExento = neto === 0 && igv === 0 && exento > 0;

    const lineas = [
        { monto: null, desc: 'Cabecera SyteLine', afectacion: 'CABECERA' }, // seq 1
    ];

    if (soloExento) {
        lineas.push({ monto: exento, desc: 'Monto Exento / Exonerado', afectacion: 'EXONERADO' }); // seq 2
    } else {
        lineas.push({ monto: neto, desc: 'Monto Neto',         afectacion: 'GRAVADO' }); // seq 2
        lineas.push({ monto: igv,  desc: 'IGV Crédito Fiscal', afectacion: 'IGV'     }); // seq 3
        if (exento > 0)    lineas.push({ monto: exento,    desc: 'Monto Exento / Exonerado', afectacion: 'EXONERADO' });
        if (retencion > 0) lineas.push({ monto: retencion, desc: 'Retención',                afectacion: 'RETENCIÓN' });
    }

    return lineas;
}

/** Devuelve la etiqueta de afectación. Para RP usa tipoLinea directo; otros usan secuencia. */
function obtenerAfectacion(seq, tipoLinea) {
    if (tipoLinea) return tipoLinea;
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
            codUnidad2Cuenta:  '',
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
let _cargandoMasiva = false;

function procesarImputacionMasiva(archivo) {
    if (_cargandoMasiva) return;
    _cargandoMasiva = true;

    const folio = $('#hdnFolio').val();
    const formData = new FormData();
    formData.append('file', archivo);

    CorporativoCore.showLoading();
    $.ajax({
        // folio va en la query string para evitar problemas de binding multipart
        url: BASE_URL + '/Comprobante/CargarImputacionMasiva?folio=' + encodeURIComponent(folio),
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: {
            'RequestVerificationToken': CorporativoCore.obtenerToken()
        },
        success: function (response) {
            _cargandoMasiva = false;
            CorporativoCore.hideLoading();
            if (response.exito) {
                listaImputaciones = response.imputaciones;
                refrescarTabla();
                calcularTotales();
                actualizarBotonesRP();
                CorporativoCore.notificarExito('Imputación masiva cargada correctamente.');
            } else {
                CorporativoCore.notificarError(response.mensaje);
            }
        },
        error: function (xhr) {
            _cargandoMasiva = false;
            CorporativoCore.hideLoading();
            const msg = xhr.responseJSON?.error?.userMessage;
            if (msg) {
                const lineas = msg.split('\n').filter(l => l.trim());
                if (lineas.length > 1 && typeof Swal !== 'undefined') {
                    const items = lineas.map(l => `<li>${$('<div>').text(l).html()}</li>`).join('');
                    Swal.fire({
                        icon: 'error',
                        title: 'Errores en la plantilla',
                        html: `<ul class="text-start small mb-0" style="max-height:300px;overflow-y:auto">${items}</ul>`,
                        confirmButtonText: 'Cerrar'
                    });
                } else {
                    CorporativoCore.notificarError(lineas[0] || msg);
                }
            } else {
                CorporativoCore.handleError(xhr, {
                    onCustom: () => CorporativoCore.notificarError('Error al procesar el archivo.')
                });
            }
        }
    });
}

// ── Descargar plantilla ───────────────────────
function descargarPlantillaImputacion() {
    window.location.href = BASE_URL+'/Comprobante/DescargarPlantillaImputacion';
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
    $(`#txtCodUnidad2${campo}`).val('');
    $(`#txtCodUnidad3${campo}`).val('');
    $(`#txtCodUnidad4${campo}`).val('');
}

function buscarCuentaContable() {
    mostrarModalBusqueda(
        BASE_URL+'/Comprobante/ObtenerCuentasContables',
        'Seleccionar Cuenta Contable',
        'seleccionar-cuenta'
    );
}

function buscarCodigoUnidad(campo, unidad, inputId) {
    const codigoOrigen = $('#txtCuentaContable').val();
    mostrarModalBusqueda(
        BASE_URL+`/Comprobante/ObtenerCodigosUnidad?campo=${campo}&unidad=${unidad}&codigo=${codigoOrigen}`,
        'Seleccionar Código de Unidad',
        'seleccionar-unidad',
        inputId
    );
}

function mostrarModalBusqueda(fetchUrl, titulo, claseItem, inputId) {
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

    const renderLista = (items) => {
        if (!items || items.length === 0) {
            $('#listaBusqueda').html('<p class="text-muted text-center py-3 small">Sin resultados</p>');
            return;
        }
        $('#listaBusqueda').html(items.map(renderItem).join(''));
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
                               id="txtFiltroBusqueda" placeholder="Buscar..." autocomplete="off" />
                    </div>
                    <div class="modal-body p-0" style="max-height:360px; overflow-y:auto;">
                        <div class="list-group list-group-flush" id="listaBusqueda">
                            <p class="text-muted text-center py-3 small">Cargando...</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>`;

    $('#modalBusqueda').remove();
    $('#modalContainer').append(html);

    const cargarDatos = (filtro) => {
        const sep = fetchUrl.includes('?') ? '&' : '?';
        const url = filtro ? `${fetchUrl}${sep}filtro=${encodeURIComponent(filtro)}` : fetchUrl;
        CorporativoQuery.ajaxGet(url, renderLista);
    };

    let debounceTimer;
    $('#txtFiltroBusqueda').on('keyup', function () {
        const texto = $(this).val().trim();
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => cargarDatos(texto), 300);
    });

    const modal = new bootstrap.Modal(document.getElementById('modalBusqueda'));
    modal.show();
    setTimeout(() => {
        $('#txtFiltroBusqueda').trigger('focus');
        cargarDatos('');
    }, 300);
}

// ════════════════════════════════════════════
// MODAL FRACCIONAR (solo RP)
// ════════════════════════════════════════════

function mostrarModalFraccionar() {
    const montoNeto   = CorporativoCore.limpiarMonto($('#txtMontoNeto').val());
    const montoExento = CorporativoCore.limpiarMonto($('#txtMontoExento').val());
    const montoIgv    = CorporativoCore.limpiarMonto($('#txtMontoIGVCredito').val());

    const soloExento  = montoNeto === 0 && montoIgv === 0 && montoExento > 0;
    const mixto       = montoNeto > 0 && montoExento > 0;

    const infoIgv = montoIgv > 0
        ? `<div class="alert alert-info py-1 px-2 mb-3 small">
               <i class="bi bi-info-circle"></i>
               IGV <strong>${CorporativoCore.formatearMonto(montoIgv)}</strong>
               se registrará como una línea única separada.
           </div>`
        : '';

    // Controles de generación (compartidos o per-sección en modo mixto)
    const controlesGen = (sufijo) => `
        <div class="row g-2 mb-3">
            <div class="col-auto">
                <label class="form-label fw-bold mb-0">Número de líneas</label>
                <input type="number" id="txtNumLineas${sufijo}" class="form-control form-control-sm"
                       value="2" min="2" max="20" style="width:80px" />
            </div>
            <div class="col-auto d-flex align-items-end">
                <div class="form-check me-3">
                    <input class="form-check-input" type="radio" name="rdTipoDiv${sufijo}"
                           id="rdMontoIgual${sufijo}" value="igual" checked />
                    <label class="form-check-label" for="rdMontoIgual${sufijo}">Montos iguales</label>
                </div>
                <div class="form-check">
                    <input class="form-check-input" type="radio" name="rdTipoDiv${sufijo}"
                           id="rdPorcentaje${sufijo}" value="porcentaje" />
                    <label class="form-check-label" for="rdPorcentaje${sufijo}">Por porcentaje</label>
                </div>
            </div>
            <div class="col-auto d-flex align-items-end">
                <button class="btn btn-secondary btn-sm btn-generar" data-sufijo="${sufijo}">
                    <i class="bi bi-arrow-clockwise"></i> Generar
                </button>
            </div>
        </div>`;

    const tablaFrac = (sufijo, label) => `
        <div class="table-responsive">
            <table class="table table-sm table-bordered">
                <thead class="table-light">
                    <tr>
                        <th>Línea</th>
                        <th class="text-end">% (opcional)</th>
                        <th class="text-end">${label}</th>
                    </tr>
                </thead>
                <tbody id="bodyFraccionar${sufijo}"></tbody>
                <tfoot>
                    <tr class="fw-bold">
                        <td>Total</td>
                        <td class="text-end" id="tdTotalPct${sufijo}"></td>
                        <td class="text-end" id="tdTotalMonto${sufijo}"></td>
                    </tr>
                </tfoot>
            </table>
        </div>`;

    let cuerpoModal = infoIgv;
    if (mixto) {
        cuerpoModal += `
            <h6 class="fw-bold text-primary mb-1"><i class="bi bi-grid"></i> Gravado (Monto Neto)</h6>
            ${controlesGen('G')}
            ${tablaFrac('G', 'Monto Neto')}
            <hr/>
            <h6 class="fw-bold text-success mb-1"><i class="bi bi-grid"></i> Exonerado (Monto Exento)</h6>
            ${controlesGen('E')}
            ${tablaFrac('E', 'Monto Exento')}`;
    } else {
        const sufijo = soloExento ? 'E' : 'G';
        const label  = soloExento ? 'Monto Exento' : 'Monto Neto';
        cuerpoModal += controlesGen(sufijo) + tablaFrac(sufijo, label);
    }

    const html = `
    <div class="modal fade" id="modalFraccionar" tabindex="-1">
        <div class="modal-dialog ${mixto ? 'modal-lg' : ''}">
            <div class="modal-content">
                <div class="modal-header py-2" style="background-color:#5b74ad;">
                    <h6 class="modal-title text-white mb-0">
                        <i class="bi bi-grid-3x3-gap"></i> Fraccionar Imputación
                    </h6>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">${cuerpoModal}</div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" class="btn btn-primary btn-sm" id="btnConfirmarFraccion">
                        <i class="bi bi-check-circle"></i> Confirmar Fraccionamiento
                    </button>
                </div>
            </div>
        </div>
    </div>`;

    $('#modalFraccionar').remove();
    $('#modalContainer').append(html);

    const modal = new bootstrap.Modal(document.getElementById('modalFraccionar'));
    modal.show();

    // Inicializar tablas
    if (mixto || !soloExento) generarFilasFraccionarSuf('G', montoNeto);
    if (mixto || soloExento)  generarFilasFraccionarSuf('E', montoExento);

    $(document).on('click', '.btn-generar', function () {
        const suf = $(this).data('sufijo');
        const monto = suf === 'G' ? montoNeto : montoExento;
        generarFilasFraccionarSuf(suf, monto);
    });

    $(document).on('input', '#bodyFraccionarG .inp-pct, #bodyFraccionarE .inp-pct', function () {
        const suf = $(this).closest('tbody').is('#bodyFraccionarG') ? 'G' : 'E';
        const monto = suf === 'G' ? montoNeto : montoExento;
        recalcularDesdePorcentajeSuf(suf, monto);
    });

    $(document).on('input', '#bodyFraccionarG .inp-monto, #bodyFraccionarE .inp-monto', function () {
        const suf = $(this).closest('tbody').is('#bodyFraccionarG') ? 'G' : 'E';
        actualizarTotalesFraccionarSuf(suf);
    });

    $('#btnConfirmarFraccion').on('click', function () {
        confirmarFraccionamiento(soloExento, mixto);
    });
}

function generarFilasFraccionarSuf(sufijo, montoTotal) {
    const n     = Math.max(2, Math.min(20, parseInt($('#txtNumLineas' + sufijo).val()) || 2));
    const tipo  = $('input[name="rdTipoDiv' + sufijo + '"]:checked').val();
    const esPct = tipo === 'porcentaje';
    const pct   = parseFloat((100 / n).toFixed(4));
    const monto = parseFloat((montoTotal / n).toFixed(2));

    let rows = '';
    for (let i = 1; i <= n; i++) {
        const esUltima  = i === n;
        const montoFila = esUltima ? parseFloat((montoTotal - monto * (n - 1)).toFixed(2)) : monto;
        const pctFila   = esUltima ? parseFloat((100 - pct * (n - 1)).toFixed(4)) : pct;

        rows += `<tr>
            <td>${i}</td>
            <td class="text-end">
                <input type="number" class="form-control form-control-sm text-end inp-pct"
                       value="${pctFila}" min="0" max="100" step="0.01"
                       style="width:80px" ${!esPct ? 'disabled' : ''} />
            </td>
            <td class="text-end">
                <input type="number" class="form-control form-control-sm text-end inp-monto"
                       value="${montoFila}" min="0" step="0.01"
                       style="width:130px" ${esPct ? 'disabled' : ''} />
            </td>
        </tr>`;
    }
    $('#bodyFraccionar' + sufijo).html(rows);
    actualizarTotalesFraccionarSuf(sufijo);
}

function recalcularDesdePorcentajeSuf(sufijo, montoTotal) {
    const filas = $('#bodyFraccionar' + sufijo + ' tr');
    filas.each(function (idx) {
        const pct   = parseFloat($(this).find('.inp-pct').val()) || 0;
        const esUlt = idx === filas.length - 1;
        if (!esUlt) {
            $(this).find('.inp-monto').val(parseFloat((montoTotal * pct / 100).toFixed(2)));
        }
    });
    const sumMonto = filas.slice(0, -1).toArray()
        .reduce((s, r) => s + (parseFloat($(r).find('.inp-monto').val()) || 0), 0);
    filas.last().find('.inp-monto').val(parseFloat((montoTotal - sumMonto).toFixed(2)));
    actualizarTotalesFraccionarSuf(sufijo);
}

function actualizarTotalesFraccionarSuf(sufijo) {
    let totalPct = 0, totalMonto = 0;
    $('#bodyFraccionar' + sufijo + ' tr').each(function () {
        totalPct   += parseFloat($(this).find('.inp-pct').val())   || 0;
        totalMonto += parseFloat($(this).find('.inp-monto').val()) || 0;
    });
    $('#tdTotalPct'  + sufijo).text(totalPct.toFixed(2) + '%');
    $('#tdTotalMonto' + sufijo).text(CorporativoCore.formatearMonto(totalMonto));
}

function leerLineasDeSufijo(sufijo) {
    const lineas = [];
    $('#bodyFraccionar' + sufijo + ' tr').each(function () {
        lineas.push({ monto: parseFloat($(this).find('.inp-monto').val()) || 0 });
    });
    return lineas;
}

function confirmarFraccionamiento(soloExento, mixto) {
    const lineasGravado = (!soloExento || mixto) ? leerLineasDeSufijo('G') : [];
    const lineasExento  = (soloExento  || mixto) ? leerLineasDeSufijo('E') : [];

    if (lineasGravado.length === 0 && lineasExento.length === 0) return;

    CorporativoQuery.ajaxPost(
        BASE_URL + '/Comprobante/FraccionarImputacion',
        { folio: $('#hdnFolio').val(), lineasGravado, lineasExento },
        function (response) {
            if (response.exito) {
                listaImputaciones = response.imputaciones;
                refrescarTabla();
                calcularTotales();
                bootstrap.Modal.getInstance(
                    document.getElementById('modalFraccionar')).hide();
                CorporativoCore.notificarExito('Imputación fraccionada correctamente.');
            }
        }
    );
}

// ════════════════════════════════════════════
// BIND EVENTOS
// ════════════════════════════════════════════

function bindEventosImputacion() {

    $('#btnAgregarDetalle').on('click', mostrarFormularioImputacion);
    $('#btnAgregarNuevaImputacion').on('click', agregarImputacion);
    $('#btnEditarDetalle').on('click', guardarEdicionImputacion);
    $('#btnCancelarDetalle').on('click', ocultarFormularioImputacion);

    $('#btnFraccionar').on('click', mostrarModalFraccionar);

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

    $('#tblImputacion').on('click', '.btn-configurar-imp', function () {
        mostrarFormularioImputacion(parseInt($(this).data('secuencia')));
    });

    $(document).on('change', '#inpFile', function () {
        if (!this.files || this.files.length === 0) return;
        const archivo = this.files[0];
        $(this).val(''); // permite re-seleccionar el mismo archivo
        procesarImputacionMasiva(archivo);
    });

    $('#btnDescargarPlantillaImputacion').on('click', descargarPlantillaImputacion);

    $('a[href="#tabImputacion"]').on('shown.bs.tab', function () {
        const folio  = $('#hdnFolio').val();
        const estado = $('#hdnCodigoEstado').val();
        const editable = estado === '' || estado === 'NUEVO' || estado === 'REGISTRADO';
        if (folio) {
            if (editable) {
                $('#barraOpcionesImputacion').removeClass('d-none');
                $('#barraAccionesImputacion').removeClass('d-none');
            }
            cargarImputaciones(folio);
        }
        actualizarBotonesRP();
    });

    // ── Códigos Unidad ────────────────────────

    $('#btnBuscarCuentaContable').on('click', buscarCuentaContable);

    $('#txtCuentaContable').on('change', function () {
        if (!CorporativoCore.esVacio($(this).val())) mostrarCodigosUnidad('Cuenta');
        else ocultarCodigosUnidad('Cuenta');
    });

    $('#btnBuscarCodUnidad1Cuenta').on('click', () => buscarCodigoUnidad('Cuenta', 1, '#txtCodUnidad1Cuenta'));
    $('#btnBuscarCodUnidad2Cuenta').on('click', () => buscarCodigoUnidad('Cuenta', 2, '#txtCodUnidad2Cuenta'));
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
