/* comprobante-gestionar.js — Nuevo / Editar comprobante */
(function () {
    'use strict';

    var BASE = (window.BASE_URL || '') + '/api/v1';

    /* ── Catálogos ──────────────────────────────────────────── */
    async function cargarCatalogo(endpoint, selectId) {
        var res = await fetch(BASE + endpoint, { credentials: 'same-origin' });
        if (!res.ok) return;
        var data = await res.json();
        var select = document.getElementById(selectId);
        if (!select) return;
        (data.data ?? []).forEach(function (item) {
            var opt = document.createElement('option');
            opt.value = item.codigo ?? item.id;
            opt.textContent = item.nombre ?? item.descripcion;
            select.appendChild(opt);
        });
    }

    async function inicializar() {
        await Promise.all([
            cargarCatalogo('/maestros/tiposdocumento', 'selTipoDocumento'),
            cargarCatalogo('/maestros/proveedores', 'selProveedor'),
            cargarCatalogo('/maestros/cuentascontables', 'selCuentaContable'),
            cargarCatalogo('/maestros/empleados', 'selEmpleado'),
        ]);
    }

    /* ── Guardar ────────────────────────────────────────────── */
    async function guardar(form) {
        var btn = document.getElementById('btnGuardar');
        CorporateDS.Button.setLoading(btn, 'Guardando…');
        try {
            var payload = Object.fromEntries(new FormData(form));
            var url = BASE + '/comprobante';
            var method = payload.id ? 'PUT' : 'POST';
            if (payload.id) url += '/' + payload.id;

            var res = await fetch(url, {
                method: method,
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (!res.ok) { await handleError(res); return; }
            var data = await res.json();
            CorporateDS.Toast.success('Comprobante guardado correctamente');
            if (!payload.id && data.data?.id)
                window.location.href = window.BASE_URL + '/Comprobante/Editar/' + data.data.id;
        } finally {
            CorporateDS.Button.stopLoading(btn);
        }
    }

    /* ── Imputaciones ───────────────────────────────────────── */
    async function agregarImputacion(comprobanteId, dto) {
        var res = await fetch(BASE + '/comprobante/' + comprobanteId + '/imputaciones', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });
        if (!res.ok) { await handleError(res); return null; }
        return (await res.json()).data;
    }

    async function eliminarImputacion(comprobanteId, imputacionId) {
        if (!confirm('¿Eliminar imputación?')) return;
        var res = await fetch(BASE + '/comprobante/' + comprobanteId + '/imputaciones/' + imputacionId, {
            method: 'DELETE', credentials: 'same-origin'
        });
        res.ok ? CorporateDS.Toast.success('Imputación eliminada') : await handleError(res);
        return res.ok;
    }

    /* ── Documentos ─────────────────────────────────────────── */
    async function subirDocumento(comprobanteId, file) {
        var form = new FormData();
        form.append('archivo', file);
        var res = await fetch(BASE + '/comprobante/' + comprobanteId + '/documentos', {
            method: 'POST', credentials: 'same-origin', body: form
        });
        if (!res.ok) { await handleError(res); return null; }
        return (await res.json()).data;
    }

    async function eliminarDocumento(comprobanteId, documentoId) {
        if (!confirm('¿Eliminar documento?')) return false;
        var res = await fetch(BASE + '/comprobante/' + comprobanteId + '/documentos/' + documentoId, {
            method: 'DELETE', credentials: 'same-origin'
        });
        res.ok ? CorporateDS.Toast.success('Documento eliminado') : await handleError(res);
        return res.ok;
    }

    async function handleError(res) {
        var msg = 'Error (' + res.status + ')';
        try { var d = await res.json(); msg = d?.error?.userMessage ?? msg; } catch (_) {}
        CorporateDS.Toast.error(msg);
    }

    window.ComprobanteGestionar = { inicializar, guardar, agregarImputacion, eliminarImputacion, subirDocumento, eliminarDocumento };
}());
