using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;

// Model untuk DataKaryawan.json
public class KaryawanJson
{
    [JsonProperty("kry_npk")]
    public string KryNpk { get; set; }

    [JsonProperty("kry_nama_karyawan")]
    public string KryNamaKaryawan { get; set; }

    [JsonProperty("kry_plant")]
    public string KryPlant { get; set; }

    [JsonProperty("kry_departemen")]
    public string KryDepartemen { get; set; }
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

    [JsonProperty("tgg_hubungan_tanggungan")]
    public string TggHubunganTanggungan { get; set; }
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
    private static List<KaryawanJson> _karyawanCache;
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
    public static List<KaryawanJson> GetAllKaryawan() => ReadAndCache(ref _karyawanCache, "DataKaryawan.json");
    public static List<TanggunganJson> GetAllTanggungan() => ReadAndCache(ref _tanggunganCache, "DataTanggungan.json");
    public static List<PenyakitJson> GetAllPenyakit() => ReadAndCache(ref _penyakitCache, "DataPenyakit.json");
    public static List<RumahSakitJson> GetAllRumahSakit() => ReadAndCache(ref _rumahSakitCache, "DataRumahSakit.json");

    // Metode Helper spesifik
    public static KaryawanJson GetKaryawanByNpk(string npk)
    {
        return GetAllKaryawan().FirstOrDefault(k => k.KryNpk == npk);
    }

    public static IEnumerable<TanggunganJson> GetTanggunganByNpk(string npk)
    {
        return GetAllTanggungan().Where(t => t.TggNpk == npk);
    }
}