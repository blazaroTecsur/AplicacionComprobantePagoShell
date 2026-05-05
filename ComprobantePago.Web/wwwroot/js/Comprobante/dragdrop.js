// ============================================
// DRAGDROP.JS - Drag & Drop XML / PDF / ZIP
//              + Panel Documentos Electrónicos
// ============================================

// ── Configuración de categorías ───────────────
const CATEGORIAS_DOC = [
    { subTipo: 'REPRESENTACION_IMPRESA', accept: ['.pdf'] },
    { subTipo: 'XML_SUNAT',             accept: ['.xml'] },
    { subTipo: 'CORREO_ORIGEN',         accept: ['.msg', '.eml'] },
    { subTipo: 'ORDEN_COMPRA',          accept: ['.pdf'] },
    { subTipo: 'XML_CDR',              accept: ['.xml', '.zip'] }
];

// Estado interno del panel
let _docsActuales = [];
let _subTipoSeleccionado = null;

// ─────────────────────────────────────────────
$(document).ready(function () {
    inicializarDragDrop();
    bindEventosDragDrop();
});

// ── Inicializar área principal drag & drop ────
function inicializarDragDrop() {
    const area = document.getElementById('areaDragDrop');
    if (!area) return;

    area.addEventListener('dragover', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('drag_over');
    });

    area.addEventListener('dragleave', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('drag_over');
    });

    area.addEventListener('drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('drag_over');
        const archivos = e.dataTransfer.files;
        if (archivos.length === 0) return;
        procesarArchivo(archivos[0]);
    });
}

// ── Procesar archivo del drag-drop principal ──
function procesarArchivo(archivo) {
    const extension = archivo.name.split('.').pop().toLowerCase();
    const esCdr = archivo.name.startsWith('R-');

    if (esCdr && (extension === 'xml' || extension === 'zip')) {
        subirDocumentoSinValidacion(archivo, 'XML_CDR');
    } else if (extension === 'xml') {
        validarXmlSunat(archivo);
    } else if (extension === 'zip') {
        validarZipSunat(archivo);
    } else if (extension === 'pdf') {
        subirDocumentoSinValidacion(archivo, 'REPRESENTACION_IMPRESA');
    } else {
        CorporativoCore.notificarError('Solo se aceptan archivos XML, PDF o ZIP.');
    }
}

// ── Validar XML contra SUNAT ──────────────────
function validarXmlSunat(archivo) {
    const formData = new FormData();
    formData.append('archivo', archivo);

    CorporativoCore.showLoading();
    $.ajax({
        url: '/Comprobante/ValidarXmlSunat',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: { 'RequestVerificationToken': CorporativoCore.obtenerToken() },
        success: function (response) {
            CorporativoCore.hideLoading();
            if (!response.codigoEstado) {
                CorporativoCore.notificarError(
                    response.mensaje || response.motivo || 'Error al validar el archivo XML.');
                return;
            }
            mostrarResultadoSunat(response, 'xml', archivo);
        },
        error: function (xhr) {
            CorporativoCore.hideLoading();
            CorporativoCore.handleError(xhr, {
                onCustom: function () {
                    CorporativoCore.notificarError('Error al validar el archivo XML.');
                }
            });
        }
    });
}

// ── Validar ZIP contra SUNAT ──────────────────
function validarZipSunat(archivo) {
    const formData = new FormData();
    formData.append('archivo', archivo);

    CorporativoCore.showLoading();
    $.ajax({
        url: '/Comprobante/ValidarZipSunat',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: { 'RequestVerificationToken': CorporativoCore.obtenerToken() },
        success: function (response) {
            CorporativoCore.hideLoading();
            if (!response.exito && response.motivo) {
                $('#divResultadoSunat').removeClass('d-none');
                $('#alertaSunat').html(`
                    <div class="alert alert-warning d-flex align-items-center gap-2">
                        <i class="bi bi-exclamation-triangle-fill fs-5"></i>
                        <div>
                            <strong>Error al procesar ZIP</strong>
                            <p class="mb-0 small">${response.motivo}</p>
                        </div>
                    </div>`);
                return;
            }
            mostrarResultadoSunat(response, 'zip', archivo);
        },
        error: function (xhr) {
            CorporativoCore.hideLoading();
            CorporativoCore.handleError(xhr, {
                onCustom: function () {
                    CorporativoCore.notificarError('Error al procesar el archivo ZIP.');
                }
            });
        }
    });
}

