using System.IO;
using nn;
using nn.fs;
using UnityEngine;
using File = nn.fs.File;
using static _JoykadeGames.Runtime.SaveSystem.Nitendo.SwitchFsErrorHandler;

namespace _JoykadeGames.Runtime.SaveSystem.Nitendo
{
    public class SwitchFileSystem : IFileSystemProvider
    {
        private readonly string mountName = "save";


        public SwitchFileSystem(string mountName)
        {
            this.mountName = mountName;
        }
        
        
        public bool Exists(string path)
        {
            EntryType entryType = 0;
            Debug.LogError("Check Exist file at path: " + path );
            Result result = FileSystem.GetEntryType(ref entryType, path);
            
            
            if (result.IsSuccess())
            {
                return entryType == EntryType.File;
            }

            
            if (FileSystem.ResultPathNotFound.Includes(result))
            {
                return false;
            }

            // Orice altă eroare este una gravă.
            result.abortUnlessSuccess();
            return false;
        }

        public bool WriteFile(string path, byte[] data)
        {
            long size = data.LongLength;
            Debug.LogError("Trying to write file: " + path + " with size: " + size);
            // Blochează notificările de sistem pe durata operațiunilor I/O critice
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();

            using (var writer = new WriteContext(mountName))
            {
                nn.Result result;
                
                result = nn.fs.File.Open(ref writer.handle, path, nn.fs.OpenFileMode.Write);
                
                if (!Check(result, out FileSystemErrorType error))
                {
                    if (error == FileSystemErrorType.PathNotFound)
                    {
                        result = nn.fs.File.Create(path, data.LongLength);
                        Debug.LogError("Create new file: " + path + " with size: " + data.LongLength);
                        if (!Check(result, out _)) return false;
                    }
                    result = nn.fs.File.Open(ref writer.handle, path, nn.fs.OpenFileMode.Write);
                    if (!Check(result, out _)) return false;
                }
                
                result = nn.fs.File.SetSize(writer.handle, data.LongLength);
            
                result = nn.fs.File.Write(writer.handle, 0, data, data.LongLength, nn.fs.WriteOption.Flush);
                result.abortUnlessSuccess();
            }
            
            return true;
        }

        public bool ReadFile(string path, out byte[] data)
        {
            data = null;

            Debug.LogError("Trying to read file: " + path);
            if (!Exists(path)) return false;

            nn.fs.FileHandle handle = new nn.fs.FileHandle();
            Result result = File.Open(ref handle, path, OpenFileMode.Read);
            
            if (!Check(result,out _)) return false;
            
            long fileSize = 0;
            result = File.GetSize(ref fileSize, handle);
            if (!result.IsSuccess())
            {
                Debug.LogError($"Switch ReadFile failed at GetSize step: {result}");
                File.Close(handle);
                return false;
            }

            data = new byte[fileSize];
            result = File.Read(handle, 0, data, fileSize);
            File.Close(handle);

            if (!Check(result, out _))
            {
                Debug.LogError($"Switch ReadFile failed at Read step: {result}");
                data = null;
                return false;
            }

            return true;
        }

    }
}