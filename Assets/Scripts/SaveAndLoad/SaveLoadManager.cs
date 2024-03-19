using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    public static string SavePath => $"{Application.persistentDataPath}/save.json";

    public static void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game saved to {SavePath}");
    }

    public static GameData LoadGame()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<GameData>(json);
        }
        Debug.LogError("Save file not found");
        return null;
    }
}
