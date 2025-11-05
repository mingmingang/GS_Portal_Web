$(document).ready(function () {
    // --- State Management ---
    const state = {
        currentYear: new Date().getFullYear(),
        allCutiData: [],
        filteredCutiData: [],
        currentTab: 'Semua',
        currentPage: 1,
        totalPages: 1,
        itemsPerPage: 6
    };

    // --- DOM Elements Cache ---
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

    // --- API Calls (Tetap sama) ---
    const api = {
        fetchApprovalList: function (year) {
            const url = `/api/CutiApi/listUnverifiedCuti?year=${year}`;
            return $.ajax({ url: url, method: 'GET' });
        },
        processApproval: function (requestId, newStatus, reason = '') {
            const url = `/api/CutiApi/processApproval`;
            return $.ajax({
                url: url,
                method: 'POST', contentType: 'application/json',
                data: JSON.stringify({ request_no: requestId, status: newStatus, reason: reason })
            });
        }
    };

    // --- Logic & Rendering ---

    function updateCutiListView() {
        state.filteredCutiData = state.allCutiData.filter(item => {
            if (state.currentTab === 'Semua') {
                return true;
            }
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

    // ==========================================================
    // ===          FUNGSI INI TELAH DIPERBAIKI             ===
    // ==========================================================
    function renderPage(page) {
        elements.listContainer.empty();
        const { filteredCutiData, itemsPerPage } = state; // Menggunakan data yang sudah difilter

        if (filteredCutiData.length === 0) {
            const message = state.allCutiData.length > 0 ? 'Tidak ada data untuk filter ini.' : 'Tidak ada pengajuan cuti yang perlu disetujui.';
            elements.noDataMessage.text(message).show();
            return;
        }
        elements.noDataMessage.hide();

        const startIndex = (page - 1) * itemsPerPage;
        const pageData = filteredCutiData.slice(startIndex, startIndex + itemsPerPage); // Melakukan slice pada data yang sudah difilter
        const cardsHtml = pageData.map(item => createCutiCardHtml(item)).join('');
        elements.listContainer.html(cardsHtml);
    }

    // GANTI FUNGSI LAMA DENGAN VERSI BARU INI
    function createCutiCardHtml(item) {
        const startDate = new Date(item.startdate).toLocaleDateString('id-ID', { day: '2-digit', month: 'short' });
        const endDate = new Date(item.enddate).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' });
        const statusText = item.request_status === 'Unverified' ? 'Menunggu Persetujuan' : item.request_status;

        return `
        <div class="cuti-card-new">
            <div class="card-header-new">
                <span class="request-no">${item.request_no}</span>
                <span class="status-badge-new status-menunggu">${statusText}</span>
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

    // ==========================================================
    // ===          FUNGSI INI TELAH DIPERBAIKI             ===
    // ==========================================================
    function setupPagination() {
        const totalPages = Math.ceil(state.filteredCutiData.length / state.itemsPerPage); // Menghitung dari data yang sudah difilter
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

    // Sisa fungsi (handleApprovalAction, renderYearFilter, etc.) dan event handler lainnya sudah benar.
    function handleApprovalAction(requestId, action) {
        const isApprove = action === 'approve';
        const config = {
            title: isApprove ? 'Setujui Pengajuan?' : 'Tolak Pengajuan?',
            text: `Anda akan ${isApprove ? 'menyetujui' : 'menolak'} pengajuan cuti ini.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: isApprove ? '#28a745' : '#dc3545',
            confirmButtonText: isApprove ? 'Ya, Setujui!' : 'Ya, Tolak!',
            cancelButtonText: 'Batal',
        };
        if (!isApprove) {
            config.input = 'textarea';
            config.inputLabel = 'Alasan Penolakan';
            config.inputValidator = (value) => !value && 'Anda harus memberikan alasan penolakan!';
        }
        Swal.fire(config).then((result) => {
            if (result.isConfirmed) {
                const newStatus = isApprove ? 'Approved' : 'Rejected';
                const reason = result.value || '';
                api.processApproval(requestId, newStatus, reason)
                    .done(() => {
                        Swal.fire('Berhasil!', 'Status pengajuan cuti telah diperbarui.', 'success');
                        loadCutiList();
                    })
                    .fail(() => Swal.fire('Gagal!', 'Terjadi kesalahan saat memproses permintaan.', 'error'));
            }
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
    elements.listContainer.on('click', '.btn-approve-new', function () {
        handleApprovalAction($(this).data('id'), 'approve');
    });
    elements.listContainer.on('click', '.btn-reject-new', function () {
        handleApprovalAction($(this).data('id'), 'reject');
    });

    // --- Inisialisasi Halaman ---
    function init() {
        elements.dateRange.text(`Periode ${state.currentYear}`);
        loadCutiList();
    }
    init();
});