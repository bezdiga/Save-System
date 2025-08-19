using System;
using System.Collections.Generic;
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem.Switch
{
    public static class DirectoryUtils
    {
        
        public static bool Exists(string path)
        {
            nn.fs.EntryType entryType = 0;
            string fullPath = path;
            
            nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, fullPath);
            
            if (result.IsSuccess())
            {
                return entryType == nn.fs.EntryType.Directory;
            }
            else if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
            {
                return false;
            }
            else
            {
                Debug.LogError($"An unexpected error occurred while checking entry type for {fullPath}: {result}");
                return false;
            }
        }
        public static string[] GetDirectories(string path)
        {
            var directoryNames = new List<string>();
            string fullPath = path;

            nn.fs.DirectoryHandle handle = new nn.fs.DirectoryHandle();
            nn.Result result = nn.fs.Directory.Open(ref handle, fullPath, nn.fs.OpenDirectoryMode.All);

            if (!result.IsSuccess())
            {
                // Dacă directorul nu există, returnăm o listă goală, nu este o eroare critică.
                if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
                {
                    return Array.Empty<string>();
                }
            
                Debug.LogError($"Failed to open directory {fullPath}: {result}");
                return Array.Empty<string>();
            }

            try
            {
                long entryCount = 0;
                result = nn.fs.Directory.GetEntryCount(ref entryCount, handle);
                if (!result.IsSuccess())
                {
                    Debug.LogError($"Failed to get entry count for {fullPath}: {result}");
                    return Array.Empty<string>();
                }

                var entryBuffer = new nn.fs.DirectoryEntry[entryCount];
                long entriesRead = 0;
            
                result = nn.fs.Directory.Read(
                    ref entriesRead,
                    entryBuffer,
                    handle,
                    entryCount
                );
            
                if (result.IsSuccess() && entriesRead > 0)
                {
                    // 3. Procesăm rezultatele din buffer
                    for (int i = 0; i < entriesRead; i++)
                    {
                        if (entryBuffer[i].entryType == nn.fs.EntryType.Directory)
                        {
                            directoryNames.Add(entryBuffer[i].name);
                            Debug.LogError("Added directory: " + entryBuffer[i].name);
                        }
                    }
                }
            }
            finally
            {
                nn.fs.Directory.Close(handle);
            }
            return directoryNames.ToArray();
        }
    }
    
    public static class FileUtils
    {
        public static bool Exists(string path)
        {
            nn.fs.EntryType entryType = 0;
            string fullPath = path;
            
            nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, fullPath);
            
            if (result.IsSuccess())
            {
                return entryType == nn.fs.EntryType.File;
            }
            else if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
            {
                return false;
            }
            else
            {
                Debug.LogError($"An unexpected error occurred while checking entry type for {fullPath}: {result}");
                return false;
            }
        }
    }
}