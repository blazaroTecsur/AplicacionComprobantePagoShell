/* comprobante-aprobar.js — Aprobar y enviar a Syteline */
(function () {
    'use strict';

    var BASE = (window.BASE_URL || '') + '/api/v1';

    async function cargarDetalle(id) {
        var res = await fetch(BASE + '/comprobante/' + id, { credentials: 'same-origin' });
        if (!res.ok) { await handleError(res); return null; }
        return (await res.json()).data;
    }

    async function aprobar(id, observacion) {
        var btn = document.getElementById('btnAprobar');
        CorporateDS.Button.setLoading(btn, 'Aprobando…');
        try {
            var res = await fetch(BASE + '/comprobante/' + id + '/aprobar', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ observacion })
            });
            if (!res.ok) { await handleError(res); return; }
            CorporateDS.Toast.success('Comprobante aprobado correctamente');
            setTimeout(function () {
                window.location.href = window.BASE_URL + '/Comprobante/Index';
            }, 1200);
        } finally {
            CorporateDS.Button.stopLoading(btn);
        }
    }

    async function enviarASyteline(id) {
        var btn = document.getElementById('btnSyteline');
        CorporateDS.Button.setLoading(btn, 'Enviando a Syteline…');
        try {
            var res = await fetch(BASE + '/comprobante/' + id + '/syteline', {
                method: 'POST',
                credentials: 'same-origin'
            });
            if (!res.ok) { await handleError(res); return; }
            CorporateDS.Toast.success('Comprobante enviado a Syteline correctamente');
            setTimeout(function () {
                window.location.href = window.BASE_URL + '/Comprobante/Index';
            }, 1200);
        } finally {
            CorporateDS.Button.stopLoading(btn);
        }
    }

    async function exportarDistribucion(id) {
        window.location.href = window.BASE_URL + '/Comprobante/Reporte/ExportarDistribucion/' + id;
    }

    async function handleError(res) {
        var msg = 'Error (' + res.status + ')';
        try { var d = await res.json(); msg = d?.error?.userMessage ?? msg; } catch (_) {}
        CorporateDS.Toast.error(msg);
    }

    window.ComprobanteAprobar = { cargarDetalle, aprobar, enviarASyteline, exportarDistribucion };
}());
