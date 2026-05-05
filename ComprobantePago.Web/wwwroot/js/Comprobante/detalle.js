// ============================================
// DETALLE.JS - Registro de Comprobantes
// ============================================

$(document).ready(function () {
    if ($('#tabsDetalle').length > 0) {
        inicializar();
        bindEventos();
    }
});

// ── Inicialización ────────────────────────────
function inicializar() {
    inicializarFechas();

    const folio = obtenerParametroUrl('folio');
    if (folio) {
        $('#hdnFolio').val(folio);
        $('#divCorreo').addClass('d-none');
        $('#Resultados').removeClass('d-none');
        cargarCombos();
        cargarComprobante(folio);
    } else {
        $('#divCorreo').removeClass('d-none');
        $('#Resultados').addClass('d-none');
        $('#btnLimpiarCorreo').removeClass('d-none');
        cargarCombos();
        bloquearCamposElectronico();
    }
}

// ── Inicializar fechas HTML5 ──────────────────
function inicializarFechas() {
    $('#txtFechaVencimiento').on('change', function () {
        const recepcion = $('#txtFechaRecepcion').val();
        if (recepcion && $(this).val()) {
            const fechaRecepcion = new Date(recepcion);
            const fechaVenc = new Date($(this).val());
            const diff = Math.round(
                (fechaVenc - fechaRecepcion) / (1000 * 60 * 60 * 24));
            if (diff >= 0) $('#txtPlazoPago').val(diff);
        }
    });
}

// ── Cargar combos ─────────────────────────────
function cargarCombos() {
    cargarTipoDocumento();
    cargarTipoSunat();
    cargarMoneda();
    cargarLugarPago();
    cargarTipoDetraccion();
    cargarTipoDocumentoAsociado();
}

function cargarTipoDocumento() {
    CorporativoQuery.ajaxGet('/Comprobante/ObtenerTiposDocumento',
        function (data) {
            let options = '<option value="">-- Seleccione --</option>';
            data.forEach(t =>
                options += `<option value="${t.codigo}">${t.descripcion}</option>`);
            $('#ddlTipoDocumento').html(options).prop('disabled', false);
        });
}

function cargarTipoSunat() {
    CorporativoQuery.ajaxGet('/Comprobante/ObtenerTiposSunat',
        function (data) {
            let options = '<option value="">-- Seleccione --</option>';
            data.forEach(t =>
                options += `<option value="${t.codigo}">${t.descripcion}</option>`);
            $('#ddlTipoSunat').html(options).prop('disabled', false);
        });
}

function cargarMoneda() {
    CorporativoQuery.ajaxGet('/Comprobante/ObtenerMonedas',
        function (data) {
            let options = '<option value="">-- Seleccione --</option>';
            data.forEach(m =>
                options += `<option value="${m.codigo}">${m.descripcion}</option>`);
            $('#ddlMoneda').html(options).prop('disabled', false);
        });
}

function cargarLugarPago() {
    CorporativoQuery.ajaxGet('/Comprobante/ObtenerLugaresPago',
        function (data) {
            let options = '<option value="">-- Seleccione --</option>';
            data.forEach(l =>
                options += `<option value="${l.codigo}">${l.descripcion}</option>`);
            $('#dldLugarPago').html(options).prop('disabled', false);
        });
}

function cargarTipoDetraccion() {
    CorporativoQuery.ajaxGet('/Comprobante/ObtenerTiposDetraccion',
        function (data) {
            const combo = $('#ddlTipoDetraccion');
            combo.empty().append(
                $('<option>', { value: '', text: '-- Seleccione --' }));
            data.forEach(function (item) {
                combo.append($('<option>', {
                    value: item.codigo,
                    text: item.descripcion,
                    'data-porcentaje': item.porcentaje
                }));
            });
        });
}

function cargarTipoDocumentoAsociado() {
    CorporativoQuery.ajaxGet('/Comprobante/ObtenerTiposDocumento',
        function (data) {
            let options = '<option value="">-- Seleccione --</option>';
            data.forEach(t =>
                options += `<option value="${t.codigo}">${t.descripcion}</option>`);
            $('#ddlTipoDocumentoAsociado').html(options);
        });
}

// ── Cargar comprobante existente ──────────────
function cargarComprobante(folio) {
    CorporativoQuery.ajaxGet(
        `/Comprobante/ObtenerDetalle?folio=${folio}`,
        function (data) {
            if (!data) {
                CorporativoCore.notificarError(
                    'No se encontró el comprobante.');
                return;
            }
            // Esperar combos y luego poblar
            setTimeout(() => {
                poblarCabecera(data);
                mostrarBotonesSegunEstado(data.codigoEstado);
                // Si es electrónico bloquear campos
                if (data.esDocumentoElectronico === 'S') {
                    bloquearCamposElectronico();
                } else {
                    habilitarCamposManual();
                }
            }, 500);
        });
}

