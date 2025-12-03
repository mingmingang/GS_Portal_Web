using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;

// Model untuk DataKaryawan.json
public class EmployeeJson
{
    public string emp_id { get; set; }

    public string emp_no { get; set; }

    [JsonProperty("Full_Name")]
    public string Full_Name { get; set; }

    public string jenis_kelamin { get; set; }

    public string worklocation_code { get; set; }

    public int status { get; set; }

    public DateTime? end_date { get; set; }

    public string pos_name_id { get; set; }

    public string id_departemen { get; set; }

    public string departemen { get; set; }

    public long plafon { get; set; }

    public string status_karyawan { get; set; }

    public string status_kawin { get; set; }

    public int golongan { get; set; }

    public string created_by { get; set; }

    public DateTime created_date { get; set; }

    public string modif_by { get; set; }

    public DateTime modif_date { get; set; }
}

// Model untuk DataTanggungan.json
public class TanggunganJson
{
    [JsonProperty("tgg_no_tanggungan")]
    public string TggNoTanggungan { get; set; }

    [JsonProperty("tgg_npk")]
    public string TggNpk { get; set; }

    [JsonProperty("tgg_nama_tanggungan")]
    public string TggNamaTanggungan { get; set; }

    [JsonProperty("tgg_hubungan")]
    public string TggHubungan { get; set; }
}

// Model untuk DataPenyakit.json
public class PenyakitJson
{
    [JsonProperty("disease_code")]
    public string DiseaseCode { get; set; }

    [JsonProperty("disease_name_id")]
    public string DiseaseNameId { get; set; }
}

// Model untuk DataRumahSakit.json
public class RumahSakitJson
{
    [JsonProperty("doctorhospital_code")]
    public string DoctorHospitalCode { get; set; }

    [JsonProperty("doctorhospital_name")]
    public string DoctorHospitalName { get; set; }

    [JsonProperty("doctorhospital_address")]
    public string DoctorHospitalAddress { get; set; }

    [JsonProperty("type_rs_name")]
    public string DoctorHospitalType { get; set; }
}

public static class JsonDataHelper
{
    private static readonly string dataPath = HostingEnvironment.MapPath("~/App_Data/JsonMasterData");

    // Cache sederhana
    private static List<EmployeeJson> _karyawanCache;
    private static List<TanggunganJson> _tanggunganCache;
    private static List<PenyakitJson> _penyakitCache;
    private static List<RumahSakitJson> _rumahSakitCache;

    private static List<T> ReadAndCache<T>(ref List<T> cache, string fileName)
    {
        if (cache == null)
        {
            var json = File.ReadAllText(Path.Combine(dataPath, fileName));
            cache = JsonConvert.DeserializeObject<List<T>>(json);
        }
        return cache;
    }

    // Metode untuk memuat masing-masing file JSON
    public static List<EmployeeJson> GetAllKaryawan() => ReadAndCache(ref _karyawanCache, "DataEmployee.json");
    public static List<TanggunganJson> GetAllTanggungan() => ReadAndCache(ref _tanggunganCache, "DataKeluarga.json");
    public static List<PenyakitJson> GetAllPenyakit() => ReadAndCache(ref _penyakitCache, "DataPenyakit.json");
    public static List<RumahSakitJson> GetAllRumahSakit() => ReadAndCache(ref _rumahSakitCache, "DataRumahSakit.json");

    // Metode Helper spesifik
    public static EmployeeJson GetKaryawanByNpk(string npk)
    {
        return GetAllKaryawan().FirstOrDefault(k => k.emp_no == npk);
    }

    public static IEnumerable<TanggunganJson> GetTanggunganByNpk(string npk)
    {
        return GetAllTanggungan().Where(t => t.TggNpk == npk);
    }
}