
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
#if UNITY_SWITCH && !UNITY_EDITOR
            return string.Format("{0}:/{1}", DataPath, SavesPath);
#elif UNITY_PS4 && !UNITY_EDITOR
            throw new Exception("PlayStation 4 platform is not supported in this implementation.");
#elif UNITY_STANDALONE || UNITY_EDITOR
        return Path.Combine(Application.persistentDataPath, DataPath, SavesPath).Replace('\\', '/');
#else
        Debug.LogWarning("Platform not supported, using StandaloneInitializer as fallback.");
        return new StandaloneInitializer().GetSavesPath();
#endif
    }
}
