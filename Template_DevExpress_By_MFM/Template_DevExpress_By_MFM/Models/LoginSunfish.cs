using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Template_DevExpress_By_MFM.Models
{
    // --- KOMPONEN DASAR & REUSABLE UNTUK SEMUA API SUNFISH ---

    /// <summary>
    /// Mewakili blok "meta" yang ada di setiap respons API Sunfish.
    /// Menggunakan [JsonProperty] agar cocok dengan nama properti di JSON (e.g., "code" bukan "Code").
    /// </summary>
    public class SunfishMetaInfo
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    /// <summary>
    /// Model dasar untuk setiap respons API yang memiliki blok "meta".
    /// Kelas-kelas respons lain akan mewarisi dari sini.
    /// </summary>
    public class SunfishApiResponse
    {
        [JsonProperty("meta")]
        public List<SunfishMetaInfo> Meta { get; set; }
    }


    // --- MODEL SPESIFIK UNTUK API `cek_login_sunfish` ---

    /// <summary>
    /// Mewakili objek data yang dikembalikan oleh API otentikasi `cek_login_sunfish`.
    /// </summary>
    public class SunfishAuthData
    {
        [JsonProperty("emp_id")]
        public string emp_id { get; set; }

        [JsonProperty("emp_no")]
        public string emp_no { get; set; } // Kunci untuk join ke API data detail

        [JsonProperty("company_id")]
        public int company_id { get; set; }

        [JsonProperty("full_name")]
        public string full_name { get; set; }

        [JsonProperty("work_location_code")]
        public string work_location_code { get; set; }

        [JsonProperty("phone")]
        public string phone { get; set; }

        [JsonProperty("photo")]
        public string photo { get; set; }

        [JsonProperty("pos_level")]
        public int pos_level { get; set; }

        [JsonProperty("created_date")]
        public DateTime created_date { get; set; }
        public string pos_name_en { get; set; }
        public string role_options { get; set; } // "Karyawan", "HC", "Atasan"
    }

    /// <summary>
    /// Mewakili keseluruhan respons JSON dari API `cek_login_sunfish`.
    /// Ini adalah model yang HILANG dari file Anda sebelumnya.
    /// </summary>
    public class SunfishAuthResponse : SunfishApiResponse
    {
        [JsonProperty("data")]
        public List<SunfishAuthData> Data { get; set; }
    }


    // --- MODEL SPESIFIK UNTUK API `getListEmp` ---

    /// <summary>
    /// Mewakili objek data karyawan yang lebih detail dari API `getListEmp`.
    /// </summary>
    public class SunfishEmployeeDetail
    {
        [JsonProperty("emp_id")]
        public string emp_id { get; set; }

        [JsonProperty("emp_no")]
        public string emp_no { get; set; }

        [JsonProperty("full_name")]
        public string full_name { get; set; }

        [JsonProperty("worklocation_code")]
        public string WorkLocationCode { get; set; }

        [JsonProperty("position")]
        public string position { get; set; }

        [JsonProperty("department_name")]
        public string department_name { get; set; }

        [JsonProperty("grade_category")]
        public string grade_category { get; set; }

        [JsonProperty("marital_status")]
        public int marital_status { get; set; }

        [JsonProperty("start_date")]
        public DateTime start_date { get; set; }
    }

    /// <summary>
    /// Mewakili keseluruhan respons JSON dari API `getListEmp`.
    /// </summary>
    public class SunfishEmployeeListResponse : SunfishApiResponse
    {
        [JsonProperty("data")]
        public List<SunfishEmployeeDetail> Data { get; set; }
    }
}