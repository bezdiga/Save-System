using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using _JoykadeGames.Runtime.SaveSystem.Nitendo;
using nn.fs;
using OdinSerializer;
using PersistenceService.Switch;
using UnityEngine;


namespace _JoykadeGames.Runtime.SaveSystem.Switch
{
    public class SwitchFileWriteReader : IWriterReader
    {
        private SerializationAsset _serialization;
        private bool isMounted = false;

        private bool disposedValue = false;
        private SwitchInitParams _switchInitParams;
        
        public IDirectorySystemProvider Directory { get; private set; }
        public IFileSystemProvider File { get; private set; }
        
        
        public SwitchFileWriteReader(SwitchInitParams initParams)
        {
            _switchInitParams = initParams ?? throw new ArgumentNullException(nameof(initParams));
            File = new SwitchFileSystem(initParams.SerializationAsset.DataPath);
            Directory = new SwitchDirectoryProvider();
            _serialization = initParams.SerializationAsset;
            MountFileSystem(initParams.UserId);
        }
        

        public void SerializeData(StorableCollection buffer, string path)
        {
            if (!isMounted)
            {
                Debug.LogError("SaveSystem Error: File system is not mounted. Cannot serialize data.");
                return;
            }
            
            Debug.LogError("SaveSystem: Serializing data to path: " + path);
            byte[] saveData;
            using (MemoryStream ms = new MemoryStream())
            {
                var context = new SerializationContext();
                var writer = new BinaryDataWriter(ms, context);
            
                SerializationUtility.SerializeValue(buffer, writer);
                saveData = ms.ToArray();
            }

            File.WriteFile(path, saveData);
        }

      

        public void TryDeserializeGameStateAsync(string folderName)
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
            StorableCollection worldData = LoadFromSaveFile(filePath);
            if (worldData != null)
            {
                if (worldData.ContainsKey("worldState"))
                    SaveGameManager._worldStateBuffer = (worldData["worldState"] as StorableCollection);
            }
        }
        
        public StorableCollection LoadFromSaveFile(string path)
        {
            if (!isMounted)
            {
                Debug.LogError("SaveSystem Error: File system is not mounted. Cannot load data.");
                return null;
            }
            // An nn.Result object is used to get the result of NintendoSDK plug-in operations.
            File.ReadFile(path, out byte[] bytes);
            
            StorableCollection collection = SerializationUtility.DeserializeValue<StorableCollection>(bytes, DataFormat.Binary);
            return collection;
        }

        public async Task<SavedGameInfo[]> ReadAllSaves()
        {
            
            string savesPath = _serialization.GetSavesPath();
            //string saveFolderPrefix = _serialization.SaveFolderPrefix;
            string saveInfoPrefix = _serialization.SaveInfoName;
            string saveExtension = _serialization.SaveExtension;
            
            IList<SavedGameInfo> saveInfos = new List<SavedGameInfo>();
            
            if (Directory.Exists(savesPath))
            {
                string[] directories = Directory.GetDirectories(savesPath,"");
                foreach (var directoryPath in directories)
                {
                    //string fileName = saveInfoPrefix + saveExtension;
                    string saveInfoPath = Path.Combine(_serialization.GetSavesPath(),directoryPath, saveInfoPrefix + saveExtension);

                    // check if file exists
                    if (!File.Exists(saveInfoPath))
                        continue;
                    
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
        

        public Task RemoveAllSaves()
        {
            nn.fs.Directory.Delete(_serialization.GetSavesPath());
            return Task.CompletedTask;
        }

        private void MountFileSystem(nn.account.Uid userId)
        {
            if (isMounted) return;
            Debug.LogError("Finish mounting file system, now loading from path: " + _serialization.DataPath);
            nn.Result result;
            result = nn.fs.SaveData.Mount(_serialization.DataPath, userId);
            isMounted = true;
            // This error handling is optional.
            // The mount operation will not fail unless the save data is already mounted or the mount name is in use.
            if (nn.fs.FileSystem.ResultTargetLocked.Includes(result))
            {
                // Save data for specified user ID is already mounted. Get account name and display an error.
                nn.account.Nickname nickname = new nn.account.Nickname();
                nn.account.Account.GetNickname(ref nickname, userId);
                Debug.LogErrorFormat("The save data for {0} is already mounted: {1}", nickname.name, result.ToString());
            }
            else if (nn.fs.FileSystem.ResultMountNameAlreadyExists.Includes(result))
            {
                // The specified mount name is already in use.
                Debug.LogErrorFormat("The mount name '{0}' is already in use: {1}", _serialization.DataPath, result.ToString());
            }
            
            // Abort if any of the initialization steps failed.
            result.abortUnlessSuccess();
        }


        #region Disposable Implementation
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Eliberează aici resursele MANAGED (alte obiecte .NET care sunt IDisposable)
                }

                // Eliberează aici resursele UNMANAGED (fișiere, handle-uri, etc.)
                if (isMounted)
                {
                    UnmountSave();
                }

                disposedValue = true;
            }
        }

        ~SwitchFileWriteReader()
        {
            Dispose(disposing: false);
        }
        
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
        
        private void UnmountSave()
        {
            Debug.LogError("Unmounting save data at path: " + _serialization.DataPath);
            isMounted = false;
            nn.fs.FileSystem.Unmount(_serialization.DataPath);
        }
        
        
    }
}