// ── Subir documento sin validación SUNAT ──────
// (PDF u otros desde el drag-drop principal)
function subirDocumentoSinValidacion(archivo, subTipo) {
    const folio = $('#hdnFolio').val();
    if (!folio) {
        CorporativoCore.notificarError(
            'Primero debe validar el comprobante XML para obtener un folio.');
        return;
    }
    _subirDocumentoConFolio(archivo, folio, subTipo);
}

// ── Subir archivo a una categoría desde panel ─
function subirDocumentoCategoria(archivo) {
    const folio = $('#hdnFolio').val();
    if (!folio) {
        CorporativoCore.notificarError('No hay un folio asignado.');
        return;
    }
    if (!_subTipoSeleccionado) return;

    const cat = CATEGORIAS_DOC.find(c => c.subTipo === _subTipoSeleccionado);
    const ext = '.' + archivo.name.split('.').pop().toLowerCase();
    if (cat && !cat.accept.includes(ext)) {
        const permitidos = cat.accept.join(', ');
        CorporativoCore.notificarError(
            `Tipo de archivo no permitido. Solo se aceptan: ${permitidos}`);
        return;
    }

    _subirDocumentoConFolio(archivo, folio, _subTipoSeleccionado);
}

// ── Helper: POST a SubirDocumentos ────────────
function _subirDocumentoConFolio(archivo, folio, subTipo) {
    const formData = new FormData();
    formData.append('folio', folio);
    formData.append('subTipo', subTipo);
    formData.append('archivos', archivo);

    CorporativoCore.showLoading();
    $.ajax({
        url: '/Comprobante/SubirDocumentos',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: { 'RequestVerificationToken': CorporativoCore.obtenerToken() },
        success: function (response) {
            CorporativoCore.hideLoading();
            if (response.exito) {
                CorporativoCore.notificarExito('Documento guardado correctamente.');
                _docsActuales = response.documentos || [];
                actualizarIndicadoresCategorias(_docsActuales);
                if (_subTipoSeleccionado) {
                    renderizarPanelDerecho(_subTipoSeleccionado, _docsActuales);
                }
            } else {
                CorporativoCore.notificarError(
                    response.mensaje || 'Error al guardar el archivo.');
            }
        },
        error: function (xhr) {
            CorporativoCore.hideLoading();
            CorporativoCore.handleError(xhr, {
                onCustom: function () {
                    CorporativoCore.notificarError('Error al subir el archivo.');
                }
            });
        }
    });
}

// ── Eliminar documento ────────────────────────
function eliminarDocumento(idDocumento) {
    Swal.fire({
        title: '¿Eliminar documento?',
        text: 'Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Eliminar',
        cancelButtonText: 'Cancelar'
    }).then(function (result) {
        if (!result.isConfirmed) return;

        CorporativoCore.showLoading();
        $.ajax({
            url: '/Comprobante/EliminarDocumento',
            type: 'POST',
            data: JSON.stringify(idDocumento),
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': CorporativoCore.obtenerToken() },
            success: function (response) {
                CorporativoCore.hideLoading();
                if (response.exito) {
                    CorporativoCore.notificarExito('Documento eliminado.');
                    // Quitar del estado local
                    _docsActuales = _docsActuales.filter(
                        d => d.idDocumento !== idDocumento);
                    actualizarIndicadoresCategorias(_docsActuales);
                    if (_subTipoSeleccionado) {
                        renderizarPanelDerecho(_subTipoSeleccionado, _docsActuales);
                    }
                } else {
                    CorporativoCore.notificarError(
                        response.mensaje || 'Error al eliminar.');
                }
            },
            error: function (xhr) {
                CorporativoCore.hideLoading();
                CorporativoCore.handleError(xhr, {
                    onCustom: function () {
                        CorporativoCore.notificarError('Error al eliminar el documento.');
                    }
                });
            }
        });
    });
}

