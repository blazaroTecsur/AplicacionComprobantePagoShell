// ============================================================
//  CORPORATIVO CORE — Capa base. Debe cargarse primero.
//  Habla con ApiResponse<T> del backend.
// ============================================================

var CorporativoCore = (function () {

    // ── Token CSRF ───────────────────────────────────────────

    function obtenerToken() {
        return $("input[name='__RequestVerificationToken']").val()
            || $("meta[name='RequestVerificationToken']").attr("content")
            || "";
    }

    // ── Configuración global jQuery AJAX ─────────────────────

    function init() {
        $.ajaxSetup({
            headers: {
                "X-Requested-With": "XMLHttpRequest",
                "RequestVerificationToken": obtenerToken()
            }
        });
    }

    // ── Parser de respuesta del backend ──────────────────────
    // Normaliza tanto ApiResponse<T> como errores de red

    function parsearRespuesta(xhr) {
        try {
            const json = typeof xhr === "string" ? JSON.parse(xhr) : xhr.responseJSON;
            if (json && typeof json.success !== "undefined") return json;
            return {
                success: false,
                error: {
                    code: "UNKNOWN_FORMAT",
                    userMessage: "Respuesta inesperada del servidor.",
                    technicalMessage: JSON.stringify(json)
                }
            };
        } catch {
            return {
                success: false,
                error: {
                    code: "NETWORK_ERROR",
                    userMessage: "Error de conexión. Verifica tu internet."
                }
            };
        }
    }

    // ── Manejador central de errores ─────────────────────────
    // options: { onValidation, onCustom, errorElement }

    function handleError(xhr, options = {}) {
        const response = parsearRespuesta(xhr);
        console.log('parsear', response);
        if (!response || response.success) return;

        const error = response.error;
        const { onValidation, onCustom, errorElement } = options;

        // 1. Errores de validación FluentValidation
        if (error.code === "VALIDATION_ERROR" && error.validationErrors) {
            if (typeof onValidation === "function") {
                onValidation(error.validationErrors);
            } else {
                mostrarErroresValidacion(error.validationErrors, errorElement);
            }
            return;
        }

        // 2. Callback personalizado por llamada
        if (typeof onCustom === "function") {
            onCustom(error);
            return;
        }

        // 3. Fallback: toast no bloqueante
        mostrarToast(error.userMessage || "Error inesperado.", "error");
    }

    // ── Errores de validación junto a campos ─────────────────

    function mostrarErroresValidacion(validationErrors, errorElement) {
        $(".field-validation-error").text("");
        $(".input-validation-error").removeClass("input-validation-error");
        $(".is-invalid").removeClass("is-invalid");
        $("#divErrores").addClass("d-none").html("");

        const listaErrores = [];

        Object.entries(validationErrors).forEach(([campo, mensajes]) => {
            listaErrores.push(mensajes[0]);
            const $span = $(`[data-valmsg-for="${campo}"]`);
            const $input = $(`[name="${campo}"]`);
            if ($span.length) $span.text(mensajes[0]).addClass("field-validation-error");
            if ($input.length) $input.addClass("is-invalid");
        });

        const $target = errorElement ? $(errorElement) : $("#divErrores");
        if ($target.length && listaErrores.length) {
            const html = "<ul>" + listaErrores.map(e => `<li>${e}</li>`).join("") + "</ul>";
            $target.html(html).removeClass("d-none");
        }
    }

    // ── Toast no bloqueante ───────────────────────────────────
    // tipo: "error" | "success" | "warning" | "info"

    function mostrarToast(mensaje, tipo = "info") {
        const iconMap = { error: "error", success: "success", warning: "warning", info: "info" };

        // Opción 1: SweetAlert2 disponible → mismo diseño que confirmar()
        if (typeof Swal !== "undefined") {
            Swal.mixin({
                toast: true,
                position: "top-end",
                showConfirmButton: false,
                timer: 4500,
                timerProgressBar: true
            }).fire({
                icon: iconMap[tipo] || "info",
                title: mensaje
            });
            return;
        }

        // Opción 2: toastr disponible
        if (typeof toastr !== "undefined") {
            toastr[iconMap[tipo] || "info"](mensaje);
            return;
        }

        // Opción 3: fallback nativo inline sin dependencias
        const colores = {
            error: { bg: "#da1e28", icon: "✕" },
            success: { bg: "#198754", icon: "✓" },
            warning: { bg: "#fd7e14", icon: "⚠" },
            info: { bg: "#0d6efd", icon: "ℹ" }
        };
        const { bg, icon } = colores[tipo] || colores.info;

        // Construir con jQuery para evitar XSS al insertar el mensaje como texto
        const $toast = $('<div>', {
            style: `position:fixed; top:24px; left:24px; z-index:9999;
                background:${bg}; color:#fff; padding:12px 18px;
                border-radius:6px; font-size:13px; max-width:340px;
                box-shadow:0 4px 12px rgba(0,0,0,.25);
                display:flex; align-items:center; gap:10px;`
        })
            .append($('<span>', { style: 'font-weight:bold', text: icon }))
            .append($('<span>').text(mensaje))
            .appendTo('body');

        setTimeout(() => $toast.fadeOut(400, () => $toast.remove()), 4500);
    }

    // ── Wrappers de notificación ──────────────────────────────

    function notificarExito(mensaje) { mostrarToast(mensaje, "success"); }
    function notificarError(mensaje) { mostrarToast(mensaje, "error"); }
    function notificarAdvertencia(mensaje) { mostrarToast(mensaje, "warning"); }
    function notificarInfo(mensaje) { mostrarToast(mensaje, "info"); }

    // ── Loading ───────────────────────────────────────────────

    function showLoading() { $('#loading').show(); }
    function hideLoading() { $('#loading').hide(); }

    // ── Confirmación (SweetAlert2) ────────────────────────────

    async function confirmar(mensaje) {
        if (typeof Swal === "undefined") {
            return window.confirm(mensaje);
        }
        const result = await Swal.fire({
            title: '¿Está seguro?',
            text: mensaje,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sí, continuar',
            cancelButtonText: 'Cancelar'
        });
        return result.isConfirmed;
    }

    // ── Formateo de fechas ────────────────────────────────────

    function formatearFecha(fechaStr) {
        var partes = fechaStr.split(/[T ]/);
        var partsFecha = partes[0].split('-');
        var fecha = partsFecha[2] + '/' + partsFecha[1] + '/' + partsFecha[0];
        if (partes.length === 1) return fecha;
        var hora = partes[1].substring(0, 5);
        return fecha + ' ' + hora;
    }

    // ── Formateo de montos ────────────────────────────────────

    function formatearMonto(valor) {
        return parseFloat(valor || 0).toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    }

    function limpiarMonto(valor) {
        return parseFloat((valor || '0').toString().replace(/,/g, '')) || 0;
    }

    // ── Validaciones básicas ──────────────────────────────────

    function esVacio(valor) {
        return !valor || valor.toString().trim() === '';
    }

    // ── Escape HTML (previene XSS en template literals) ──────

    function escaparHtml(valor) {
        return $('<span>').text(valor ?? '').html();
    }

    return {
        init,
        handleError,
        mostrarToast,
        mostrarErroresValidacion,
        obtenerToken,
        formatearFecha,
        formatearMonto,
        limpiarMonto,
        esVacio,
        escaparHtml,
        showLoading,
        hideLoading,
        confirmar,
        notificarExito,
        notificarError,
        notificarAdvertencia,
        notificarInfo
    };

})();

