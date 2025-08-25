using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Template_DevExpress_By_MFM.Models;
using Template_DevExpress_By_MFM.Utils;

namespace Template_DevExpress_By_MFM.Controllers
{
    public class ReimbursementApiController : ApiController
    {
        private GSDbContextGSTrack db;

        public ReimbursementApiController()
        {
            try
            {
                db = new GSDbContextGSTrack(@".", "DB_GSTRACK", "sa", "aangaang");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FATAL: Database connection failed. {ex.Message}");
                throw new Exception("Tidak dapat terhubung ke database.", ex);
            }
        }

        // GET: api/Reimbursement
        [SessionCheck]
        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, int? year, string statuses = null)
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Session tidak valid atau NPK kosong");
                }

                string sessionNpk = sessionLogin.npk;
                string userJabatan = sessionLogin.userjabatan;

                var karyawan = db.TlkpKaryawans.FirstOrDefault(k => k.kry_npk == sessionNpk);
                if (karyawan == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data karyawan tidak ditemukan.");
                }

                var pengaturanDb = db.t_pengaturan
                .Where(p => p.PgtrCode == "rmb" && p.PgtrStatus == 1)
                .ToDictionary(p => p.PgtrNama, p => p.PgtrDesc);

                // 3. Inisialisasi summary object
                var summary = new ReimbursementSummary();

                decimal.TryParse(pengaturanDb.GetValueOrDefault("rmb_plafon_rawat_inap", "0"), out decimal plafonInap);
                decimal.TryParse(pengaturanDb.GetValueOrDefault("rmb_plafon_maternity", "0"), out decimal plafonMaternity);
                summary.PlafonRawatInap = plafonInap;
                summary.PlafonMaternity = plafonMaternity;

                // Plafon KB
                decimal.TryParse(pengaturanDb.GetValueOrDefault("rmb_plafon_kb", "0"), out decimal plafonKb);
                summary.PlafonKb = plafonKb;
                var creationYear = karyawan.kry_created_date.Year;
                int selectedYearForKb = year ?? DateTime.Now.Year;
                var startCycleYear = creationYear + (int)Math.Floor((selectedYearForKb - creationYear) / 3.0) * 3;
                var endCycleYear = startCycleYear + 2;
                summary.NoteKb = $"Plafon per 3 tahun ({startCycleYear} - {endCycleYear})";

                // Plafon Rawat Jalan (logika kompleks)
                try
                {
                    var plafonKeluarga = JsonConvert.DeserializeObject<List<decimal>>(pengaturanDb.GetValueOrDefault("rmb_plafon_rawat_jalan_keluarga", "[0,0]"));
                    var plafonLajang = JsonConvert.DeserializeObject<List<decimal>>(pengaturanDb.GetValueOrDefault("rmb_plafon_rawat_jalan_lajang", "[0,0]"));

                    bool isKawin = karyawan.kry_status_kawin.Equals("Kawin", StringComparison.OrdinalIgnoreCase);

                    if (karyawan.kry_golongan >= 1 && karyawan.kry_golongan <= 3)
                    {
                        summary.PlafonRawatJalan = isKawin ? plafonKeluarga[0] : plafonLajang[0];
                    }
                    else if (karyawan.kry_golongan >= 4)
                    {
                        summary.PlafonRawatJalan = isKawin ? plafonKeluarga[1] : plafonLajang[1];
                    }
                    summary.NoteRawatJalan = $"{(isKawin ? "Keluarga" : "Lajang")} - Golongan {karyawan.kry_golongan}";
                }
                catch (Exception ex)
                {
                    // Handle jika format JSON di DB salah
                    summary.PlafonRawatJalan = 0;
                    summary.NoteRawatJalan = "Error saat mengambil data plafon";
                    // Opsional: Log ex.Message untuk debugging
                }

                int selectedYear = year ?? DateTime.Now.Year;
                var startDate = new DateTime(selectedYear, 1, 1);
                var endDate = startDate.AddYears(1);

                List<string> statusList = new List<string>();
                if (!string.IsNullOrEmpty(statuses))
                {
                    try
                    {
                        statusList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(statuses);
                    }
                    catch { /* biarkan kosong jika parsing gagal */ }
                }

                // Step 1: Query data mentah dengan semua kolom yang dibutuhkan
                var rawQuery = from rbm in db.gs_track_reimbursement
                               join kry in db.TlkpKaryawans
                                   on rbm.KryNpk equals kry.kry_npk into kry_join
                               from kry in kry_join.DefaultIfEmpty()

                               join dgs in db.gs_track_diagnosa
                                   on rbm.DgsId equals dgs.DgsId into dgs_join
                               from dgs in dgs_join.DefaultIfEmpty()

                               join org in db.gs_track_orang
                                   on rbm.OrgId equals org.OrgId into org_join
                               from org in org_join.DefaultIfEmpty()

                               where rbm.RmbTanggalMulai >= startDate
                                       && rbm.RmbTanggalMulai < endDate
                                       && rbm.KryNpk == sessionNpk
                               select new
                               {
                                   rbm.RmbId,
                                   rbm.RmbNoRequest,
                                   rbm.KryNpk,
                                   rbm.RmbTanggalMulai,
                                   rbm.RmbTanggalAkhir,
                                   rbm.RmbJenisClaim,
                                   rbm.OrgId,
                                   rbm.DgsId,
                                   rbm.RsId,
                                   rbm.RmbBiayaPeriksa,
                                   rbm.RmbStatus,
                                   rbm.RmbAlasanPembatalan,
                                   rbm.RmbAlasanPenolakan,
                                   rbm.RbmFilePathKwitansi,
                                   rbm.RbmFilePathHasilLab,
                                   rbm.RbmFilePathRincianObat,
                                   rbm.RbmFilePathResumeMedis,
                                   rbm.RmbCreatedBy,
                                   rbm.RmbCreatedDate,
                                   rbm.RmbModifBy,
                                   rbm.RmbModifDate,

                                   NamaKaryawan = kry != null ? kry.kry_nama_karyawan : "N/A",
                                   StatusKawin = kry != null ? kry.kry_status_kawin : "N/A",
                                   NamaDiagnosa = dgs != null ? dgs.DgsNama : "N/A",
                                   NamaPasien = org != null ? org.OrgNama : (kry != null ? kry.kry_nama_karyawan : "N/A"),
                                   HubunganPasien = org != null ? org.OrgHubungan : "Anda"
                               };

                if (statusList.Any())
                {
                    rawQuery = rawQuery.Where(r => statusList.Contains(r.RmbStatus));
                }

                var allDataForYear = rawQuery.ToList();

                var summaryCalculation = allDataForYear
                .GroupBy(item => item.RmbJenisClaim)
                .Select(g => new {
                    Tipe = g.Key,
                    Digunakan = g.Where(i => i.RmbStatus == "Disetujui").Sum(i => i.RmbBiayaPeriksa ?? 0),
                    Unrealize = g.Where(i => i.RmbStatus == "Menunggu Persetujuan" || i.RmbStatus == "Belum Diverifikasi").Sum(i => i.RmbBiayaPeriksa ?? 0)
                }).ToList();

                foreach (var calc in summaryCalculation)
                {
                    switch (calc.Tipe)
                    {
                        case "Rawat Jalan":
                            summary.RawatJalanDigunakan = calc.Digunakan;
                            summary.RawatJalanUnrealize = calc.Unrealize;
                            break;
                        case "Rawat Inap":
                            summary.RawatInapDigunakan = calc.Digunakan;
                            summary.RawatInapUnrealize = calc.Unrealize;
                            break;
                        case "Maternity":
                            summary.MaternityDigunakan = calc.Digunakan;
                            summary.MaternityUnrealize = calc.Unrealize;
                            break;
                        case "KB":
                            summary.KbDigunakan = calc.Digunakan;
                            summary.KbUnrealize = calc.Unrealize;
                            break;
                    }
                }

                var modelList = allDataForYear.Select(item => new ReimbursementModel
                {
                    RmbId = item.RmbId,
                    RmbNoRequest = item.RmbNoRequest,
                    KryNpk = item.KryNpk,
                    RmbTanggalMulai = item.RmbTanggalMulai,
                    RmbTanggalAkhir = item.RmbTanggalAkhir,
                    RmbJenisClaim = item.RmbJenisClaim,
                    OrgId = item.OrgId,
                    DgsId = item.DgsId,
                    RsId = item.RsId,
                    RmbBiayaPeriksa = item.RmbBiayaPeriksa,
                    RmbStatus = item.RmbStatus,
                    RmbAlasanPembatalan = item.RmbAlasanPembatalan,
                    RmbAlasanPenolakan = item.RmbAlasanPenolakan,
                    RbmFilePathResumeMedis = item.RbmFilePathResumeMedis,
                    RbmFilePathRincianObat = item.RbmFilePathRincianObat,
                    RbmFilePathHasilLab = item.RbmFilePathHasilLab,
                    RbmFilePathKwitansi = item.RbmFilePathKwitansi,
                    RmbCreatedBy = item.RmbCreatedBy,
                    RmbCreatedDate = item.RmbCreatedDate,
                    RmbModifBy = item.RmbModifBy,
                    RmbModifDate = item.RmbModifDate,
                    NamaKaryawan = item.NamaKaryawan,
                    StatusKawin = item.StatusKawin,
                    NamaDiagnosa = item.NamaDiagnosa,
                    NamaPasien = item.NamaPasien,
                    HubunganPasien = item.HubunganPasien,
                    durasi = GetDurasi(item.RmbTanggalMulai, item.RmbTanggalAkhir),
                    StatusSortOrder = GetStatusSortOrder(item.RmbStatus, userJabatan)
                });

                if (loadOptions.Sort == null || loadOptions.Sort.Length == 0)
                {
                    // Terapkan sorting default kita
                    loadOptions.Sort = new[] {
                        // Urutkan berdasarkan kolom 'StatusSortOrder' secara menaik (ascending)
                        new SortingInfo { Selector = nameof(ReimbursementModel.StatusSortOrder), Desc = false }, 
                        // Lalu urutkan berdasarkan tanggal dibuat secara menurun (descending)
                        new SortingInfo { Selector = nameof(ReimbursementModel.RmbCreatedDate), Desc = true }
                    };
                }

                var loadResultForGrid = DataSourceLoader.Load(modelList, loadOptions);

                var finalResult = new ReimbursementLoadResult
                {
                    data = loadResultForGrid.data,
                    totalCount = loadResultForGrid.totalCount,
                    summary = summary
                };

                //var finalResult = new
                //{
                //    data = loadResultForGrid.data,
                //    totalCount = loadResultForGrid.totalCount,
                //    summary = summary, // ini summary custom kamu
                //    groupCount = loadResultForGrid.groupCount
                //};

                return Request.CreateResponse(HttpStatusCode.OK, finalResult);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        private int GetStatusSortOrder(string status, string userJabatan)
        {
            if (userJabatan == "HC2")
            {
                // Urutan prioritas untuk HC
                switch (status)
                {
                    case "Belum Diverifikasi": return 1; // Prioritas utama untuk HC
                    case "Menunggu Persetujuan": return 2;
                    case "Disetujui": return 3;
                    case "Ditolak": return 4;
                    case "Dibatalkan": return 5;
                    default: return 99;
                }
            }
            else if (userJabatan == "Atasan")
            {
                // Urutan prioritas untuk Atasan
                switch (status)
                {
                    case "Menunggu Persetujuan": return 1; // Prioritas utama untuk Atasan
                    case "Belum Diverifikasi": return 2;
                    case "Disetujui": return 3;
                    case "Ditolak": return 4;
                    case "Dibatalkan": return 5;
                    default: return 99;
                }
            } else
            {
                // Urutan prioritas untuk Karyawan
                switch (status)
                {
                    case "Menunggu Persetujuan": return 1; // Prioritas utama untuk Karyawan
                    case "Belum Diverifikasi": return 2;
                    case "Disetujui": return 3;
                    case "Ditolak": return 4;
                    case "Dibatalkan": return 5;
                    default: return 99;
                }
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/GetAtasanAndHC2")]
        public HttpResponseMessage GetAtasanAndHC2(DataSourceLoadOptions loadOptions, int? year, string statuses = null)
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    // Validasi sesi tetap penting untuk keamanan
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Session tidak valid atau NPK kosong");
                }

                int selectedYear = year ?? DateTime.Now.Year;
                var startDate = new DateTime(selectedYear, 1, 1);
                var endDate = startDate.AddYears(1);

                string userJabatan = sessionLogin.userjabatan;

                List<string> statusList = new List<string>();
                if (!string.IsNullOrEmpty(statuses))
                {
                    try
                    {
                        statusList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(statuses);
                    }
                    catch { /* biarkan kosong jika parsing gagal */ }
                }

                var rawQuery = from rbm in db.gs_track_reimbursement
                               join kry in db.TlkpKaryawans
                                   on rbm.KryNpk equals kry.kry_npk into kry_join
                               from kry in kry_join.DefaultIfEmpty()

                               join dgs in db.gs_track_diagnosa
                                   on rbm.DgsId equals dgs.DgsId into dgs_join
                               from dgs in dgs_join.DefaultIfEmpty()

                               join org in db.gs_track_orang
                                   on rbm.OrgId equals org.OrgId into org_join
                               from org in org_join.DefaultIfEmpty()

                               where rbm.RmbCreatedDate >= startDate && rbm.RmbCreatedDate < endDate
                               select new
                               {
                                   rbm.RmbId,
                                   rbm.RmbNoRequest,
                                   rbm.KryNpk,
                                   rbm.RmbTanggalMulai,
                                   rbm.RmbTanggalAkhir,
                                   rbm.RmbJenisClaim,
                                   rbm.OrgId,
                                   rbm.DgsId,
                                   rbm.RsId,
                                   rbm.RmbBiayaPeriksa,
                                   rbm.RmbStatus,
                                   rbm.RmbAlasanPembatalan,
                                   rbm.RmbAlasanPenolakan,
                                   rbm.RmbCreatedBy,
                                   rbm.RmbCreatedDate,
                                   rbm.RmbModifBy,
                                   rbm.RmbModifDate,
                                   NamaKaryawan = kry != null ? kry.kry_nama_karyawan : "N/A",
                                   NamaDiagnosa = dgs != null ? dgs.DgsNama : "N/A",
                                   NamaPasien = org != null ? org.OrgNama : (kry != null ? kry.kry_nama_karyawan : "N/A"),
                               };

                if (statusList.Any())
                {
                    rawQuery = rawQuery.Where(r => statusList.Contains(r.RmbStatus));
                }

                var allDataForYear = rawQuery.ToList();

                //var sortedData = allDataForYear
                //            .OrderBy(item => GetStatusSortOrder(item.RmbStatus))
                //            .ThenByDescending(item => item.RmbCreatedDate) // Opsional: urutkan data baru di atas
                //            .ToList();

                var summary = new ReimbursementAtasanSummary
                {
                    CountDisetujui = allDataForYear.Count(r => r.RmbStatus == "Disetujui"),
                    CountDitolak = allDataForYear.Count(r => r.RmbStatus == "Ditolak"),
                    CountMenunggu = allDataForYear.Count(r => r.RmbStatus == "Menunggu Persetujuan"),
                    CountBelumVerifikasi = allDataForYear.Count(r => r.RmbStatus == "Belum Diverifikasi")
                };

                var modelList = allDataForYear.Select(item => new ReimbursementModel
                {
                    RmbId = item.RmbId,
                    RmbNoRequest = item.RmbNoRequest,
                    KryNpk = item.KryNpk,
                    RmbTanggalMulai = item.RmbTanggalMulai,
                    RmbTanggalAkhir = item.RmbTanggalAkhir,
                    RmbJenisClaim = item.RmbJenisClaim,
                    OrgId = item.OrgId,
                    DgsId = item.DgsId,
                    RsId = item.RsId,
                    RmbBiayaPeriksa = item.RmbBiayaPeriksa,
                    RmbStatus = item.RmbStatus,
                    RmbAlasanPembatalan = item.RmbAlasanPembatalan,
                    RmbAlasanPenolakan = item.RmbAlasanPenolakan,
                    RmbCreatedBy = item.RmbCreatedBy,
                    RmbCreatedDate = item.RmbCreatedDate,
                    RmbModifBy = item.RmbModifBy,
                    RmbModifDate = item.RmbModifDate,
                    NamaKaryawan = item.NamaKaryawan,
                    NamaDiagnosa = item.NamaDiagnosa,
                    NamaPasien = item.NamaPasien,
                    durasi = GetDurasi(item.RmbTanggalMulai, item.RmbTanggalAkhir),
                    StatusSortOrder = GetStatusSortOrder(item.RmbStatus, userJabatan)
                });

                if (loadOptions.Sort == null || loadOptions.Sort.Length == 0)
                {
                    loadOptions.Sort = new[] {
                        new SortingInfo { Selector = nameof(ReimbursementModel.StatusSortOrder), Desc = false }, 
                        new SortingInfo { Selector = nameof(ReimbursementModel.RmbCreatedDate), Desc = true }   
                    };
                }

                var loadResultForGrid = DataSourceLoader.Load(modelList, loadOptions);

                var finalResult = new ReimbursementAtasanLoadResult
                {
                    data = loadResultForGrid.data,
                    totalCount = loadResultForGrid.totalCount,
                    summary = summary
                };

                return Request.CreateResponse(HttpStatusCode.OK, finalResult);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan pada server: " + ex.Message);
            }
        }

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/GetInitialData")] // URL baru yang akan kita panggil
        public HttpResponseMessage GetInitialData()
        {
            try
            {
                // Query untuk mencari tanggal reimbursement paling awal
                var firstReimbursementDate = db.gs_track_reimbursement
                                               .OrderBy(r => r.RmbCreatedDate)
                                               .Select(r => (DateTime?)r.RmbCreatedDate) // Pilih sebagai nullable DateTime
                                               .FirstOrDefault();

                // Tentukan tahun. Jika database kosong, gunakan tahun sekarang.
                int earliestYear = firstReimbursementDate?.Year ?? DateTime.Now.Year;

                // Kembalikan hanya tahunnya dalam format JSON sederhana
                return Request.CreateResponse(HttpStatusCode.OK, new { earliestYear = earliestYear });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Gagal mengambil data inisial: " + ex.Message);
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

        [SessionCheck]
        [HttpGet]
        [Route("api/ReimbursementApi/GetDetail/{id}")]
        public HttpResponseMessage GetDetail(long id)
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Session tidak valid atau NPK kosong.");
                }
                string sessionNpk = sessionLogin.npk;

                // TAHAP 1: Ambil data dari database ke dalam objek anonim
                var rawData = (from rbm in db.gs_track_reimbursement
                               join kry in db.TlkpKaryawans
                                   on rbm.KryNpk equals kry.kry_npk into kry_join
                               from kry in kry_join.DefaultIfEmpty()

                               join dgs in db.gs_track_diagnosa
                                   on rbm.DgsId equals dgs.DgsId into dgs_join
                               from dgs in dgs_join.DefaultIfEmpty()

                               join org in db.gs_track_orang
                                   on rbm.OrgId equals org.OrgId into org_join
                               from org in org_join.DefaultIfEmpty()

                               join rs in db.gs_track_rumah_sakit
                                   on rbm.RsId equals rs.RsId into rs_join
                               from rs in rs_join.DefaultIfEmpty()

                               where rbm.RmbId == id
                               select new // <--- Ini adalah objek anonim
                               {
                                   // Ambil semua field yang dibutuhkan
                                   rbm.RmbId,
                                   rbm.RmbNoRequest,
                                   rbm.KryNpk,
                                   rbm.RmbTanggalMulai,
                                   rbm.RmbTanggalAkhir,
                                   rbm.RmbJenisClaim,
                                   rbm.OrgId,
                                   rbm.DgsId,
                                   rbm.RsId,
                                   rbm.RmbNamaDokter,
                                   rbm.RmbBiayaPeriksa,
                                   rbm.RmbJenisPembayaran,
                                   rbm.RmbStatus,
                                   rbm.RmbAlasanPembatalan,
                                   rbm.RmbAlasanPenolakan,
                                   rbm.RmbDiagnosaOther,
                                   rbm.RmbCreatedBy,
                                   rbm.RmbCreatedDate,

                                   NamaKaryawan = kry != null ? kry.kry_nama_karyawan : "N/A",
                                   NamaDiagnosa = dgs != null ? dgs.DgsNama : "N/A",
                                   NamaPasien = org != null ? org.OrgNama : (kry != null ? kry.kry_nama_karyawan : "Anda"),
                                   NamaRumahSakit = rs != null ? rs.RsNama : "N/A",
                                   HubunganPasien = org != null ? org.OrgHubungan : "Diri Sendiri",
                                   TipeRs = rs != null ? rs.RsTipe : "N/A"
                               }).FirstOrDefault();

                if (rawData == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, $"Reimbursement dengan ID {id} tidak ditemukan.");
                }

                // TAHAP 2: Setelah data ada di memori, mapping ke ReimbursementModel
                var reimbursementDetail = new ReimbursementModel
                {
                    RmbId = rawData.RmbId,
                    RmbNoRequest = rawData.RmbNoRequest,
                    KryNpk = rawData.KryNpk,
                    RmbTanggalMulai = rawData.RmbTanggalMulai,
                    RmbTanggalAkhir = rawData.RmbTanggalAkhir,
                    RmbJenisClaim = rawData.RmbJenisClaim,
                    OrgId = rawData.OrgId,
                    DgsId = rawData.DgsId,
                    RsId = rawData.RsId,
                    RmbNamaDokter = rawData.RmbNamaDokter,
                    RmbBiayaPeriksa = rawData.RmbBiayaPeriksa,
                    RmbJenisPembayaran = rawData.RmbJenisPembayaran,
                    RmbStatus = rawData.RmbStatus,
                    RmbAlasanPembatalan = rawData.RmbAlasanPembatalan,
                    RmbAlasanPenolakan = rawData.RmbAlasanPenolakan,
                    RmbDiagnosaOther = rawData.RmbDiagnosaOther,
                    RmbCreatedBy = rawData.RmbCreatedBy,
                    RmbCreatedDate = rawData.RmbCreatedDate,

                    // Properti NotMapped
                    NamaKaryawan = rawData.NamaKaryawan,
                    NamaDiagnosa = rawData.NamaDiagnosa,
                    NamaPasien = rawData.NamaPasien,
                    NamaRumahSakit = rawData.NamaRumahSakit,
                    HubunganPasien = rawData.HubunganPasien,
                    TipeRs = rawData.TipeRs
                };

                return Request.CreateResponse(HttpStatusCode.OK, reimbursementDetail);
            }
            catch (Exception ex)
            {
                // PENTING: Jangan lupa kembalikan ke versi production setelah debugging selesai
                // return Request.CreateResponse(HttpStatusCode.InternalServerError, "Terjadi kesalahan saat mengambil data detail.");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()); // Biarkan ini untuk sementara
            }
        }

        // KOREKSI 3: Pindahkan #region untuk mengelompokkan semua endpoint dropdown
        #region Dropdown Data Sources

        // POST: api/Reimbursement
        [HttpGet]
        [Route("api/reimbursement/GetPasien")]
        public HttpResponseMessage GetPasien()
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (string.IsNullOrEmpty(sessionLogin?.npk))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session tidak valid.");
                }

                var npk = sessionLogin.npk;

                // 1. Ambil data diri sendiri
                var self = new { OrgId = 0, OrgNama = sessionLogin.fullname, OrgHubungan = "Anda" };

                // 2. Ambil data keluarga dari tabel gs_track_orang
                var keluarga = db.gs_track_orang
                                 .Where(o => o.KryNpk == npk)
                                 .Select(o => new { o.OrgId, o.OrgNama, o.OrgHubungan })
                                 .ToList();

                // 3. Gabungkan
                var result = new List<object> { self };
                result.AddRange(keluarga);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/reimbursement/GetDiagnosa")]
        public HttpResponseMessage GetDiagnosa()
        {
            try
            {
                var result = db.gs_track_diagnosa
                               .Select(d => new { d.DgsId, d.DgsNama })
                               .OrderBy(d => d.DgsNama)
                               .ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("api/reimbursement/getRumahSakit")]
        public HttpResponseMessage GetRumahSakit(string tipe)
        {
            try
            {
                if (string.IsNullOrEmpty(tipe))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new List<object>());
                }

                // Pastikan nama tabel (db.gs_track_rumah_sakit) dan nama kolom (rs_tipe, dll)
                // sudah sesuai dengan definisi di DbContext dan class model Anda.
                var result = db.gs_track_rumah_sakit
                               .Where(rs => rs.RsTipe == tipe)
                               .Select(rs => new {
                                   RsId = rs.RsId,       // <-- PERBAIKAN: Beri nama properti 'RsId'
                                   RsNama = rs.RsNama    // <-- PERBAIKAN: Beri nama properti 'RsNama'
                               })
                               .OrderBy(rs => rs.RsNama) // <-- Urutkan berdasarkan properti baru
                               .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        [SessionCheck]
        [HttpGet]
        [Route("api/reimbursement/generateno")]
        public HttpResponseMessage GenerateNo()
        {
            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session expired, silakan login ulang.");
                }

                long newId = GenerateNoPengajuan(sessionLogin.npk);
                return Request.CreateResponse(HttpStatusCode.OK, new { RbmId = newId });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [SessionCheck]
        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "Permintaan harus berupa multipart/form-data.");
            }

            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Session expired, silakan login ulang.");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var formValuesContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "values");
                if (formValuesContent == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data form ('values') tidak ditemukan.");
                }

                var jsonValues = await formValuesContent.ReadAsStringAsync();
                var formData = JObject.Parse(jsonValues);
                var newEntity = new ReimbursementModel();

                // ============================
                // Mapping dengan parsing aman
                // ============================

                // RbmId (PK wajib diisi, pastikan unik)
                if (int.TryParse(formData["RmbId"]?.ToString(), out int RmbId))
                    newEntity.RmbId = RmbId;
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RmbId wajib diisi.");

                // KryNpk dari session
                if (string.IsNullOrWhiteSpace(sessionLogin.npk))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "NPK dari session kosong.");
                newEntity.KryNpk = sessionLogin.npk;

                // RbmTanggalMulai (wajib)
                if (DateTime.TryParseExact(formData["RmbTanggalMulai"]?.ToString(),
                                           "yyyy-MM-dd",
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out DateTime tglMulai))
                {
                    newEntity.RmbTanggalMulai = tglMulai;
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Format tanggal mulai tidak valid (yyyy-MM-dd).");
                }

                // RbmTipe (wajib, max 50 char)
                newEntity.RmbJenisClaim = formData["RmbJenisClaim"]?.ToString();
                if (string.IsNullOrWhiteSpace(newEntity.RmbJenisClaim))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RmbJenisClaim wajib diisi.");

                // OrgId (nullable, kalau 0 dianggap null)
                if (long.TryParse(formData["OrgId"]?.ToString(), out long orgId))
                    newEntity.OrgId = orgId == 0 ? (long?)null : orgId;

                // RsId (wajib)
                if (int.TryParse(formData["RsId"]?.ToString(), out int rsId))
                    newEntity.RsId = rsId;
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RsId wajib diisi.");

                // RbmCost (wajib, decimal 18,2)
                if (decimal.TryParse(formData["RmbBiayaPeriksa"]?.ToString(), out decimal rbmCost))
                    newEntity.RmbBiayaPeriksa = rbmCost;
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RmbBiayaPeriksa wajib diisi.");

                // DgsId (wajib, varchar(10))
                newEntity.DgsId = formData["DgsId"]?.ToString();
                if (string.IsNullOrWhiteSpace(newEntity.DgsId))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "DgsId wajib diisi.");

                // RbmDokter (wajib, varchar(255))
                newEntity.RmbNamaDokter = formData["RmbNamaDokter"]?.ToString();
                if (string.IsNullOrWhiteSpace(newEntity.RmbNamaDokter))
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "RmbNamaDokter wajib diisi.");

                // RbmTanggalSelesai (opsional)
                var tglSelesaiStr = formData["RmbTanggalAkhir"]?.ToString();
                if (!string.IsNullOrWhiteSpace(tglSelesaiStr) &&
                    DateTime.TryParseExact(tglSelesaiStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tglSelesai))
                {
                    newEntity.RmbTanggalAkhir = tglSelesai;
                }

                // Diagnosa Other hanya kalau DgsId == "1"
                if (newEntity.DgsId == "1")
                {
                    newEntity.RmbDiagnosaOther = formData["RmbDiagnosaOther"]?.ToString();
                }

                // ============================
                // Proses file upload
                // ============================
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".pdf" };
                var fileContents = provider.Contents.Where(c => c.Headers.ContentDisposition.FileName != null).ToList();

                foreach (var file in fileContents)
                {
                    var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                    if (string.IsNullOrEmpty(fileName)) continue;

                    var fieldName = file.Headers.ContentDisposition.Name.Trim('\"');
                    var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

                    if (!string.IsNullOrEmpty(extension) && !allowedExtensions.Contains(extension))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"Tipe file tidak diizinkan: '{fileName}'.");
                    }

                    var fileBytes = await file.ReadAsByteArrayAsync();
                    var base64String = Convert.ToBase64String(fileBytes);
                    var mimeType = MimeMapping.GetMimeMapping(fileName);
                    var dataUri = $"data:{mimeType};base64,{base64String}";

                    switch (fieldName)
                    {
                        case "kwitansiFile": newEntity.RbmFilePathKwitansi = dataUri; break;
                        case "rincianObatFile": newEntity.RbmFilePathRincianObat = dataUri; break;
                        case "hasilLabFile": newEntity.RbmFilePathHasilLab = dataUri; break;
                        case "resumeMedis": newEntity.RbmFilePathResumeMedis = dataUri; break;
                    }
                }

                // ============================
                // Default values
                // ============================
                newEntity.RmbStatus = "Menunggu Persetujuan";
                newEntity.RmbCreatedBy = sessionLogin.npk;
                newEntity.RmbCreatedDate = DateTime.Now;

                // ============================
                // Save ke DB
                // ============================
                db.gs_track_reimbursement.Add(newEntity);
                await db.SaveChangesAsync();

                var responseModel = new
                {
                    RbmId = newEntity.RmbId,
                    Message = "Data berhasil disimpan."
                };

                return Request.CreateResponse(HttpStatusCode.OK, responseModel);
            }
            catch (Exception ex)
            {
                // Ambil pesan inner exception supaya tahu error SQL dari EF
                var inner = ex.InnerException?.InnerException?.Message
                            ?? ex.InnerException?.Message
                            ?? "";

                return Request.CreateErrorResponse(
                    HttpStatusCode.InternalServerError,
                    $"Error: {ex.Message}\nInner: {inner}\nStack: {ex.StackTrace}"
                );
            }
        }

        private long GenerateNoPengajuan(string npk)
        {
            DateTime today = DateTime.Now;
            string prefix = $"{npk}{today:yyMMdd}";

            long minRange = long.Parse(prefix + "00");
            long maxRange = long.Parse(prefix + "99");

            var lastData = db.gs_track_reimbursement
                .Where(r => r.RmbId >= minRange && r.RmbId <= maxRange)
                .OrderByDescending(r => r.RmbId)
                .FirstOrDefault();

            int nextIncrement = 1;
            if (lastData != null)
            {
                string lastNo = lastData.RmbId.ToString();
                string lastIncrementStr = lastNo.Substring(lastNo.Length - 2);
                if (int.TryParse(lastIncrementStr, out int lastIncrement))
                {
                    nextIncrement = lastIncrement + 1;
                }
            }

            string newIdStr = $"{prefix}{nextIncrement:00}";
            return long.Parse(newIdStr);
        }

        [SessionCheck]
        [HttpPut] // Menggunakan HttpPut karena ini adalah update status
        [Route("api/ReimbursementApi/Cancel")]
        public HttpResponseMessage CancelReimbursement([FromBody] CancelRequestModel model)
        {
            if (model == null || model.RmbId <= 0 || string.IsNullOrWhiteSpace(model.AlasanPembatalan))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Data tidak valid. Pastikan ID dan Alasan Pembatalan terisi." });
            }

            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "Sesi tidak valid." });
                }
                string sessionNpk = sessionLogin.npk;

                // Cari data reimbursement di database
                var reimbursement = db.gs_track_reimbursement.FirstOrDefault(r => r.RmbId == model.RmbId);

                if (reimbursement == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = $"Pengajuan dengan ID {model.RmbId} tidak ditemukan." });
                }

                // PERIKSA KEPEMILIKAN: Pastikan yang membatalkan adalah pemilik pengajuan
                if (reimbursement.KryNpk != sessionNpk)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "Anda tidak memiliki hak untuk membatalkan pengajuan ini." });
                }

                var allowedStatuses = new List<string> { "Menunggu Persetujuan", "Belum Diverifikasi" };

                // PERIKSA STATUS: Hanya bisa dibatalkan jika statusnya masih "Menunggu" atau "Draft"
                // Sesuaikan dengan nama status di sistem Anda, contoh: "Menunggu Approval", "Submitted", dll.
                if (!allowedStatuses.Contains(reimbursement.RmbStatus))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = $"Pengajuan ini tidak dapat dibatalkan karena sudah diproses (Status: {reimbursement.RmbStatus})." });
                }

                // UPDATE DATA
                reimbursement.RmbStatus = "Dibatalkan"; // Sesuai permintaan
                reimbursement.RmbAlasanPembatalan = model.AlasanPembatalan;
                reimbursement.RmbModifBy = sessionNpk; // Sesuai permintaan
                reimbursement.RmbModifDate = DateTime.Now; // Sesuai permintaan

                db.gs_track_reimbursement.AddOrUpdate(reimbursement);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Pengajuan berhasil dibatalkan.", RbmId = reimbursement.RmbId });
            }
            catch (Exception ex)
            {
                // Logging error (opsional tapi sangat disarankan)
                // Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = "Terjadi kesalahan internal: " + ex.Message });
            }
        }

        [SessionCheck]
        [HttpPut]
        [Route("api/ReimbursementApi/reject")]
        public HttpResponseMessage RejectReimbursement([FromBody] RejectRequestModel model) // Menggunakan model baru
        {
            if (model == null || model.RmbId <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Data ID tidak valid." });
            }

            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                string sessionNpk = sessionLogin.npk;
                string userJabatan = sessionLogin.userjabatan;

                var reimbursement = db.gs_track_reimbursement.FirstOrDefault(r => r.RmbId == model.RmbId);
                if (reimbursement == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Pengajuan tidak ditemukan." });
                }

                // Cek apakah user boleh melakukan aksi pada status ini
                bool canTakeAction = (userJabatan == "Atasan" && reimbursement.RmbStatus == "Menunggu Persetujuan") ||
                                     (userJabatan == "HC2" && reimbursement.RmbStatus == "Belum Diverifikasi");

                if (!canTakeAction)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = $"Aksi tidak diizinkan untuk status saat ini ({reimbursement.RmbStatus})." });
                }

                reimbursement.RmbStatus = "Ditolak";
                reimbursement.RmbAlasanPenolakan = model.AlasanPenolakan;
                reimbursement.RmbModifBy = sessionNpk;
                reimbursement.RmbModifDate = DateTime.Now;

                db.Entry(reimbursement).State = EntityState.Modified;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Pengajuan berhasil ditolak." });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = "Terjadi kesalahan internal: " + ex.Message });
            }
        }

        [SessionCheck]
        [HttpPut]
        [Route("api/ReimbursementApi/approve")]
        public HttpResponseMessage ApproveReimbursement([FromBody] ApproveRequestModel model)
        {
            if (model == null || model.RmbId <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Data ID tidak valid." });
            }

            try
            {
                var sessionLogin = (SessionLogin)System.Web.HttpContext.Current.Session["SHealth"];
                if (sessionLogin == null || string.IsNullOrEmpty(sessionLogin.npk))
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "Sesi tidak valid." });
                }

                string sessionNpk = sessionLogin.npk;
                // Ambil JABATAN dari SESI, bukan dari KLIEN
                string userJabatan = sessionLogin.userjabatan;

                var reimbursement = db.gs_track_reimbursement.FirstOrDefault(r => r.RmbId == model.RmbId);
                if (reimbursement == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = $"Pengajuan dengan ID {model.RmbId} tidak ditemukan." });
                }

                // Terapkan logika workflow
                if (userJabatan == "Atasan")
                {
                    if (reimbursement.RmbStatus == "Menunggu Persetujuan")
                    {
                        reimbursement.RmbStatus = "Belum Diverifikasi";
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Hanya dapat menyetujui pengajuan dengan status 'Menunggu Persetujuan'." });
                    }
                }
                else if (userJabatan == "HC2")
                {
                    if (reimbursement.RmbStatus == "Belum Diverifikasi")
                    {
                        reimbursement.RmbStatus = "Disetujui";
                        reimbursement.RmbBiayaDiganti = reimbursement.RmbBiayaPeriksa;
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Hanya dapat menyetujui pengajuan dengan status 'Belum Diverifikasi'." });
                    }
                }
                else
                {
                    // Jika jabatan tidak sesuai, tolak aksi
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "Anda tidak memiliki hak untuk melakukan aksi ini." });
                }

                reimbursement.RmbModifBy = sessionNpk;
                reimbursement.RmbModifDate = DateTime.Now;

                db.Entry(reimbursement).State = EntityState.Modified;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, new { Message = "Pengajuan berhasil disetujui.", Data = reimbursement });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = "Terjadi kesalahan internal: " + ex.Message });
            }
        }
    }
}

public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
    }
}