// ── Poblar campos de cabecera ─────────────────
function poblarCabecera(data) {
    $('#txtFolio').val(data.folio);
    $('#hdnFolio').val(data.folio);
    $('#txtNumeroDocumentoIdentidad').val(data.ruc);
    $('#txtRazonSocial').val(data.razonSocial);
    $('#txtSerie').val(data.serie);
    $('#txtNumero').val(data.numero);
    $('#txtFechaEmision').val(data.fechaEmision);
    $('#txtFechaRecepcion').val(data.fechaRecepcion);
    $('#txtTasaCambio').val(data.tasaCambio);
    $('#txtCR').val(data.centroResponsabilidad);
    $('#txtDesCR').val(data.descripcionCR);
    $('#txtObservacion').val(data.observacion);
    $('#txtPlazoPago').val(data.plazoPago);
    $('#txtRucBenef').val(data.rucBenef);
    $('#txtRazonSocialBenef').val(data.razonSocialBenef);
    $('#txtRolDigitacion').val(data.rolDigitacion);
    $('#txtFechaDigitacion').val(data.fechaDigitacion);
    $('#txtRolAutorizacion').val(data.rolAutorizacion);
    $('#txtFechaAutorizacion').val(data.fechaAutorizacion);
    $('#txtRolAprobacion').val(data.rolAprobacion);
    $('#txtFechaAprobacion').val(data.fechaAprobacion);
    $('#txtEstadoComprobante').val(data.estado);
    $('#txtRolAnulacion').val(data.rolAnulacion);
    $('#txtFechaAnulacion').val(data.fechaAnulacion);
    $('#txtMensaje').val(data.mensaje);

    // Hidden fields
    $('#hdnCodigoEstado').val(data.codigoEstado);
    $('#hdnEsDocumentoElectronico').val(data.esDocumentoElectronico);
    $('#hdnRequiereDetraccion').val(data.requiereDetraccion);
    $('#hdnAplicaIGV').val(data.aplicaIGV);
    $('#hdnOrigen').val(data.origen);

    // Combos — usar esperarComboYAsignar para garantizar carga
    esperarComboYAsignar('#ddlTipoDocumento', data.tipoDocumento);
    esperarComboYAsignar('#ddlTipoSunat', data.tipoSunat);
    esperarComboYAsignar('#ddlMoneda', data.moneda);
    esperarComboYAsignar('#dldLugarPago', data.lugarPago);

    // Montos
    poblarMontos(data);

    // Detracción
    if (data.tieneDetraccion) {
        $('#TieneDetraccion').prop('checked', true);
        $('#NoTieneDetraccion').prop('checked', false);
        habilitarCamposDetraccion(true);
        esperarComboYAsignar('#ddlTipoDetraccion', data.tipoDetraccion);
        $('#txtPorcentajeDetraccion').val(data.porcentajeDetraccion);
        $('#txtConstanciaDeposito').val(data.constanciaDeposito);
        $('#txtFechaDeposito').val(data.fechaDeposito);
    } else {
        $('#TieneDetraccion').prop('checked', false);
        $('#NoTieneDetraccion').prop('checked', true);
        habilitarCamposDetraccion(false);
    }

    // Fecha vencimiento
    if (data.plazoPago) {
        $('#panelFechaVencimiento').removeClass('d-none');
        $('#txtFechaVencimiento').val(data.fechaVencimiento);
    }

    // Aduana
    if (data.requiereAduana === 'S') {
        $('#panelCodAduana').removeClass('d-none');
        $('#panelAnoAduana').removeClass('d-none');
    }

    // Radio facturación
    if (data.esDocumentoElectronico === 'S') {
        $('#rdoFacturacionElectronica').prop('checked', true);
        $('#rdoFacturacionManual').prop('checked', false);
    } else {
        $('#rdoFacturacionManual').prop('checked', true);
        $('#rdoFacturacionElectronica').prop('checked', false);
    }

    // Empleado
    if (data.esEmpleado) {
        $('#chkEsEmpleado').prop('checked', true);
        $('#hdnEmpleadoCodigo').val(data.empleadoCodigo);
        $('#hdnEmpleadoNombre').val(data.empleadoNombre);
        $('#txtEmpleadoDisplay').val(`${data.empleadoCodigo} - ${data.empleadoNombre}`);
        $('#divSelectorEmpleado').removeClass('d-none');
    }

    // Habilitar campos post-folio
    habilitarCamposManuales();
}

