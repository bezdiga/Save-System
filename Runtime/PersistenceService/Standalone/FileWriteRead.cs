using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using _JoykadeGames.Runtime.SaveSystem.Standlone;
using OdinSerializer;
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class FileWriteRead : IWriterReader
    {
        private SerializationAsset _serialization;
        
        public FileWriteRead(StandaloneParams @params)
        {
            _serialization = @params.SerializationAsset;
        }

        public string GetSaveFolderPath => "_saveGamePath";
        public StorableCollection LoadFromSaveFile(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"SaveSystem Error: File {path} not exist at path {path}");
                return null;
            }

            Debug.Log("Loading save file from path: " + path);
            byte[] bytes = File.ReadAllBytes(path);
            StorableCollection gamedata = SerializationUtility.DeserializeValue<StorableCollection>(bytes, DataFormat.Binary);

            return gamedata;
        }
        
        public void SerializeData(StorableCollection buffer, string path)
        {
            Stream stream = File.Open(path, FileMode.Create);
            var context = new SerializationContext();
            var writer = new BinaryDataWriter(stream, context);

            SerializationUtility.SerializeValue(buffer, writer);

            stream.Close();
        }

        public async Task<SavedGameInfo[]> ReadAllSaves()
        {
            string savesPath = _serialization.GetSavesPath();
            string saveFolderPrefix = _serialization.SaveFolderPrefix;
            string saveInfoPrefix = _serialization.SaveInfoName;
            string saveExtension = _serialization.SaveExtension;
            IList<SavedGameInfo> saveInfos = new List<SavedGameInfo>();

            
            if (Directory.Exists(savesPath))
            {
                string[] directories = Directory.GetDirectories(savesPath, $"{saveFolderPrefix}*");
                foreach (var directoryPath in directories)
                {
                    string fileName = saveInfoPrefix + saveExtension;
                    string saveInfoPath = Path.Combine(directoryPath, saveInfoPrefix + saveExtension);

                    // check if file exists
                    if (!File.Exists(saveInfoPath))
                        continue;

                    Debug.LogError("Current save info path: " + saveInfoPath);
                    var info = LoadFromSaveFile(saveInfoPath);

                    var timeString = info.GetT<string>("dateTime");
                    DateTime createdTime;
                    if (DateTime.TryParseExact(
                            timeString,
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime parsedTime))
                    {
                        createdTime = parsedTime;
                    }
                    else
                    {
                        Debug.LogError("Format de timp invalid: " + timeString);
                        createdTime = DateTime.MinValue;
                    }
                    saveInfos.Add(new SavedGameInfo()
                    {
                        Id = info.GetT<string>("id"),
                        Scene = info.GetT<string>("scene"),
                        TimeSaved = createdTime,
                        TimePlayed = TimeSpan.FromSeconds(info.GetT<float>("timePlayed")),
                        Foldername = Path.GetFileName(directoryPath),
                    });
                    
                }
                
                return saveInfos.OrderByDescending(x => x.TimeSaved).ToArray();
            }

            return new SavedGameInfo[0];
        }

        /// <summary>
        /// Remove all saved games.
        /// </summary>
        public async Task RemoveAllSaves()
        {
            string savesPath = _serialization.GetSavesPath();
            string saveFolderPrefix = _serialization.SaveFolderPrefix;

            if (Directory.Exists(savesPath))
            {
                string[] directories = Directory.GetDirectories(savesPath, $"{saveFolderPrefix}*");
                if (directories.Length > 0)
                {
                    Task[] deleteTasks = new Task[directories.Length];
                    for (int i = 0; i < directories.Length; i++)
                    {
                        string directoryPath = directories[i];
                        deleteTasks[i] = Task.Run(() => Directory.Delete(directoryPath, true));
                    }
                    await Task.WhenAll(deleteTasks);
                }
            }
        }
        
    }
}