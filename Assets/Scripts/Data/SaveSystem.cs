using System.IO;
using UnityEngine;

namespace Magetender.Data
{
    public static class SaveSystem
    {
        private const string FileName = "save.json";

        public static void WriteData()
        {
            var gameManager = GameManager.Instance;
            var currentMonster = CurrentMonster.Instance;

            if (gameManager == null || currentMonster == null)
                return;

            var data = new SaveData
            {
                coins = gameManager.Coins,
                day = gameManager.Day,
                currentEncounterIndex = currentMonster.currentEncounterIndex,
				unlockedIngredientIds = gameManager.GetUnlockedIngredientIds(),
                tutorialCompleted = gameManager.TutorialCompleted
            };

            SaveGame(data);
        }

        public static void WriteLoseState()
        {
            int day = GameManager.Instance != null ? GameManager.Instance.Day : 1;
            SaveGame(new SaveData
            {
                coins = 0,
                day = day,
                currentEncounterIndex = 0,
                tutorialCompleted = GameManager.Instance != null && GameManager.Instance.TutorialCompleted
            });
        }

        public static void SaveGame(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(Application.persistentDataPath, FileName);
            File.WriteAllText(path, json);
            Debug.Log($"[SaveSystem] Saved to: {path}");
        }

        public static SaveData LoadGame()
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonUtility.FromJson<SaveData>(json);
        }

        public static void ClearSave()
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("[SaveSystem] Save cleared.");
            }
        }
    }
}