// ── Poblar montos dinámicos ───────────────────
function poblarMontos(data) {
    const montoNeto      = data.montoNeto      || 0;
    const montoExento    = data.montoExento    || 0;
    const montoIGV       = data.montoIGVCredito || 0;
    const montoTotal     = data.montoTotal     || 0;
    const montoRetencion = data.montoRetencion || 0;
    const montoBruto     = data.montoBruto     || 0;

    $('#MontoNeto').removeClass('d-none');
    $('#txtMontoNeto').val(CorporativoCore.formatearMonto(montoNeto));

    $('#MontoExento').removeClass('d-none');
    $('#txtMontoExento').val(CorporativoCore.formatearMonto(montoExento));

    // Subtotal = Monto Neto + Exento (base total antes de IGV)
    $('#MontoSubtotal').removeClass('d-none');
    $('#txtMontoSubtotal').val(CorporativoCore.formatearMonto(montoNeto + montoExento));

    if (data.montoIGVCosto) {
        $('#MontoIGVCosto').removeClass('d-none');
        $('#lblMontoIGVCosto').text(data.lblMontoIGVCosto || 'IGV Costo');
        $('#txtMontoIGVCosto').val(CorporativoCore.formatearMonto(data.montoIGVCosto));
    }

    $('#MontoIGVCredito').removeClass('d-none');
    $('#txtMontoIGVCredito').val(CorporativoCore.formatearMonto(montoIGV));
    // Porcentaje IGV directo del XML/BD; si es 0 no mostrar nada
    const pctIGV = (data.porcentajeIGV && data.porcentajeIGV > 0)
        ? parseFloat(data.porcentajeIGV).toFixed(2)
        : '0.00';
    $('#txtMontoIGVCreditoPorcentajeIGV').val(pctIGV);

    $('#MontoTotal').removeClass('d-none');
    $('#txtMontoTotal').val(CorporativoCore.formatearMonto(montoTotal));

    $('#MontoRetencion').removeClass('d-none');
    $('#txtMontoRetencion').val(CorporativoCore.formatearMonto(montoRetencion));
    // Editable solo si la retención es 0 (el usuario la ingresará manualmente)
    $('#txtMontoRetencion').prop('readonly', montoRetencion !== 0);

    $('#MontoBruto').removeClass('d-none');
    $('#txtMontoBruto').val(CorporativoCore.formatearMonto(montoBruto));
}

function bloquearTodosLosCampos() {
    // Campos del detalle
    $('#btnBuscarProveedorPrincipal, #btnBuscarRucBenef, #btnBuscarCRPrincipal')
        .prop('disabled', true);
    $('#ddlTipoDocumento, #ddlTipoSunat, #ddlMoneda, #dldLugarPago, #ddlTipoDetraccion')
        .prop('disabled', true);
    $('#TieneDetraccion, #NoTieneDetraccion').prop('disabled', true);
    $('#txtSerie, #txtNumero, #txtFechaRecepcion, #txtCR, #txtPlazoPago, #txtTasaCambio, ' +
      '#txtObservacion, #txtRucBenef, #txtRazonSocialBenef, ' +
      '#txtPorcentajeDetraccion, #txtConstanciaDeposito, ' +
      '#txtMontoIGVCreditoPorcentajeIGV')
        .prop('readonly', true);
    $('#txtFechaEmision, #txtFechaDeposito').prop('disabled', true);
    $('.monto').prop('readonly', true);
    // Empleado
    $('#btnBuscarEmpleado').prop('disabled', true);
    $('#chkEsEmpleado').prop('disabled', true);
    // Barras de imputación
    $('#barraOpcionesImputacion, #barraAccionesImputacion').addClass('d-none');
}

// ── Mostrar botones según estado ──────────────
function mostrarBotonesSegunEstado(estado) {
    ocultarTodosLosBotones();
    $('#btnAtras').removeClass('d-none');

    switch (estado) {
        case 'NUEVO':
            mostrarBotonesNuevo();
            break;
        case 'REGISTRADO':
            $('#btnRegistrar, #btnEnviar, #btnLimpiar')
                .removeClass('d-none');
            $('#btnImprimirComprobante, #btnVistaPrevia')
                .removeClass('d-none');
            $('#btnAnular, #btnModoImputacion')
                .removeClass('d-none');
            break;
        case 'ENVIADO':
            bloquearTodosLosCampos();
            $('#btnAutorizarDetalle, #btnImprimirComprobante')
                .removeClass('d-none');
            $('#btnVistaPrevia, #btnAnular').removeClass('d-none');
            break;
        case 'AUTORIZADO':
            bloquearTodosLosCampos();
            $('#btnAprobarDetalle, #btnImprimirComprobante')
                .removeClass('d-none');
            $('#btnVistaPrevia, #btnAnular').removeClass('d-none');
            break;
        case 'APROBADO':
            bloquearTodosLosCampos();
            $('#btnImprimirComprobante, #btnVistaPrevia')
                .removeClass('d-none');
            break;
        case 'ANULADO':
            bloquearTodosLosCampos();
            $('#btnImprimirComprobante, #btnVistaPrevia')
                .removeClass('d-none');
            break;
        case 'DERIVADO SYT':
            bloquearTodosLosCampos();
            $('#btnImprimirComprobante, #btnVistaPrevia')
                .removeClass('d-none');
            break;
    }
}

function mostrarBotonesNuevo() {
    ocultarTodosLosBotones();
    $('#btnLimpiar, #btnRegistrar, #btnAtras, #btnModoImputacion')
        .removeClass('d-none');
}

function ocultarTodosLosBotones() {
    $('#barraAcciones .btn').addClass('d-none');
}