// Inicializar al cargar la página
$(function () { CorporativoCore.init(); });

// ============================================================
//  CORPORATIVO SELECT — Sin cambios funcionales, agrega errores
// ============================================================

var CorporativoSelect = (function () {

    function cargar(config) {
        $.ajax({
            url: config.url,
            type: "GET",
            success: function (response) {
                const datos = response.data ?? response;

                if (config.list) {
                    $.each(config.list, function (_, litem) {
                        const combo = $(litem.element);
                        combo.empty();
                        const data = datos.filter(r => r[config.filtro] == litem.type);
                        if (data) {
                            combo.append($("<option>", { value: "", text: "== SELECCIONAR ==" }));
                            $.each(data, function (_, item) {
                                combo.append($("<option>", { value: item[litem.id], text: item[litem.text] }));
                            });
                        }
                    });
                } else {
                    const combo = $(config.element);
                    combo.empty();
                    $.each(datos, function (_, data) {
                        combo.append($("<option>", { value: data[config.id], text: data[config.text] }));
                    });
                }
            },
            error: function (xhr) {
                CorporativoCore.handleError(xhr);
            }
        });
    }

    return { cargar };

})();

// ============================================================
//  CORPORATIVO QUERY — Delega error a CorporativoCore
// ============================================================

var CorporativoQuery = (function () {

    function submit(config) {
        CorporativoCore.showLoading();

        $.ajax({
            url: config.url,
            type: config.type || "GET",
            contentType: "application/json",
            data: config.data ? JSON.stringify(config.data) : undefined,
            timeout: config.timeout || 30000,

            success: function (response) {
                if (response && response.success === false) {
                    CorporativoCore.handleError(
                        { responseJSON: response },
                        { onValidation: config.onValidation, onCustom: config.onError }
                    );
                    return;
                }
                if (config.success) config.success(response.data ?? response);
            },

            error: function (xhr) {
                CorporativoCore.handleError(xhr, {
                    onValidation: config.onValidation,
                    onCustom: config.onError,
                    errorElement: config.errorElement
                });
                if (config.error) config.error(xhr);
            },

            complete: function () {
                CorporativoCore.hideLoading();
                if (config.complete) config.complete();
            }
        });
    }

    // ── ajaxGet / ajaxPost como aliases de submit ─────────────
    // Compatibilidad con código que usaba ajaxGet() / ajaxPost()

    function ajaxGet(url, onSuccess, onError) {
        submit({
            url,
            type: "GET",
            success: onSuccess,
            error: onError
        });
    }

    function ajaxPost(url, data, onSuccess, onError) {
        submit({
            url,
            type: "POST",
            data,
            success: onSuccess,
            error: onError
        });
    }

    return { submit, ajaxGet, ajaxPost };

})();

