using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using nn.fs;
using OdinSerializer;
using UnityEngine;


namespace _JoykadeGames.Runtime.SaveSystem.Switch
{
    public class SwitchFileWriteReader : IWriterReader
    {
        private SerializationAsset _serialization;
        private bool isMounted = false;

        private bool disposedValue = false;
        private SwitchInitParams _switchInitParams;
        public SwitchFileWriteReader(SwitchInitParams initParams)
        {
            _switchInitParams = initParams ?? throw new ArgumentNullException(nameof(initParams));
            _serialization = initParams.SerializationAsset;
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
            //UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
            
            nn.Result result;
        
            nn.fs.FileHandle handle = new nn.fs.FileHandle();
            
            
            result = nn.fs.File.Open(ref handle, path, nn.fs.OpenFileMode.Write);
            
            if (!result.IsSuccess())
            {
                if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
                {
                    result = nn.fs.File.Create(path, saveData.LongLength);
                    if (!result.IsSuccess())
                    {
                        if(FileSystem.ResultTargetLocked.Includes(result))
                        {
                            Debug.LogError($"SaveSystem Error: File already exists and is locked: {path}");
                        }
                        else if (FileSystem.ResultPathNotFound.Includes(result))
                        {
                            Debug.LogError($"SaveSystem Error: Path not found when trying to create file: {path}");
                        }
                        else if(FileSystem.ResultUsableSpaceNotEnough.Includes(result))
                        {
                            Debug.LogError($"SaveSystem Error: Not enough usable space to create file at: {path}");
                        }
                        else Debug.LogError($"File did not exist and creation at: {path} failed: {result}");
                        return;
                    }
                    
                    result = nn.fs.File.Open(ref handle, path, nn.fs.OpenFileMode.Write);
                    if (!result.IsSuccess())
                    {
                        Debug.LogError($"SaveSystem Error: Failed to open a file we just created: {result}");
                        return ;
                    }
                }
                else if(nn.fs.FileSystem.ResultUsableSpaceNotEnough.Includes(result))
                {
                    // DA, nu există suficient spațiu liber pentru a scrie fișierul.
                    Debug.LogError($"SaveSystem Error: Not enough usable space to write the file: {result}");
                    return ;
                }
                else
                {
                    
                    Debug.LogError($"SaveSystem Error: Failed to open file for an unexpected reason: {result}");
                    return ;
                }
            }
            
            result = nn.fs.File.SetSize(handle, saveData.LongLength);
            
            result = nn.fs.File.Write(handle, 0, saveData, saveData.LongLength, nn.fs.WriteOption.Flush);
            result.abortUnlessSuccess();
            
            nn.fs.File.Close(handle);
            
        
            //UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
        }

        public void StartSaveOperation()
        {
            Debug.Log("Entering exit request handling section...");
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
        }
        public void EndSaveOperation()
        {
            var result = nn.fs.FileSystem.Commit(_serialization.DataPath);
            result.abortUnlessSuccess();
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
            Debug.Log("Ending exit request handling section...");
        }

        public void TryDeserializeGameStateAsync(string folderName)
        {
            // get saves path
            string savesPath = SaveGameManager._serializationAsset.GetSavesPath();
            string saveFolderPath = Path.Combine(savesPath, folderName);

            // check if directory exists
            if (!DirectoryUtils.Exists(saveFolderPath))
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
            nn.Result result;
            
            nn.fs.FileHandle handle = new nn.fs.FileHandle();
            
            result = nn.fs.File.Open(ref handle, path, nn.fs.OpenFileMode.Read);
            if (!result.IsSuccess())
            {
                if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
                {
                    Debug.LogFormat("SaveSystem Error: File not found: {0}", path);

                    return null;
                }
                else
                {
                    Debug.LogErrorFormat("SaveSystem Error: Unable to open {0}: {1}", path, result.ToString());

                    return null;
                }
            }
            
            long fileSize = 0;
            nn.fs.File.GetSize(ref fileSize, handle);
            
            byte[] bytes = new byte[fileSize];
            nn.fs.File.Read(handle, 0, bytes, fileSize);
            nn.fs.File.Close(handle);
            
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
            
            if (DirectoryUtils.Exists(savesPath))
            {
                string[] directories = DirectoryUtils.GetDirectories(savesPath);
                foreach (var directoryPath in directories)
                {
                    //string fileName = saveInfoPrefix + saveExtension;
                    string saveInfoPath = Path.Combine(_serialization.GetSavesPath(),directoryPath, saveInfoPrefix + saveExtension);

                    // check if file exists
                    if (!FileUtils.Exists(saveInfoPath))
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
            Debug.LogError("SaveSystem warning : RemoveAllSaves is not implemented for SwitchFileWriteReader.");
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