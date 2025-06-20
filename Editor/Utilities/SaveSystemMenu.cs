using System.IO;
using _JoykadeGames.Code.Runtime.Scriptables;
using UnityEditor;
using UnityEngine;

namespace _JoykadeGames.Editor
{
    public static class SaveSystemMenu
    {
        private static string saveSystemAssetPath = "Assets/Resources/Data/";
        [MenuItem("Save System/Initialize Save Sytem")]
        public static void CreateSaveSettings()
        {
            string directory = Path.GetDirectoryName(saveSystemAssetPath);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            SerializationAsset newSettings = ScriptableObject.CreateInstance<SerializationAsset>();
            ObjectDataBase dataBase = ScriptableObject.CreateInstance<ObjectDataBase>();

            if (!File.Exists($"{saveSystemAssetPath}SerializationAsset.asset"))
            {
                AssetDatabase.CreateAsset(newSettings, $"{saveSystemAssetPath}/SerializationAsset.asset");
                Debug.Log("Create SerializationAsset successfully");
            }
            if (!File.Exists($"{saveSystemAssetPath}/ObjectDatabase.asset"))
            {
                AssetDatabase.CreateAsset(dataBase, $"{saveSystemAssetPath}ObjectDatabase.asset");
                Debug.Log("Create DataBase successfully");
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = newSettings;
            EditorUtility.FocusProjectWindow();
        }
    }
}