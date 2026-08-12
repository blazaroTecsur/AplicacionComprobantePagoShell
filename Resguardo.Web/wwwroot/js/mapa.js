let map, marker, geocoder, searchBox;

var fncMapa = {
    lat: null,
    lng: null,
    zoom: 15,    
    convert: function (val) {    
        
        var REGEX_COORD = /^[\s]*(-?\d{1,3}(?:\.\d+)?)\s*[,;\s]\s*(-?\d{1,3}(?:\.\d+)?)[\s]*$/;
        const match = val.match(REGEX_COORD);
        if (!match) return null;
        const lat = parseFloat(match[1]);
        const lng = parseFloat(match[2]);
        if (lat < -90 || lat > 90) return null;   // fuera de rango → no es coordenada válida
        if (lng < -180 || lng > 180) return null;
        return { lat: lat, lng: lng };
    },
    markerTitle: 'Arrastra para mover',
    init: function () {
        $('#mapa_btnCapturar').on('click', async function () {

            const $btn = $(this);
            $btn.prop('disabled', true).html('<i class="bi bi-arrow-clockwise"></i> Obteniendo Dirección');

            const pos = {
                lat: marker.getPosition().lat(),
                lng: marker.getPosition().lng()
            };
            // Geocodificación inversa
            const direccion = await fncMapa.obtenerDireccion(pos);
            $("#txtCoordenada").val(pos.lat.toFixed(8) + "," + pos.lng.toFixed(8));
            $("#txtCoordenada").attr('def', pos.lat.toFixed(8) + "," + pos.lng.toFixed(8));
            $("#txtDireccion").val(direccion);

            $btn.prop('disabled', false).html('<i class="bi bi-geo-alt-fill"></i> Capturar Ubicación');
            $("#modalMapa").modal("hide");
        });     

        // ── Buscar por coordenada ─────────────────────────────
        $('#mapa_btnBuscar').on('click', async function () {
            const valor = $('#mapa_txtBuscar').val().trim();
            if (!valor) return;

            // 1. Intentar interpretar como coordenadas
            const coords = fncMapa.convert(valor);
            if (coords) {
                fncMapa.moverMarcador(coords);
                return;
            }

            // 2. Si no son coordenadas, buscar como dirección de texto
            try {
                const { results } = await geocoder.geocode({ address: valor });
                if (results && results[0]) {
                    const pos = {
                        lat: results[0].geometry.location.lat(),
                        lng: results[0].geometry.location.lng()
                    };
                    fncMapa.moverMarcador(pos);
                }
            } catch (e) {
                alert('No se encontró la ubicación.');
            }
        });

        // También buscar al presionar Enter
        $('#mapa_txtBuscar').on('keydown', function (e) {
            if (e.key === 'Enter') $('#mapa_btnBuscar').trigger('click');
        });
    },
    moverMarcador: function (pos) {
        marker.setPosition(pos);
        map.panTo(pos);
        map.setZoom(fncMapa.zoom);
    },
    obtenerDireccion: function (pos) {
        return new Promise((resolve, reject) => {
            geocoder.geocode({ location: pos }, (results, status) => {
                if (status === 'OK' && results[0]) {
                    resolve(results[0].formatted_address);
                } else {
                    resolve('Dirección no disponible');
                }
            });
        });
    }    
}
// ─────────────────────────────────────────────────────────────
//  initMap — llamado por el callback de la API de Google Maps
// ─────────────────────────────────────────────────────────────
window.initMap = function () {

    const posInicial = { lat: fncMapa.lat, lng: fncMapa.lng };

    // 1. Crear el mapa
    map = new google.maps.Map(document.getElementById('mapa'), {
        center: posInicial,
        zoom: fncMapa.zoom,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        disableDefaultUI: false,
        mapTypeControl: true,
        mapTypeControlOptions: {
            style: google.maps.MapTypeControlStyle.DROPDOWN_MENU
        }
    });

    // 2. Geocoder para geocodificación inversa
    geocoder = new google.maps.Geocoder();

    // 3. Marcador arrastrable
    marker = new google.maps.Marker({
        position: posInicial,
        map: map,
        draggable: true,
        title: fncMapa.markerTitle,
        animation: google.maps.Animation.DROP,
        icon: {
            url: 'https://maps.google.com/mapfiles/ms/icons/red-dot.png',
            scaledSize: new google.maps.Size(40, 40),
            anchor: new google.maps.Point(20, 40)
        }
    });
    
    map.addListener('click', (e) => {
        const pos = { lat: e.latLng.lat(), lng: e.latLng.lng() };
        marker.setPosition(pos);
        map.panTo(pos);        
    });
}