// ── Guardar comprobante ───────────────────────
function guardarComprobante() {
    if (!validarCabecera()) return;

    // Validar mínimo 3 líneas de imputación (solo si el comprobante ya tiene folio asignado)
    const folioActual = $('#hdnFolio').val();
    if (folioActual && typeof listaImputaciones !== 'undefined' && listaImputaciones.length < 3) {
        CorporativoCore.notificarAdvertencia(
            'Debe registrar al menos 3 líneas de imputación contable antes de guardar.');
        return;
    }

    CorporativoQuery.ajaxPost(
        '/Comprobante/Guardar',
        { comprobante: obtenerDatosCabecera() },
        function (response) {
            if (response.exito) {
                CorporativoCore.notificarExito(
                    'Comprobante guardado correctamente.');
                $('#txtFolio').val(response.folio);
                $('#hdnFolio').val(response.folio);
                mostrarBotonesSegunEstado('REGISTRADO');
                $('#barraOpcionesImputacion').removeClass('d-none');
                $('#barraAccionesImputacion').removeClass('d-none');
            } else {
                CorporativoCore.notificarError(response.mensaje);
            }
        }
    );
}

// ── Obtener datos de cabecera ─────────────────
function obtenerDatosCabecera() {
    return {
        folio: $('#hdnFolio').val(),
        ruc: $('#txtNumeroDocumentoIdentidad').val(),
        razonSocial: $('#txtRazonSocial').val(),
        tipoDocumento: $('#ddlTipoDocumento').val(),
        tipoSunat: $('#ddlTipoSunat').val(),
        serie: $('#txtSerie').val(),
        numero: $('#txtNumero').val(),
        fechaEmision: $('#txtFechaEmision').val(),
        fechaRecepcion: $('#txtFechaRecepcion').val(),
        moneda: $('#ddlMoneda').val(),
        tasaCambio: parseFloat($('#txtTasaCambio').val()) || 1,
        centroResponsabilidad: $('#txtCR').val(),
        lugarPago: $('#dldLugarPago').val(),
        plazoPago: $('#txtPlazoPago').val(),
        fechaVencimiento: $('#txtFechaVencimiento').val(),
        rucBenef: $('#txtRucBenef').val(),
        observacion: $('#txtObservacion').val(),
        ordenCompra: $('#txtOrdenCompra').val(),
        factMultiple: $('#chkFactMultiple').is(':checked'),
        tieneDetraccion: $('#TieneDetraccion').is(':checked'),
        tipoDetraccion: $('#ddlTipoDetraccion').val(),
        porcentajeDetraccion: parseFloat(
            $('#txtPorcentajeDetraccion').val()
                .replace(',', '.')) || 0,
        montoDetraccion: parseFloat(
            $('#hdnMontoDetraccion').val()) || 0,
        constanciaDeposito: $('#txtConstanciaDeposito').val(),
        fechaDeposito: $('#txtFechaDeposito').val(),
        esDocumentoElectronico: $('#rdoFacturacionElectronica')
            .is(':checked') ? 'S' : 'N',
        aplicaIGV: $('#hdnAplicaIGV').val(),
        origen: $('#hdnOrigen').val(),
        montoNeto: CorporativoCore.limpiarMonto($('#txtMontoNeto').val()),
        montoExento: CorporativoCore.limpiarMonto($('#txtMontoExento').val()),
        porcentajeIGV: parseFloat($('#txtMontoIGVCreditoPorcentajeIGV').val()) || 0,
        montoIGVCredito: CorporativoCore.limpiarMonto($('#txtMontoIGVCredito').val()),
        montoTotal: CorporativoCore.limpiarMonto($('#txtMontoTotal').val()),
        montoBruto: CorporativoCore.limpiarMonto($('#txtMontoBruto').val()),
        montoRetencion: CorporativoCore.limpiarMonto($('#txtMontoRetencion').val()),
        esEmpleado: $('#chkEsEmpleado').is(':checked'),
        empleadoCodigo: $('#hdnEmpleadoCodigo').val(),
        empleadoNombre: $('#hdnEmpleadoNombre').val()
    };
}

// ── Validaciones ──────────────────────────────
function validarCabecera() {
    const campos = [
        {
            selector: '#txtNumeroDocumentoIdentidad',
            msg: 'Debe ingresar el RUC del proveedor.'
        },
        {
            selector: '#ddlTipoDocumento',
            msg: 'Debe seleccionar el tipo de comprobante.'
        },
        {
            selector: '#txtSerie',
            msg: 'Debe ingresar la serie.'
        },
        {
            selector: '#txtNumero',
            msg: 'Debe ingresar el número.'
        },
        {
            selector: '#txtFechaEmision',
            msg: 'Debe ingresar la fecha de emisión.'
        },
        {
            selector: '#ddlMoneda',
            msg: 'Debe seleccionar la moneda.'
        },
    ];

    for (const campo of campos) {
        if (CorporativoCore.esVacio($(campo.selector).val())) {
            CorporativoCore.notificarAdvertencia(campo.msg);
            $(campo.selector).focus();
            return false;
        }
    }
    return true;
}

