using System;
using System.IO;
using OdinSerializer;
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class FileWriteRead
    {
        private string _saveGamePath;
        
        public FileWriteRead(string saveFolderPath)
        {
            _saveGamePath = saveFolderPath;
        }

        public string GetSaveFolderPath => _saveGamePath;
        public StorableCollection LoadFromSaveFile(string fileName)
        {

            string path = Path.Combine(_saveGamePath, fileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"SaveSystem Error: File {fileName} not exist at path {path}");
                return null;
            }

            byte[] bytes = File.ReadAllBytes(path);
            StorableCollection gamedata = SerializationUtility.DeserializeValue<StorableCollection>(bytes, DataFormat.Binary);

            return gamedata;
        }
        
        public void SerializeData(StorableCollection buffer, string fileName)
        {
            string path = Path.Combine(_saveGamePath, fileName);
            Stream stream = File.Open(path, FileMode.Create);
            var context = new SerializationContext();
            var writer = new BinaryDataWriter(stream, context);

            SerializationUtility.SerializeValue(buffer, writer);

            stream.Close();
        }
        
    }
}