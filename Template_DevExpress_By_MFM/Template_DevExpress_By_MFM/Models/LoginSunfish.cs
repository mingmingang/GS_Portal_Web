using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
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

    public class StringOrArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<string>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (token.Type == JTokenType.Array)
            {
                // Jika datanya SUDAH array, langsung konversi
                return token.ToObject<List<string>>();
            }
            else if (token.Type == JTokenType.String)
            {
                // Jika datanya adalah string TUNGGAL, buatkan List baru
                return new List<string> { token.ToString() };
            }
            else if (token.Type == JTokenType.Null)
            {
                // Jika datanya null, kembalikan list kosong
                return new List<string>();
            }

            // --- PERBAIKAN DI SINI ---
            // Menghapus " error: " yang salah ketik
            throw new JsonSerializationException("Tipe token tidak terduga: " + token.Type.ToString());
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    // --- MODEL SPESIFIK UNTUK API `cek_login_sunfish` ---

    // --- MODEL SPESIFIK UNTUK API cek_login_sunfish ---

    /// <summary>
    /// Mewakili objek data yang dikembalikan oleh API otentikasi cek_login_sunfish.
    /// </summary>
    public class SunfishAuthData
    {
        [JsonProperty("emp_id")]
        public string emp_id { get; set; }

        [JsonProperty("emp_no")]
        public string emp_no { get; set; } // Kunci untuk join ke API data detail

        [JsonProperty("company_id")]
        public int? company_id { get; set; } // Changed to nullable

        [JsonProperty("full_name")]
        public string full_name { get; set; }

        [JsonProperty("work_location_code")]
        public string work_location_code { get; set; }

        [JsonProperty("grade_code")]
        public string grade_code { get; set; }

        [JsonProperty("maritalstatus")]
        public int? maritalstatus { get; set; } // Changed to nullable - INI YANG MENYEBABKAN ERROR

        [JsonProperty("phone")]
        public string phone { get; set; }

        [JsonProperty("photo")]
        public string photo { get; set; }

        [JsonProperty("pos_level")]
        public int? pos_level { get; set; } // Changed to nullable

        [JsonProperty("created_date")]
        public DateTime? created_date { get; set; } // Changed to nullable
        public string pos_name_en { get; set; }

        [JsonProperty("role_options")]
        [JsonConverter(typeof(StringOrArrayConverter))]
        public List<string> role_options { get; set; } // "Karyawan", "HC", "Atasan"
    }

    /// <summary>
    /// Mewakili keseluruhan respons JSON dari API cek_login_sunfish.
    /// Ini adalah model yang HILANG dari file Anda sebelumnya.
    /// </summary>
    public class SunfishAuthResponse : SunfishApiResponse
    {
        [JsonProperty("data")]
        public List<SunfishAuthData> Data { get; set; }
    }


    // --- MODEL SPESIFIK UNTUK API getListEmp ---

    /// <summary>
    /// Mewakili objek data karyawan yang lebih detail dari API getListEmp.
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
        public int? marital_status { get; set; } // Changed to nullable

        [JsonProperty("start_date")]
        public DateTime? start_date { get; set; } // Changed to nullable
    }

    /// <summary>
    /// Mewakili keseluruhan respons JSON dari API getListEmp.
    /// </summary>
    public class SunfishEmployeeListResponse : SunfishApiResponse
    {
        [JsonProperty("data")]
        public List<SunfishEmployeeDetail> Data { get; set; }
    }
}