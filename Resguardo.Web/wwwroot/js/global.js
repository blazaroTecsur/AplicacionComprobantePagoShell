
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

            // Respuesta que no sigue ApiResponse<T> (ej: endpoint legacy)
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
        console.log('handleError', xhr);
        const response = parsearRespuesta(xhr);        
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
        // 3. Fallback: toast no bloqueante (reemplaza alert)
        mostrarToast(error.userMessage || "Error inesperado.", "error");
    }
    // ── Errores de validación junto a campos ─────────────────
    function mostrarErroresValidacion(validationErrors, errorElement) {
        // Limpia estado previo
        $(".field-validation-error").text("");
        $(".input-validation-error").removeClass("input-validation-error");
        $(".is-invalid").removeClass("is-invalid");
        $("#divErrores").addClass("d-none").html("");

        const listaErrores = [];

        Object.entries(validationErrors).forEach(([campo, mensajes]) => {
            listaErrores.push(mensajes[0]);

            // Intenta pintar junto al campo si existe el span de validación
            const $span = $(`[data-valmsg-for="${campo}"]`);
            const $input = $(`[name="${campo}"]`);
            if ($span.length) $span.text(mensajes[0]).addClass("field-validation-error");
            if ($input.length) $input.addClass("is-invalid");
        });
        // Si hay un elemento contenedor explícito, úsalo
        const $target = errorElement ? $(errorElement) : $("#divErrores");
        if ($target.length && listaErrores.length) {
            const html = "<ul>" + listaErrores.map(e => `<li>${e}</li>`).join("") + "</ul>";
            $target.html(html).removeClass("d-none");
        }
    }
    // ── Toast no bloqueante (reemplaza todos los alert) ──────
    // tipo: "error" | "success" | "warning" | "info"
    function mostrarToast(mensaje, tipo = "info") {
        const colores = {
            error: { bg: "#da1e28", icon: "✕" },
            success: { bg: "#198754", icon: "✓" },
            warning: { bg: "#fd7e14", icon: "⚠" },
            info: { bg: "#0d6efd", icon: "ℹ" }
        };
        const { bg, icon } = colores[tipo] || colores.info;

        const $toast = $(`
            <div style="
                position:fixed; bottom:24px; right:24px; z-index:9999;
                background:${bg}; color:#fff; padding:12px 18px;
                border-radius:6px; font-size:13px; max-width:340px;
                box-shadow:0 4px 12px rgba(0,0,0,.25);
                display:flex; align-items:center; gap:10px;">
                <span style="font-weight:bold">${icon}</span>
                <span>${mensaje}</span>
            </div>
        `).appendTo("body");

        setTimeout(() => $toast.fadeOut(400, () => $toast.remove()), 10000);
    }    
    function formatearFecha(fechaStr) {

        // Separa fecha y hora por "T" o por espacio
        var partes = fechaStr.split(/[T ]/);
        var partsFecha = partes[0].split('-');   // ["2026", "03", "19"]
        var fecha = partsFecha[2] + '/' + partsFecha[1] + '/' + partsFecha[0];

        // Si no viene hora, retorna solo la fecha
        if (partes.length === 1) return fecha;

        // Toma solo HH:MM (descarta segundos si vienen)
        var hora = partes[1].substring(0, 5);   // "14:30"

        return fecha + ' ' + hora;             // "19/03/2026 14:30"
    }
    function validarDni(dni) {

        var esValido = true;
        if (dni) {
            if (!$.isNumeric(dni) || dni.length != 8) {
                esValido = false;
            }
        }
        return esValido;
    }
    function abrirGoogleMaps(coordenada) {
        var url = "https://www.google.com/maps?q=" + coordenada;
        window.open(url, "_blank");
    }
    function tienePermiso(permisosBD, permisosUsuario) {
        return permisosUsuario.some(pu =>
            permisosBD.some(pb => pb === pu)
        );
    }
    return { init, handleError, mostrarToast, mostrarErroresValidacion, obtenerToken, formatearFecha, validarDni, abrirGoogleMaps, tienePermiso };

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
                // Si el backend devuelve ApiResponse<T>, extraer .data
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
            // ← NUEVO: antes fallaba silenciosamente
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
        $.ajax({
            url: config.url,
            type: config.type || "GET",
            contentType: "application/json",
            data: config.data ? JSON.stringify(config.data) : undefined,
            timeout: config.timeout || 30000,
            success: function (response) {
                // Verifica success del ApiResponse<T>
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
                // Delega a Core — elimina manejarError duplicado
                CorporativoCore.handleError(xhr, {
                    onValidation: config.onValidation,
                    onCustom: config.onError,
                    errorElement: config.errorElement
                });
                if (config.error) config.error(xhr);
            },
            complete: function () {
                if (config.complete) config.complete();
            }
        });
    }

    return { submit };

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
        
        const disabledFields = form.find(':input:disabled').prop('disabled', false);

        const obj = {};
        form.serializeArray().forEach(function (item) {
            if (obj[item.name]) {
                if (!Array.isArray(obj[item.name])) obj[item.name] = [obj[item.name]];
                obj[item.name].push(item.value);
            } else {
                obj[item.name] = item.value;
            }
        });
        
        disabledFields.prop('disabled', true);

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
        return { formulario, ...tablas };
    }
    function bloquearBoton(btn) { if (btn) $(btn).prop("disabled", true); }
    function desbloquearBoton(btn) { if (btn) $(btn).prop("disabled", false); }
    function submit(config) {
        const payload = construirPayload(config);
        if (config.beforeSend) config.beforeSend(payload);        
        bloquearBoton(config.button);

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
                // Delega a Core — elimina manejarError duplicado
                CorporativoCore.handleError(xhr, {
                    onValidation: config.onValidation,
                    onCustom: config.onError,
                    errorElement: config.errorElement
                });
                if (config.error) config.error(xhr);
            },
            complete: function () {
                desbloquearBoton(config.button);
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

    // Validación frontend básica (se mantiene igual)
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

    // NUEVO: pinta errores de FluentValidation del backend en campos del form
    function pintarErroresBackend(validationErrors, formSelector) {

        // Limpia estado previo dentro del form
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
            if ($span.length) $span.text(mensajes[0]).addClass("field-validation-error");

            // Si no hay span de validación, agrega feedback de Bootstrap
            else {
                const $feedback = $(selector).siblings(".invalid-feedback");
                if ($feedback.length) $feedback.text(mensajes[0]);
            }
        });
    }

    return { validarFormulario, pintarErroresBackend };

})();