// ============================================================
//  CORPORATIVO FORM — Delega error a CorporativoCore
// ============================================================

var CorporativoForm = (function () {

    function serializarFormulario(formSelector) {
        const form = $(formSelector);
        if (form.length === 0) {
            console.error("Formulario no encontrado:", formSelector);
            return {};
        }

        const obj = {};
        form.serializeArray().forEach(function (item) {
            if (obj[item.name]) {
                if (!Array.isArray(obj[item.name])) obj[item.name] = [obj[item.name]];
                obj[item.name].push(item.value);
            } else {
                obj[item.name] = item.value;
            }
        });

        form.find("input[type=checkbox]").each(function () {
            obj[this.name] = obj.hasOwnProperty(this.name);
        });

        return obj;
    }

    function obtenerDatosTablas(tablas) {
        const resultado = {};
        if (!tablas) return resultado;
        tablas.forEach(function (t) {
            if (!t.nombre || !t.tabla) { console.warn("Tabla inválida", t); return; }
            resultado[t.nombre] = t.tabla.getData();
        });
        return resultado;
    }

    function construirPayload(config) {
        const formulario = serializarFormulario(config.form);
        const tablas = obtenerDatosTablas(config.tablas);
        return { [config.formulario]: formulario, ...tablas };
    }

    function bloquearBoton(btn) { if (btn) $(btn).prop("disabled", true); }
    function desbloquearBoton(btn) { if (btn) $(btn).prop("disabled", false); }

    function submit(config) {
        const payload = construirPayload(config);
        if (config.beforeSend) config.beforeSend(payload);

        bloquearBoton(config.button);
        CorporativoCore.showLoading();

        $.ajax({
            url: config.url,
            method: config.method || "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            headers: { "RequestVerificationToken": CorporativoCore.obtenerToken() },
            timeout: config.timeout || 30000,

            success: function (response) {
                if (response && response.success === false) {
                    CorporativoCore.handleError(
                        { responseJSON: response },
                        {
                            onValidation: config.onValidation,
                            onCustom: config.onError,
                            errorElement: config.errorElement
                        }
                    );
                    return;
                }
                if (config.success) config.success(response.data ?? response);
            },

            error: function (xhr) {
                console.log('xhr', xhr);
                CorporativoCore.handleError(xhr, {
                    onValidation: config.onValidation,
                    onCustom: config.onError,
                    errorElement: config.errorElement
                });
                if (config.error) config.error(xhr);
            },

            complete: function () {
                desbloquearBoton(config.button);
                CorporativoCore.hideLoading();
                if (config.complete) config.complete();
            }
        });
    }

    return { submit, serializarFormulario };

})();

// ============================================================
//  CORPORATIVO VALIDATOR — Agrega integración con backend
// ============================================================

var CorporativoValidator = (function () {

    function validarFormulario(formSelector) {
        let valido = true;
        $(formSelector + " [required]").each(function () {
            if (!$(this).val()) {
                $(this).addClass("is-invalid");
                valido = false;
            } else {
                $(this).removeClass("is-invalid");
            }
        });
        return valido;
    }

    function pintarErroresBackend(validationErrors, formSelector) {
        if (formSelector) {
            $(formSelector + " .is-invalid").removeClass("is-invalid");
            $(formSelector + " .field-validation-error").text("");
        }

        Object.entries(validationErrors).forEach(([campo, mensajes]) => {
            const selector = formSelector
                ? `${formSelector} [name="${campo}"]`
                : `[name="${campo}"]`;

            $(selector).addClass("is-invalid");

            const $span = $(`[data-valmsg-for="${campo}"]`);
            if ($span.length) {
                $span.text(mensajes[0]).addClass("field-validation-error");
            } else {
                const $feedback = $(selector).siblings(".invalid-feedback");
                if ($feedback.length) $feedback.text(mensajes[0]);
            }
        });
    }

    return { validarFormulario, pintarErroresBackend };

})();

