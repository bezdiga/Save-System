using System.IO;
using nn;
using nn.fs;
using UnityEngine;
using File = nn.fs.File;


namespace _JoykadeGames.Runtime.SaveSystem.Nitendo
{
    public class SwitchFileSystem : IFileSystemProvider
    {
        private FileHandle fileHandle;
        private readonly string mountName = "save";

        
        private string GetFullPath(string relativePath)
        {
            return $"{mountName}:/{relativePath}";
        }
        
        
        public bool Exists(string path)
        {
            EntryType entryType = 0;
            Result result = FileSystem.GetEntryType(ref entryType, path);
            
            if (result.IsSuccess())
            {
                return entryType == EntryType.File;
            }

            // Dacă eroarea este specific "PathNotFound", atunci nu există. E un caz normal.
            if (FileSystem.ResultPathNotFound.Includes(result))
            {
                return false;
            }

            // Orice altă eroare este una gravă.
            //result.abortUnlessSuccess();
            return false;
        }

        public bool WriteFile(string path, byte[] data)
        {
            string fullPath = GetFullPath(path);
            long size = data.LongLength;

            // Blochează notificările de sistem pe durata operațiunilor I/O critice
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();

            // Pe Switch, Create șterge și recreează fișierul, perfect pentru suprascriere.
            Result result = File.Create(fullPath, size);
            if (!result.IsSuccess())
            {
                Debug.LogError($"Switch WriteFile failed at Create step: {result}");
                UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
                return false;
            }

            result = File.Open(ref fileHandle, fullPath, OpenFileMode.Write);
            if (!result.IsSuccess())
            {
                Debug.LogError($"Switch WriteFile failed at Open step: {result}");
                UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
                return false;
            }

            result = File.Write(fileHandle, 0, data, size, WriteOption.Flush);
            File.Close(fileHandle); 

            if (!result.IsSuccess())
            {
                Debug.LogError($"Switch WriteFile failed at Write step: {result}");
                UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
                return false;
            }
            
            result = FileSystem.Commit(mountName);
            
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
        
            if (!result.IsSuccess())
            {
                Debug.LogError($"Switch WriteFile failed at Commit step: {result}");
                return false;
            }

            return true;
        }

        public bool ReadFile(string path, out byte[] data)
        {
            data = null;
            string fullPath = GetFullPath(path);

            if (!Exists(path)) return false;

            Result result = File.Open(ref fileHandle, fullPath, OpenFileMode.Read);
            if (!result.IsSuccess())
            {
                Debug.LogError($"Switch ReadFile failed at Open step: {result}");
                return false;
            }

            long fileSize = 0;
            result = File.GetSize(ref fileSize, fileHandle);
            if (!result.IsSuccess())
            {
                Debug.LogError($"Switch ReadFile failed at GetSize step: {result}");
                File.Close(fileHandle);
                return false;
            }

            data = new byte[fileSize];
            result = File.Read(fileHandle, 0, data, fileSize);
            File.Close(fileHandle);

            if (!result.IsSuccess())
            {
                Debug.LogError($"Switch ReadFile failed at Read step: {result}");
                data = null;
                return false;
            }

            return true;
        }

    }
}