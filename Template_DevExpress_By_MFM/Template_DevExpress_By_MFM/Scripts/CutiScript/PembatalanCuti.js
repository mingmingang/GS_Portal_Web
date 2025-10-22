(function ($) {
    'use strict';

    var PembatalanCutiPage = {
        config: {
            cutiId: '',
            userNpk: '',
            backUrl: ''
        },

        elements: {
            mainContent: null,
            form: null,
            requestNo: null,
            leaveType: null,
            startDate: null,
            endDate: null,
            cancellationDatesTbody: null,
            cancellationReason: null
        },

        init: function (config) {
            this.config = $.extend(this.config, config);
            this.cacheElements();
            this.bindEvents();
            this.loadCancellationData(this.config.cutiId);
        },

        cacheElements: function () {
            this.elements = {
                mainContent: $('.main-content'),
                form: $('#cancellation-form'),
                requestNo: $('#request_no'),
                leaveType: $('#leave_type'),
                startDate: $('#start_date'),
                endDate: $('#end_date'),
                cancellationDatesTbody: $('#cancellation-dates-tbody'),
                cancellationReason: $('#cancellation_reason')
            };
        },

        bindEvents: function () {
            var self = this;
            this.elements.form.on('submit', function (e) {
                e.preventDefault();
                self.handleSubmit();
            });
        },

        loadCancellationData: function (id) {
            var self = this;
            if (!id) {
                self.elements.mainContent.html('<p style="text-align:center;">ID Pengajuan Cuti tidak valid.</p>');
                return;
            }

            const url = `/api/CutiApi/detailCuti/${id}`;
            $.ajax({
                url: url,
                type: 'GET',
                success: function (response) {
                    if (response && response.data && response.data.length > 0) {
                        const cuti = response.data[0];
                        self.populateForm(cuti);
                        self.generateCancellationDates(cuti.startdate, cuti.enddate);
                    } else {
                        self.elements.mainContent.html('<p style="text-align:center;">Detail cuti tidak ditemukan.</p>');
                    }
                },
                error: function () {
                    self.elements.mainContent.html('<p style="text-align:center;">Gagal memuat data cuti.</p>');
                }
            });
        },

        populateForm: function (data) {
            const options = { year: 'numeric', month: 'long', day: 'numeric' };
            this.elements.requestNo.val(data.request_no);
            this.elements.leaveType.val(data.leave_code); // Ganti dengan nama tipe cuti jika ada di API
            this.elements.startDate.val(new Date(data.startdate).toLocaleDateString('id-ID', options));
            this.elements.endDate.val(new Date(data.enddate).toLocaleDateString('id-ID', options));
        },

        generateCancellationDates: function (startDateStr, endDateStr) {
            const $tbody = this.elements.cancellationDatesTbody;
            $tbody.empty();

            const startParts = startDateStr.split('T')[0].split('-').map(Number);
            const endParts = endDateStr.split('T')[0].split('-').map(Number);

            const startDate = new Date(Date.UTC(startParts[0], startParts[1] - 1, startParts[2]));
            const endDate = new Date(Date.UTC(endParts[0], endParts[1] - 1, endParts[2]));

            if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
                $tbody.html('<tr><td colspan="2" style="text-align:center; color: red;">Format tanggal tidak valid.</td></tr>');
                return;
            }

            const displayOptions = { year: 'numeric', month: 'long', day: 'numeric', timeZone: 'UTC' };
            let currentDate = new Date(startDate);

            while (currentDate <= endDate) {
                const formattedDate = currentDate.toLocaleDateString('id-ID', displayOptions);
                const yyyy = currentDate.getUTCFullYear();
                const mm = String(currentDate.getUTCMonth() + 1).padStart(2, '0');
                const dd = String(currentDate.getUTCDate()).padStart(2, '0');
                const valueDate = `${yyyy}-${mm}-${dd}`;

                const dateRowHtml = `
                <tr>
                    <td>
                        <label for="date-${valueDate}" style="cursor: pointer;">${formattedDate}</label>
                    </td>
                    <td style="text-align: right;">
                        <input class="form-check-input" type="checkbox" value="${valueDate}" id="date-${valueDate}">
                    </td>
                </tr>`;
                $tbody.append(dateRowHtml);
                currentDate.setUTCDate(currentDate.getUTCDate() + 1);
            }
        },

        handleSubmit: function () {
            var self = this;
            const selectedDates = [];
            this.elements.cancellationDatesTbody.find('input[type="checkbox"]:checked').each(function () {
                selectedDates.push($(this).val());
            });

            const reason = this.elements.cancellationReason.val().trim();

            if (selectedDates.length === 0) {
                Swal.fire('Peringatan', 'Silakan pilih minimal satu tanggal untuk dibatalkan.', 'warning');
                return;
            }
            if (reason === '') {
                Swal.fire('Peringatan', 'Alasan pembatalan tidak boleh kosong.', 'warning');
                return;
            }

            Swal.fire({
                title: 'Konfirmasi Pembatalan',
                text: `Anda akan membatalkan cuti pada ${selectedDates.length} hari yang dipilih. Lanjutkan?`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Ya, Lanjutkan!',
                cancelButtonText: 'Batal'
            }).then((result) => {
                if (result.isConfirmed) {
                    const submissionData = {
                        request_no: self.elements.requestNo.val(),
                        cancellation_reason: reason,
                        cancelled_dates: selectedDates,
                        cancelled_by: self.config.userNpk
                    };

                    $.ajax({
                        url: '/api/CutiApi/cancelCuti',
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify(submissionData),
                        success: function (response) {
                            Swal.fire('Berhasil!', 'Pengajuan pembatalan cuti Anda telah berhasil dikirim.', 'success')
                                .then(() => {
                                    window.location.href = self.config.backUrl;
                                });
                        },
                        error: function (xhr) {
                            Swal.fire('Gagal!', 'Terjadi kesalahan saat mengirim pengajuan. ' + (xhr.responseJSON?.message || ''), 'error');
                        }
                    });
                }
            });
        }
    };

    window.PembatalanCutiPage = PembatalanCutiPage;

})(jQuery);