// ── Cargar documentos electrónicos ────────────
function cargarDocumentosElectronicos(folio) {
    if (!folio) return;
    $.ajax({
        url: '/Comprobante/DocumentosElectronicos',
        type: 'GET',
        data: { folio: folio },
        success: function (docs) {
            _docsActuales = docs || [];
            actualizarIndicadoresCategorias(_docsActuales);
            // Seleccionar primera categoría con archivo, o la primera de la lista
            const primera = _docsActuales.length > 0
                ? _docsActuales[0].subTipo
                : CATEGORIAS_DOC[0].subTipo;
            seleccionarCategoria(primera);
        }
    });
}

// ── Seleccionar categoría del panel izquierdo ─
function seleccionarCategoria(subTipo) {
    _subTipoSeleccionado = subTipo;

    // Resaltar en la lista
    $('#listaCategoriasDocs .doc-cat-item')
        .removeClass('doc-cat-activa');
    $(`#listaCategoriasDocs .doc-cat-item[data-subtipo="${subTipo}"]`)
        .addClass('doc-cat-activa');

    renderizarPanelDerecho(subTipo, _docsActuales);
}

// ── Renderizar panel derecho ──────────────────
function renderizarPanelDerecho(subTipo, docs) {
    const doc = docs.find(d => d.subTipo === subTipo) || null;
    const cat = CATEGORIAS_DOC.find(c => c.subTipo === subTipo);
    const accept = cat ? cat.accept.join(',') : '*';
    const panel = $('#panelContenidoDoc');

    if (doc) {
        // ── Tarjeta de archivo existente ──────
        const ext = doc.nombreArchivo.split('.').pop().toLowerCase();
        let icono, colorIcono;
        if (ext === 'pdf') {
            icono = 'bi-file-earmark-pdf-fill';
            colorIcono = '#dc3545';
        } else if (ext === 'xml') {
            icono = 'bi-file-earmark-code-fill';
            colorIcono = '#ffc107';
        } else {
            icono = 'bi-file-earmark-fill';
            colorIcono = '#6c757d';
        }

        const tamanio = doc.tamanioBytes > 1024
            ? (doc.tamanioBytes / 1024).toFixed(1) + ' KB'
            : doc.tamanioBytes + ' B';

        panel.html(`
            <div class="d-flex flex-column align-items-center justify-content-center"
                 style="min-height:240px">
                <p class="text-muted small mb-3">
                    <strong>Nombre:</strong> ${doc.nombreArchivo}
                    <span class="ms-2 text-muted">(${tamanio})</span>
                </p>
                <div class="border rounded p-4 text-center"
                     style="width:200px; border-style: dashed !important;">
                    <i class="bi ${icono} fs-1" style="color:${colorIcono}"></i>
                </div>
                <div class="mt-3 d-flex gap-2">
                    <a href="/Comprobante/DescargarDocumento?id=${doc.idDocumento}"
                       class="btn btn-success btn-sm" target="_blank">
                        <i class="bi bi-download"></i> Descargar
                    </a>
                    <button type="button" class="btn btn-danger btn-sm"
                            onclick="eliminarDocumento(${doc.idDocumento})">
                        <i class="bi bi-trash"></i> Eliminar
                    </button>
                </div>
            </div>`);
    } else {
        // ── Zona de drop vacía ────────────────
        const acceptLabel = cat ? cat.accept.join(', ') : '';
        panel.html(`
            <div id="zonaDropCategoria"
                 class="d-flex flex-column align-items-center justify-content-center"
                 style="min-height:240px; border: 2px dashed #ccc;
                        border-radius: 8px; cursor: pointer;"
                 onclick="$('#inpDocCategoria').trigger('click')">
                <i class="bi bi-cloud-upload fs-2 text-muted"></i>
                <p class="text-muted mt-2 mb-1">Arrastra el archivo aquí o haz clic</p>
                <p class="text-muted small">Tipos permitidos: <strong>${acceptLabel}</strong></p>
            </div>`);

        // Actualizar el accept del input
        $('#inpDocCategoria').attr('accept', accept);

        // Drag-drop sobre la zona de categoría
        const zona = document.getElementById('zonaDropCategoria');
        if (zona) {
            zona.addEventListener('dragover', function (e) {
                e.preventDefault();
                e.stopPropagation();
                $(this).css('background', '#f0f8ff');
            });
            zona.addEventListener('dragleave', function (e) {
                e.preventDefault();
                e.stopPropagation();
                $(this).css('background', '');
            });
            zona.addEventListener('drop', function (e) {
                e.preventDefault();
                e.stopPropagation();
                $(this).css('background', '');
                const files = e.dataTransfer.files;
                if (files.length > 0) subirDocumentoCategoria(files[0]);
            });
        }
    }
}

