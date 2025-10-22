$(document).ready(function () {
    // --- State Management (Sama seperti CutiAtasan.js) ---
    const state = {
        currentYear: new Date().getFullYear(),
        allCutiData: [],
        filteredCutiData: [],
        currentTab: 'Semua',
        currentPage: 1,
        totalPages: 1,
        itemsPerPage: 6
    };

    // --- DOM Elements Cache (Sama seperti CutiAtasan.js) ---
    const elements = {
        listContainer: $('.cuti-list-container'),
        noDataMessage: $('.no-data-message'),
        dateRange: $('.date-range'),
        paginationControls: $('#pagination-controls'),
        pageInfo: $('#page-info'),
        prevPageBtn: $('#prev-page-btn'),
        nextPageBtn: $('#next-page-btn'),
        filterBtn: $('#filterBtn'),
        tabsContainer: $('.cuti-tabs'),
        modalBackdrop: $('#modalBackdrop'),
        filterTahunModal: $('#filterTahunModal'),
        yearList: $('.year-list'),
        modalCloseBtn: $('.modal-close-btn')
    };

    // ==========================================================
    // ===       PERUBAHAN UTAMA ADA DI URL API INI         ===
    // ==========================================================
    const api = {
        // Ganti URL untuk mengambil data 'Partially Approved'
        fetchApprovalList: function (year) {
            // URL ini harus cocok dengan Route yang baru Anda buat
            const url = `/api/CutiApi/listPartiallyApprovedCuti?year=${year}`;
            return $.ajax({ url: url, method: 'GET' });
        },
        // Fungsi processApproval bisa jadi tetap sama jika endpointnya bisa menangani approval dari HC
        processApproval: function (requestId, newStatus, reason = '') {
            const url = `/api/CutiApi/processApproval`; // Sesuaikan jika perlu
            return $.ajax({
                url: url,
                method: 'POST', contentType: 'application/json',
                data: JSON.stringify({ request_no: requestId, status: newStatus, reason: reason })
            });
        }
    };

    // --- Logic & Rendering (Sama seperti CutiAtasan.js) ---
    function updateCutiListView() {
        state.filteredCutiData = state.allCutiData.filter(item => {
            if (state.currentTab === 'Semua') return true;
            let itemCategory = '';
            if (item.leave_code.startsWith('CP')) itemCategory = 'CP';
            else if (item.leave_code.startsWith('CB') || item.leave_code === 'Cuti Besar') itemCategory = 'CB';
            else itemCategory = 'CK';
            return itemCategory === state.currentTab;
        });
        state.currentPage = 1;
        renderPage(1);
        setupPagination();
    }

    function renderPage(page) {
        elements.listContainer.empty();
        const { filteredCutiData, itemsPerPage } = state;
        if (filteredCutiData.length === 0) {
            const message = state.allCutiData.length > 0 ? 'Tidak ada data untuk filter ini.' : 'Tidak ada pengajuan cuti yang perlu disetujui.';
            elements.noDataMessage.text(message).show();
            return;
        }
        elements.noDataMessage.hide();
        const startIndex = (page - 1) * itemsPerPage;
        const pageData = filteredCutiData.slice(startIndex, startIndex + itemsPerPage);
        const cardsHtml = pageData.map(item => createCutiCardHtml(item)).join('');
        elements.listContainer.html(cardsHtml);
    }

    // ==========================================================
    // ===        FUNGSI CARD DIBUAT LEBIH DINAMIS          ===
    // ==========================================================
    function createCutiCardHtml(item) {
        const startDate = new Date(item.startdate).toLocaleDateString('id-ID', { day: '2-digit', month: 'short' });
        const endDate = new Date(item.enddate).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' });

        // Logika untuk status dan class CSS
        let statusText = item.request_status;
        let statusClass = '';
        if (statusText.toLowerCase().includes('partially approved')) {
            statusClass = 'status-partial'; // Class untuk status kuning
            statusText = 'Menunggu Persetujuan HC'; // Teks yang lebih ramah pengguna
        }

        return `
        <div class="cuti-card-new">
            <div class="card-header-new">
                <span class="request-no">${item.request_no}</span>
                <span class="status-badge-new ${statusClass}">${statusText}</span>
            </div>
            <div class="card-body-new">
                <p class="employee-name">${item.full_name}</p>
                <p class="leave-type-new">${item.leave_code}</p>
                <div class="date-info-new">
                    <i class="far fa-clock"></i>
                    <span>${startDate} s/d ${endDate}</span>
                </div>
            </div>
            <div class="card-actions-new">
                <a href="/Manage/ManageDetailCuti?id=${item.request_no}" class="action-btn-new">Lihat Detail &rarr;</a>
            </div>
        </div>
    `;
    }

    function setupPagination() {
        const totalPages = Math.ceil(state.filteredCutiData.length / state.itemsPerPage);
        state.totalPages = totalPages;
        if (totalPages <= 1) {
            elements.paginationControls.hide();
        } else {
            elements.paginationControls.show();
            elements.pageInfo.text(`Halaman ${state.currentPage} dari ${totalPages}`);
            elements.prevPageBtn.prop('disabled', state.currentPage === 1);
            elements.nextPageBtn.prop('disabled', state.currentPage === totalPages);
        }
    }

    // Fungsi lainnya tetap sama (loadCutiList, changePage, renderYearFilter, event handlers, dll.)
    // ... (salin sisa fungsi dari CutiAtasan.js ke sini) ...
    function changePage(direction) {
        const newPage = state.currentPage + direction;
        if (newPage >= 1 && newPage <= state.totalPages) {
            state.currentPage = newPage;
            renderPage(newPage);
            setupPagination();
        }
    }

    function loadCutiList() {
        elements.listContainer.empty();
        elements.noDataMessage.text('Memuat daftar pengajuan...').show();
        elements.paginationControls.hide();
        api.fetchApprovalList(state.currentYear)
            .done(function (response) {
                state.allCutiData = (response && response.data) ? response.data : [];
                updateCutiListView();
            })
            .fail(function () {
                state.allCutiData = [];
                updateCutiListView();
                elements.noDataMessage.text('Gagal memuat data. Coba lagi nanti.').show();
            });
    }

    function renderYearFilter() {
        const currentYear = new Date().getFullYear();
        let yearHtml = '';
        for (let i = currentYear; i >= currentYear - 5; i--) {
            yearHtml += `<div class="year-item ${i === state.currentYear ? 'active' : ''}" data-year="${i}">${i}</div>`;
        }
        elements.yearList.html(yearHtml);
    }

    // --- Event Handlers ---
    elements.tabsContainer.on('click', '.cuti-tab-item', function () {
        const $this = $(this);
        if ($this.hasClass('active')) return;
        elements.tabsContainer.find('.cuti-tab-item').removeClass('active');
        $this.addClass('active');
        state.currentTab = $this.data('tab-code');
        updateCutiListView();
    });
    elements.prevPageBtn.on('click', () => changePage(-1));
    elements.nextPageBtn.on('click', () => changePage(1));
    elements.filterBtn.on('click', () => {
        renderYearFilter();
        elements.filterTahunModal.show();
        elements.modalBackdrop.show();
    });
    elements.modalCloseBtn.add(elements.modalBackdrop).on('click', function (e) {
        if (e.target === this) {
            elements.filterTahunModal.hide();
            elements.modalBackdrop.hide();
        }
    });
    elements.yearList.on('click', '.year-item', function () {
        state.currentYear = $(this).data('year');
        elements.dateRange.text(`Periode ${state.currentYear}`);
        loadCutiList();
        elements.filterTahunModal.hide();
        elements.modalBackdrop.hide();
    });
    // Tidak ada event handler approve/reject di list, karena aksi dilakukan di halaman detail

    // --- Inisialisasi Halaman ---
    function init() {
        elements.dateRange.text(`Periode ${state.currentYear}`);
        loadCutiList();
    }
    init();
});