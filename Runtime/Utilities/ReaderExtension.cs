using System.IO;
using System.Threading.Tasks;
using _JoykadeGames.Runtime.SaveSystem;
using UnityEngine;

namespace _JoykadeGames
{
    public static class ReaderExtension
    {
        /// <summary>
        /// Try to Deserialize and Validate Game State
        /// </summary>
        public static void TryDeserializeGameStateAsync(this IWriterReader writeRead,string folderName)
        {
            // get saves path
            string savesPath = SaveGameManager._serializationAsset.GetSavesPath();
            string saveFolderPath = Path.Combine(savesPath, folderName);

            // check if directory exists
            if (!Directory.Exists(saveFolderPath))
            {
                Debug.LogError("Save folder does not exist: " + saveFolderPath);
                return;
            }

            // deserialize saved game info
            string filePath = Path.Combine(saveFolderPath, SaveGameManager._serializationAsset.SaveDataName + SaveGameManager._serializationAsset.SaveExtension);
            StorableCollection worldData = writeRead.LoadFromSaveFile(filePath);
            if (worldData != null)
            {
                if (worldData.ContainsKey("worldState"))
                    SaveGameManager._worldStateBuffer = (worldData["worldState"] as StorableCollection);
            }
        }
        
        /// <summary>
        /// Remove all saved games.
        /// </summary>
        public static async Task RemoveAllSaves(this IWriterReader writerReader)
        {
            await writerReader.RemoveAllSaves();
        }
    }
}