// ============================================================
//  CORPORATIVO GRID — Usa CorporativoCore para errores/toast
// ============================================================
var CorporativoGrid = (function () {

    function obtenerHeaders(config) {
        var headers = { "Content-Type": "application/json" };
        var token = CorporativoCore.obtenerToken(); // ← usa Core
        if (token) headers["RequestVerificationToken"] = token;
        if (config.headers) Object.assign(headers, config.headers);
        return headers;
    }
    function mostrarCargando(config) {
        if (config.loadingElement) $(config.loadingElement).show();
    }
    function ocultarCargando(config) {
        if (config.loadingElement) $(config.loadingElement).hide();
    }
    function crear(config) {
        const pageSize = config.pageSize || 10;

        const tabla = new Tabulator(config.element, {
            height: config.height || "500px",
            layout: config.layout || "fitColumns",
            headerSortElement: null,
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
                            return response.json().then(function (body) {
                                // Lanza el ApiResponse<T> completo para que el catch lo lea
                                throw body;
                            });
                        }
                        return response.json();
                    })
                    .then(function (body) {

                        // Si el backend devuelve ApiResponse<T> envuelto
                        const datos = body.data ?? body;
                        return {
                            data: datos.data || [],
                            last_page: Math.max(1, Math.ceil((datos.total || 0) / pageSize))
                        };

                        //return {
                        //    data: body.data || [],
                        //    last_page: Math.max(1, Math.ceil((body.total || 0) / pageSize))
                        //};
                    })
                    .catch(function (err) {
                        ocultarCargando(config);

                        // Delega a Core si es un ApiResponse<T> de error
                        if (err && typeof err.success !== "undefined") {
                            CorporativoCore.handleError(
                                { responseJSON: err },
                                { onCustom: config.onError }
                            );
                        } else {
                            // Error de red sin respuesta JSON
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
                return response; // ya normalizado en .then de arriba
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