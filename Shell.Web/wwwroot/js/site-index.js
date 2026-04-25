var Layout = (function () {
    'use strict';

    /* IDs de todos los dropdowns del shell */
    var DROP_IDS = ['notifDrop', 'userDrop', 'envDrop'];

    /* ── Sidebar ───────────────────────────────────────────── */
    function toggleSidebar() {
        var sb = document.getElementById('layoutSidebar');
        var ov = document.getElementById('sbOverlay');
        var btn = document.getElementById('hamburgerBtn');
        var open = sb.classList.toggle('open');
        ov.classList.toggle('show', open);
        btn.setAttribute('aria-expanded', String(open));
    }

    function closeSidebar() {
        document.getElementById('layoutSidebar').classList.remove('open');
        document.getElementById('sbOverlay').classList.remove('show');
        document.getElementById('hamburgerBtn').setAttribute('aria-expanded', 'false');
    }

    /* ── Dropdowns ─────────────────────────────────────────── */
    function toggleDrop(id) {
        var target = document.getElementById(id);
        if (!target) return;
        var willOpen = !target.classList.contains('open');
        DROP_IDS.forEach(function (d) {
            var el = document.getElementById(d);
            if (el) el.classList.remove('open');
        });
        if (willOpen) target.classList.add('open');

        /* Sincronizar aria-expanded en el botón disparador */
        var triggers = {
            notifDrop: 'notifBtn',
            userDrop: 'userBtn',
            envDrop: 'envBadge'
        };
        if (triggers[id]) {
            var btn = document.getElementById(triggers[id]);
            if (btn) btn.setAttribute('aria-expanded', String(willOpen));
        }
    }

    function closeAllDrops() {
        DROP_IDS.forEach(function (d) {
            var el = document.getElementById(d);
            if (el) el.classList.remove('open');
        });
    }

    /* Cerrar dropdowns al hacer clic fuera */
    document.addEventListener('click', function (e) {
        var inTrigger = e.target.closest('[onclick*="Layout.toggleDrop"]') ||
            e.target.closest('[onclick*="Layout.setEnv"]') ||
            e.target.closest('#notifBtn') ||
            e.target.closest('#userBtn') ||
            e.target.closest('#envBadge');
        if (inTrigger) return;
        DROP_IDS.forEach(function (d) {
            var el = document.getElementById(d);
            if (el && !el.contains(e.target)) el.classList.remove('open');
        });
    });

    /* ── Selector de entorno ───────────────────────────────── */
    function setEnv(env, label) {
        /* Checkmarks */
        ['prod', 'qa', 'dev'].forEach(function (e) {
            var ic = document.getElementById('envChk-' + e);
            if (ic) ic.style.display = (e === env) ? '' : 'none';
        });
        /* Badge del sidebar */
        var badge = document.getElementById('envBadge');
        if (badge) {
            badge.className = 'layout-env-badge env-' +
                (env === 'dev' ? 'dev' : env === 'qa' ? 'qa' : 'prod');
            var lbl = document.getElementById('envBadgeLabel');
            if (lbl) lbl.textContent = label;
        }
        /* Pill del topbar (tenant · env) */
        var utenant = document.querySelector('.layout-topbar-utenant');
        if (utenant) {
            var parts = utenant.textContent.split('·');
            utenant.textContent = (parts[0] ? parts[0].trim() : '') + ' · ' + label;
        }
        /* Pill del welcome banner del Dashboard (si existe) */
        var wbPill = document.getElementById('wbEnvPill');
        if (wbPill) {
            wbPill.className = 'wb-pill env-' +
                (env === 'dev' ? 'dev' : env === 'qa' ? 'qa' : 'prod');
            wbPill.innerHTML =
                '<i class="bi bi-circle-fill" style="font-size:.5rem;" aria-hidden="true"></i> ' + label;
        }
        closeAllDrops();
        var tipos = { prod: 'success', qa: 'warning', dev: 'danger' };
        CorporateDS.Toast.show({
            type: tipos[env] || 'info',
            title: 'Entorno',
            message: 'Cambiado a <strong>' + label + '</strong>'
        });
    }

    /* ── Badge de notificaciones ───────────────────────────── */
    function setNotifCount(n) {
        var els = [
            document.getElementById('topbarNotifCount'),
            document.getElementById('notifBadgeCount'),
            document.getElementById('sbNotifBadge')
        ];
        els.forEach(function (el) {
            if (!el) return;
            el.textContent = n;
            el.style.display = n > 0 ? '' : 'none';
        });
    }

    /* ── Modal: Actualizar datos ───────────────────────────── */
    function abrirModalActualizar() {
        closeAllDrops();
        new bootstrap.Modal(document.getElementById('layoutUpdateModal')).show();
    }

    async function guardarActualizacion() {
        var btn = document.getElementById('layoutBtnGuardarUpd');
        CorporateDS.Button.setLoading(btn, 'Guardando…');
        await new Promise(function (r) { setTimeout(r, 1400); }); /* fetch real aquí */
        CorporateDS.Button.stopLoading(btn);
        bootstrap.Modal.getInstance(
            document.getElementById('layoutUpdateModal')
        ).hide();
        CorporateDS.Toast.success('Datos actualizados correctamente', 'Perfil');
    }

    /* ── Modal: Cerrar sesión ──────────────────────────────── */
    function confirmarLogout() {
        closeAllDrops();
        new bootstrap.Modal(document.getElementById('layoutLogoutModal')).show();
    }

    /* ── Init ──────────────────────────────────────────────── */
    CorporateDS.Alert.init();

    /* API pública */
    return {
        toggleSidebar: toggleSidebar,
        closeSidebar: closeSidebar,
        toggleDrop: toggleDrop,
        closeAllDrops: closeAllDrops,
        setEnv: setEnv,
        setNotifCount: setNotifCount,
        abrirModalActualizar: abrirModalActualizar,
        guardarActualizacion: guardarActualizacion,
        confirmarLogout: confirmarLogout
    };

    /* ── Reloj en tiempo real ──────────────────────────────── */
    function actualizarReloj() {
        var now = new Date();
        var h = now.getHours() % 12 || 12;
        var m = String(now.getMinutes()).padStart(2, '0');
        var ampm = now.getHours() < 12 ? 'AM' : 'PM';
        var el = document.getElementById('wbClock');
        if (el) el.innerHTML =
            '<i class="bi bi-clock" aria-hidden="true"></i> ' + h + ':' + m + ' ' + ampm;
    }
    actualizarReloj();
    setInterval(actualizarReloj, 30000);

    /* ── Marcar notificaciones como leídas ─────────────────── *
       Limpia los dots de esta página Y los del dropdown
       del Layout (que lleva su propio badge).               */
    window.dbMarcarLeidas = function (e) {
        e.preventDefault();
        /* Dots del panel de esta vista */
        document.querySelectorAll('.db-notif-dot').forEach(function (d) { d.remove(); });
        /* Badge del panel */
        var badge = document.getElementById('dbNotifCount');
        if (badge) badge.style.display = 'none';
        /* Sincronizar badge del Layout (topbar + sidebar) */
        Layout.setNotifCount(0);
        /* Dots del dropdown del Layout */
        document.querySelectorAll('#notifDrop .layout-notif-dot').forEach(function (d) { d.remove(); });
        CorporateDS.Toast.success('Todas las notificaciones marcadas como leídas');
    };

    /* ── Navegación rápida ─────────────────────────────────── */
    window.dbNavegar = function (url) {
        if (url && url !== '#') {            
            window.location.href = url;
        } else {
            CorporateDS.Toast.info('Módulo en construcción');
        }
    };
}());