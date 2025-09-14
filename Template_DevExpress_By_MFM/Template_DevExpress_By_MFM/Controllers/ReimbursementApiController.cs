using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using ImageMagick; // Tambahkan using untuk Magick.NET
using Newtonsoft.Json;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;
using static Dapper.SqlMapper;

namespace Template_DevExpress_By_MFM.Controllers
{
    // Pastikan attribute SessionCheck ada dan berfungsi dengan benar
    // [SessionCheck] 
    public class ReimbursementApiController : ApiController
    {
        //private GSDbContextGSTrack dbGstrack;
        private GSDbContextGSMedcare dbMedcare;
        // Path utama untuk menyimpan file. Sesuai permintaan.
        private const string MainUploadPath = @"D:\Publish\Uploads\Reimbursements";

        public ReimbursementApiController()
        {
            try
            {
                // Inisialisasi DbContext sesuai permintaan
                dbMedcare = new GSDbContextGSMedcare(@".", "DB_GSMEDCARE", "azet", "123"); // Asumsi koneksi string ada di Web.config
                //dbGstrack = new GSDbContextGSTrack(@".", "DB_GSTRACK", "azet", "123"); // Asumsi koneksi string ada di Web.config
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"FATAL: Database connection failed. {ex.Message}");
                // Lemparkan exception agar bisa ditangani di level lebih tinggi
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Tidak dapat terhubung ke database."));
            }
        }

