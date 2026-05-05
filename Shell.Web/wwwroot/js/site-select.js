(function () {
    'use strict';

    /* ── Partículas decorativas ─────────────────────────────── */
    const container = document.getElementById('bgParticles');
    if (container) {
        for (let i = 0; i < 22; i++) {
            const p = document.createElement('div');
            p.className = 'particle';
            const size = Math.random() * 6 + 2;
            p.style.cssText = [
                `width:${size}px`,
                `height:${size}px`,
                `left:${Math.random() * 100}%`,
                `bottom:${Math.random() * 20 - 10}%`,
                `animation-duration:${8 + Math.random() * 14}s`,
                `animation-delay:${Math.random() * 10}s`,
                `opacity:${(0.05 + Math.random() * 0.15).toFixed(2)}`
            ].join(';');
            container.appendChild(p);
        }
    }

    /* ── Inicializar alerts ─────────────────────────────────── */
    CorporateDS.Alert.init();
}());