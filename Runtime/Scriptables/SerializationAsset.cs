
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SerializationAsset", menuName = "JoykadeGames/SerializationAsset")]
public class SerializationAsset : ScriptableObject
{
    [Header("Scenes")]
    public string LevelManagerScene = "LevelManager";
    [Header("Path")]
    public string SavesPath = "Saves";
    public string DataPath = "Data";
    public string ConfigPath;
    
    public string SaveInfoName = "game";
    public string SaveDataName = "data";
    public string SaveConfigName = "config";
    public string SaveExtension = ".sav";
    public string SaveFolderPrefix ="WorldSave_";
    public long MaxSaveSize = 2 * 1024 * 1024; // 2 MB
    public bool SingleSave;
    public bool UseSceneNames;
    public long DataSize => MaxSaveSize;

    public string GetSavesPath()
    {
        return Path.Combine(Application.persistentDataPath, DataPath, SavesPath).Replace('\\', '/');
    }
}