// ── Limpiar formulario ────────────────────────
function limpiarFormulario() {
    // Campos texto
    $('#txtFolio, #txtNumeroDocumentoIdentidad, #txtRazonSocial').val('');
    $('#ddlTipoDocumento, #ddlTipoSunat, #txtSerie, #txtNumero').val('');
    $('#txtFechaEmision, #txtFechaRecepcion, #ddlMoneda, #txtTasaCambio').val('');
    $('#txtCR, #txtDesCR, #dldLugarPago, #txtPlazoPago').val('');
    $('#txtFechaVencimiento, #txtRucBenef, #txtRazonSocialBenef').val('');
    $('#txtObservacion, #txtOrdenCompra').val('');
    $('#chkFactMultiple').prop('checked', false);

    // Detracción
    $('#TieneDetraccion').prop('checked', false);
    $('#NoTieneDetraccion').prop('checked', true);
    $('#ddlTipoDetraccion, #txtPorcentajeDetraccion').val('');
    $('#txtConstanciaDeposito, #txtFechaDeposito').val('');
    $('#hdnMontoDetraccion').val('0');
    habilitarCamposDetraccion(false);

    // Montos
    $('#txtMontoNeto, #txtMontoExento, #txtMontoIGVCosto').val('');
    $('#txtMontoIGVCredito, #txtMontoTotal, #txtMontoBruto').val('');
    $('#txtMontoRetencion, #txtMontoMultas, #txtValorAduana').val('');

    // Estado
    $('#txtRolDigitacion, #txtRolAutorizacion, #txtRolAprobacion').val('');
    $('#txtFechaAprobacion, #txtEstadoComprobante').val('');
    $('#txtRolAnulacion, #txtFechaAnulacion, #txtMensaje').val('');

    // Hidden
    $('#hdnFolio, #hdnCodigoEstado').val('');

    // Paneles condicionales
    $('#panelFechaVencimiento, #panelCodAduana, #panelAnoAduana')
        .addClass('d-none');
    $('#divOrdenCompra, #divTipoOrden').addClass('d-none');

    // Montos dinámicos
    $('#MontoNeto, #MontoExento, #MontoSubtotal, #MontoIGVCosto, #MontoIGVCredito')
        .addClass('d-none');
    $('#MontoTotal, #MontoBruto, #MontoRetencion, #MontoMultas, #ValorAduana')
        .addClass('d-none');

    // Restaurar vista inicial
    $('#divCorreo').removeClass('d-none');
    $('#Resultados').addClass('d-none');
    $('#divResultadoSunat').addClass('d-none');
    $('#alertaSunat').html('');

    // Restaurar radio a electrónica
    $('#rdoFacturacionElectronica').prop('checked', true);
    $('#rdoFacturacionManual').prop('checked', false);

    // Bloquear campos electrónicos
    bloquearCamposElectronico();

    // Ocultar botones
    ocultarTodosLosBotones();
    $('#btnLimpiarCorreo').removeClass('d-none');
}

// ── Bloquear campos modo electrónico ──────────
function bloquearCamposElectronico() {
    $('#txtNumeroDocumentoIdentidad').prop('readonly', true);
    $('#txtRazonSocial').prop('readonly', true);
    $('#txtRucBenef').prop('readonly', true);
    $('#txtRazonSocialBenef').prop('readonly', true);
    $('#txtSerie').prop('readonly', true);
    $('#txtNumero').prop('readonly', true);
    $('#txtFechaEmision').prop('disabled', true);
    $('#ddlTipoSunat').prop('disabled', true);
    $('#btnBuscarProveedorPrincipal').prop('disabled', true);
    $('#btnBuscarRucBenef').prop('disabled', true);
    $('.monto').prop('readonly', true);
}

// ── Habilitar campos modo manual ──────────────
function habilitarCamposManual() {
    // txtNumeroDocumentoIdentidad se mantiene readonly: solo se llena con el botón de búsqueda
    $('#btnBuscarProveedorPrincipal').prop('disabled', false);
    $('#ddlTipoDocumento').prop('disabled', false);
    $('#ddlTipoSunat').prop('disabled', false);
    $('#txtSerie').prop('readonly', false);
    $('#txtNumero').prop('readonly', false);
    $('#txtFechaEmision').prop('disabled', false);
    $('#txtFechaRecepcion').prop('disabled', false);
    $('#ddlMoneda').prop('disabled', false);
    $('#txtTasaCambio').prop('readonly', false);
    $('#dldLugarPago').prop('disabled', false);
    $('#txtObservacion').prop('readonly', false);
    $('#txtRucBenef').prop('readonly', false);
    $('#txtRazonSocialBenef').prop('readonly', false);
    $('#btnBuscarRucBenef').prop('disabled', false);
    $('#TieneDetraccion').prop('disabled', false);
    $('#NoTieneDetraccion').prop('disabled', false);
    $('.monto').prop('readonly', false);
    $('#txtMontoIGVCreditoPorcentajeIGV').prop('readonly', false);
    habilitarCamposDetraccion($('#TieneDetraccion').is(':checked'));
}