// ============================================================
//  CORPORATIVO MODAL — Modales dinámicos cargados por AJAX
// ============================================================

var CorporativoModal = (function () {

    // Abre un modal cargando HTML desde una URL
    // config: { url, container, onCerrar }
    function abrir(config) {
        const url = typeof config === "string" ? config : config.url;
        const container = (typeof config === "object" && config.container)
            ? config.container
            : '#modalContainer';
        const onCerrar = typeof config === "object" ? config.onCerrar : arguments[1];

        CorporativoQuery.ajaxGet(url, function (html) {
            $(container).html(html);
            const $modalEl = $(container + ' .modal').first();
            const modal = new bootstrap.Modal($modalEl[0]);
            modal.show();
            if (typeof onCerrar === "function") {
                $modalEl.on('hidden.bs.modal', onCerrar);
            }
        });
    }

    return { abrir };

})();

// ============================================================
//  CORPORATIVO GRID — Usa CorporativoCore para errores/toast
// ============================================================

var CorporativoGrid = (function () {

    function obtenerHeaders(config) {
        var headers = { "Content-Type": "application/json" };
        var token = CorporativoCore.obtenerToken();
        if (token) headers["RequestVerificationToken"] = token;
        if (config.headers) Object.assign(headers, config.headers);
        return headers;
    }

    function mostrarCargando(config) {
        if (config.loadingElement) $(config.loadingElement).show();
        else CorporativoCore.showLoading();
    }

    function ocultarCargando(config) {
        if (config.loadingElement) $(config.loadingElement).hide();
        else CorporativoCore.hideLoading();
    }

    function crear(config) {
        const pageSize = config.pageSize || 10;

        const tabla = new Tabulator(config.element, {
            height: config.height || "500px",
            layout: config.layout || "fitColumns",
            pagination: true,
            paginationMode: "remote",
            paginationSize: pageSize,
            paginationSizeSelector: config.paginationSizeSelector || false,
            ajaxURL: config.url,
            ajaxConfig: { method: "POST" },

            ajaxRequestFunc: function (url, ajaxConfig, params) {
                mostrarCargando(config);

                params = params || {};
                var payload = {
                    page: params.page || 1,
                    pageSize: params.size || pageSize,
                    filtros: config.filtros ? config.filtros() : {}
                };

                if (config.beforeRequest) config.beforeRequest(payload);

                return fetch(url, {
                    method: "POST",
                    headers: obtenerHeaders(config),
                    body: JSON.stringify(payload)
                })
                    .then(function (response) {
                        if (!response.ok) {
                            return response.json().then(function (body) { throw body; });
                        }
                        return response.json();
                    })
                    .then(function (body) {
                        const datos = body.data ?? body;
                        return {
                            data: datos.data || [],
                            last_page: Math.max(1, Math.ceil((datos.total || 0) / pageSize))
                        };
                    })
                    .catch(function (err) {
                        ocultarCargando(config);
                        if (err && typeof err.success !== "undefined") {
                            CorporativoCore.handleError(
                                { responseJSON: err },
                                { onCustom: config.onError }
                            );
                        } else {
                            CorporativoCore.mostrarToast(
                                config.mensajeError || "No se pudo cargar la información.",
                                "error"
                            );
                        }
                        throw err;
                    })
                    .finally(function () {
                        ocultarCargando(config);
                    });
            },

            ajaxResponse: function (url, params, response) {
                if (config.afterResponse) config.afterResponse(response);
                return response;
            },

            ajaxError: function (error) {
                if (config.onError) config.onError(error);
            },

            columns: config.columns
        });

        agregarFunciones(tabla, config);
        return tabla;
    }

    function agregarFunciones(tabla, config) {
        tabla.refrescar = function () { return tabla.replaceData(); };
        tabla.buscar = function () {
            tabla.getPage() !== 1 ? tabla.setPage(1) : tabla.replaceData();
        };
        tabla.limpiar = function () { tabla.clearData(); };
        tabla.obtenerSeleccion = function () { return tabla.getSelectedData(); };
        tabla.obtenerDatos = function () { return tabla.getData(); };
        tabla.cambiarUrl = function (nuevaUrl) {
            config.url = nuevaUrl;
            tabla.setData(nuevaUrl);
        };
    }

    return { crear };

})();