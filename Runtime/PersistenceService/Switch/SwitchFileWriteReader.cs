using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using nn.fs;
using OdinSerializer;
using UnityEngine;
using File = nn.fs.File;

namespace _JoykadeGames.Runtime.SaveSystem.Switch
{
    public class SwitchFileWriteReader : IWriterReader
    {
        private SerializationAsset _serialization;
        private bool isMounted = false;
        
        public SwitchFileWriteReader(SwitchInitParams initParams)
        {
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
            
            byte[] saveData;
            using (MemoryStream ms = new MemoryStream())
            {
                var context = new SerializationContext();
                var writer = new BinaryDataWriter(ms, context);
            
                SerializationUtility.SerializeValue(buffer, writer);
                saveData = ms.ToArray();
            }
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
            
            nn.Result result;
        
            nn.fs.FileHandle handle = new nn.fs.FileHandle();
            
            
            result = nn.fs.File.Open(ref handle, path, nn.fs.OpenFileMode.Write);
            
            if (!result.IsSuccess())
            {
                if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
                {
                    // DA, fișierul nu există. Acesta este un caz normal. Îl creăm.
                    result = nn.fs.File.Create(path, _serialization.DataSize);
                    if (!result.IsSuccess())
                    {
                        Debug.LogError($"File did not exist and creation failed: {result}");
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
            
            result = nn.fs.File.SetSize(handle, _serialization.DataSize);
            
            result = nn.fs.File.Write(handle, 0, saveData, _serialization.DataSize, nn.fs.WriteOption.Flush);
            result.abortUnlessSuccess();
            
            nn.fs.File.Close(handle);
        
            result = nn.fs.FileSystem.Commit(_serialization.DataPath);
            result.abortUnlessSuccess();
        
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
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
            string saveFolderPrefix = _serialization.SaveFolderPrefix;
            string saveInfoPrefix = _serialization.SaveInfoName;
            string saveExtension = _serialization.SaveExtension;
            IList<SavedGameInfo> saveInfos = new List<SavedGameInfo>();

            if (DirectoryUtils.Exists(savesPath))
            {
                string[] directories = DirectoryUtils.GetDirectories(savesPath);
                foreach (var directoryPath in directories)
                {
                    string fileName = saveInfoPrefix + saveExtension;
                    string saveInfoPath = Path.Combine(directoryPath, saveInfoPrefix + saveExtension);

                    // check if file exists
                    if (!FileUtils.Exists(saveInfoPath))
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
        
        ~SwitchFileWriteReader()
        {
            if (isMounted)
            {
                UnmountSave();
            }
        }
        private void UnmountSave()
        {
            Debug.LogError("Unmounting save data at path: " + _serialization.DataPath);
            isMounted = false;
            nn.fs.FileSystem.Unmount(_serialization.DataPath);
        }
        
        
    }
}