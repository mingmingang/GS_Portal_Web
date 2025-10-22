(function ($) {
    'use strict';

    var DetailCutiPage = {
        config: {
            cutiId: null,
            userPlant: '',
            backUrl: ''
        },

        elements: {},

        init: function (config) {
            this.config = $.extend(this.config, config);
            this.cacheElements();
            this.loadCutiDetail(this.config.cutiId);
        },

        cacheElements: function () {
            this.elements = {
                mainContent: $('.detail-main-content'),
                status: $('#detail-status'),
                noPengajuan: $('#detail-no-pengajuan'),
                plant: $('#detail-plant'),
                tipeCuti: $('#detail-tipe-cuti'),
                tglPengajuan: $('#detail-tgl-pengajuan'),
                tglCuti: $('#detail-tgl-cuti'),
                durasi: $('#detail-durasi'),
                keterangan: $('#detail-keterangan'),
                lampiranContainer: $('#detail-lampiran-container'),
                cancellationRow: $('#detail-cancellation-row'),
                cancellationReason: $('#detail-cancellation-reason')
            };
        },

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
        },

        getStatusBadgeClass: function (apiStatus) {
            const statusText = this.mapStatus(apiStatus);
            switch (statusText) {
                case 'Disetujui':
                case 'Terlaksana':
                    return 'badge-success';
                case 'Menunggu Persetujuan':
                    return 'badge-warning';
                case 'Ditolak':
                case 'Dibatalkan':
                    return 'badge-danger';
                default:
                    return 'badge-secondary';
            }
        },

        loadCutiDetail: function (id) {
            var self = this;
            if (!id) {
                self.elements.mainContent.html('<p style="text-align:center;">ID Pengajuan Cuti tidak valid.</p>');
                return;
            }

            $.ajax({
                url: `/api/CutiApi/detailCuti/${id}`,
                type: 'GET',
                success: function (response) {
                    if (response && response.data && response.data.length > 0) {
                        const cuti = response.data[0];
                        console.log("data cutii", cuti)
                        const statusText = self.mapStatus(cuti.request_status);

                        // Hapus placeholder/loading state
                        $('.placeholder-glow').removeClass('placeholder-glow');

                        // Isi data ke elemen
                        self.elements.status.text(statusText).attr('class', 'status-badge').addClass(self.getStatusBadgeClass(cuti.request_status));
                        self.elements.noPengajuan.text(cuti.request_no);
                        self.elements.plant.text(self.config.userPlant || '-');
                        self.elements.tipeCuti.text(cuti.leave_code || '-');

                        const tglPengajuan = new Date(cuti.requestdate);
                        self.elements.tglPengajuan.text(tglPengajuan.toLocaleDateString('id-ID', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }));

                        const tglMulai = new Date(cuti.startdate);
                        const tglSelesai = new Date(cuti.enddate);
                        self.elements.tglCuti.text(`${tglMulai.toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' })} - ${tglSelesai.toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' })}`);

                        self.elements.durasi.text(`${cuti.totaldays} hari`);
                        let originalRemark = cuti.remark || '-';
                        let cancellationReason = null;

                        // Tentukan string pembatas yang kita cari
                        const separator = "Cancellation Reason:";

                        // Cek jika statusnya 'Cancelled' DAN string remark mengandung pembatas
                        if (cuti.request_status === 'Cancelled' && originalRemark.includes(separator)) {
                            // Pecah string remark menjadi dua bagian berdasarkan separator
                            const parts = originalRemark.split(separator);

                            // Bagian pertama adalah keterangan asli (hapus spasi dan newline yang tidak perlu)
                            originalRemark = parts[0].trim();

                            // Bagian kedua adalah alasan pembatalan (hapus spasi yang tidak perlu)
                            cancellationReason = parts[1].trim();
                        }

                        // Sekarang, tampilkan hasilnya ke elemen yang sesuai
                        self.elements.keterangan.text(originalRemark || '-'); // Tampilkan keterangan asli

                        if (cancellationReason) {
                            self.elements.cancellationReason.text(cancellationReason);
                            self.elements.cancellationRow.css('display', 'flex');
                        }

                        const fileName = cuti.refdoc;
                        if (fileName) {
                            const fileUrl = `/api/CutiApi/getLampiran/${encodeURIComponent(fileName)}`;
                            const linkHtml = `<a href="${fileUrl}" target="_blank" class="attachment-link"><i class="fas fa-paperclip"></i> Lihat Lampiran</a>`;
                            self.elements.lampiranContainer.html(linkHtml);
                        } else {
                            self.elements.lampiranContainer.html('<p class="text-muted" style="margin:0;">Tidak ada lampiran.</p>');
                        }
                    } else {
                        self.elements.mainContent.html('<p style="text-align:center;">Detail cuti tidak ditemukan.</p>');
                    }
                },
                error: function () {
                    self.elements.mainContent.html('<p style="text-align:center;">Gagal memuat detail cuti.</p>');
                }
            });
        }
    };

    // Ekspos objek ke global scope agar bisa dipanggil dari view
    window.DetailCutiPage = DetailCutiPage;

})(jQuery);