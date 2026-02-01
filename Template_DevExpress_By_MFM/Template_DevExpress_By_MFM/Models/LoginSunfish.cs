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
                return token.ToObject<List<string>>();
            }
            else if (token.Type == JTokenType.String)
            {
                return new List<string> { token.ToString() };
            }
            return new List<string>();
        }

        public override bool CanWrite { get { return false; } }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


    // --- MODEL SPESIFIK UNTUK API cek_login_sunfish ---

    /// <summary>
    /// Mewakili objek data yang dikembalikan oleh API otentikasi cek_login_sunfish.
    /// </summary>
    public class SunfishAuthData
    {
        [JsonProperty("emp_id")]
        public string emp_id { get; set; }

        [JsonProperty("emp_no")]
        public string emp_no { get; set; }

        [JsonProperty("company_id")]
        public int? company_id { get; set; }

        [JsonProperty("full_name")]
        public string full_name { get; set; }

        [JsonProperty("work_location_code")]
        public string work_location_code { get; set; }

        [JsonProperty("grade_code")]
        public string grade_code { get; set; }

        [JsonProperty("maritalstatus")]
        public int? maritalstatus { get; set; }

        [JsonProperty("phone")]
        public string phone { get; set; }

        [JsonProperty("photo")]
        public string photo { get; set; }

        [JsonProperty("pos_level")]
        public int? pos_level { get; set; }

        [JsonProperty("created_date")]
        public DateTime? created_date { get; set; }

        // --- TAMBAHAN DATA BARU DARI JSON ---

        [JsonProperty("dept_id")]
        public int? dept_id { get; set; }

        [JsonProperty("dept_code")]
        public string dept_code { get; set; }

        [JsonProperty("dept_name")]
        public string dept_name { get; set; }

        [JsonProperty("pos_name_en")]
        public string pos_name_en { get; set; }

        [JsonProperty("pos_name_id")]
        public string pos_name_id { get; set; }
        [JsonProperty("start_date")]
        public string start_date { get; set; }

        [JsonProperty("end_date")]
        public string end_date { get; set; }

        // Menggunakan converter agar aman jika API mengirim string "GA" atau array ["GA", "HC"]
        [JsonProperty("role_options")]
        [JsonConverter(typeof(StringOrArrayConverter))]
        public List<string> role_options { get; set; }
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

        [JsonProperty("pos_name_id")]
        public string department_name { get; set; }

        [JsonProperty("grade_code")]
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