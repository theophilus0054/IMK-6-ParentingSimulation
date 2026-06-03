using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    private string saveFilePath;
    private const int SAVE_VERSION = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        try
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "babysave.json");
            Debug.Log($"[SaveLoad] Save path: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoad] Failed to set save path: {e.Message}");
        }
    }

    // Class penampung data yang akan di-serialize ke JSON
    [System.Serializable]
    public class SaveData
    {
        public int version = SAVE_VERSION;
        public float babyHealth;           // Health (0-100)
        public float babyOxygenLevel;      // Oxygen level (0-100)
        public float babyTemperature;
        
        // Disease save data
        public int diseaseType; // 0 = None, 1 = CommonCold, 2 = Pneumonia
        public float diseaseSeverity;
        public float diseaseElapsedTime;
    }

    public void SaveGame()
    {
        try
        {
            GameManager gm = GameManager.Instance;
            BabyBehavior baby = gm.babyBehavior;

            if (baby == null)
            {
                Debug.LogError("[SaveLoad] BabyBehavior tidak ditemukan!");
                return;
            }

            SaveData data = new SaveData
            {
                version = SAVE_VERSION,
                babyHealth = Mathf.Clamp(baby.health, 0f, 100f),
                babyOxygenLevel = Mathf.Clamp(baby.oxygenLevel, 0f, 100f),
                babyTemperature = Mathf.Clamp(baby.temperature, 36f, 41f),
                diseaseType = (int)baby.currentDisease,
                diseaseSeverity = baby.GetDiseaseSeverity() * 100f,
                diseaseElapsedTime = baby.diseaseElapsedTime
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"[SaveLoad] Game Saved! Health: {data.babyHealth:F0}%, Oxygen: {data.babyOxygenLevel:F0}%");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoad] Failed to save game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        try
        {
            if (!File.Exists(saveFilePath))
            {
                Debug.LogWarning("[SaveLoad] Save file not found. Starting fresh.");
                return;
            }

            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // Validate version
            if (data.version != SAVE_VERSION)
            {
                Debug.LogWarning($"[SaveLoad] Save version mismatch! Expected {SAVE_VERSION}, got {data.version}");
                return;
            }

            GameManager gm = GameManager.Instance;
            BabyBehavior baby = gm.babyBehavior;

            if (baby == null)
            {
                Debug.LogError("[SaveLoad] BabyBehavior tidak ditemukan!");
                return;
            }

            baby.health = Mathf.Clamp(data.babyHealth, 0f, 100f);
            baby.oxygenLevel = Mathf.Clamp(data.babyOxygenLevel, 0f, 100f);
            baby.temperature = Mathf.Clamp(data.babyTemperature, 36f, 41f);

            // Load disease state
            if (data.diseaseType > 0)
            {
                baby.currentDisease = (BabyBehavior.DiseaseState)data.diseaseType;
                baby.diseaseElapsedTime = Mathf.Max(0f, data.diseaseElapsedTime);
            }
            else
            {
                baby.currentDisease = BabyBehavior.DiseaseState.None;
            }

            Debug.Log($"[SaveLoad] Game Loaded! Health: {data.babyHealth:F0}%, Oxygen: {data.babyOxygenLevel:F0}%");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoad] Failed to load game: {e.Message}");
        }
    }

    public void DeleteSaveFile()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.Log("[SaveLoad] Save file deleted.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoad] Failed to delete save file: {e.Message}");
        }
    }
}
