/* comprobante-autorizar.js — Firmar y derivar comprobante */
(function () {
    'use strict';

    var BASE = (window.BASE_URL || '') + '/api/v1';

    async function cargarDetalle(id) {
        var res = await fetch(BASE + '/comprobante/' + id, { credentials: 'same-origin' });
        if (!res.ok) { await handleError(res); return null; }
        return (await res.json()).data;
    }

    async function firmar(id, observacion) {
        var btn = document.getElementById('btnFirmar');
        CorporateDS.Button.setLoading(btn, 'Firmando…');
        try {
            var res = await fetch(BASE + '/comprobante/' + id + '/firmar', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ observacion })
            });
            if (!res.ok) { await handleError(res); return; }
            CorporateDS.Toast.success('Comprobante firmado correctamente');
            setTimeout(function () {
                window.location.href = window.BASE_URL + '/Comprobante/Index';
            }, 1200);
        } finally {
            CorporateDS.Button.stopLoading(btn);
        }
    }

    async function derivar(id, destinatarioId, observacion) {
        var btn = document.getElementById('btnDerivar');
        CorporateDS.Button.setLoading(btn, 'Derivando…');
        try {
            var res = await fetch(BASE + '/comprobante/' + id + '/derivar', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ destinatarioId, observacion })
            });
            if (!res.ok) { await handleError(res); return; }
            CorporateDS.Toast.success('Comprobante derivado correctamente');
            setTimeout(function () {
                window.location.href = window.BASE_URL + '/Comprobante/Index';
            }, 1200);
        } finally {
            CorporateDS.Button.stopLoading(btn);
        }
    }

    async function handleError(res) {
        var msg = 'Error (' + res.status + ')';
        try { var d = await res.json(); msg = d?.error?.userMessage ?? msg; } catch (_) {}
        CorporateDS.Toast.error(msg);
    }

    window.ComprobanteAutorizar = { cargarDetalle, firmar, derivar };
}());
