// --- CutiMenu.js ---

const CutiApp = {
    // Konfigurasi dan State Aplikasi
    state: {
        employeeId: null,
        currentYear: new Date().getFullYear(),
        allCutiData: [],
        filteredData: [],
        currentPage: 1,
        itemsPerPage: 6,
    },

    // Penyimpanan elemen DOM yang sering digunakan
    elements: {
        container: null,
        summaryTbody: null,
        masaBerlakuText: null,
        listContainer: null,
        noDataMessage: null,
        paginationControls: null,
        backdrop: null,
        panduanModal: null,
        filterTahunModal: null,
        yearList: null,
    },

    // Inisialisasi aplikasi
    init: function () {
        // Ambil employeeId dari data attribute di HTML
        this.state.employeeId = $('.cuti-page-container').data('employee-id');
        if (!this.state.employeeId) {
            console.error("Employee ID not found!");
            return;
        }

        // Panggil fungsi untuk cache elemen DOM
        this.cacheDOMElements();
        // Panggil fungsi untuk mendaftarkan event listeners
        this.bindEvents();
        // Muat data awal saat halaman pertama kali dibuka
        this.loadInitialData();
    },

    // Menyimpan referensi ke elemen DOM agar tidak query berulang kali
    cacheDOMElements: function () {
        this.elements.container = $('.cuti-page-container');
        this.elements.summaryTbody = $('#summary-tbody');
        this.elements.masaBerlakuText = $('#masa-berlaku-text');
        this.elements.listContainer = $('.cuti-list-container');
        this.elements.noDataMessage = $('.no-data-message');
        this.elements.paginationControls = $('#pagination-controls');
        this.elements.backdrop = $('#modalBackdrop');
        this.elements.panduanModal = $('#panduanModal');
        this.elements.filterTahunModal = $('#filterTahunModal');
        this.elements.yearList = $('.year-list');
    },

    // Mendaftarkan semua event listener di satu tempat
    bindEvents: function () {
        const self = this; // Simpan konteks 'this'

        // Event untuk tab cuti
        this.elements.container.on('click', '.cuti-tab-item', function () {
            $('.cuti-tab-item').removeClass('active');
            $(this).addClass('active');
            self.handleTabChange();
        });

        // Event untuk filter status
        this.elements.container.on('click', '.status-filter-badge', function () {
            $('.status-filter-badge').removeClass('active');
            $(this).addClass('active');
            self.updateCutiListView();
        });

        // Event untuk tombol pagination
        $('#prev-page-btn').on('click', () => self.changePage(-1));
        $('#next-page-btn').on('click', () => self.changePage(1));

        // Event untuk modal
        $('#panduanBtn').on('click', () => self.openModal(self.elements.panduanModal));
        $('#filterBtn').on('click', () => {
            self.renderYearList(self.state.currentYear);
            self.openModal(self.elements.filterTahunModal);
        });
        $('.modal-close-btn').on('click', () => self.closeModal());
        this.elements.backdrop.on('click', (e) => {
            if ($(e.target).is(self.elements.backdrop)) self.closeModal();
        });

        // Event untuk filter tahun di modal
        this.elements.yearList.on('click', '.year-arrow', function () {
            const direction = $(this).data('direction') === 'up' ? 1 : -1;
            self.state.currentYear += direction;
            self.renderYearList(self.state.currentYear);
        });
        this.elements.yearList.on('click', '.year-item:not(.selected)', function () {
            self.state.currentYear = parseInt($(this).text().trim());
            self.closeModal();
            self.handleYearChange();
        });
    },

    // Logika yang dijalankan saat tab diganti
    handleTabChange: function () {
        const selectedLeaveCode = $('.cuti-tab-item.active').data('tab-code');
        const $summaryCard = $('#summary-card-container');
        const $masaBerlaku = $('#masa-berlaku-container');

        if (selectedLeaveCode === 'CK') {
            $summaryCard.hide();
            $masaBerlaku.hide();
        } else {
            $summaryCard.show();
            $masaBerlaku.show();
            this.fetchSummaryData(selectedLeaveCode);
        }
        this.triggerDataLoad();
    },

    // Logika yang dijalankan saat tahun diganti
    handleYearChange: function () {
        const activeLeaveCode = $('.cuti-tab-item.active').data('tab-code');
        if (activeLeaveCode !== 'CK') {
            this.fetchSummaryData(activeLeaveCode);
        }
        this.triggerDataLoad();
    },

    // Memuat data awal
    loadInitialData: function () {
        const initialLeaveCode = $('.cuti-tab-item.active').data('tab-code');
        this.fetchSummaryData(initialLeaveCode);
        this.triggerDataLoad();
    },

    // Mengambil data jatah cuti dari API
    fetchSummaryData: function (leaveCode) {
        const { employeeId, currentYear } = this.state;
        const url = `/api/CutiApi/listJatahCuti?emp_id=${employeeId}&leave_code=${leaveCode}&from=${currentYear}-01-01&to=${currentYear}-12-31`;

        this.elements.summaryTbody.html('<tr><td colspan="2" style="text-align:center;">Memuat data...</td></tr>');
        this.elements.masaBerlakuText.find('.date-highlight').text('...');

        $.ajax({
            url: url,
            type: 'GET',
            success: (response) => {
                if (response && response.data && response.data.length > 0) {
                    const latestData = response.data.sort((a, b) => new Date(b.created_date) - new Date(a.created_date))[0];
                    this.renderSummary(latestData);
                } else {
                    this.elements.summaryTbody.html('<tr><td colspan="2" style="text-align:center;">Data jatah cuti tidak ditemukan.</td></tr>');
                    this.elements.masaBerlakuText.find('.date-highlight').text('-');
                }
            },
            error: () => {
                this.elements.summaryTbody.html('<tr><td colspan="2" style="text-align:center;">Gagal memuat data.</td></tr>');
                this.elements.masaBerlakuText.find('.date-highlight').text('-');
            }
        });
    },

    // Merender tampilan summary jatah cuti
    renderSummary: function (data) {
        const totalEntitlement = (data.entitlement || 0) + (data.bringforward || 0) + (data.adjustment || 0);
        const sisaCutiAsli = data.remaining || 0;

        const summaryHtml = `
            <tr><td>Hak Cuti</td><td>${totalEntitlement.toFixed(0)}</td></tr>
            <tr><td>Pemakaian Cuti</td><td>${(data.used || 0).toFixed(0)}</td></tr>
            <tr id="cuti-on-progress-row"><td>Cuti On Progress</td><td>0</td></tr>
            <tr class="sisa-cuti" data-original-sisa="${sisaCutiAsli}">
                <td>Sisa Cuti</td>
                <td>${sisaCutiAsli.toFixed(0)}</td>
            </tr>`;
        this.elements.summaryTbody.html(summaryHtml);

        if (data.endvaliddate) {
            const endDate = new Date(data.endvaliddate);
            this.elements.masaBerlakuText.find('.date-highlight').text(endDate.toLocaleDateString('id-ID', { year: 'numeric', month: 'long', day: 'numeric' }));
        } else {
            this.elements.masaBerlakuText.find('.date-highlight').text('Tidak terbatas');
        }
        this.updateCutiListView(); // Re-calculate sisa cuti after summary is rendered
    },

    formatDateToDDMMYYYY: function (date) {
        if (!(date instanceof Date) || isNaN(date)) {
            return ''; // Kembalikan string kosong jika input tidak valid
        }
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0'); // Bulan dimulai dari 0 (Januari=0)
        const year = date.getFullYear();
        return `${day}/${month}/${year}`;
    },

    // Memicu pengambilan data list cuti berdasarkan tab dan tahun
    triggerDataLoad: function () {
        const selectedLeaveCode = $('.cuti-tab-item.active').data('tab-code');
        const { currentYear } = this.state;

        let fromDateObj, toDateObj, displayRangeText;

        if (selectedLeaveCode === 'CB') {
            const startYear = currentYear - 4;
            fromDateObj = new Date(startYear, 0, 1); // Bulan 0 = Januari
            toDateObj = new Date(currentYear, 11, 31); // Bulan 11 = Desember
            displayRangeText = `1 Jan ${startYear} - 31 Des ${currentYear}`;
        } else if (selectedLeaveCode === 'CP') {
            const nextYear = currentYear + 1;
            fromDateObj = new Date(currentYear, 0, 1);
            toDateObj = new Date(nextYear, 2, 31); // Bulan 2 = Maret
            displayRangeText = `1 Jan ${currentYear} - 31 Mar ${nextYear}`;
        } else { // CK
            fromDateObj = new Date(currentYear, 0, 1);
            toDateObj = new Date(currentYear, 11, 31);
            displayRangeText = `1 Jan ${currentYear} - 31 Des ${currentYear}`;
        }

        // Format tanggal ke DD/MM/YYYY sebelum dikirim ke API
        const fromDate = this.formatDateToDDMMYYYY(fromDateObj);
        const toDate = this.formatDateToDDMMYYYY(toDateObj);

        $('.date-range').text(displayRangeText);
        this.fetchCutiList(fromDate, toDate);
    },

    // Mengambil data list pengajuan cuti dari API
    fetchCutiList: function (fromDate, toDate) {
        // URL sekarang akan menerima format tanggal yang baru (DD/MM/YYYY)
        const url = `/api/CutiApi/listCuti?emp_id=${this.state.employeeId}&from=${fromDate}&to=${toDate}`;

        this.elements.listContainer.html('');
        this.elements.paginationControls.hide();
        this.elements.noDataMessage.text('Memuat daftar cuti...').show();
        this.state.allCutiData = [];

        $.ajax({
            url: url,
            type: 'GET',
            success: (response) => {
                // Console.log ini akan menunjukkan URL dengan format tanggal yang baru
                console.log("API URL Called:", url);
                this.state.allCutiData = (response && response.data) ? response.data : [];
                this.updateCutiListView();
            },
            error: () => {
                this.state.allCutiData = [];
                this.updateCutiListView();
                this.elements.noDataMessage.text('Gagal memuat daftar pengajuan cuti.').show();
            }
        });
    },

    // Memfilter dan memperbarui tampilan list cuti
    updateCutiListView: function () {
        const selectedCategoryCode = $('.cuti-tab-item.active').data('tab-code');
        const selectedStatus = $('.status-filter-badge.active').data('status');

        this.state.filteredData = this.state.allCutiData.filter(cuti => {
            const statusText = this.mapStatus(cuti.request_status);

            let filterCategory = '';
            if (cuti.leave_code.startsWith('CP')) filterCategory = 'CP';
            else if (cuti.leave_code === 'Cuti Besar') filterCategory = 'CB';
            else filterCategory = 'CK';

            const categoryMatch = (selectedCategoryCode === 'CK') ? (filterCategory === 'CK') : (filterCategory === selectedCategoryCode);
            const statusMatch = (selectedStatus === 'Semua') || (statusText === selectedStatus) ||
                (selectedStatus === 'Terlaksana' && (statusText === 'Terlaksana' || statusText === 'Belum Terlaksana'));

            return categoryMatch && statusMatch;
        });

        // Hitung ulang cuti on progress dan sisa cuti
        const onProgressCount = this.state.allCutiData
            .filter(cuti => this.mapStatus(cuti.request_status) === 'Menunggu Persetujuan')
            .reduce((total, cuti) => total + cuti.totaldays, 0);

        $('#cuti-on-progress-row td:last-child').text(onProgressCount.toFixed(0));

        const $sisaCutiRow = $('.sisa-cuti');
        const originalSisaCuti = parseFloat($sisaCutiRow.data('original-sisa')) || 0;
        $sisaCutiRow.find('td:last-child').text((originalSisaCuti - onProgressCount).toFixed(0));

        this.state.currentPage = 1;
        this.renderPage(1);
        this.setupPagination();
    },

    // Merender satu halaman dari data cuti yang sudah difilter
    renderPage: function (page) {
        this.elements.listContainer.html('');
        const { filteredData, itemsPerPage } = this.state;

        if (filteredData.length === 0) {
            this.elements.noDataMessage.text(this.state.allCutiData.length > 0 ? 'Tidak ada data dengan filter ini.' : 'Tidak ada pengajuan cuti pada periode ini.').show();
            return;
        }
        this.elements.noDataMessage.hide();

        const startIndex = (page - 1) * itemsPerPage;
        const pageData = filteredData.slice(startIndex, startIndex + itemsPerPage);

        const cardsHtml = pageData.map(cuti => this.createCutiCard(cuti)).join('');
        this.elements.listContainer.html(cardsHtml);
    },

    // Membuat HTML untuk satu kartu cuti
    createCutiCard: function (cuti) {
        const statusText = this.mapStatus(cuti.request_status);
        const statusClass = 'status-' + statusText.toLowerCase().replace(/ /g, '-');
        const startDate = new Date(cuti.startdate).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' });
        const endDate = new Date(cuti.enddate).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' });
        const canCancel = !['Terlaksana', 'Ditolak', 'Dibatalkan', 'Draft'].includes(statusText);

        let displayLeaveType = cuti.leave_desc;
        if (cuti.leave_code.startsWith('CP')) displayLeaveType = 'Cuti Pribadi';
        else if (cuti.leave_code === 'Cuti Besar') displayLeaveType = 'Cuti Besar';

        const actionLink = statusText === 'Draft'
            ? `<a href="/Manage/ManageEditCuti?id=${cuti.request_no}" class="lihat-link edit-link">Edit &rarr;</a>`
            : `<a href="/Manage/ManageDetailCuti?id=${cuti.request_no}" class="lihat-link">Lihat &rarr;</a>`;

        return `
        <div class="cuti-item-card" data-status="${statusText}">
            <div class="cuti-item-header">
                <span class="cuti-id">${cuti.request_no}</span>
                <div>
                    <span class="cuti-item-status ${statusClass}">${statusText}</span>
                    ${canCancel ? `<a href="/Manage/ManagePembatalanCuti?id=${cuti.request_no}" class="cuti-item-cancel-btn">X</a>` : ''}
                </div>
            </div>
            <div class="cuti-item-body"><div class="cuti-type">${displayLeaveType}</div></div>
            <div class="cuti-item-footer">
                <div class="date-info"><i class="fas fa-clock"></i> <span>${startDate} s/d ${endDate}</span></div>
                ${actionLink}
            </div>
        </div>`;
    },

    // Mengatur tampilan dan status tombol pagination
    setupPagination: function () {
        const totalPages = Math.ceil(this.state.filteredData.length / this.state.itemsPerPage);

        if (totalPages <= 1) {
            this.elements.paginationControls.hide();
        } else {
            this.elements.paginationControls.show();
            $('#page-info').text(`Halaman ${this.state.currentPage} dari ${totalPages}`);
            $('#prev-page-btn').prop('disabled', this.state.currentPage === 1);
            $('#next-page-btn').prop('disabled', this.state.currentPage === totalPages);
        }
    },

    // Logika untuk pindah halaman
    changePage: function (direction) {
        const newPage = this.state.currentPage + direction;
        const totalPages = Math.ceil(this.state.filteredData.length / this.state.itemsPerPage);
        if (newPage >= 1 && newPage <= totalPages) {
            this.state.currentPage = newPage;
            this.renderPage(newPage);
            this.setupPagination();
        }
    },

    // Fungsi utilitas untuk membuka dan menutup modal
    openModal: function (modalElement) {
        this.elements.backdrop.addClass('show');
        modalElement.show();
    },
    closeModal: function () {
        this.elements.backdrop.removeClass('show');
        this.elements.panduanModal.hide();
        this.elements.filterTahunModal.hide();
    },

    // Merender daftar tahun pada modal filter
    renderYearList: function (selectedYear) {
        this.elements.yearList.html(`
            <div class="year-arrow" data-direction="up"><i class="fas fa-chevron-up"></i></div>
            <div class="year-item">${selectedYear - 1}</div>
            <div class="year-item selected">${selectedYear}</div>
            <div class="year-item">${selectedYear + 1}</div>
            <div class="year-arrow" data-direction="down"><i class="fas fa-chevron-down"></i></div>
        `);
    },

    // Memetakan status dari API ke teks yang lebih ramah pengguna
    mapStatus: function (apiStatus) {
        const statusMap = {
            "Unverified": "Menunggu Persetujuan",
            "Partially Approved": "Disetujui Sebagian",
            "Fully Approved": "Disetujui",
            "Rejected": "Ditolak",
            "Cancelled": "Dibatalkan",
            "Closed": "Terlaksana",
            "Revised": "Direvisi",
            "Draft": "Draft"
        };
        return statusMap[apiStatus] || apiStatus;
    }
};

// Inisialisasi aplikasi setelah dokumen siap
$(document).ready(function () {
    CutiApp.init();
});
