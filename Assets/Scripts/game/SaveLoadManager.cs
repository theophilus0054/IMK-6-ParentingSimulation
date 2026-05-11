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
        public int day;
        public int phaseIndex;
        public float babyHunger;
        public float babyComfort;
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
            BabyDisease disease = baby.GetComponent<BabyDisease>();

            if (baby == null || disease == null)
            {
                Debug.LogError("[SaveLoad] BabyBehavior atau BabyDisease tidak ditemukan!");
                return;
            }

            SaveData data = new SaveData
            {
                version = SAVE_VERSION,
                day = gm.currentDay,
                phaseIndex = (int)gm.currentPhase,
                babyHunger = Mathf.Clamp(baby.hunger, 0f, 100f),
                babyComfort = Mathf.Clamp(baby.comfort, 0f, 100f),
                babyTemperature = Mathf.Clamp(baby.temperature, 36f, 41f),
                diseaseType = (int)disease.currentDisease.type,
                diseaseSeverity = Mathf.Clamp(disease.currentDisease.severityLevel, 0f, 100f),
                diseaseElapsedTime = disease.currentDisease.elapsedTime
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"[SaveLoad] Game Saved! Day {data.day}, Phase {(GameManager.GamePhase)data.phaseIndex}");
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
            BabyDisease disease = baby.GetComponent<BabyDisease>();

            if (baby == null || disease == null)
            {
                Debug.LogError("[SaveLoad] BabyBehavior atau BabyDisease tidak ditemukan!");
                return;
            }

            // Validate data ranges
            gm.currentDay = Mathf.Max(1, data.day);
            gm.currentPhase = (GameManager.GamePhase)data.phaseIndex;
            baby.hunger = Mathf.Clamp(data.babyHunger, 0f, 100f);
            baby.comfort = Mathf.Clamp(data.babyComfort, 0f, 100f);
            baby.temperature = Mathf.Clamp(data.babyTemperature, 36f, 41f);

            // Load disease state
            if (data.diseaseType > 0)
            {
                disease.currentDisease.type = (BabyDisease.DiseaseType)data.diseaseType;
                disease.currentDisease.severityLevel = Mathf.Clamp(data.diseaseSeverity, 0f, 100f);
                disease.currentDisease.elapsedTime = Mathf.Max(0f, data.diseaseElapsedTime);
            }

            Debug.Log($"[SaveLoad] Game Loaded! Day {data.day}, Phase {(GameManager.GamePhase)data.phaseIndex}");
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