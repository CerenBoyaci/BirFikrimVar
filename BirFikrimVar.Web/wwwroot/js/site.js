(function () {
    'use strict';

    // ===== Sidebar toggle (mobil) =====
    var sidebar = document.getElementById('sidebar');
    var overlay = document.getElementById('sidebarOverlay');
    var toggleBtn = document.getElementById('sidebarToggle');

    function openSidebar() {
        if (!sidebar) return;
        sidebar.classList.add('open');
        if (overlay) overlay.classList.add('open');
        document.body.style.overflow = 'hidden';
    }

    function closeSidebar() {
        if (!sidebar) return;
        sidebar.classList.remove('open');
        if (overlay) overlay.classList.remove('open');
        document.body.style.overflow = '';
    }

    if (toggleBtn) toggleBtn.addEventListener('click', openSidebar);
    if (overlay) overlay.addEventListener('click', closeSidebar);

    // ===== Aktif menü linkini URL'den bul =====
    var currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.menu-link').forEach(function (link) {
        var href = link.getAttribute('href');
        if (!href) return;
        if (href !== '/' && currentPath.indexOf(href.toLowerCase()) === 0) {
            link.classList.add('active');
        }
    });

    // ===== Puanlama ortalama hesaplama =====
    function initPuanlama() {
        var inputs = document.querySelectorAll('.puan-input');
        var display = document.querySelector('.puan-ortalama-val');
        if (!inputs.length || !display) return;

        function hesapla() {
            var toplam = 0, adet = 0;
            inputs.forEach(function (inp) {
                var v = parseFloat(inp.value);
                if (!isNaN(v)) {
                    inp.value = Math.min(10, Math.max(1, Math.round(v)));
                    toplam += parseFloat(inp.value);
                    adet++;
                }
            });
            display.textContent = adet > 0 ? (toplam / adet).toFixed(1) : '—';
        }

        inputs.forEach(function (inp) {
            inp.addEventListener('input', hesapla);
        });
    }

    initPuanlama();

}());