// ── Activar modo manual ───────────────────────
function activarModoManual() {
    $('#divCorreo').addClass('d-none');
    $('#Resultados').removeClass('d-none');
    $('#hdnEsDocumentoElectronico').val('N');
    habilitarCamposManual();
    $('#MontoNeto, #MontoExento, #MontoSubtotal, #MontoIGVCosto, #MontoIGVCredito')
        .removeClass('d-none');
    $('#MontoTotal, #MontoBruto, #MontoRetencion').removeClass('d-none');
    mostrarBotonesNuevo();
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
    $('#btnBuscarProveedorPrincipal').prop('disabled', false);
    $('#btnBuscarCRPrincipal').prop('disabled', false);
}

// ── Habilitar/deshabilitar campos detracción ──
function habilitarCamposDetraccion(habilitar) {
    $('#ddlTipoDetraccion').prop('disabled', !habilitar);
    $('#txtPorcentajeDetraccion').prop('readonly', !habilitar);
    $('#txtConstanciaDeposito').prop('readonly', !habilitar);
    $('#txtFechaDeposito').prop('disabled', !habilitar);
}

// ── Calcular fecha vencimiento ────────────────
function calcularFechaVencimiento(fechaEmision, diasPlazo) {
    if (!fechaEmision || diasPlazo == null || isNaN(diasPlazo)) return '';
    const fecha = new Date(fechaEmision);
    if (isNaN(fecha)) return '';
    fecha.setDate(fecha.getDate() + diasPlazo);
    return fecha.toISOString().split('T')[0];
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
            combo.val(valor);
        }
    }, 5000);
}

// ── Utilidades ────────────────────────────────
function obtenerParametroUrl(nombre) {
    return new URLSearchParams(window.location.search).get(nombre);
}

// ── Buscar empleado (modal) ───────────────────
function buscarEmpleado() {
    CorporativoQuery.ajaxGet('/Comprobante/ObtenerEmpleados',
        function (data) {
            if (!data || data.length === 0) {
                CorporativoCore.notificarInfo('No hay empleados disponibles.');
                return;
            }
            mostrarModalBusqueda(data, 'Seleccionar Empleado', 'seleccionar-empleado');
        });
}

// ── Recalcular montos en modo manual ──────────
function recalcularMontos() {
    const neto     = CorporativoCore.limpiarMonto($('#txtMontoNeto').val()) || 0;
    const exento   = CorporativoCore.limpiarMonto($('#txtMontoExento').val()) || 0;
    const pct      = parseFloat($('#txtMontoIGVCreditoPorcentajeIGV').val()) || 0;
    const ret      = CorporativoCore.limpiarMonto($('#txtMontoRetencion').val()) || 0;
    const igv      = Math.round(neto * pct / 100 * 100) / 100;
    const subtotal = neto + exento;
    const total    = subtotal + igv;
    const bruto    = total - ret;

    $('#txtMontoIGVCredito').val(CorporativoCore.formatearMonto(igv));
    $('#txtMontoSubtotal').val(CorporativoCore.formatearMonto(subtotal));
    $('#txtMontoTotal').val(CorporativoCore.formatearMonto(total));
    $('#txtMontoBruto').val(CorporativoCore.formatearMonto(bruto));
}