// ── Actualizar indicadores de categorías ──────
function actualizarIndicadoresCategorias(docs) {
    $('#listaCategoriasDocs .doc-cat-item').each(function () {
        const st = $(this).data('subtipo');
        const tiene = docs.some(d => d.subTipo === st);
        $(this).find('.doc-cat-badge').remove();
        if (tiene) {
            $(this).append(
                '<span class="doc-cat-badge"></span>');
        }
    });
}

// ── Mostrar resultado validación SUNAT ────────
function mostrarResultadoSunat(response, tipo, archivo) {
    $('#divResultadoSunat').removeClass('d-none');

    const estado = response.estadoSunat;
    const codigo = response.codigoEstado;
    let alerta = '';

    switch (codigo) {
        case '1': // ACEPTADO
            alerta = `
                <div class="alert alert-success d-flex align-items-center gap-2">
                    <i class="bi bi-check-circle-fill fs-5"></i>
                    <div>
                        <strong>Comprobante ACEPTADO por SUNAT</strong>
                        <p class="mb-0 small">
                            El archivo ha sido validado correctamente.
                            Se poblarán los campos automáticamente.
                        </p>
                    </div>
                </div>`;
            Swal.fire({
                title: 'Comprobante validado',
                html: `
                    Se registró el comprobante con folio:
                    <strong>${response.folio}</strong>.<br>
                    Este ya ha sido validado ante SUNAT.<br><br>
                    Favor de proceder a completar los datos y
                    <strong>ENVIAR</strong> el comprobante vía SyteLine.
                `,
                icon: 'info',
                confirmButtonText: 'Aceptar',
                confirmButtonColor: '#185FA5'
            }).then(() => {
                $('#hdnFolio').val(response.folio);
                $('#txtFolio').val(response.folio).addClass('folio-generado');
                poblarCamposDesdeXml(response.datos);
                mostrarVistaDetalle();
                cargarDocumentosElectronicos(response.folio);
                // ZIP: el backend extrae y guarda los documentos internos (incluido R-*.zip CDR)
                // XML: guardar el archivo validado como XML_SUNAT
                if (archivo && tipo === 'xml') {
                    _subirDocumentoConFolio(archivo, response.folio, 'XML_SUNAT');
                }
            });
            break;

        case '3': // AUTORIZADO
            alerta = `
                <div class="alert alert-success d-flex align-items-center gap-2">
                    <i class="bi bi-check-circle-fill fs-5"></i>
                    <div>
                        <strong>Comprobante AUTORIZADO por imprenta</strong>
                        <p class="mb-0 small">
                            El comprobante tiene autorización de imprenta.
                            Se poblarán los campos automáticamente.
                        </p>
                    </div>
                </div>`;
            Swal.fire({
                title: 'Comprobante validado',
                html: `
                    Se registró el comprobante con folio:
                    <strong>${response.folio}</strong>.<br>
                    Este ya ha sido validado ante SUNAT.<br><br>
                    Favor de proceder a completar los datos y
                    <strong>ENVIAR</strong> el comprobante vía SyteLine.
                `,
                icon: 'info',
                confirmButtonText: 'Aceptar',
                confirmButtonColor: '#185FA5'
            }).then(() => {
                $('#hdnFolio').val(response.folio);
                $('#txtFolio').val(response.folio).addClass('folio-generado');
                poblarCamposDesdeXml(response.datos);
                mostrarVistaDetalle();
                cargarDocumentosElectronicos(response.folio);
                // ZIP: el backend extrae y guarda los documentos internos (incluido R-*.zip CDR)
                // XML: guardar el archivo validado como XML_SUNAT
                if (archivo && tipo === 'xml') {
                    _subirDocumentoConFolio(archivo, response.folio, 'XML_SUNAT');
                }
            });
            break;

        case '2': // ANULADO
            alerta = `
                <div class="alert alert-warning d-flex align-items-center gap-2">
                    <i class="bi bi-exclamation-triangle-fill fs-5"></i>
                    <div>
                        <strong>Comprobante ANULADO en SUNAT</strong>
                        <p class="mb-0 small">
                            Este comprobante fue comunicado en una baja.
                            No puede ser procesado.
                        </p>
                    </div>
                </div>`;
            break;

        case '4': // NO AUTORIZADO
            alerta = `
                <div class="alert alert-danger d-flex align-items-center gap-2">
                    <i class="bi bi-x-circle-fill fs-5"></i>
                    <div>
                        <strong>Comprobante NO AUTORIZADO por imprenta</strong>
                        <p class="mb-0 small">
                            Este comprobante no fue autorizado por imprenta.
                            No puede ser procesado.
                        </p>
                    </div>
                </div>`;
            break;

        case '0': // NO EXISTE
            alerta = `
                <div class="alert alert-secondary d-flex align-items-center gap-2">
                    <i class="bi bi-question-circle-fill fs-5"></i>
                    <div>
                        <strong>Comprobante NO EXISTE en SUNAT</strong>
                        <p class="mb-0 small">
                            Este comprobante no ha sido informado a SUNAT.
                            Verifique los datos del archivo.
                        </p>
                    </div>
                </div>`;
            break;

        case 'ERROR':
            alerta = `
                <div class="alert alert-danger d-flex align-items-center gap-2">
                    <i class="bi bi-exclamation-triangle-fill fs-5"></i>
                    <div>
                        <strong>Error al consultar SUNAT</strong>
                        <p class="mb-0 small">${response.motivo || 'No se pudo conectar con el servicio de validación.'}</p>
                    </div>
                </div>`;
            break;

        default:
            alerta = `
                <div class="alert alert-secondary d-flex align-items-center gap-2">
                    <i class="bi bi-question-circle-fill fs-5"></i>
                    <div>
                        <strong>Estado desconocido: ${estado}</strong>
                        <p class="mb-0 small">${response.motivo || ''}</p>
                    </div>
                </div>`;
    }

    $('#alertaSunat').html(alerta);
}