        // GET: api/ReimbursementApi
        // File: ReimbursementApiController.cs
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, int? year, string statuses = null)
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Session tidak valid.");
                }
                string sessionNpk = sessionLogin.npk;
                //var karyawan = dbGstrack.TlkpEmp.FirstOrDefault(k => k.EmpNpk == sessionNpk);
                var karyawan = JsonDataHelper.GetKaryawanByNpk(sessionNpk);
                if (karyawan == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data karyawan tidak ditemukan.");
                }
                int selectedYear = year ?? DateTime.Now.Year;


                #region Detailed Summary Calculation

                var allReimbursementsForYear = dbMedcare.ReimbursementModels
                    .Where(r => r.RmbNpk == sessionNpk && r.RmbTanggalMulai.Year == selectedYear)
                    .ToList();

                var pengaturanDb = dbMedcare.PengaturanModels
                    .Where(p => p.PgtrCode == "rmb" && p.PgtrStatus == 1)
                    .ToDictionary(p => p.PgtrNama, p => p.PgtrDesc);

                Func<string, ReimbursementCardSummary> CalculateSummaryForClaimType = (claimType) =>
                {
                    var summaryCard = new ReimbursementCardSummary();
                    bool isKawin = (karyawan.status_kawin ?? "").Equals("Kawin", StringComparison.OrdinalIgnoreCase);
                    int golongan = karyawan?.golongan ?? 0;

                    switch (claimType)
                    {
                        case "Rawat Jalan":
                            var plafonKeluarga = JsonConvert.DeserializeObject<List<decimal>>(pengaturanDb.GetValueOrDefault("rmb_plafon_rawat_jalan_keluarga", "[0,0]"));
                            var plafonLajang = JsonConvert.DeserializeObject<List<decimal>>(pengaturanDb.GetValueOrDefault("rmb_plafon_rawat_jalan_lajang", "[0,0]"));
                            if (golongan >= 1 && golongan <= 3)
                                summaryCard.Plafon = isKawin ? plafonKeluarga[0] : plafonLajang[0];
                            else if (golongan >= 4)
                                summaryCard.Plafon = isKawin ? plafonKeluarga[1] : plafonLajang[1];
                            summaryCard.Note = $"{(isKawin ? "Keluarga" : "Lajang")} - Gol. {golongan}";
                            break;

                        case "KB":
                            string plafonKbString = pengaturanDb.GetValueOrDefault("rmb_plafon_kb", "0");
                            decimal plafonKbValue = 0;

                            if (!decimal.TryParse(plafonKbString, out plafonKbValue))
                            {
                                try
                                {
                                    var plafonKbList = JsonConvert.DeserializeObject<List<decimal>>(plafonKbString);
                                    plafonKbValue = plafonKbList?.FirstOrDefault() ?? 0;
                                }
                                catch
                                {
                                    plafonKbValue = 0;
                                }
                            }

                            summaryCard.Plafon = plafonKbValue;
                            summaryCard.Note = "Karyawati atau Istri";
                            break;

                        case "Rawat Inap":
                        case "Maternity":
                        case "Kacamata":
                            summaryCard.Plafon = 0;
                            summaryCard.Note = "Sesuai hak golongan";
                            break;
                        default:
                            summaryCard.Plafon = 0;
                            summaryCard.Note = string.Empty;
                            break;
                    }

                    summaryCard.Digunakan = allReimbursementsForYear
                        .Where(r => r.RmbJenisClaim == claimType && r.RmbStatus == "Disetujui")
                        .Sum(r => r.RmbBiayaDiganti ?? 0);

                    summaryCard.Unrealize = allReimbursementsForYear
                        .Where(r => r.RmbJenisClaim == claimType && r.RmbStatus == "Belum Diproses")
                        .Sum(r => r.RmbBiayaPeriksa ?? 0);

                    summaryCard.Sisa = (summaryCard.Plafon > 0) ? (summaryCard.Plafon - summaryCard.Digunakan) : 0;

                    return summaryCard;
                };

                var detailedSummary = new DetailedReimbursementSummary
                {
                    RawatJalan = CalculateSummaryForClaimType("Rawat Jalan"),
                    RawatInap = CalculateSummaryForClaimType("Rawat Inap"),
                    Maternity = CalculateSummaryForClaimType("Maternity"),
                    KB = CalculateSummaryForClaimType("KB"),
                    Kacamata = CalculateSummaryForClaimType("Kacamata")
                };
                #endregion

                var startDate = new DateTime(selectedYear, 1, 1);
                var endDate = new DateTime(selectedYear + 1, 1, 1);
                IQueryable<ReimbursementModel> query = dbMedcare.ReimbursementModels
                    .Where(rbm => rbm.RmbNpk == sessionNpk &&
                                  rbm.RmbTanggalMulai >= startDate &&
                                  rbm.RmbTanggalMulai < endDate);
                List<string> statusList = null;
                if (!string.IsNullOrEmpty(statuses)) { try { statusList = JsonConvert.DeserializeObject<List<string>>(statuses); } catch { /* ignore */ } }
                if (statusList != null && statusList.Any()) { query = query.Where(rbm => statusList.Contains(rbm.RmbStatus)); }
                var reimbursementsFromDb = query.ToList();

                #region Enrich Data
                var allTanggungan = JsonDataHelper.GetAllTanggungan();
                var allPenyakit = JsonDataHelper.GetAllPenyakit();
                var allRumahSakit = JsonDataHelper.GetAllRumahSakit();
                var karyawanJsonInfo = JsonDataHelper.GetKaryawanByNpk(sessionNpk);

                foreach (var rbm in reimbursementsFromDb)
                {
                    rbm.NamaKaryawan = karyawanJsonInfo?.Full_Name ?? "N/A"; // Karyawan seharusnya selalu ada
                    rbm.StatusKawin = karyawan.status_kawin;

                    // [MODIFIKASI 1] Logika untuk Nama Rumah Sakit
                    if (string.IsNullOrWhiteSpace(rbm.RmbRumahSakit))
                    {
                        rbm.NamaRumahSakit = "-";
                        rbm.TipeRs = "-";
                    }
                    else
                    {
                        var rumahSakitJson = allRumahSakit.FirstOrDefault(rs => rs.DoctorHospitalCode == rbm.RmbRumahSakit);
                        rbm.NamaRumahSakit = rumahSakitJson?.DoctorHospitalName ?? "N/A"; // Gagal cari -> "N/A"
                        rbm.TipeRs = rumahSakitJson?.DoctorHospitalType ?? "N/A";
                    }

                    // [MODIFIKASI 2] Logika untuk Nama Diagnosa
                    if (string.IsNullOrWhiteSpace(rbm.RmbDiagnosa))
                    {
                        rbm.NamaDiagnosa = rbm.RmbDiagnosaOther ?? "-"; // Jika ada "other" gunakan itu, jika tidak baru "-"
                    }
                    else
                    {
                        var diagnosaJson = allPenyakit.FirstOrDefault(p => p.DiseaseCode == rbm.RmbDiagnosa);
                        // Jika diagnosa 'Other' (kode '1'), utamakan teks dari RmbDiagnosaOther
                        if (rbm.RmbDiagnosa == "1")
                        {
                            rbm.NamaDiagnosa = rbm.RmbDiagnosaOther ?? "N/A"; // Jika other diisi tapi teksnya kosong, anggap N/A
                        }
                        else
                        {
                            rbm.NamaDiagnosa = diagnosaJson?.DiseaseNameId ?? "N/A"; // Gagal cari -> "N/A"
                        }
                    }

                    // [MODIFIKASI 3] Logika untuk Nama Pasien & Hubungan
                    // Di sini, NULL atau kosong punya arti bisnis: "reimbursement untuk karyawan sendiri"
                    if (string.IsNullOrWhiteSpace(rbm.RmbNamaPasien) || rbm.RmbNamaPasien.Equals(rbm.RmbNpk))
                    {
                        rbm.NamaPasien = rbm.NamaKaryawan;
                        rbm.HubunganPasien = "Employee";
                    }
                    else
                    {
                        var pasienJson = allTanggungan.FirstOrDefault(t => t.TggNoTanggungan == rbm.RmbNamaPasien);
                        rbm.NamaPasien = pasienJson?.TggNamaTanggungan ?? "N/A"; // Gagal cari -> "N/A"
                        // PERBAIKAN 1: Menyesuaikan dengan nama field di JSON "tgg_hubungan"
                        rbm.HubunganPasien = pasienJson?.TggHubungan ?? "N/A";
                    }

                    rbm.durasi = GetDurasi(rbm.RmbTanggalMulai, rbm.RmbTanggalAkhir);
                    rbm.StatusSortOrder = GetStatusSortOrder(rbm.RmbStatus);
                }
                #endregion

                if (loadOptions.Sort == null || loadOptions.Sort.Length == 0)
                {
                    loadOptions.Sort = new[] {
                new SortingInfo { Selector = nameof(ReimbursementModel.StatusSortOrder), Desc = false },
                new SortingInfo { Selector = nameof(ReimbursementModel.RmbCreatedDate), Desc = true }
            };
                }
                var loadResultForGrid = DataSourceLoader.Load(reimbursementsFromDb, loadOptions);

                return Request.CreateResponse(HttpStatusCode.OK, new ReimbursementLoadResult
                {
                    data = loadResultForGrid.data,
                    totalCount = loadResultForGrid.totalCount,
                    summary = detailedSummary
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/GetDetail/{id}")]
        public HttpResponseMessage GetDetail(int id) // Parameter diubah ke int sesuai model RmbId
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Session tidak valid.");
                }

                // TAHAP 1: Ambil data reimbursement mentah dari GSMEDCARE berdasarkan ID
                var reimbursement = dbMedcare.ReimbursementModels.FirstOrDefault(r => r.RmbId == id);

                if (reimbursement == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, $"Reimbursement dengan ID {id} tidak ditemukan.");
                }

                // Pastikan pengguna hanya bisa melihat detail pengajuannya sendiri (Security check)
                if (reimbursement.RmbNpk != sessionLogin.npk)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Anda tidak memiliki hak untuk melihat detail pengajuan ini.");
                }

                // TAHAP 2: "Enrich" data dengan informasi dari JSON (sama seperti di method Get)

                // Ambil data dari Karyawan JSON
                var karyawanJsonInfo = JsonDataHelper.GetKaryawanByNpk(reimbursement.RmbNpk);
                reimbursement.NamaKaryawan = karyawanJsonInfo?.Full_Name ?? "N/A";

                // [MODIFIKASI 1] Logika untuk Nama Rumah Sakit
                if (string.IsNullOrWhiteSpace(reimbursement.RmbRumahSakit))
                {
                    reimbursement.NamaRumahSakit = "-";
                    reimbursement.TipeRs = "-";
                }
                else
                {
                    var rumahSakitJson = JsonDataHelper.GetAllRumahSakit()
                        .FirstOrDefault(rs => rs.DoctorHospitalCode == reimbursement.RmbRumahSakit);
                    reimbursement.NamaRumahSakit = rumahSakitJson?.DoctorHospitalName ?? "N/A";
                    reimbursement.TipeRs = rumahSakitJson?.DoctorHospitalType ?? "N/A";
                }

                // [MODIFIKASI 2] Logika untuk Nama Diagnosa
                if (string.IsNullOrWhiteSpace(reimbursement.RmbDiagnosa))
                {
                    reimbursement.NamaDiagnosa = reimbursement.RmbDiagnosaOther ?? "-";
                }
                else
                {
                    var diagnosaJson = JsonDataHelper.GetAllPenyakit()
                        .FirstOrDefault(p => p.DiseaseCode == reimbursement.RmbDiagnosa);
                    if (reimbursement.RmbDiagnosa == "1")
                    {
                        reimbursement.NamaDiagnosa = reimbursement.RmbDiagnosaOther ?? "N/A";
                    }
                    else
                    {
                        reimbursement.NamaDiagnosa = diagnosaJson?.DiseaseNameId ?? "N/A";
                    }
                }

                // [MODIFIKASI 3] Logika untuk Nama Pasien & Hubungan
                if (string.IsNullOrWhiteSpace(reimbursement.RmbNamaPasien) || reimbursement.RmbNamaPasien.Equals(reimbursement.RmbNpk))
                {
                    reimbursement.NamaPasien = reimbursement.NamaKaryawan;
                    reimbursement.HubunganPasien = "Employee";
                }
                else
                {
                    var pasienJson = JsonDataHelper.GetAllTanggungan()
                        .FirstOrDefault(t => t.TggNoTanggungan == reimbursement.RmbNamaPasien);
                    reimbursement.NamaPasien = pasienJson?.TggNamaTanggungan ?? "N/A";
                    // PERBAIKAN 1: Menyesuaikan dengan nama field di JSON "tgg_hubungan"
                    reimbursement.HubunganPasien = pasienJson?.TggHubungan ?? "N/A";
                }

                // Hitung durasi
                reimbursement.durasi = GetDurasi(reimbursement.RmbTanggalMulai, reimbursement.RmbTanggalAkhir);

                // TAHAP 3: Kirim data yang sudah lengkap
                return Request.CreateResponse(HttpStatusCode.OK, reimbursement);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // GET api/ReimbursementApi/GetInitialData
        [SessionCheck]
        [HttpGet, Route("api/ReimbursementApi/GetInitialData")]
        public HttpResponseMessage GetInitialData()
        {
            try
            {
                var firstDate = dbMedcare.ReimbursementModels
                    .OrderBy(r => r.RmbCreatedDate)
                    .Select(r => r.RmbCreatedDate)
                    .FirstOrDefault();

                int earliestYear = (firstDate.HasValue && firstDate.Value > DateTime.MinValue) ? firstDate.Value.Year : DateTime.Now.Year;
                return Request.CreateResponse(HttpStatusCode.OK, new { earliestYear });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Gagal mengambil data inisial: " + ex.Message);
            }
        }

        private int GetStatusSortOrder(string status)
        {
            switch (status)
            {
                // PERBAIKAN 2: Urutan ini sudah benar sesuai permintaan.
                case "Belum Diproses": return 1;
                case "Disetujui": return 2;
                case "Ditolak": return 3;
                case "Dibatalkan": return 4;
                default: return 99;
            }
        }

        private string GetDurasi(DateTime mulai, DateTime? selesai)
        {
            if (selesai.HasValue && selesai.Value.Date >= mulai.Date)
            {
                int days = (selesai.Value.Date - mulai.Date).Days + 1;
                return $"{days} hari";
            }
            return "1 hari";
        }

        #region Dropdown Data Sources
        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/GetPasien")]
        public HttpResponseMessage GetPasien()
        {
            var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];

            if (string.IsNullOrEmpty(sessionLogin?.npk))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid.");

            // 1. Ambil data karyawan yang sedang login
            var karyawan = JsonDataHelper.GetKaryawanByNpk(sessionLogin.npk);
            if (karyawan == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Data karyawan tidak ditemukan.");

            // 2. Buat list untuk menampung hasil
            var pasienList = new List<object>();

            // 3. TAMBAHKAN KARYAWAN SEBAGAI PILIHAN PERTAMA
            pasienList.Add(new
            {
                Value = karyawan.emp_no, // Gunakan NPK sebagai Value
                Text = $"{karyawan.Full_Name} (Anda)"
            });

            // 4. Ambil data tanggungan
            var tanggungan = JsonDataHelper.GetTanggunganByNpk(sessionLogin.npk)
                .Select(t => new {
                    Value = t.TggNoTanggungan,
                    Text = $"{t.TggNamaTanggungan} ({t.TggHubungan})"
                });

            // 5. Gabungkan data karyawan dengan data tanggungan
            pasienList.AddRange(tanggungan);

            return Request.CreateResponse(HttpStatusCode.OK, pasienList);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/GetDiagnosa")]
        public HttpResponseMessage GetDiagnosa()
        {
            // Mengambil data dari JSON sesuai dengan struktur DataPenyakit.json
            var result = JsonDataHelper.GetAllPenyakit()
                           .Select(d => new {
                               Value = d.DiseaseCode,
                               Text = d.DiseaseNameId
                           })
                           .OrderBy(d => d.Text)
                           .ToList();
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/GetRumahSakit")]
        public HttpResponseMessage GetRumahSakit() // 1. Parameter tipeRsName dihapus
        {
            try
            {
                // Panggil helper Anda untuk mendapatkan semua data rumah sakit
                var semuaRumahSakit = JsonDataHelper.GetAllRumahSakit();

                // 2. Klausa .Where(...) dihapus dari kueri
                //    Sekarang langsung mengambil semua data, mengubah formatnya, lalu mengurutkan
                var result = semuaRumahSakit
                               .Select(rs => new {
                                   Value = rs.DoctorHospitalCode,
                                   Text = rs.DoctorHospitalName
                               })
                               .OrderBy(rs => rs.Text) // Mengurutkan berdasarkan nama tetap ide yang bagus
                               .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                // Blok catch tetap dipertahankan untuk penanganan error yang baik
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Gagal memuat data Rumah Sakit dari JSON: " + ex.Message);
            }
        }
        #endregion

        [SessionCheck] // Pastikan session check aktif
        [HttpGet]
        [Route("api/ReimbursementApi/file/{id:int}/{fileKey}")]
        public HttpResponseMessage GetReimbursementFile(int id, string fileKey)
        {
            try
            {
                var sessionLogin = (SessionLogin)HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Sesi tidak valid atau telah berakhir.");

                // 1. Ambil data dari database
                var reimbursement = dbMedcare.ReimbursementModels.AsNoTracking().FirstOrDefault(r => r.RmbId == id);
                if (reimbursement == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data reimbursement tidak ditemukan.");

                // 2. [PENTING] Pengecekan Keamanan: Pastikan pengguna hanya bisa akses filenya sendiri
                if (reimbursement.RmbNpk != sessionLogin.npk)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Anda tidak memiliki hak akses untuk file ini.");

                // 3. Tentukan path file mana yang akan diambil berdasarkan fileKey
                // Penggunaan switch/case ini aman untuk mencegah path traversal attack
                string relativePath = null;
                switch (fileKey.ToLowerInvariant())
                {
                    case "kwitansi": relativePath = reimbursement.RmbFilePathKwitansi; break;
                    case "rincianobat": relativePath = reimbursement.RmbFilePathRincianObat; break;
                    case "hasillab": relativePath = reimbursement.RmbFilePathHasilLab; break;
                    case "resumemedis": relativePath = reimbursement.RmbFilePathResumeMedis; break;
                    default:
                        // Jika fileKey tidak dikenal, kembalikan error.
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Tipe file tidak valid.");
                }

                if (string.IsNullOrEmpty(relativePath))
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Path file tidak terdaftar untuk tipe yang diminta.");

                // 4. Gabungkan path utama dengan nama file dari database
                string fullPath = Path.Combine(MainUploadPath, relativePath);

                if (!File.Exists(fullPath))
                    return Request.CreateResponse(HttpStatusCode.NotFound, "File fisik tidak ditemukan di server. Path: " + fullPath);

                // 5. Baca file sebagai byte array dan kirimkan sebagai response
                var fileBytes = File.ReadAllBytes(fullPath);
                var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(fileBytes) };

                // Set content type (MIME type) secara dinamis
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(Path.GetFileName(fullPath)));

                // Set content disposition ke 'inline' agar browser mencoba menampilkannya, bukan langsung download
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileName = Path.GetFileName(fullPath)
                };

                return response;
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error GetReimbursementFile: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan saat mengambil file.", ex);
            }
        }

        [SessionCheck] // Session check disarankan untuk keamanan
        [AcceptVerbs("GET", "HEAD")]
        [Route("api/ReimbursementApi/pdfimage/{imageName}")]
        public HttpResponseMessage GetReimbursementPdfImage(string imageName)
        {
            try
            {
                // 1. [PENTING] Validasi input imageName untuk keamanan
                if (string.IsNullOrWhiteSpace(imageName) ||
                    imageName.Contains("..") || imageName.Contains("/") || imageName.Contains("\\") ||
                    !imageName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Nama file tidak valid.");
                }

                // 2. Rekonstruksi path berdasarkan nama file gambar
                // Contoh imageName: "RO-231026-5081756-001_MyKwitansi_a1b2_01.jpg"

                // -> fileNameWithoutExt: "RO-231026-5081756-001_MyKwitansi_a1b2_01"
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(imageName);

                int lastUnderscoreIndex = fileNameWithoutExt.LastIndexOf('_');
                // Pastikan ada underscore dan bukan di awal
                if (lastUnderscoreIndex <= 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Format nama file gambar tidak valid.");
                }

                // -> basePdfFileName: "RO-231026-5081756-001_MyKwitansi_a1b2"
                // Ini adalah nama file PDF asli (tanpa ekstensi) yang menjadi dasar pembuatan gambar.
                string basePdfFileName = fileNameWithoutExt.Substring(0, lastUnderscoreIndex);

                // -> subFolderName: "RO-231026-5081756-001_MyKwitansi_a1b2_IMG"
                // Sesuai dengan logika di method SaveFileAsync.
                string subFolderName = $"{basePdfFileName}_IMG";

                // 3. Gabungkan path utama, sub-folder, dan nama gambar untuk mendapatkan path lengkap
                string fullImagePath = Path.Combine(MainUploadPath, subFolderName, imageName);

                if (!File.Exists(fullImagePath))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Gambar '{imageName}' tidak ditemukan. Path: " + fullImagePath);
                }

                // 4. Baca file gambar dan kirimkan sebagai response
                var fileBytes = File.ReadAllBytes(fullImagePath);
                var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(fileBytes) };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                // Untuk gambar, tidak perlu Content-Disposition karena browser akan langsung menampilkannya.
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetReimbursementPdfImage: {ex.Message}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST api/ReimbursementApi
        [SessionCheck]
        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "Permintaan harus multipart/form-data.");
            }

            var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session expired.");

            try
            {
                var provider = await Request.Content.ReadAsMultipartAsync();

                var formValuesContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "values");
                if (formValuesContent == null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data form ('values') tidak ditemukan.");

                var jsonValues = await formValuesContent.ReadAsStringAsync();
                var newEntity = JsonConvert.DeserializeObject<ReimbursementModel>(jsonValues);

                // --- Generate No Request ---
                // PENTING: Generate nomor request SEKARANG, agar bisa dipakai untuk nama file
                newEntity.RmbNoRequest = GenerateNoPengajuan(sessionLogin.npk).ToString();

                // --- Proses File dengan NAMA BARU---
                var fileContents = provider.Contents.Where(c => c.Headers.ContentDisposition.FileName != null).ToList();
                foreach (var file in fileContents)
                {
                    string fieldName = file.Headers.ContentDisposition.Name.Trim('\"');
                    // Panggil SaveFileAsync dengan parameter fileType
                    string savedFilePath = await SaveFileAsync(file, newEntity.RmbNoRequest, fieldName);

                    switch (fieldName)
                    {
                        case "kwitansiFile": newEntity.RmbFilePathKwitansi = savedFilePath; break;
                        case "rincianObatFile": newEntity.RmbFilePathRincianObat = savedFilePath; break;
                        case "hasilLabFile": newEntity.RmbFilePathHasilLab = savedFilePath; break;
                        case "resumeMedis": newEntity.RmbFilePathResumeMedis = savedFilePath; break;
                    }
                }

                // =========================================================================
                // --- TAMBAHAN: Mengisi Data yang Hilang (Enrichment) ---
                // =========================================================================

                // 1. Ambil data pasien dari JSON berdasarkan RmbReimFor (ID Tanggungan)
                //var tanggunganPasien = JsonDataHelper.GetTanggunganByNpk(sessionLogin.npk)
                //                        .FirstOrDefault(t => t.TggNoTanggungan == newEntity.RmbNamaPasien);

                //if (tanggunganPasien != null)
                //{
                //    // Jika reimbursement untuk tanggungan
                //    newEntity.RmbNamaPasien = tanggunganPasien.TggNamaTanggungan;
                //    // PERBAIKAN 1: Menyesuaikan dengan nama field di JSON "tgg_hubungan"
                //    newEntity.RmbHubunganPasien = tanggunganPasien.TggHubungan;
                //}
                //else
                //{
                //    // Jika reimbursement untuk diri sendiri (karyawan)
                //    var karyawanInfo = JsonDataHelper.GetKaryawanByNpk(sessionLogin.npk);
                //    newEntity.RmbNamaPasien = karyawanInfo?.Full_Name;
                //    newEntity.RmbHubunganPasien = "Employee";
                //}

                // Ambil ID Pasien yang dikirim dari form. 
                // Ingat: 'newEntity.RmbNamaPasien' saat ini berisi TggNoTanggungan (ID).
                string idPasienDariForm = newEntity.RmbNamaPasien;

                // Cek jika ID yang dikirim adalah NPK karyawan sendiri
                if (idPasienDariForm == sessionLogin.npk)
                {
                    // Jika ya, ambil data karyawan
                    var karyawanInfo = JsonDataHelper.GetKaryawanByNpk(sessionLogin.npk);
                    newEntity.RmbNamaPasien = karyawanInfo?.emp_no; // ISI DENGAN NAMA LENGKAP
                    newEntity.RmbHubunganPasien = "Employee";
                }
                else
                {
                    // Jika bukan, cari di data tanggungan
                    var tanggunganPasien = JsonDataHelper.GetTanggunganByNpk(sessionLogin.npk)
                                            .FirstOrDefault(t => t.TggNoTanggungan == idPasienDariForm);
                    if (tanggunganPasien != null)
                    {
                        // Jika tanggungan ditemukan...
                        newEntity.RmbNamaPasien = tanggunganPasien.TggNoTanggungan; // ISI DENGAN NAMA TANGGUNGAN
                        newEntity.RmbHubunganPasien = tanggunganPasien.TggHubungan;
                    }
                    else
                    {
                        // Fallback jika ID tidak ditemukan sama sekali (seharusnya tidak terjadi)
                        newEntity.RmbNamaPasien = "Data Pasien Tidak Ditemukan";
                        newEntity.RmbHubunganPasien = "N/A";
                    }
                }

                // 2. Set RmbTanggalAkhir jika jenis claimnya Rawat Jalan atau KB
                if (newEntity.RmbJenisClaim == "Rawat Jalan" || newEntity.RmbJenisClaim == "KB" || newEntity.RmbJenisClaim == "Kacamata")
                {
                    newEntity.RmbTanggalAkhir = newEntity.RmbTanggalMulai;
                }

                // 3. Pastikan RmbDiagnosaOther tidak null jika diagnosa adalah '1'
                if (newEntity.RmbDiagnosa != "1")
                {
                    newEntity.RmbDiagnosaOther = null; // Kosongkan jika bukan 'Lainnya'
                }

                // --- Set Nilai Default & Wajib ---
                newEntity.RmbNpk = sessionLogin.npk;
                newEntity.RmbPlant = sessionLogin.userplant;
                newEntity.RmbReimFrom = "E";
                newEntity.RmbTipeRumahSakit = "Non Rayon";
                newEntity.RmbJenisPembayaran = "Transfer";
                // PERBAIKAN 2: Status awal langsung 'Belum Diproses' agar langsung di urutan teratas
                newEntity.RmbStatus = "Belum Diproses";
                newEntity.RmbCreatedBy = sessionLogin.npk;
                newEntity.RmbCreatedDate = DateTime.Now;

                // --- Simpan ke Database ---
                dbMedcare.ReimbursementModels.Add(newEntity);
                await dbMedcare.SaveChangesAsync();

                return Request.CreateResponse(HttpStatusCode.Created, new
                {
                    newEntity.RmbId,
                    newEntity.RmbNoRequest,
                    Message = "Data berhasil disimpan."
                });
            }
            catch (Exception ex)
            {
                // Berikan detail error yang lebih baik untuk debugging
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // PUT untuk pembatalan
        [SessionCheck]
        [HttpPut, Route("api/ReimbursementApi/Cancel")]
        public HttpResponseMessage CancelReimbursement([FromBody] CancelRequestModel model)
        {
            if (!ModelState.IsValid)
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);

            var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
            if (sessionLogin == null)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "Sesi tidak valid." });

            var reimbursement = dbMedcare.ReimbursementModels.Find(model.RmbId);
            if (reimbursement == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, $"ID {model.RmbId} tidak ditemukan.");

            if (reimbursement.RmbNpk != sessionLogin.npk)
                return Request.CreateResponse(HttpStatusCode.Forbidden, "Anda tidak berhak membatalkan pengajuan ini.");

            var allowedStatuses = new List<string> { "Belum Diproses" }; // Status yang boleh dibatalkan
            if (!allowedStatuses.Contains(reimbursement.RmbStatus))
                return Request.CreateResponse(HttpStatusCode.BadRequest, $"Pengajuan ini tidak dapat dibatalkan (Status: {reimbursement.RmbStatus}).");

            reimbursement.RmbStatus = "Dibatalkan";
            reimbursement.RmbAlasanPembatalan = model.AlasanPembatalan;
            reimbursement.RmbModifBy = sessionLogin.npk;
            reimbursement.RmbModifDate = DateTime.Now;

            dbMedcare.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK, "Pengajuan berhasil dibatalkan.");
        }

        #region Helper Methods for File and Request Number

        private string GenerateNoPengajuan(string npk)
        {
            DateTime today = DateTime.Now;

            // 1. Prefix dibuat sebagai string. 
            //    Jika npk adalah "003045", maka prefix akan menjadi "003045250914". Nol tetap ada.
            string prefix = $"{npk}{today:yyMMdd}";

            var lastRequest = dbMedcare.ReimbursementModels
                .Where(r => r.RmbNoRequest.StartsWith(prefix))
                .OrderByDescending(r => r.RmbNoRequest)
                .Select(r => r.RmbNoRequest)
                .FirstOrDefault();

            int nextIncrement = 1;

            if (lastRequest != null)
            {
                string lastIncrementStr = lastRequest.Substring(lastRequest.Length - 2);
                if (int.TryParse(lastIncrementStr, out int lastIncrement))
                {
                    nextIncrement = lastIncrement + 1;
                }
            }

            // 2. Hasil akhir dikembalikan sebagai string.
            //    Ini adalah gabungan dari "003045250914" dan "01" (misalnya).
            //    Hasilnya adalah string "00304525091401". Nol tidak akan pernah hilang.
            return $"{prefix}{nextIncrement:D2}";
        }

        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage GenerateNo()
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session expired, silakan login ulang.");
                }

                // 1. Ubah tipe variabel dari 'long' menjadi 'string' agar sesuai
                string newNoRequest = GenerateNoPengajuan(sessionLogin.npk);

                // 2. Ubah nama properti JSON dari 'RbmId' menjadi 'RmbNoRequest'
                //    agar tidak membingungkan dengan Primary Key (RmbId)
                return Request.CreateResponse(HttpStatusCode.OK, new { RmbNoRequest = newNoRequest });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        private string GetFriendlyFileName(string fieldName)
        {
            switch (fieldName.ToLowerInvariant())
            {
                case "kwitansifile": return "Kwitansi";
                case "rincianobatfile": return "RincianObat";
                case "hasillabfile": return "HasilLab";
                case "resumemedis": return "ResumeMedis";
                default: return "Lampiran";
            }
        }

        private async Task<string> SaveFileAsync(HttpContent fileContent, string requestNumber, string fileType)
        {
            string originalFileName = fileContent.Headers.ContentDisposition.FileName.Trim('\"');
            string extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            string friendlyName = GetFriendlyFileName(fileType);
            string uniqueFileName = $"{friendlyName}_{requestNumber}{extension}";

            string directoryPath = MainUploadPath;
            Directory.CreateDirectory(directoryPath);

            string fullPath = Path.Combine(directoryPath, uniqueFileName);
            var fileBytes = await fileContent.ReadAsByteArrayAsync();
            File.WriteAllBytes(fullPath, fileBytes);

            if (extension == ".pdf")
            {
                string pdfFileNameWithoutExt = Path.GetFileNameWithoutExtension(uniqueFileName);
                string imageSubFolderPath = Path.Combine(directoryPath, $"{pdfFileNameWithoutExt}_IMG");
                Directory.CreateDirectory(imageSubFolderPath);

                try
                {
                    var settings = new MagickReadSettings { Density = new Density(150, 150) };
                    using (var images = new MagickImageCollection())
                    {
                        images.Read(fullPath, settings);
                        int page = 1;
                        foreach (var image in images)
                        {
                            string outputImageName = $"{pdfFileNameWithoutExt}_{page:D2}.jpg";
                            string outputImagePath = Path.Combine(imageSubFolderPath, outputImageName);
                            image.Write(outputImagePath);
                            page++;
                        }
                    }
                }
                catch (Exception pdfEx)
                {
                    System.Diagnostics.Debug.WriteLine($"PDF Conversion Error for {uniqueFileName}: {pdfEx.Message}");
                }
            }

            return uniqueFileName;
        }
        #endregion
    }

    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }
}