// ── Bind de eventos ───────────────────────────
function bindEventos() {

    // Guardar
    $('#btnRegistrar').on('click', guardarComprobante);

    // Radios facturación
    $('#rdoFacturacionManual').on('change', function () {
        if ($(this).is(':checked')) activarModoManual();
    });

    $('#rdoFacturacionElectronica').on('change', function () {
        if ($(this).is(':checked')) {
            CorporativoCore.confirmar(
                '¿Desea volver a Facturación Electrónica? ' +
                'Se limpiará el formulario.')
                .then(ok => {
                    if (ok) {
                        limpiarFormulario();
                    } else {
                        $('#rdoFacturacionManual').prop('checked', true);
                        $('#rdoFacturacionElectronica').prop('checked', false);
                    }
                });
        }
    });

    // Limpiar
    $('#btnLimpiar').on('click', async function () {
        const ok = await CorporativoCore.confirmar(
            '¿Desea limpiar el formulario?');
        if (ok) limpiarFormulario();
    });

    // Salir
    $('#btnAtras').on('click', async function () {
        const ok = await CorporativoCore.confirmar(
            '¿Desea salir sin guardar?');
        if (ok) window.location.href = '/Comprobante/Index';
    });

    // Anular
    $('#btnAnular').on('click', async function () {
        const ok = await CorporativoCore.confirmar(
            '¿Está seguro de anular este comprobante?');
        if (ok) anularComprobante();
    });

    // Enviar
    $('#btnEnviar').on('click', async function () {
        const ok = await CorporativoCore.confirmar(
            '¿Desea enviar el comprobante?');
        if (ok) enviarComprobante();
    });

    // Firmar / Autorizar
    $('#btnAutorizarDetalle').on('click', async function () {
        const ok = await CorporativoCore.confirmar(
            '¿Desea firmar el comprobante?');
        if (ok) firmarComprobante();
    });

    // Aprobar
    $('#btnAprobarDetalle').on('click', async function () {
        const ok = await CorporativoCore.confirmar(
            '¿Desea aprobar el comprobante?');
        if (ok) aprobarComprobante();
    });

    // Derivar
    $('#btnDerivar').on('click', async function () {
        const ok = await CorporativoCore.confirmar(
            '¿Desea derivar el comprobante?');
        if (ok) derivarComprobante();
    });

    // Plazo de pago → calcular vencimiento (base: fecha recepción)
    $('#txtPlazoPago').on('change', function () {
        const plazo = parseInt($(this).val());
        const fechaRecepcion = $('#txtFechaRecepcion').val();

        if (isNaN(plazo) || plazo < 0) {
            $('#panelFechaVencimiento').addClass('d-none');
            $('#txtFechaVencimiento').val('');
            return;
        }

        if (CorporativoCore.esVacio(fechaRecepcion)) {
            CorporativoCore.notificarAdvertencia(
                'Debe ingresar la fecha de recepción primero.');
            $(this).val('');
            return;
        }

        $('#txtFechaVencimiento').val(
            calcularFechaVencimiento(fechaRecepcion, plazo));
        $('#panelFechaVencimiento').removeClass('d-none');
    });

    // Fecha recepción cambia → recalcular vencimiento si ya hay plazo
    $('#txtFechaRecepcion').on('change', function () {
        const plazo = parseInt($('#txtPlazoPago').val());
        if (!isNaN(plazo) && plazo >= 0 && $(this).val()) {
            $('#txtFechaVencimiento').val(
                calcularFechaVencimiento($(this).val(), plazo));
            $('#panelFechaVencimiento').removeClass('d-none');
        }
    });

    // Detracción Si/No
    $('#TieneDetraccion').on('change', function () {
        if ($(this).is(':checked')) {
            $('#NoTieneDetraccion').prop('checked', false);
            habilitarCamposDetraccion(true);
        }
    });

    $('#NoTieneDetraccion').on('change', function () {
        if ($(this).is(':checked')) {
            $('#TieneDetraccion').prop('checked', false);
            $('#ddlTipoDetraccion').val('');
            $('#txtPorcentajeDetraccion').val('');
            $('#txtConstanciaDeposito').val('');
            $('#txtFechaDeposito').val('');
            habilitarCamposDetraccion(false);
        }
    });

    // Tipo detracción → porcentaje automático
    $('#ddlTipoDetraccion').on('change', function () {
        const porcentaje = $(this).find('option:selected')
            .data('porcentaje');
        if (porcentaje) {
            $('#txtPorcentajeDetraccion').val(
                CorporativoCore.formatearMonto(porcentaje));
            // Calcular monto detracción
            const total = parseFloat(
                $('#txtMontoTotal').val().replace(/,/g, '')) || 0;
            const monto = total * porcentaje / 100;
            $('#hdnMontoDetraccion').val(monto.toFixed(2));
        } else {
            $('#txtPorcentajeDetraccion').val('');
        }
    });

    // Validación SUNAT (xlsx)
    $('#btnValidacionSunat').on('click', function () {
        $('#inpFileValSunat').trigger('click');
    });

    $('#inpFileValSunat').on('change', function () {
        if (this.files.length > 0) subirArchivoSunat(this.files[0]);
    });

    // Imputación masiva
    $('#btnExplorar').on('click', function () {
        $('#inpFile').trigger('click');
    });

    $('#inpFile').on('change', function () {
        if (this.files.length > 0) subirImputacionMasiva(this.files[0]);
    });

    // Descargar imputación (PDF descarga directa)
    $('#btnImprimirComprobante').on('click', function () {
        const folio = $('#hdnFolio').val();
        if (!folio) return;
        window.open(`/Comprobante/ObtenerPdf?folio=${folio}&descargar=true`, '_blank');
    });

    // Vista previa PDF
    $('#btnVistaPrevia').on('click', function () {
        const folio = $('#hdnFolio').val();
        if (folio) {
            $('a[href="#tabImpresion"]').tab('show');
            $('#pdfComprobante').attr('src',
                `/Comprobante/ObtenerPdf?folio=${folio}`);
        }
    });

    // ── Proveedor ─────────────────────────────
    $('#btnBuscarProveedorPrincipal').on('click', function () {
        CorporativoQuery.ajaxGet('/Comprobante/ObtenerProveedores',
            function (data) {
                if (!data || data.length === 0) {
                    CorporativoCore.notificarInfo('No hay proveedores disponibles.');
                    return;
                }
                mostrarModalBusqueda(data, 'Seleccionar Proveedor', 'seleccionar-proveedor');
            });
    });

    $(document).on('click', '.seleccionar-proveedor', function (e) {
        e.preventDefault();
        const ruc = $(this).data('codigo');
        // descripcion format: "RUC - NombreProveedor"
        const desc = $(this).find('.text-muted').text().trim();
        const nombre = desc.includes(' - ')
            ? desc.split(' - ').slice(1).join(' - ')
            : desc;
        $('#txtNumeroDocumentoIdentidad').val(ruc);
        $('#txtRazonSocial').val(nombre);
        if ($('#rdoFacturacionManual').is(':checked')) {
            $('#txtRucBenef').val(ruc);
            $('#txtRazonSocialBenef').val(nombre);
        }
        bootstrap.Modal.getInstance(
            document.getElementById('modalBusqueda')).hide();
    });

    // ── ¿Es empleado? ────────────────────────
    $('#chkEsEmpleado').on('change', function () {
        if ($(this).is(':checked')) {
            $('#divSelectorEmpleado').removeClass('d-none');
        } else {
            $('#divSelectorEmpleado').addClass('d-none');
            $('#txtEmpleadoDisplay').val('');
            $('#hdnEmpleadoCodigo').val('');
            $('#hdnEmpleadoNombre').val('');
        }
    });

    $('#btnBuscarEmpleado').on('click', buscarEmpleado);

    $(document).on('click', '.seleccionar-empleado', function (e) {
        e.preventDefault();
        const codigo = $(this).data('codigo');
        // descripcion format: "CODIGO - NombreCompleto"
        const desc = $(this).find('.text-muted').text().trim();
        const nombre = desc.includes(' - ')
            ? desc.split(' - ').slice(1).join(' - ')
            : desc;
        $('#hdnEmpleadoCodigo').val(codigo);
        $('#hdnEmpleadoNombre').val(nombre);
        $('#txtEmpleadoDisplay').val(`${codigo} - ${nombre}`);
        bootstrap.Modal.getInstance(
            document.getElementById('modalBusqueda')).hide();
    });

    // ── Facturación manual: auto-calcular montos ──
    $('#txtMontoNeto, #txtMontoIGVCreditoPorcentajeIGV, #txtMontoExento')
        .on('change', function () {
            if ($('#rdoFacturacionManual').is(':checked')) recalcularMontos();
        });

    $('#txtMontoRetencion').on('change', function () {
        if ($('#rdoFacturacionManual').is(':checked')) {
            const total = CorporativoCore.limpiarMonto($('#txtMontoTotal').val()) || 0;
            const ret   = CorporativoCore.limpiarMonto($(this).val()) || 0;
            $('#txtMontoBruto').val(CorporativoCore.formatearMonto(total - ret));
        }
    });
}

