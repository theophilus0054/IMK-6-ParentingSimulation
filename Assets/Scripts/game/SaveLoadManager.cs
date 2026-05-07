using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        saveFilePath = Application.persistentDataPath + "/babysave.json";
    }

    // Class penampung data yang akan di-serialize ke JSON
    [System.Serializable]
    public class SaveData
    {
        public int day;
        public int phaseIndex;
        public float babyHunger;
        public float babyComfort;
        public float babyTemperature;
    }

    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            day = GameManager.Instance.currentDay,
            phaseIndex = (int)GameManager.Instance.currentPhase,
            babyHunger = GameManager.Instance.babyBehavior.hunger,
            babyComfort = GameManager.Instance.babyBehavior.comfort,
            babyTemperature = GameManager.Instance.babyBehavior.temperature
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game Saved to: " + saveFilePath);
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            GameManager.Instance.currentDay = data.day;
            GameManager.Instance.currentPhase = (GameManager.GamePhase)data.phaseIndex;
            
            GameManager.Instance.babyBehavior.hunger = data.babyHunger;
            GameManager.Instance.babyBehavior.comfort = data.babyComfort;
            GameManager.Instance.babyBehavior.temperature = data.babyTemperature;

            Debug.Log("Game Loaded.");
        }
        else
        {
            Debug.Log("Save file not found. Starting fresh.");
        }
    }
}