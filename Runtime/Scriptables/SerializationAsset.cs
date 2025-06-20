
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "SerializationAsset", menuName = "JoykadeGames/SerializationAsset")]
public class SerializationAsset : ScriptableObject
{
    [Header("Path")]
    public string SavesPath = "Saves";
    public string DataPath = "Data";
    public string ConfigPath;
    
    public string SaveInfoName = "game";
    public string SaveDataName = "data";
    public string SaveExtention = ".sav";
    public string SaveFolderPrefix ="WorldSave_";
    
    public bool SingleSave;
    public string GetSavesPath()
    {
        return Path.Combine(Application.persistentDataPath, DataPath, SavesPath).Replace('\\', '/');
    }
}