// ── Poblar campos desde XML/PDF ───────────────
function poblarCamposDesdeXml(datos) {
    if (!datos) return;

    $('#txtFolio').val($('#hdnFolio').val());
    $('#txtNumeroDocumentoIdentidad').val(datos.ruc);
    $('#txtRazonSocial').val(datos.razonSocial);
    $('#txtRucBenef').val(datos.ruc);
    $('#txtRazonSocialBenef').val(datos.razonSocial);
    $('#txtSerie').val(datos.serie);
    $('#txtNumero').val(datos.numero);
    poblarFecha('#txtFechaEmision', datos.fechaEmision);

    esperarComboYAsignar('#ddlTipoDocumento', 'FP');
    esperarComboYAsignar('#ddlTipoSunat', datos.tipoSunat);
    esperarComboYAsignar('#ddlMoneda', datos.moneda);
    esperarComboYAsignar('#dldLugarPago', '04');

    // Tipo de cambio: viene calculado desde el servidor (PEN=1, otras monedas del XML)
    if (datos.tasaCambio && datos.tasaCambio > 0) {
        $('#txtTasaCambio').val(datos.tasaCambio.toFixed(3));
    }

    const montoNeto      = datos.montoNeto      || 0;
    const montoExento    = datos.montoExento    || 0;
    const montoIGV       = datos.montoIGVCredito || datos.montoIGV || 0;
    const montoTotal     = datos.montoTotal     || 0;
    const montoRetencion = datos.montoRetencion || 0;
    const montoBruto     = datos.montoBruto     || 0;

    $('#MontoNeto').removeClass('d-none');
    $('#txtMontoNeto').val(CorporativoCore.formatearMonto(montoNeto));

    $('#MontoExento').removeClass('d-none');
    $('#txtMontoExento').val(CorporativoCore.formatearMonto(montoExento));

    // Subtotal = Monto Neto + Exento
    $('#MontoSubtotal').removeClass('d-none');
    $('#txtMontoSubtotal').val(CorporativoCore.formatearMonto(montoNeto + montoExento));

    $('#MontoIGVCredito').removeClass('d-none');
    $('#txtMontoIGVCredito').val(CorporativoCore.formatearMonto(montoIGV));
    // Porcentaje IGV directo del XML
    const pctIGV = (datos.porcentajeIGV && datos.porcentajeIGV > 0)
        ? parseFloat(datos.porcentajeIGV).toFixed(2)
        : '0.00';
    $('#txtMontoIGVCreditoPorcentajeIGV').val(pctIGV);

    $('#MontoTotal').removeClass('d-none');
    $('#txtMontoTotal').val(CorporativoCore.formatearMonto(montoTotal));

    $('#MontoRetencion').removeClass('d-none');
    $('#txtMontoRetencion').val(CorporativoCore.formatearMonto(montoRetencion));
    // Editable si retención es 0
    $('#txtMontoRetencion').prop('readonly', montoRetencion !== 0);

    $('#MontoBruto').removeClass('d-none');
    $('#txtMontoBruto').val(CorporativoCore.formatearMonto(montoBruto));

    if (datos.plazoPago != null && datos.plazoPago !== '') {
        $('#txtPlazoPago').val(datos.plazoPago);
        const fechaVenc = calcularFechaVencimiento(
            datos.fechaEmision, parseInt(datos.plazoPago));
        if (fechaVenc) {
            poblarFecha('#txtFechaVencimiento', fechaVenc);
            $('#panelFechaVencimiento').removeClass('d-none');
        }
    }

    $('#hdnEstadoSunat').val('ACEPTADO');
    $('#hdnEsDocumentoElectronico').val('S');
    $('#rdoFacturacionElectronica').prop('checked', true);

    if (datos.tieneDetraccion) {
        $('#TieneDetraccion').prop('checked', true);
        $('#NoTieneDetraccion').prop('checked', false);
        esperarComboYAsignar('#ddlTipoDetraccion', datos.codigoDetraccion);
        $('#txtPorcentajeDetraccion')
            .val(CorporativoCore.formatearMonto(datos.porcentajeDetraccion))
            .prop('readonly', false);
        $('#txtConstanciaDeposito').prop('readonly', false);
        $('#txtFechaDeposito').prop('disabled', false);
        const montoDetraccion = datos.montoDetraccion > 0
            ? datos.montoDetraccion
            : (datos.montoTotal * datos.porcentajeDetraccion / 100);
        $('#hdnMontoDetraccion').val(montoDetraccion.toFixed(2));
    } else {
        $('#TieneDetraccion').prop('checked', false);
        $('#NoTieneDetraccion').prop('checked', true);
        $('#ddlTipoDetraccion').prop('disabled', true);
        $('#txtPorcentajeDetraccion').prop('readonly', true);
        $('#txtConstanciaDeposito').prop('readonly', true);
        $('#txtFechaDeposito').prop('disabled', true);
    }

    // Documento Asociado (Nota de Crédito / Débito)
    if (datos.tipoDocumentoAsociado || datos.serieAsociado || datos.numeroAsociado) {
        esperarComboYAsignar('#ddlTipoDocumentoAsociado', datos.tipoDocumentoAsociado);
        $('#txtSerieAsociado').val(datos.serieAsociado || '');
        $('#txtNumeroAsociado').val(datos.numeroAsociado || '');
    }

    bloquearCamposElectronico();
    CorporativoCore.notificarExito('Campos poblados correctamente.');
}

