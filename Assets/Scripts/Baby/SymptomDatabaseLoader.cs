using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Loader untuk SymptomDatabase dari file JSON
/// Menyediakan validasi dan error handling untuk database loading
/// </summary>
public class SymptomDatabaseLoader : MonoBehaviour
{
    public static SymptomDatabaseLoader Instance { get; private set; }

    [Header("Database Configuration")]
    [SerializeField] private string jsonFileName = "symptom_database.json";
    [SerializeField] private bool loadFromStreamingAssets = true;
    [SerializeField] private bool validateOnLoad = true;

    private SymptomDatabase cachedDatabase;
    private bool isLoaded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Wrapper class untuk JSON deserialization
    /// </summary>
    [System.Serializable]
    private class SymptomDatabaseJson
    {
        public List<SymptomDatabase.SymptomData> symptoms = new List<SymptomDatabase.SymptomData>();
        public List<SymptomDatabase.DiseaseData> diseases = new List<SymptomDatabase.DiseaseData>();
    }

    /// <summary>
    /// Load database dari JSON file
    /// </summary>
    public SymptomDatabase LoadDatabase()
    {
        if (isLoaded && cachedDatabase != null)
        {
            Debug.Log("[SymptomDatabaseLoader] Database sudah di-cache, menggunakan data yang tersimpan");
            return cachedDatabase;
        }

        string jsonPath = GetJsonPath();

        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"[SymptomDatabaseLoader] File JSON tidak ditemukan: {jsonPath}");
            return null;
        }

        try
        {
            string jsonContent = File.ReadAllText(jsonPath);
            SymptomDatabaseJson jsonData = JsonUtility.FromJson<SymptomDatabaseJson>(jsonContent);

            if (jsonData == null)
            {
                Debug.LogError("[SymptomDatabaseLoader] Gagal melakukan parsing JSON");
                return null;
            }

            // Buat ScriptableObject untuk database
            cachedDatabase = ScriptableObject.CreateInstance<SymptomDatabase>();
            cachedDatabase.symptoms = jsonData.symptoms;
            cachedDatabase.diseases = jsonData.diseases;

            Debug.Log($"[SymptomDatabaseLoader] Database berhasil dimuat dari: {jsonPath}");
            Debug.Log($"  - Jumlah symptoms: {cachedDatabase.symptoms.Count}");
            Debug.Log($"  - Jumlah diseases: {cachedDatabase.diseases.Count}");

            // Validasi jika enabled
            if (validateOnLoad)
            {
                if (ValidateDatabase(cachedDatabase))
                {
                    Debug.Log("[SymptomDatabaseLoader] Database validation: PASSED ✓");
                    isLoaded = true;
                }
                else
                {
                    Debug.LogError("[SymptomDatabaseLoader] Database validation: FAILED ✗");
                    cachedDatabase = null;
                    return null;
                }
            }
            else
            {
                isLoaded = true;
            }

            return cachedDatabase;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SymptomDatabaseLoader] Error loading database: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Load database dari JSON dengan path custom
    /// </summary>
    public SymptomDatabase LoadDatabaseFromPath(string customPath)
    {
        if (!File.Exists(customPath))
        {
            Debug.LogError($"[SymptomDatabaseLoader] File JSON tidak ditemukan: {customPath}");
            return null;
        }

        try
        {
            string jsonContent = File.ReadAllText(customPath);
            SymptomDatabaseJson jsonData = JsonUtility.FromJson<SymptomDatabaseJson>(jsonContent);

            if (jsonData == null)
            {
                Debug.LogError("[SymptomDatabaseLoader] Gagal melakukan parsing JSON");
                return null;
            }

            cachedDatabase = ScriptableObject.CreateInstance<SymptomDatabase>();
            cachedDatabase.symptoms = jsonData.symptoms;
            cachedDatabase.diseases = jsonData.diseases;

            Debug.Log($"[SymptomDatabaseLoader] Database berhasil dimuat dari custom path: {customPath}");

            if (validateOnLoad)
            {
                ValidateDatabase(cachedDatabase);
            }

            isLoaded = true;
            return cachedDatabase;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SymptomDatabaseLoader] Error loading database from custom path: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Dapatkan database yang sudah di-cache
    /// </summary>
    public SymptomDatabase GetDatabase()
    {
        if (!isLoaded)
        {
            Debug.LogWarning("[SymptomDatabaseLoader] Database belum di-load. Panggil LoadDatabase() terlebih dahulu");
            return null;
        }
        return cachedDatabase;
    }

    /// <summary>
    /// Validasi database dengan error reporting
    /// </summary>
    public bool ValidateDatabase(SymptomDatabase database)
    {
        if (database == null)
        {
            Debug.LogError("[SymptomDatabaseLoader] Database is null");
            return false;
        }

        List<string> errors;
        bool isValid = database.ValidateDatabase(out errors);

        if (!isValid)
        {
            Debug.LogError("[SymptomDatabaseLoader] Database validation errors:");
            foreach (string error in errors)
            {
                Debug.LogError($"  ✗ {error}");
            }
        }

        return isValid;
    }

    /// <summary>
    /// Reset cache
    /// </summary>
    public void ClearCache()
    {
        if (cachedDatabase != null)
        {
            Destroy(cachedDatabase);
        }
        cachedDatabase = null;
        isLoaded = false;
        Debug.Log("[SymptomDatabaseLoader] Cache cleared");
    }

    /// <summary>
    /// Dapatkan path file JSON
    /// </summary>
    private string GetJsonPath()
    {
        if (loadFromStreamingAssets)
        {
            return Path.Combine(Application.streamingAssetsPath, jsonFileName);
        }
        else
        {
            return Path.Combine(Application.persistentDataPath, jsonFileName);
        }
    }

    /// <summary>
    /// Export database saat ini ke file JSON (untuk testing/backup)
    /// </summary>
    public void ExportDatabaseToJson(SymptomDatabase database, string outputPath)
    {
        try
        {
            SymptomDatabaseJson jsonData = new SymptomDatabaseJson
            {
                symptoms = database.symptoms,
                diseases = database.diseases
            };

            string json = JsonUtility.ToJson(jsonData, true);
            File.WriteAllText(outputPath, json);

            Debug.Log($"[SymptomDatabaseLoader] Database successfully exported to: {outputPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SymptomDatabaseLoader] Error exporting database: {e.Message}");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor helper untuk mengecek file path
    /// </summary>
    public void EditorPrintPaths()
    {
        Debug.Log($"[SymptomDatabaseLoader] Streaming Assets Path: {Application.streamingAssetsPath}");
        Debug.Log($"[SymptomDatabaseLoader] Persistent Data Path: {Application.persistentDataPath}");
        Debug.Log($"[SymptomDatabaseLoader] Current JSON Path: {GetJsonPath()}");
    }
#endif
}
