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
                cancellationReason: $('#detail-cancellation-reason'),
                reasonRow: $('#detail-cancellation-row'),
                reasonLabel: $('#detail-cancellation-row .detail-label'),
                reasonText: $('#detail-cancellation-reason'),
            };
        },

        // --- FUNGSI BARU UNTUK MAPPING PLANT ---
        mapPlantCode: function (plantCode) {
            switch (plantCode) {
                case 'K':
                    return 'Karawang';
                case 'J':
                    return 'Jakarta';
                case 'S':
                    return 'Semarang';
                default:
                    return plantCode || '-';
            }
        },
        // -----------------------------------------

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
                        const statusText = self.mapStatus(cuti.request_status);

                        $('.placeholder-glow').removeClass('placeholder-glow');

                        self.elements.status.text(statusText).attr('class', 'status-badge').addClass(self.getStatusBadgeClass(cuti.request_status));
                        self.elements.noPengajuan.text(cuti.request_no);

                        // --- BARIS YANG DIPERBARUI ---
                        // Panggil fungsi pemetaan untuk mendapatkan nama lengkap plant
                        const fullPlantName = self.mapPlantCode(self.config.userPlant);
                        self.elements.plant.text(fullPlantName);
                        // -----------------------------

                        self.elements.tipeCuti.text(cuti.leave_code || '-');

                        const tglPengajuan = new Date(cuti.requestdate);
                        self.elements.tglPengajuan.text(tglPengajuan.toLocaleDateString('id-ID', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }));

                        const tglMulai = new Date(cuti.startdate);
                        const tglSelesai = new Date(cuti.enddate);
                        self.elements.tglCuti.text(`${tglMulai.toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' })} - ${tglSelesai.toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' })}`);

                        self.elements.durasi.text(`${cuti.totaldays} hari`);
                        let originalRemark = cuti.remark || '-';
                        let extraReason = null;
                        let reasonLabel = '';

                        const reasonSeparators = [
                            { status: 'Cancelled', separator: 'Cancellation Reason:', label: 'Alasan Pembatalan' },
                            { status: 'Rejected', separator: 'Rejection Reason:', label: 'Alasan Penolakan' }
                        ];

                        const matchedSeparator = reasonSeparators.find(s => s.status === cuti.request_status);

                        if (matchedSeparator && originalRemark.includes(matchedSeparator.separator)) {
                            const parts = originalRemark.split(matchedSeparator.separator);
                            originalRemark = parts[0].trim() || '-';
                            extraReason = parts[1].trim();
                            reasonLabel = matchedSeparator.label;
                        }

                        self.elements.keterangan.text(originalRemark);
                        self.elements.reasonRow.hide();
                        if (extraReason) {
                            self.elements.reasonLabel.text(reasonLabel);
                            self.elements.reasonText.text(extraReason);
                            self.elements.reasonRow.css('display', 'flex');
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

    window.DetailCutiPage = DetailCutiPage;

})(jQuery);