// ── Mostrar vista de detalle ──────────────────
function mostrarVistaDetalle() {
    $('#divCorreo').addClass('d-none');
    $('#Resultados').removeClass('d-none');
    mostrarBotonesNuevo();
    habilitarCamposManuales();
    $('#barraOpcionesImputacion').removeClass('d-none');
    $('#barraAccionesImputacion').removeClass('d-none');
}

// ── Habilitar campos manuales post-folio ──────
function habilitarCamposManuales() {
    $('#txtFechaRecepcion').prop('disabled', false);
    $('#txtCR').prop('readonly', false);
    $('#dldLugarPago').prop('disabled', false);
    $('#txtPlazoPago').prop('readonly', false);
    $('#ddlMoneda').prop('disabled', false);
    $('#txtTasaCambio').prop('readonly', false);
    $('#txtObservacion').prop('readonly', false);
    $('#btnBuscarCRPrincipal').prop('disabled', false);
    // btnBuscarProveedorPrincipal queda bloqueado: proveedor viene del XML
}

// ── Bind eventos ──────────────────────────────
function bindEventosDragDrop() {

    // Botón seleccionar archivo (drag-drop principal)
    $('#btnSeleccionarArchivo').on('click', function () {
        $('#inpArchivoComprobante').trigger('click');
    });

    $('#inpArchivoComprobante').on('change', function () {
        if (this.files.length > 0) {
            procesarArchivo(this.files[0]);
            $(this).val('');
        }
    });

    $('#btnLimpiarCorreo').on('click', function () {
        $('#divResultadoSunat').addClass('d-none');
        $('#alertaSunat').html('');
        $('#areaDragDrop').removeClass('drag_over');
        CorporativoCore.notificarInfo('Área limpiada.');
    });

    // Clic en categoría del panel izquierdo
    $(document).on('click', '.doc-cat-item', function () {
        const subTipo = $(this).data('subtipo');
        seleccionarCategoria(subTipo);
    });

    // Input file del panel de categorías
    $('#inpDocCategoria').on('change', function () {
        if (this.files.length > 0) {
            subirDocumentoCategoria(this.files[0]);
            $(this).val('');
        }
    });

    // Cargar docs al activar la pestaña
    $('a[href="#tabDocElectronicos"]').on('shown.bs.tab', function () {
        const folio = $('#hdnFolio').val();
        if (folio) cargarDocumentosElectronicos(folio);
    });
}

// ── Esperar combo y asignar valor ─────────────
function esperarComboYAsignar(selector, valor) {
    if (!valor) return;

    const intervalo = setInterval(function () {
        const combo = $(selector);
        if (combo.find('option').length > 1) {
            combo.val(valor);
            clearInterval(intervalo);
        }
    }, 100);

    setTimeout(function () {
        clearInterval(intervalo);
        const combo = $(selector);
        if (!combo.val() || combo.val() !== valor) {
            combo.append(`<option value="${valor}" selected>${valor}</option>`);
        }
    }, 5000);
}

// ── Poblar fecha con Flatpickr ────────────────
function poblarFecha(selector, valor) {
    if (!valor) return;
    const partes = valor.split('/');
    if (partes.length === 3) {
        $(selector).val(`${partes[2]}-${partes[1]}-${partes[0]}`);
    } else {
        $(selector).val(valor);
    }
}
