/* comprobante-index.js — Lista / búsqueda de comprobantes */
(function () {
    'use strict';

    var BASE = (window.BASE_URL || '') + '/api/v1';
    var ROLES = window.ROLES || [];

    function puedeEditar()   { return ROLES.includes('COMP.GUARDAR'); }
    function puedeEnviar()   { return ROLES.includes('COMP.ENVIAR'); }
    function puedeAnular()   { return ROLES.includes('COMP.ANULAR'); }
    function puedeAutorizar(){ return ROLES.includes('COMP.AUTORIZAR'); }
    function puedeAprobar()  { return ROLES.includes('COMP.APROBAR'); }
    function puedeReporte()  { return ROLES.includes('COMP.RPT'); }

    /* ── Carga de la grilla ─────────────────────────────────── */
    async function buscar(filtros) {
        var params = new URLSearchParams(filtros || {});
        var res = await fetch(BASE + '/comprobante/buscar?' + params, { credentials: 'same-origin' });
        if (!res.ok) { handleError(res); return []; }
        var data = await res.json();
        return data.data ?? [];
    }

    function renderAcciones(comp) {
        var btns = [];
        btns.push('<a href="' + window.BASE_URL + '/Comprobante/Detalle/' + comp.id + '" class="corp-btn corp-btn--ghost corp-btn--sm">Ver</a>');
        if (puedeEditar() && comp.estado === 'BORRADOR')
            btns.push('<a href="' + window.BASE_URL + '/Comprobante/Editar/' + comp.id + '" class="corp-btn corp-btn--ghost corp-btn--sm">Editar</a>');
        if (puedeEnviar() && comp.estado === 'BORRADOR')
            btns.push('<button onclick="Comprobante.enviar(\'' + comp.id + '\')" class="corp-btn corp-btn--primary corp-btn--sm">Enviar</button>');
        if (puedeAnular() && ['BORRADOR','ENVIADO'].includes(comp.estado))
            btns.push('<button onclick="Comprobante.anular(\'' + comp.id + '\')" class="corp-btn corp-btn--danger corp-btn--sm">Anular</button>');
        if (puedeAutorizar() && comp.estado === 'ENVIADO')
            btns.push('<a href="' + window.BASE_URL + '/Comprobante/Autorizar/' + comp.id + '" class="corp-btn corp-btn--warning corp-btn--sm">Autorizar</a>');
        if (puedeAprobar() && comp.estado === 'FIRMADO')
            btns.push('<a href="' + window.BASE_URL + '/Comprobante/Aprobar/' + comp.id + '" class="corp-btn corp-btn--success corp-btn--sm">Aprobar</a>');
        if (puedeReporte())
            btns.push('<a href="' + window.BASE_URL + '/Comprobante/Reporte/ExportarCabecera/' + comp.id + '" class="corp-btn corp-btn--ghost corp-btn--sm">Excel</a>');
        return btns.join(' ');
    }

    /* ── Acciones rápidas ───────────────────────────────────── */
    async function enviar(id) {
        if (!confirm('¿Confirma el envío del comprobante?')) return;
        var res = await fetch(BASE + '/comprobante/' + id + '/enviar', { method: 'POST', credentials: 'same-origin' });
        res.ok ? location.reload() : handleError(res);
    }

    async function anular(id) {
        if (!confirm('¿Confirma la anulación del comprobante?')) return;
        var res = await fetch(BASE + '/comprobante/' + id + '/anular', { method: 'POST', credentials: 'same-origin' });
        res.ok ? location.reload() : handleError(res);
    }

    async function handleError(res) {
        var msg = 'Error en la operación (' + res.status + ')';
        try { var d = await res.json(); msg = d?.error?.userMessage ?? msg; } catch (_) {}
        CorporateDS.Toast.error(msg);
    }

    /* API pública */
    window.Comprobante = { buscar, enviar, anular, renderAcciones };
}());