// ── Acciones del flujo ────────────────────────
function _accionComprobante(url, estadoDestino, mensajeExito, redirigir = false) {
    const folio = $('#hdnFolio').val();
    CorporativoQuery.ajaxPost(url,
        { comprobante: { folio } },
        function (response) {
            if (response.exito) {
                if (redirigir) {
                    CorporativoCore.notificarExito(mensajeExito);
                    setTimeout(() => window.location.href = '/Comprobante/Index', 1500);
                } else {
                    mostrarBotonesSegunEstado(estadoDestino);
                    CorporativoCore.notificarExito(mensajeExito);
                }
            } else {
                CorporativoCore.notificarError(response.mensaje);
            }
        }
    );
}

function enviarComprobante() {
    _accionComprobante('/Comprobante/Enviar', 'ENVIADO',
        'Comprobante enviado correctamente.', true);
}
function firmarComprobante() {
    _accionComprobante('/Comprobante/Firmar', 'AUTORIZADO',
        'Comprobante autorizado correctamente.', true);
}
function aprobarComprobante() {
    _accionComprobante('/Comprobante/Aprobar', 'APROBADO',
        'Comprobante aprobado correctamente.', true);
}
function anularComprobante() {
    _accionComprobante('/Comprobante/Anular', 'ANULADO',
        'Comprobante anulado correctamente.', true);
}
function derivarComprobante() {
    _accionComprobante('/Comprobante/Derivar', 'REGISTRADO',
        'Comprobante derivado correctamente.');
}

// ── Upload FormData ───────────────────────────
function _subirFormData(url, formData, mensajeExito, mensajeError) {
    CorporativoCore.showLoading();
    $.ajax({
        url,
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
                CorporativoCore.notificarExito(mensajeExito);
            } else {
                CorporativoCore.notificarError(response.mensaje);
            }
        },
        error: function (xhr) {
            CorporativoCore.hideLoading();
            CorporativoCore.handleError(xhr, {
                onCustom: function () {
                    CorporativoCore.notificarError(mensajeError);
                }
            });
        }
    });
}

function subirArchivoSunat(archivo) {
    const fd = new FormData();
    fd.append('file', archivo);
    _subirFormData('/Comprobante/ValidarSunat', fd,
        'Validación SUNAT completada.',
        'Error al procesar el archivo SUNAT.');
}

function subirImputacionMasiva(archivo) {
    const fd = new FormData();
    fd.append('file', archivo);
    _subirFormData('/Comprobante/CargarImputacionMasiva', fd,
        'Imputación cargada correctamente.',
        'Error al cargar el archivo de imputación.');
}