// Gunakan IIFE (Immediately Invoked Function Expression) untuk membuat scope privat
// dan menerima jQuery ($) sebagai argumen.
(function ($) {
    'use strict';

    // Buat objek untuk menampung semua logika halaman
    var TambahCutiPage = {
        // Properti untuk menyimpan data dari server
        config: {
            userNpk: '',
            requestForName: '',
            redirectUrl: ''
        },

        // Cache untuk elemen DOM yang sering digunakan
        elements: {},

        // Variabel state
        allLeaveTypes: [],
        stream: null,
        capturedBlob: null,

        // Fungsi inisialisasi, dipanggil dari view
        init: function (config) {
            // Gabungkan konfigurasi default dengan yang dikirim dari view
            this.config = $.extend(this.config, config);
            this.cacheElements();
            this.bindEvents();
            this.loadLeaveTypes();
            this.generateNewCutiId();

            this.elements.permohonanUntuk.val(this.config.requestForName);
        },

        // Fungsi untuk menyimpan referensi elemen DOM
        cacheElements: function () {
            this.elements = {
                cutiForm: $("#cutiForm"),
                cutiIdInput: $("#cuti_id"),
                permohonanUntuk: $("#permohonan_untuk"),
                tipeCuti: $("#tipe_cuti"),
                cutiKhususSection: $("#cutiKhususSection"),
                subTipeCuti: $("#sub_tipe_cuti"),
                lampiranSection: $("#lampiranSection"),
                lampiranFile: $("#lampiranFile"),
                lampiranRequired: $("#lampiranRequired"),
                tanggalMulai: $("#TanggalMulai"),
                tanggalSelesai: $("#TanggalSelesai"),
                durasiInput: $("#Durasi"),
                keterangan: $("#Keterangan"),
                fileNameDisplay: $("#fileNameDisplay"),
                btnSubmit: $("#btnSubmit"),
                btnSaveDraft: $("#btnSaveDraft"),
                // Elemen Kamera
                cameraModal: $("#cameraModal"),
                videoPreview: $("#videoPreview"),
                imagePreview: $("#imagePreview"),
                canvasCapture: $("#canvasCapture")[0],
                cameraFooter: $("#cameraFooter"),
                btnOpenCamera: $("#btnOpenCamera"),
                btnCloseCamera: $("#closeCamera"),
                btnBrowseFile: $("#btnBrowseFile")
            };
        },

        // Fungsi untuk mendaftarkan semua event listener
        bindEvents: function () {
            var self = this; // Simpan referensi 'this'

            self.elements.tipeCuti.on("change", self.handleTipeCutiChange.bind(self));
            self.elements.tanggalMulai.on('change', self.handleTanggalChange.bind(self));
            self.elements.tanggalSelesai.on('change', self.handleTanggalChange.bind(self));
            self.elements.lampiranFile.on("change", self.handleFileChange.bind(self));
            self.elements.btnSubmit.on("click", function () { self.handleSubmitForm(false); });
            self.elements.btnSaveDraft.on("click", function () { self.handleSubmitForm(true); });

            // Listener Tombol Lampiran & Kamera
            self.elements.btnBrowseFile.on("click", function () {
                self.elements.lampiranFile.attr('accept', '.jpg,.jpeg,.png,.pdf,.zip').removeAttr('capture');
                self.elements.lampiranFile.click();
            });
            self.elements.btnOpenCamera.on("click", self.openCamera.bind(self));
            self.elements.btnCloseCamera.on("click", self.closeCamera.bind(self));
            $(document).on("click", "#btnCapture", self.captureImage.bind(self));
            $(document).on("click", "#btnRetake", self.initializeCamera.bind(self));
            $(document).on("click", "#btnUsePhoto", self.useCapturedPhoto.bind(self));
        },

        // --- FUNGSI-FUNGSI LOGIKA (sebagian besar sama seperti sebelumnya) ---

        // Fungsi Kamera
        showCaptureButton: function () {
            this.elements.cameraFooter.html(`
                <button type="button" id="btnCapture" class="btn-action btn-submit2">
                    <i class="fa fa-camera"></i> Ambil Foto
                </button>
            `);
        },
        showConfirmButtons: function () {
            this.elements.cameraFooter.html(`
                <button type="button" id="btnRetake" class="btn-action btn-browse">Ambil Ulang</button>
                <button type="button" id="btnUsePhoto" class="btn-action btn-submit">Gunakan Foto Ini</button>
            `);
        },
        initializeCamera: async function () {
            try {
                if (this.stream) { this.stream.getTracks().forEach(track => track.stop()); }
                this.stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } });
                this.elements.videoPreview[0].srcObject = this.stream;
                this.elements.imagePreview.hide();
                this.elements.videoPreview.show();
                this.showCaptureButton();
            } catch (err) {
                console.error("Error accessing camera: ", err);
                this.elements.cameraModal.fadeOut();
                Swal.fire('Error', 'Tidak bisa mengakses kamera. Pastikan Anda memberikan izin dan menggunakan koneksi HTTPS.', 'error');
            }
        },
        openCamera: async function () {
            this.elements.cameraModal.fadeIn();
            await this.initializeCamera();
        },
        closeCamera: function () {
            if (this.stream) { this.stream.getTracks().forEach(track => track.stop()); }
            this.elements.cameraModal.fadeOut();
        },
        captureImage: function () {
            const context = this.elements.canvasCapture.getContext('2d');
            this.elements.canvasCapture.width = this.elements.videoPreview[0].videoWidth;
            this.elements.canvasCapture.height = this.elements.videoPreview[0].videoHeight;
            context.drawImage(this.elements.videoPreview[0], 0, 0, this.elements.canvasCapture.width, this.elements.canvasCapture.height);
            this.stream.getTracks().forEach(track => track.stop());
            var self = this;
            this.elements.canvasCapture.toBlob(function (blob) {
                self.capturedBlob = blob;
                const imageUrl = URL.createObjectURL(blob);
                self.elements.videoPreview.hide();
                self.elements.imagePreview.attr('src', imageUrl).show();
                self.showConfirmButtons();
            }, 'image/jpeg', 0.9);
        },
        useCapturedPhoto: function () {
            if (!this.capturedBlob) return;
            const capturedFile = new File([this.capturedBlob], "capture.jpg", { type: "image/jpeg" });
            const dataTransfer = new DataTransfer();
            dataTransfer.items.add(capturedFile);
            this.elements.lampiranFile[0].files = dataTransfer.files;
            this.elements.lampiranFile.trigger('change');
            this.closeCamera();
        },

        // Fungsi Form
        generateNewCutiId: function () { this.elements.cutiIdInput.val("LVRYYYYMMDDXXXXX"); },

        loadLeaveTypes: function () {
            var self = this;
            $.ajax({
                url: '/api/CutiApi/listTipeCuti',
                type: 'GET',
                data: { company_id: '13559' }, 
                success: function (response) {
                    if (response && response.data) {
                        self.allLeaveTypes = response.data;
                    }
                },
                error: function () {
                    console.error("Gagal memuat jenis cuti.");
                }
            });
        },

        handleTipeCutiChange: function () {
            const selectedType = this.elements.tipeCuti.val();
            this.elements.cutiKhususSection.slideUp();
            this.elements.subTipeCuti.prop('required', false).val('');
            this.elements.lampiranRequired.hide();
            this.elements.lampiranFile.prop('required', false);
            if (selectedType === "Cuti Khusus") {
                this.elements.subTipeCuti.empty().append('<option value="">-- Pilih Jenis Cuti Khusus --</option>');
                const cutiKhususOptions = this.allLeaveTypes.filter(tipe => tipe.leave_code !== 'CP' && tipe.leave_code !== 'CB');
                cutiKhususOptions.forEach(item => {
                    this.elements.subTipeCuti.append(`<option value="${item.leave_code}">${item.leavename_id}</option>`);
                });
                this.elements.cutiKhususSection.slideDown();
                this.elements.subTipeCuti.prop('required', true);
                this.elements.lampiranRequired.show();
                this.elements.lampiranFile.prop('required', true);
                this.elements.lampiranSection.slideDown();
            } else if (selectedType === "Cuti Pribadi" || selectedType === "Cuti Besar") {
                this.elements.lampiranSection.slideDown();
            } else {
                this.elements.lampiranSection.slideUp();
            }
        },

        handleTanggalChange: function () {
            const startDateVal = this.elements.tanggalMulai.val();
            if (startDateVal) {
                this.elements.tanggalSelesai.attr('min', startDateVal);
                if (this.elements.tanggalSelesai.val() && this.elements.tanggalSelesai.val() < startDateVal) {
                    this.elements.tanggalSelesai.val('');
                }
            }
            this.hitungDurasi();
        },

        hitungDurasi: function () {
            const tglAwal = this.elements.tanggalMulai.val();
            const tglAkhir = this.elements.tanggalSelesai.val();
            if (tglAwal && tglAkhir) {
                const startDate = new Date(tglAwal);
                const endDate = new Date(tglAkhir);
                if (endDate < startDate) { this.elements.durasiInput.val(''); return; }
                const diffTime = endDate.getTime() - startDate.getTime();
                const diffDays = Math.round(diffTime / (1000 * 3600 * 24)) + 1;
                this.elements.durasiInput.val(diffDays);
            } else {
                this.elements.durasiInput.val('');
            }
        },

        handleFileChange: function (e) {
            const file = e.target.files[0];
            if (!file) {
                this.elements.fileNameDisplay.text("");
                return;
            }
            this.elements.fileNameDisplay.text(file.name);
        },

        handleSubmitForm: function (isDraft) {
            var self = this;
            if (!isDraft && !this.elements.cutiForm[0].checkValidity()) {
                this.elements.cutiForm[0].reportValidity();
                return;
            }

            const confirmationData = {
                title: isDraft ? 'Simpan sebagai Draft?' : 'Kirim Pengajuan Cuti?',
                text: isDraft ? 'Data pengajuan cuti ini akan disimpan sebagai draft.' : 'Pastikan semua data sudah benar sebelum dikirim.',
                icon: 'question',
                confirmButtonText: isDraft ? 'Ya, Simpan!' : 'Ya, Kirim!',
                cancelButtonText: 'Batal'
            };

            Swal.fire({
                ...confirmationData,
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33'
            }).then((result) => {
                if (result.isConfirmed) {
                    const $clickedButton = isDraft ? self.elements.btnSaveDraft : self.elements.btnSubmit;
                    const originalHtml = $clickedButton.html();
                    $clickedButton.prop("disabled", true).html('<i class="fas fa-spinner fa-spin"></i> Processing...');

                    const formData = new FormData();
                    let finalLeaveCode = self.elements.tipeCuti.val();

                    if (finalLeaveCode === "Cuti Khusus") { finalLeaveCode = self.elements.subTipeCuti.val(); }
                    else if (finalLeaveCode === "Cuti Pribadi") { finalLeaveCode = "CP"; }
                    else if (finalLeaveCode === "Cuti Besar") { finalLeaveCode = "CB"; }

                    formData.append('status', isDraft ? 'Draft' : 'Unverified');
                    formData.append('requestedby', self.config.userNpk);
                    formData.append('requestfor', self.config.userNpk);
                    formData.append('leave_code', finalLeaveCode);
                    formData.append('leave_startdate', self.elements.tanggalMulai.val());
                    formData.append('leave_enddate', self.elements.tanggalSelesai.val());
                    formData.append('totaldays', self.elements.durasiInput.val());
                    formData.append('remark', self.elements.keterangan.val());

                    if (self.elements.lampiranFile[0].files.length > 0) {
                        formData.append('lampiranFile', self.elements.lampiranFile[0].files[0]);
                    }

                    $.ajax({
                        url: '/api/CutiApi/createCuti', type: 'POST', data: formData,
                        processData: false, contentType: false,
                        success: function (response) {
                            let title = isDraft ? 'Berhasil Disimpan!' : 'Berhasil Dikirim!';
                            let message = isDraft ? 'Pengajuan cuti Anda berhasil disimpan sebagai draft.' : 'Pengajuan cuti Anda berhasil disubmit.';
                            if (response && response.data && response.data.length > 0 && response.data[0].request_no) {
                                message += `<br>No. Pengajuan: <strong>${response.data[0].request_no}</strong>`;
                            }
                            Swal.fire({ icon: 'success', title: title, html: message, confirmButtonText: 'OK' })
                                .then(() => { window.location.href = self.config.redirectUrl; });
                        },
                        error: function (xhr) {
                            const errorMsg = xhr.responseJSON ? xhr.responseJSON.message : 'Terjadi kesalahan.';
                            Swal.fire({ icon: 'error', title: 'Gagal!', text: errorMsg });
                        },
                        complete: function () {
                            $clickedButton.prop("disabled", false).html(originalHtml);
                        }
                    });
                }
            });
        }
    };

    // Saat DOM siap, panggil fungsi init dari view
    // Kita akan memanggilnya dari file .cshtml setelah mendefinisikan config
    window.TambahCutiPage = TambahCutiPage;

})(jQuery);